"""Road pixel detection using CLAHE-enhanced local contrast.

Roads on Albion minimaps are thin (~2-3px), warm-colored lines.
CLAHE on the R-B (warmth) channel dramatically amplifies road
contrast on most biomes. We then use morphological top-hat to
extract thin features from the enhanced signal.
"""

import cv2
import numpy as np


def detect_roads(image_rgba):
    """Detect road pixels using CLAHE-enhanced warmth + top-hat.

    Pipeline:
    1. CLAHE on R-B channel to enhance local warmth contrast
    2. Top-hat on enhanced R-B to extract thin warm features
    3. Hue filter (H < 30) to reject non-warm false positives
    4. Morphological cleanup: enforce thinness, remove noise

    Args:
        image_rgba: (H, W, 4) uint8 RGBA image array (original, not graded).

    Returns:
        Boolean mask of detected road pixels.
    """
    alpha = image_rgba[:, :, 3]
    opaque = alpha > 128
    rgb = image_rgba[:, :, :3]

    if np.sum(opaque) < 100:
        return np.zeros(alpha.shape, dtype=bool)

    r = rgb[:, :, 0].astype(np.float32)
    b = rgb[:, :, 2].astype(np.float32)
    hsv = cv2.cvtColor(rgb, cv2.COLOR_RGB2HSV)
    h = hsv[:, :, 0].astype(np.float32)

    # --- CLAHE on R-B warmth channel ---
    rb_raw = np.clip(r - b + 128, 0, 255).astype(np.uint8)
    clahe = cv2.createCLAHE(clipLimit=4.0, tileGridSize=(8, 8))
    rb_enhanced = clahe.apply(rb_raw)

    # --- Top-hat on CLAHE-enhanced R-B: extract thin warm features ---
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (11, 11))
    tophat = cv2.morphologyEx(rb_enhanced, cv2.MORPH_TOPHAT, kernel)

    vals = tophat[opaque]
    p97 = np.percentile(vals, 97)
    thresh = max(p97 * 0.5, 12)
    road_mask = (tophat > thresh) & (h < 35) & opaque

    return _morphological_cleanup(road_mask)


def _morphological_cleanup(road_mask):
    """Clean up road mask: close gaps, enforce thinness, remove noise."""
    mask_uint8 = road_mask.astype(np.uint8) * 255

    # Close small gaps
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
    mask_uint8 = cv2.morphologyEx(mask_uint8, cv2.MORPH_CLOSE, kernel, iterations=1)

    # Remove isolated noise
    mask_uint8 = cv2.morphologyEx(mask_uint8, cv2.MORPH_OPEN, kernel, iterations=1)

    # Width filter: roads are thin (2-3px), reject wider blobs
    dist = cv2.distanceTransform(mask_uint8, cv2.DIST_L2, 5)
    thin = (mask_uint8 > 0) & (dist <= 4)

    # Remove small connected components (noise)
    thin_uint8 = thin.astype(np.uint8) * 255
    num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(thin_uint8, connectivity=8)
    cleaned = np.zeros_like(thin)
    for i in range(1, num_labels):
        if stats[i, cv2.CC_STAT_AREA] >= 20:
            cleaned[labels == i] = True

    return cleaned
