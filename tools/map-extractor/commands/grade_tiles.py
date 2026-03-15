"""Apply OryxBot color grading to all worldmap tiles.

Two-pass: first compute global mean, then grade with fixed contrast center.
"""

import os
import time
import concurrent.futures
from PIL import Image
import numpy as np

ZOOM_LEVELS = {
    0: (1, 2),
    1: (2, 4),
    2: (4, 7),
    3: (8, 11),
    4: (16, 19),
    5: (32, 35),
    6: (64, 67),
}


def pre_grade(arr):
    """Apply darkening and gold warmth only (before contrast), return float32 array."""
    arr = arr.astype(np.float32)
    arr *= 0.58
    norm = np.clip(arr / 150.0, 0, 1)
    arr[:, :, 0] += norm[:, :, 0] * 30
    arr[:, :, 1] += norm[:, :, 1] * 16
    arr[:, :, 2] -= norm[:, :, 2] * 22
    return arr


def compute_tile_stats(args, tiles_dir):
    """Pass 1: compute sum and count of pre-graded pixel values."""
    z, x, y = args
    path = os.path.join(tiles_dir, str(z), f"map_{x}_{y}.webp")
    if not os.path.exists(path):
        return 0.0, 0

    try:
        img = Image.open(path).convert("RGB")
        arr = np.array(img)
        pre = pre_grade(arr)
        return float(pre.sum()), pre.size
    except Exception:
        return 0.0, 0


def grade_tile(arr, global_mean):
    """Apply full OryxBot color grading with fixed global contrast center."""
    arr = arr.astype(np.float32)

    # Darken
    arr *= 0.58

    # Gold warmth
    norm = np.clip(arr / 150.0, 0, 1)
    arr[:, :, 0] += norm[:, :, 0] * 30
    arr[:, :, 1] += norm[:, :, 1] * 16
    arr[:, :, 2] -= norm[:, :, 2] * 22

    # Contrast boost with FIXED global mean
    arr = (arr - global_mean) * 1.25 + global_mean

    # Desaturate
    gray = arr.mean(axis=2, keepdims=True)
    arr = arr * 0.70 + gray * 0.30

    return np.clip(arr, 0, 255).astype(np.uint8)


def process_tile(args, tiles_dir, global_mean):
    z, x, y = args
    path = os.path.join(tiles_dir, str(z), f"map_{x}_{y}.webp")
    if not os.path.exists(path):
        return "MISS"

    try:
        img = Image.open(path).convert("RGB")
        arr = np.array(img)
        graded = Image.fromarray(grade_tile(arr, global_mean))
        graded.save(path, "WEBP", quality=85)
        return "OK"
    except Exception as e:
        return f"FAIL: {e}"


def run(output_dir, workers=8):
    tiles_dir = os.path.join(output_dir, "tiles", "maps")

    tasks = []
    for z, (cols, rows) in ZOOM_LEVELS.items():
        for x in range(cols):
            for y in range(rows):
                tasks.append((z, x, y))

    total = len(tasks)

    # Pass 1: compute global mean from z=4 tiles
    print("Pass 1: Computing global mean from z=4 tiles...")
    z4_tasks = [(z, x, y) for z, x, y in tasks if z == 4]
    start = time.time()
    total_sum = 0.0
    total_count = 0

    with concurrent.futures.ThreadPoolExecutor(max_workers=workers) as executor:
        futures = [executor.submit(compute_tile_stats, t, tiles_dir) for t in z4_tasks]
        for f in concurrent.futures.as_completed(futures):
            s, c = f.result()
            total_sum += s
            total_count += c

    global_mean = total_sum / total_count if total_count > 0 else 60.0
    elapsed = time.time() - start
    print(f"  Global mean: {global_mean:.2f} (from {len(z4_tasks)} z4 tiles in {elapsed:.1f}s)")

    # Pass 2: grade all tiles with fixed global mean
    print(f"\nPass 2: Grading {total} tiles with global_mean={global_mean:.2f}...")
    start = time.time()
    ok = fail = miss = 0

    with concurrent.futures.ThreadPoolExecutor(max_workers=workers) as executor:
        futures = {executor.submit(process_tile, t, tiles_dir, global_mean): t for t in tasks}
        for i, future in enumerate(concurrent.futures.as_completed(futures), 1):
            result = future.result()
            if result == "OK":
                ok += 1
            elif result == "MISS":
                miss += 1
            else:
                fail += 1
                print(f"  {futures[future]}: {result}")
            if i % 500 == 0 or i == total:
                elapsed = time.time() - start
                print(f"  Progress: {i}/{total} ({ok} graded, {fail} failed) [{elapsed:.1f}s]")

    elapsed = time.time() - start
    print(f"\nDone: {ok} graded, {miss} missing, {fail} failed in {elapsed:.1f}s")
