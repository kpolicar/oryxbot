#!/usr/bin/env python3
"""Download Albion Online map tiles from albionfreemarket CDN."""

import os
import sys
import urllib.request
import concurrent.futures
import time

BASE_URL = "https://cdn.albionfreemarket.com/AlbionWorld/map/maps"
OUTPUT_DIR = "/home/oryxbot/app/web/OryxBot.MapViewer/data/tiles/maps"

# Tile grid dimensions per zoom level (discovered by probing)
ZOOM_LEVELS = {
    0: (1, 2),    # 1x2
    1: (2, 4),    # 2x4
    2: (4, 7),    # 4x7
    3: (8, 11),   # 8x11
    4: (16, 19),  # 16x19
    5: (32, 35),  # 32x35
    6: (64, 67),  # 64x67
}

HEADERS = {
    "Referer": "https://albionfreemarket.com/",
    "User-Agent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
}

def download_tile(args):
    z, x, y = args
    url = f"{BASE_URL}/{z}/map_{x}_{y}.webp"
    out_dir = os.path.join(OUTPUT_DIR, str(z))
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, f"map_{x}_{y}.webp")

    if os.path.exists(out_path) and os.path.getsize(out_path) > 0:
        return f"SKIP {url}"

    try:
        req = urllib.request.Request(url, headers=HEADERS)
        with urllib.request.urlopen(req) as resp:
            data = resp.read()
        with open(out_path, "wb") as f:
            f.write(data)
        return f"OK   {url} ({len(data)} bytes)"
    except Exception as e:
        return f"FAIL {url}: {e}"

def main():
    tasks = []
    for z, (cols, rows) in ZOOM_LEVELS.items():
        for x in range(cols):
            for y in range(rows):
                tasks.append((z, x, y))

    total = len(tasks)
    print(f"Downloading {total} tiles across {len(ZOOM_LEVELS)} zoom levels...")

    done = 0
    failed = 0
    skipped = 0
    start = time.time()

    with concurrent.futures.ThreadPoolExecutor(max_workers=20) as executor:
        futures = {executor.submit(download_tile, t): t for t in tasks}
        for future in concurrent.futures.as_completed(futures):
            result = future.result()
            done += 1
            if result.startswith("FAIL"):
                failed += 1
                print(result)
            elif result.startswith("SKIP"):
                skipped += 1

            if done % 200 == 0 or done == total:
                elapsed = time.time() - start
                print(f"Progress: {done}/{total} ({skipped} skipped, {failed} failed) [{elapsed:.1f}s]")

    elapsed = time.time() - start
    print(f"\nDone! {done} tiles processed ({skipped} skipped, {failed} failed) in {elapsed:.1f}s")

if __name__ == "__main__":
    main()
