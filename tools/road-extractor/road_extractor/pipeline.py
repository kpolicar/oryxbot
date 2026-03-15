"""Per-image processing pipeline."""

import os
import urllib.request

import cv2
import numpy as np
from PIL import Image

from .coords import local_to_pixel
from .detect import detect_roads
from .paths import build_road_paths

CDN_BASE = "https://cdn.albionfreemarket.com/AlbionWorld/map/images"
CDN_HEADERS = {
    "Referer": "https://albionfreemarket.com/",
    "User-Agent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36",
}


def _download_original(image_file, cache_dir):
    """Download original (ungraded) minimap from CDN."""
    webp_name = os.path.splitext(image_file)[0] + '.webp'
    cache_path = os.path.join(cache_dir, webp_name)

    if os.path.exists(cache_path) and os.path.getsize(cache_path) > 0:
        return cache_path

    url = f"{CDN_BASE}/{webp_name}"
    try:
        req = urllib.request.Request(url, headers=CDN_HEADERS)
        with urllib.request.urlopen(req, timeout=15) as resp:
            data = resp.read()
        os.makedirs(os.path.dirname(cache_path), exist_ok=True)
        with open(cache_path, 'wb') as f:
            f.write(data)
        return cache_path
    except Exception:
        return None


def process_location(location, cache_dir, debug_dir=None):
    """Process a single location to extract road paths."""
    loc_id = location['id']
    bounds_min = location['minimapBoundsMin']
    bounds_max = location['minimapBoundsMax']
    exits = location.get('exits', [])
    image_file = location.get('imageFile', '')

    original_path = _download_original(image_file, cache_dir)
    if original_path is None:
        return {'locationId': loc_id, 'paths': []}

    img = Image.open(original_path).convert('RGBA')
    img_arr = np.array(img)
    img_size = img_arr.shape[0]

    exit_pixels = {}
    for ex in exits:
        pos = ex['position']
        px, py = local_to_pixel(pos, bounds_min, bounds_max, img_size)
        exit_pixels[ex['id']] = (py, px)

    if len(exit_pixels) < 2:
        return {'locationId': loc_id, 'paths': []}

    road_mask = detect_roads(img_arr)

    paths = build_road_paths(
        img_arr, road_mask, exit_pixels,
        bounds_min, bounds_max, img_size,
    )

    if debug_dir:
        _save_debug(img_arr, road_mask, exit_pixels, paths,
                     bounds_min, bounds_max, loc_id, debug_dir, img_size)

    return {'locationId': loc_id, 'paths': paths}


def _save_debug(img_arr, road_mask, exit_pixels, paths,
                bounds_min, bounds_max, loc_id, debug_dir, img_size):
    """Save a debug visualization image."""
    os.makedirs(debug_dir, exist_ok=True)
    canvas = img_arr[:, :, :3].copy()

    # Road mask in semi-transparent pink
    if road_mask is not None and np.any(road_mask):
        overlay = canvas.copy()
        overlay[road_mask, 0] = 255
        overlay[road_mask, 1] = 100
        overlay[road_mask, 2] = 100
        canvas = cv2.addWeighted(canvas, 0.6, overlay, 0.4, 0)

    # Draw paths
    colors = [(0, 255, 0), (255, 255, 0), (0, 255, 255), (255, 0, 255),
              (255, 128, 0), (128, 255, 0), (0, 128, 255), (255, 0, 128)]
    if paths:
        for pi, p in enumerate(paths):
            color = colors[pi % len(colors)]
            coords = p.get('coordinates', [])
            # Convert local coords back to pixels
            pts = []
            for lx, ly in coords:
                px, py = local_to_pixel([lx, ly], bounds_min, bounds_max, img_size)
                pts.append((int(round(px)), int(round(py))))
            for ci in range(len(pts) - 1):
                x1, y1 = pts[ci]
                x2, y2 = pts[ci + 1]
                cv2.line(canvas, (x1, y1), (x2, y2), color, 2)

    # Exit positions as blue circles
    for exit_id, (ey, ex) in exit_pixels.items():
        cv2.circle(canvas, (int(round(ex)), int(round(ey))), 8, (0, 100, 255), 2)

    out_path = os.path.join(debug_dir, f'{loc_id}_debug.png')
    Image.fromarray(canvas).save(out_path)
