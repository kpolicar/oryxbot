"""Skeletonization of road masks."""

import cv2
import numpy as np
from skimage.morphology import skeletonize


def skeletonize_roads(road_mask):
    """Reduce road mask to 1-pixel-wide skeleton.

    Erodes the mask first to thin thick road regions, then skeletonizes.
    Removes small disconnected fragments.

    Args:
        road_mask: Boolean mask of detected road pixels.

    Returns:
        Boolean skeleton mask.
    """
    mask_uint8 = road_mask.astype(np.uint8) * 255
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
    thinned = cv2.erode(mask_uint8, kernel, iterations=2)

    skeleton = skeletonize(thinned > 0)

    # Remove small disconnected fragments
    skel_uint8 = skeleton.astype(np.uint8) * 255
    num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(skel_uint8, connectivity=8)
    cleaned = np.zeros_like(skeleton)
    for i in range(1, num_labels):
        if stats[i, cv2.CC_STAT_AREA] >= 15:
            cleaned[labels == i] = True

    return cleaned
