#!/usr/bin/env python3
"""
Extract road paths from all Albion Online minimap images.

Uses color-biased Dijkstra pathfinding between cluster exits:
1. Download original (ungraded) minimap from CDN
2. Build cost map from color distance to known road color per biome
3. Pathfind between all pairs of cluster exits (excluding dungeon exits)
4. Filter paths by road color coverage (reject non-road connections)
5. Convert pixel paths to minimap local coordinates
6. Output roadPaths.json for the map-viewer
"""
import cv2
import numpy as np
from PIL import Image
import json
import os
import re
import math
import sys
import urllib.request
from concurrent.futures import ProcessPoolExecutor, as_completed
from skimage.graph import MCP_Geometric

# ── Config ──

DATA_DIR = os.path.join(os.path.dirname(__file__), '..', 'map-viewer', 'data')
LOC_FILE = os.path.join(DATA_DIR, 'albionLocations.json')
CACHE_DIR = os.path.join(DATA_DIR, 'originals_cache')
OUTPUT_FILE = os.path.join(DATA_DIR, 'roadPaths.json')
CDN_BASE = 'https://cdn.albionfreemarket.com/AlbionWorld/map/images'

BIOME_COLORS = {
    'ST': (0xFF, 0xC3, 0x9F),  # steppe/desert
    'FR': (0xA7, 0x74, 0x4E),  # forest
    'SW': (0xB1, 0x89, 0x4B),  # swamp
    'MN': (0xCF, 0xAB, 0xA8),  # mountain
    'HL': (0x9E, 0x70, 0x47),  # highland
}

# Pathfinding params
COLOR_DIST_NORM = 150.0   # normalize color distance
COST_POWER = 2            # cost exponent (higher = stronger road preference)
ON_ROAD_THRESH = 50       # color distance threshold for "on road"
MIN_ROAD_COVERAGE = 0.40  # min fraction of path on road to keep
PATH_SIMPLIFY_PX = 3      # simplify paths to reduce point count

# ── Coord transform ──

_ANGLE = -45 * math.pi / 180
_COS, _SIN, _SCALE = math.cos(_ANGLE), math.sin(_ANGLE), 0.7

def local_to_pixel(pos, bmin, bmax, img_size=725):
    cx, cy = (bmin[0]+bmax[0])/2, (bmin[1]+bmax[1])/2
    dx, dy = pos[0]-cx, pos[1]-cy
    rx = (dx*_COS - dy*_SIN)*_SCALE
    ry = (dx*_SIN + dy*_COS)*_SCALE
    nx = (rx+cx-bmin[0])/(bmax[0]-bmin[0])
    ny = (ry+cy-bmin[1])/(bmax[1]-bmin[1])
    return nx*img_size, (1-ny)*img_size

def pixel_to_local(px, py, bmin, bmax, img_size=725):
    cx, cy = (bmin[0]+bmax[0])/2, (bmin[1]+bmax[1])/2
    nx = px / img_size
    ny = 1 - (py / img_size)
    rx = nx * (bmax[0]-bmin[0]) + bmin[0] - cx
    ry = ny * (bmax[1]-bmin[1]) + bmin[1] - cy
    rx /= _SCALE
    ry /= _SCALE
    inv_cos, inv_sin = math.cos(-_ANGLE), math.sin(-_ANGLE)
    ddx = rx*inv_cos - ry*inv_sin
    ddy = rx*inv_sin + ry*inv_cos
    return ddx+cx, ddy+cy

# ── Helpers ──

def get_biome(filename):
    for code in BIOME_COLORS:
        if f'_{code}_' in filename:
            return code
    return None

def is_cluster_exit(exit_data):
    tid = exit_data.get('targetLocationId', '')
    return bool(re.match(r'^\d+$', tid.strip())) if tid else False

def download_original(webp_name):
    """Download original minimap from CDN, cache locally."""
    os.makedirs(CACHE_DIR, exist_ok=True)
    cache_path = os.path.join(CACHE_DIR, webp_name)
    if os.path.exists(cache_path):
        return cache_path
    url = f'{CDN_BASE}/{webp_name}'
    try:
        req = urllib.request.Request(url, headers={
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
        })
        with urllib.request.urlopen(req) as resp:
            with open(cache_path, 'wb') as f:
                f.write(resp.read())
        return cache_path
    except Exception as e:
        print(f'  Download failed {webp_name}: {e}', file=sys.stderr)
        return None

def simplify_path(path_pixels, tolerance=PATH_SIMPLIFY_PX):
    """Reduce path points using Douglas-Peucker via OpenCV."""
    if len(path_pixels) < 3:
        return path_pixels
    pts = np.array(path_pixels, dtype=np.float32).reshape(-1, 1, 2)
    simplified = cv2.approxPolyDP(pts, tolerance, closed=False)
    return simplified.reshape(-1, 2)

def color_distance(rgb, target):
    diff = rgb.astype(np.float32) - np.array(target, dtype=np.float32)
    return np.sqrt(np.sum(diff**2, axis=2))

# ── Core pipeline ──

def process_location(loc):
    """Process a single location: download image, detect roads, return paths."""
    loc_id = loc['id']
    image_file = loc.get('imageFile', '')
    webp_name = image_file.replace('.png', '.webp')
    biome = get_biome(image_file)

    if not biome:
        return loc_id, None, 'unknown biome'

    road_color = BIOME_COLORS[biome]
    bmin = loc['minimapBoundsMin']
    bmax = loc['minimapBoundsMax']

    # Get cluster exits only
    all_exits = [e for e in loc.get('exits', []) if e.get('position')]
    exits = [e for e in all_exits if is_cluster_exit(e)]

    if len(exits) < 2:
        return loc_id, None, f'only {len(exits)} cluster exits'

    # Download original
    img_path = download_original(webp_name)
    if not img_path:
        return loc_id, None, 'download failed'

    try:
        img = Image.open(img_path).convert('RGBA')
    except Exception as e:
        return loc_id, None, f'load failed: {e}'

    arr = np.array(img)
    rgb = arr[:, :, :3]
    opaque = arr[:, :, 3] > 128
    h, w = rgb.shape[:2]

    # Exit pixel positions
    # Handle both array and dict position formats
    exit_pixels = []
    for e in exits:
        pos = e['position']
        if isinstance(pos, dict):
            pos = [pos['x'], pos['y']]
        exit_pixels.append(local_to_pixel(pos, bmin, bmax, w))

    # Build cost map from color distance
    cdist = color_distance(rgb, road_color)
    cdist_smooth = cv2.GaussianBlur(cdist, (5, 5), 1.0)
    cost = np.clip(cdist_smooth / COLOR_DIST_NORM, 0, 1) ** COST_POWER
    cost[~opaque] = 1e6
    cost = np.maximum(cost, 0.001)

    # Pathfind between all pairs
    mcp = MCP_Geometric(cost)
    paths = []

    for i in range(len(exit_pixels)):
        for j in range(i + 1, len(exit_pixels)):
            si = (int(np.clip(exit_pixels[i][1], 0, h-1)),
                  int(np.clip(exit_pixels[i][0], 0, w-1)))
            sj = (int(np.clip(exit_pixels[j][1], 0, h-1)),
                  int(np.clip(exit_pixels[j][0], 0, w-1)))

            try:
                mcp.find_costs([si])
                path = np.array(mcp.traceback(sj))
                if len(path) < 5:
                    continue

                # Check road coverage
                path_cdist = cdist[path[:, 0], path[:, 1]]
                on_road = np.sum(path_cdist < ON_ROAD_THRESH) / len(path_cdist)

                if on_road < MIN_ROAD_COVERAGE:
                    continue

                # Convert pixel path (row, col) to (x, y) pixels then simplify
                path_xy = path[:, ::-1].astype(np.float32)  # (col, row) = (x, y)
                simplified = simplify_path(path_xy)

                # Convert to local minimap coordinates
                local_coords = []
                for px, py in simplified:
                    lx, ly = pixel_to_local(float(px), float(py), bmin, bmax, w)
                    local_coords.append([round(lx, 1), round(ly, 1)])

                # Get exit position in same format
                exit_i_pos = exits[i]['position']
                exit_j_pos = exits[j]['position']
                if isinstance(exit_i_pos, dict):
                    exit_i_pos = [exit_i_pos['x'], exit_i_pos['y']]
                if isinstance(exit_j_pos, dict):
                    exit_j_pos = [exit_j_pos['x'], exit_j_pos['y']]

                paths.append({
                    'from': exits[i].get('targetLocationId', ''),
                    'to': exits[j].get('targetLocationId', ''),
                    'coverage': round(on_road, 2),
                    'points': local_coords,
                })
            except Exception:
                continue

    if not paths:
        return loc_id, None, 'no roads found'

    return loc_id, {'paths': paths}, f'{len(paths)} roads'


def main():
    import argparse
    parser = argparse.ArgumentParser(description='Extract roads from minimap images')
    parser.add_argument('--workers', type=int, default=8)
    parser.add_argument('--single', type=str, default=None, help='Process single location ID')
    parser.add_argument('--debug', action='store_true')
    args = parser.parse_args()

    # Load locations
    with open(LOC_FILE) as f:
        locations = json.load(f)

    # Filter to world locations with images and 2+ exits
    candidates = []
    for loc in locations:
        if not loc.get('imageFile'):
            continue
        biome = get_biome(loc['imageFile'])
        if not biome:
            continue
        exits = [e for e in loc.get('exits', []) if e.get('position')]
        cluster_exits = [e for e in exits if is_cluster_exit(e)]
        if len(cluster_exits) < 2:
            continue
        if args.single and loc['id'] != args.single:
            continue
        candidates.append(loc)

    print(f'Processing {len(candidates)} locations with {args.workers} workers...')

    results = {}
    errors = 0

    with ProcessPoolExecutor(max_workers=args.workers) as pool:
        futures = {pool.submit(process_location, loc): loc['id'] for loc in candidates}

        for i, future in enumerate(as_completed(futures), 1):
            loc_id = futures[future]
            try:
                rid, data, msg = future.result()
                if data:
                    results[rid] = data
                    if args.debug:
                        print(f'  [{i}/{len(candidates)}] {rid}: {msg}')
                else:
                    errors += 1
                    if args.debug:
                        print(f'  [{i}/{len(candidates)}] {rid}: {msg}')
            except Exception as e:
                errors += 1
                print(f'  [{i}/{len(candidates)}] {loc_id}: ERROR {e}', file=sys.stderr)

            if i % 50 == 0:
                print(f'  Progress: {i}/{len(candidates)} ({len(results)} roads found)')

    # Write output
    output = {
        'version': 1,
        'roads': results,
    }

    os.makedirs(os.path.dirname(OUTPUT_FILE), exist_ok=True)
    with open(OUTPUT_FILE, 'w') as f:
        json.dump(output, f, separators=(',', ':'))

    print(f'\nDone! {len(results)} locations with roads, {errors} failed/skipped')
    print(f'Output: {OUTPUT_FILE}')


if __name__ == '__main__':
    main()
