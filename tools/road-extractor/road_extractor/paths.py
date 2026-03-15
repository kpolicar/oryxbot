"""Path extraction between exits using detection-biased shortest paths."""

import cv2
import numpy as np
from scipy.sparse import lil_matrix
from scipy.sparse.csgraph import dijkstra, minimum_spanning_tree
from skimage.measure import approximate_polygon

from .coords import pixels_to_local

# Downsample factor for the pathfinding grid
SCALE = 4


def build_cost_map(image_rgba, road_mask):
    """Build a traversal cost map from image features and road detection.

    Detected road pixels get very low cost, non-road gets cost based on
    local brightness (brighter = slightly cheaper).

    Args:
        image_rgba: Original RGBA image array.
        road_mask: Boolean mask of detected road pixels.

    Returns:
        2D float32 cost map, same size as image.
    """
    H, W = image_rgba.shape[:2]
    opaque = image_rgba[:, :, 3] > 128
    opaque_f = opaque.astype(np.float32)

    gray = cv2.cvtColor(image_rgba[:, :, :3], cv2.COLOR_RGB2GRAY).astype(np.float32)
    g_blur = cv2.blur(gray * opaque_f, (21, 21))
    cnt_blur = np.maximum(cv2.blur(opaque_f, (21, 21)), 0.001)
    local_mean = g_blur / cnt_blur
    rel_bright = gray - local_mean

    # Normalize brightness to [0, 1]
    vals = rel_bright[opaque]
    lo, hi = np.percentile(vals, 5), np.percentile(vals, 95)
    bright_score = np.clip((rel_bright - lo) / (hi - lo + 1e-6), 0, 1)

    # Dilate road mask so pathfinding can snap to roads within a few pixels
    road_f = road_mask.astype(np.float32)
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
    road_dilated = cv2.dilate(road_f * 255, kernel, iterations=1) / 255

    # Cost layers:
    # - Detected road: 0.05 (very cheap)
    # - Near road (dilated): 0.15
    # - Terrain: 0.7 - 0.3 * brightness (brighter = slightly cheaper)
    # - Non-opaque: 1000 (impassable)
    cost = np.where(road_f > 0, 0.05,
           np.where(road_dilated > 0, 0.15,
           0.7 - 0.3 * bright_score))
    cost = cost * opaque_f + (~opaque).astype(np.float32) * 1000

    return cost


def _build_graph(cost_map, opaque_mask, scale=SCALE):
    """Build a sparse graph from a downsampled cost map."""
    H, W = cost_map.shape
    small_cost = cv2.resize(cost_map, (W // scale, H // scale), interpolation=cv2.INTER_AREA)
    small_opaque = cv2.resize(opaque_mask.astype(np.uint8) * 255,
                              (W // scale, H // scale),
                              interpolation=cv2.INTER_NEAREST) > 128
    sh, sw = small_cost.shape

    n = sh * sw
    graph = lil_matrix((n, n), dtype=np.float32)

    for y in range(sh):
        for x in range(sw):
            if not small_opaque[y, x]:
                continue
            idx = y * sw + x
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    if dy == 0 and dx == 0:
                        continue
                    ny, nx = y + dy, x + dx
                    if 0 <= ny < sh and 0 <= nx < sw and small_opaque[ny, nx]:
                        step = 1.414 if (dy != 0 and dx != 0) else 1.0
                        nidx = ny * sw + nx
                        graph[idx, nidx] = (small_cost[y, x] + small_cost[ny, nx]) / 2 * step

    return graph.tocsr(), sh, sw


def find_exit_paths(cost_map, opaque_mask, road_mask, exit_pixel_positions, scale=SCALE):
    """Find shortest paths between exits through the cost map.

    Only keeps paths where a significant portion follows detected roads.
    Not all exits have roads — paths through pure terrain are filtered out.

    Args:
        cost_map: 2D float32 cost map.
        opaque_mask: Boolean mask of opaque (traversable) pixels.
        road_mask: Boolean mask of detected road pixels.
        exit_pixel_positions: Dict of exit_id -> (py, px).
        scale: Downsample factor.

    Returns:
        List of path dicts with 'pixel_coords' and 'exit_ids'.
    """
    if len(exit_pixel_positions) < 2:
        return []

    graph, sh, sw = _build_graph(cost_map, opaque_mask, scale)

    # Map exits to graph nodes
    exit_ids = list(exit_pixel_positions.keys())
    exit_nodes = []
    for eid in exit_ids:
        ey, ex = exit_pixel_positions[eid]
        sy, sx = int(round(ey)) // scale, int(round(ex)) // scale
        sy = min(max(sy, 0), sh - 1)
        sx = min(max(sx, 0), sw - 1)
        exit_nodes.append(sy * sw + sx)

    # Dijkstra from all exit nodes
    dist_matrix, predecessors = dijkstra(graph, indices=exit_nodes,
                                         return_predecessors=True)

    # Dilate road mask for proximity checking (path doesn't need to be
    # exactly on road pixels, just near them)
    road_dilated = cv2.dilate(road_mask.astype(np.uint8) * 255,
                              cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (9, 9)),
                              iterations=1) > 0

    has_roads = np.sum(road_mask) > 50  # whether detection found anything

    # Try all pairs, filter by road coverage
    paths = []
    for i in range(len(exit_ids)):
        for j in range(i + 1, len(exit_ids)):
            src = exit_nodes[i]
            tgt = exit_nodes[j]

            if np.isinf(dist_matrix[i, exit_nodes[j]]):
                continue

            pixel_path = _reconstruct_pixel_path(predecessors[i], src, tgt, sw, scale)
            if not pixel_path or len(pixel_path) < 2:
                continue

            # Check what fraction of the path is near detected roads
            if has_roads:
                on_road = 0
                for py, px in pixel_path:
                    if 0 <= py < road_dilated.shape[0] and 0 <= px < road_dilated.shape[1]:
                        if road_dilated[py, px]:
                            on_road += 1
                road_fraction = on_road / len(pixel_path)

                # Only keep paths where >30% follows detected roads
                if road_fraction < 0.3:
                    continue

            # Prepend/append exit positions
            ey_i, ex_i = exit_pixel_positions[exit_ids[i]]
            ey_j, ex_j = exit_pixel_positions[exit_ids[j]]
            pixel_path = [(int(round(ey_i)), int(round(ex_i)))] + pixel_path + \
                         [(int(round(ey_j)), int(round(ex_j)))]

            paths.append({
                'pixel_coords': pixel_path,
                'exit_ids': [exit_ids[i], exit_ids[j]],
            })

    return paths


def _reconstruct_pixel_path(predecessors, src, tgt, sw, scale):
    """Reconstruct a pixel path from Dijkstra predecessors."""
    path = []
    cur = tgt
    max_steps = 10000
    while cur != src and cur >= 0 and max_steps > 0:
        py, px = divmod(cur, sw)
        path.append((py * scale, px * scale))
        cur = predecessors[cur]
        max_steps -= 1

    if cur < 0:
        return []

    py, px = divmod(src, sw)
    path.append((py * scale, px * scale))
    path.reverse()
    return path


def simplify_path(coords_yx, tolerance=3.0):
    """Simplify a path using Ramer-Douglas-Peucker."""
    if len(coords_yx) <= 2:
        return coords_yx
    coords = np.array(coords_yx, dtype=np.float64)
    simplified = approximate_polygon(coords, tolerance=tolerance)
    return simplified.tolist()


def build_road_paths(image_rgba, road_mask, exit_pixel_positions,
                     bounds_min, bounds_max, img_size=725):
    """Build final road path data structure.

    Combines road detection with shortest-path routing between exits.

    Args:
        image_rgba: Original RGBA image array.
        road_mask: Boolean mask of detected road pixels.
        exit_pixel_positions: Dict of exit_id -> (py, px).
        bounds_min: [x, y] minimap bounds min.
        bounds_max: [x, y] minimap bounds max.
        img_size: Image size in pixels.

    Returns:
        List of path dicts with 'coordinates' and 'exitIds'.
    """
    opaque = image_rgba[:, :, 3] > 128
    cost_map = build_cost_map(image_rgba, road_mask)

    raw_paths = find_exit_paths(cost_map, opaque, road_mask, exit_pixel_positions)

    paths = []
    for raw in raw_paths:
        pixel_coords = raw['pixel_coords']
        if len(pixel_coords) < 2:
            continue

        simplified = simplify_path(pixel_coords, tolerance=3.0)

        # Convert from (y, x) pixel coords to (x, y) local coords
        pixel_xy = np.array([(x, y) for y, x in simplified], dtype=np.float64)
        local_coords = pixels_to_local(pixel_xy, bounds_min, bounds_max, img_size)

        coordinates = [[round(c[0], 1), round(c[1], 1)] for c in local_coords]

        paths.append({
            'coordinates': coordinates,
            'exitIds': raw['exit_ids'],
        })

    return paths
