#!/usr/bin/env python3
"""Apply OryxBot color grading to minimap overlay images."""

import os
import time
import concurrent.futures
from PIL import Image
import numpy as np

IMAGES_DIR = "/home/oryxbot/app/web/OryxBot.MapViewer/data/tiles/images"


def grade_tile(arr):
    arr = arr.astype(np.float32)
    brightness = arr.mean(axis=2)

    dark_mask = np.clip(1.0 - brightness / 120.0, 0, 1) ** 1.2
    dark_mask3 = dark_mask[:, :, np.newaxis]
    target_dark = np.array([10, 11, 13], dtype=np.float32)
    arr = arr * (1 - dark_mask3 * 0.92) + target_dark * dark_mask3 * 0.92

    arr *= 0.58

    norm = np.clip(arr / 150.0, 0, 1)
    arr[:, :, 0] += norm[:, :, 0] * 30
    arr[:, :, 1] += norm[:, :, 1] * 16
    arr[:, :, 2] -= norm[:, :, 2] * 22

    mean = arr.mean()
    arr = (arr - mean) * 1.25 + mean

    gray = arr.mean(axis=2, keepdims=True)
    arr = arr * 0.70 + gray * 0.30

    return np.clip(arr, 0, 255).astype(np.uint8)


def process_image(path):
    try:
        img = Image.open(path)
        has_alpha = img.mode == "RGBA"

        if has_alpha:
            alpha = np.array(img)[:, :, 3]
            rgb = np.array(img.convert("RGB"))
        else:
            rgb = np.array(img.convert("RGB"))

        if rgb.std() < 2:
            return "SKIP"

        graded_rgb = grade_tile(rgb)

        if has_alpha:
            result = np.dstack([graded_rgb, alpha])
            Image.fromarray(result, "RGBA").save(path, "WEBP", quality=85)
        else:
            Image.fromarray(graded_rgb).save(path, "WEBP", quality=85)
        return "OK"
    except Exception as e:
        return f"FAIL: {e}"


def main():
    files = [
        os.path.join(IMAGES_DIR, f)
        for f in os.listdir(IMAGES_DIR)
        if f.endswith(".webp")
    ]
    total = len(files)
    print(f"Grading {total} minimap images...")
    start = time.time()
    ok = skip = fail = 0

    with concurrent.futures.ThreadPoolExecutor(max_workers=8) as executor:
        futures = {executor.submit(process_image, f): f for f in files}
        for i, future in enumerate(concurrent.futures.as_completed(futures), 1):
            result = future.result()
            if result == "OK":
                ok += 1
            elif result == "SKIP":
                skip += 1
            else:
                fail += 1
                print(f"  {os.path.basename(futures[future])}: {result}")
            if i % 100 == 0 or i == total:
                elapsed = time.time() - start
                print(f"  Progress: {i}/{total} [{elapsed:.1f}s]")

    elapsed = time.time() - start
    print(f"\nDone: {ok} graded, {skip} skipped, {fail} failed in {elapsed:.1f}s")


if __name__ == "__main__":
    main()
