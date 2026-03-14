#!/usr/bin/env python3
"""Apply OryxBot color grading to all worldmap tiles."""

import os
import sys
import time
import concurrent.futures
from PIL import Image
import numpy as np

TILES_DIR = "/home/oryxbot/app/web/OryxBot.MapViewer/data/tiles/maps"

ZOOM_LEVELS = {
    0: (1, 2),
    1: (2, 4),
    2: (4, 7),
    3: (8, 11),
    4: (16, 19),
    5: (32, 35),
    6: (64, 67),
}


def grade_tile(arr):
    """Apply OryxBot premium dark-gold color grading."""
    arr = arr.astype(np.float32)
    brightness = arr.mean(axis=2)

    # Crush dark areas (ocean) to near-black #0a0b0d
    dark_mask = np.clip(1.0 - brightness / 120.0, 0, 1) ** 1.2
    dark_mask3 = dark_mask[:, :, np.newaxis]
    target_dark = np.array([10, 11, 13], dtype=np.float32)
    arr = arr * (1 - dark_mask3 * 0.92) + target_dark * dark_mask3 * 0.92

    # Darken land
    arr *= 0.58

    # Gold warmth on visible land
    norm = np.clip(arr / 150.0, 0, 1)
    arr[:, :, 0] += norm[:, :, 0] * 30
    arr[:, :, 1] += norm[:, :, 1] * 16
    arr[:, :, 2] -= norm[:, :, 2] * 22

    # Contrast boost
    mean = arr.mean()
    arr = (arr - mean) * 1.25 + mean

    # Desaturate for muted premium feel
    gray = arr.mean(axis=2, keepdims=True)
    arr = arr * 0.70 + gray * 0.30

    return np.clip(arr, 0, 255).astype(np.uint8)


def process_tile(args):
    z, x, y = args
    path = os.path.join(TILES_DIR, str(z), f"map_{x}_{y}.webp")
    if not os.path.exists(path):
        return "MISS"

    try:
        img = Image.open(path).convert("RGB")
        arr = np.array(img)

        # Skip fully uniform tiles (blank/ocean) - just darken them
        if arr.std() < 2:
            dark = Image.new("RGB", img.size, (10, 11, 13))
            dark.save(path, "WEBP", quality=85)
            return "BLANK"

        graded = Image.fromarray(grade_tile(arr))
        graded.save(path, "WEBP", quality=85)
        return "OK"
    except Exception as e:
        return f"FAIL: {e}"


def main():
    tasks = []
    for z, (cols, rows) in ZOOM_LEVELS.items():
        for x in range(cols):
            for y in range(rows):
                tasks.append((z, x, y))

    total = len(tasks)
    print(f"Grading {total} tiles...")
    start = time.time()
    ok = blank = fail = miss = 0

    with concurrent.futures.ThreadPoolExecutor(max_workers=8) as executor:
        futures = {executor.submit(process_tile, t): t for t in tasks}
        for i, future in enumerate(concurrent.futures.as_completed(futures), 1):
            result = future.result()
            if result == "OK":
                ok += 1
            elif result == "BLANK":
                blank += 1
            elif result == "MISS":
                miss += 1
            else:
                fail += 1
                print(f"  {futures[future]}: {result}")
            if i % 500 == 0 or i == total:
                elapsed = time.time() - start
                print(f"  Progress: {i}/{total} ({ok} graded, {blank} blank, {fail} failed) [{elapsed:.1f}s]")

    elapsed = time.time() - start
    print(f"\nDone: {ok} graded, {blank} blank, {miss} missing, {fail} failed in {elapsed:.1f}s")


if __name__ == "__main__":
    main()
