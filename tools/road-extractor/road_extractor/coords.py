"""Coordinate transforms between minimap local space and pixel space.

Matches the map-viewer's CoordTransform.localToMapCoords() logic:
  1. Center on bounds midpoint
  2. Rotate -45° and scale by 0.7
  3. Normalize to [0, 1] by bounds range
  4. Flip Y axis (Leaflet Y increases up, pixel Y increases down)
  5. Multiply by image size to get pixels
"""

import math
import numpy as np

_ANGLE = -45 * math.pi / 180
_COS = math.cos(_ANGLE)
_SIN = math.sin(_ANGLE)
_SCALE = 0.7


def local_to_pixel(pos, bounds_min, bounds_max, img_size=725):
    """Convert a local minimap position [x, y] to pixel coordinates (px, py)."""
    bmin_x, bmin_y = bounds_min
    bmax_x, bmax_y = bounds_max
    cx = (bmin_x + bmax_x) / 2
    cy = (bmin_y + bmax_y) / 2

    dx = pos[0] - cx
    dy = pos[1] - cy

    rx = (dx * _COS - dy * _SIN) * _SCALE
    ry = (dx * _SIN + dy * _COS) * _SCALE

    norm_x = (rx + cx - bmin_x) / (bmax_x - bmin_x)
    norm_y = (ry + cy - bmin_y) / (bmax_y - bmin_y)

    # Y-flip: Leaflet Y increases upward, pixel Y increases downward
    return norm_x * img_size, (1 - norm_y) * img_size


def pixel_to_local(px, py, bounds_min, bounds_max, img_size=725):
    """Convert pixel coordinates (px, py) to local minimap position [x, y]."""
    bmin_x, bmin_y = bounds_min
    bmax_x, bmax_y = bounds_max
    cx = (bmin_x + bmax_x) / 2
    cy = (bmin_y + bmax_y) / 2

    # Reverse Y-flip
    norm_x = px / img_size
    norm_y = 1 - (py / img_size)

    # Reverse normalize
    rx = norm_x * (bmax_x - bmin_x) + bmin_x - cx
    ry = norm_y * (bmax_y - bmin_y) + bmin_y - cy

    # Reverse scale
    rx /= _SCALE
    ry /= _SCALE

    # Reverse rotation (+45°)
    inv_cos = math.cos(-_ANGLE)
    inv_sin = math.sin(-_ANGLE)
    dx = rx * inv_cos - ry * inv_sin
    dy = rx * inv_sin + ry * inv_cos

    return dx + cx, dy + cy


def pixels_to_local(pixel_coords, bounds_min, bounds_max, img_size=725):
    """Batch convert an array of pixel coordinates (N, 2) to local coords."""
    bmin = np.array(bounds_min, dtype=np.float64)
    bmax = np.array(bounds_max, dtype=np.float64)
    center = (bmin + bmax) / 2
    span = bmax - bmin

    coords = np.asarray(pixel_coords, dtype=np.float64)

    # Reverse Y-flip, then reverse normalize
    norm_x = coords[:, 0] / img_size
    norm_y = 1 - (coords[:, 1] / img_size)

    rx = norm_x * span[0] + bmin[0] - center[0]
    ry = norm_y * span[1] + bmin[1] - center[1]

    # Reverse scale
    rx /= _SCALE
    ry /= _SCALE

    # Reverse rotation
    inv_cos = math.cos(-_ANGLE)
    inv_sin = math.sin(-_ANGLE)
    dx = rx * inv_cos - ry * inv_sin
    dy = rx * inv_sin + ry * inv_cos
    result = np.column_stack([dx + center[0], dy + center[1]])
    return result
