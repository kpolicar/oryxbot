"""Download Albion Online map tiles from albionfreemarket CDN."""

import os
import urllib.request
import concurrent.futures
import time

BASE_URL = "https://cdn.albionfreemarket.com/AlbionWorld/map/maps"

ZOOM_LEVELS = {
    0: (1, 2),
    1: (2, 4),
    2: (4, 7),
    3: (8, 11),
    4: (16, 19),
    5: (32, 35),
    6: (64, 67),
}

HEADERS = {
    "Referer": "https://albionfreemarket.com/",
    "User-Agent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
}


def download_tile(args, output_dir):
    z, x, y = args
    url = f"{BASE_URL}/{z}/map_{x}_{y}.webp"
    out_dir = os.path.join(output_dir, "tiles", "maps", str(z))
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, f"map_{x}_{y}.webp")

    if os.path.exists(out_path) and os.path.getsize(out_path) > 0:
        return "SKIP"

    try:
        req = urllib.request.Request(url, headers=HEADERS)
        with urllib.request.urlopen(req) as resp:
            data = resp.read()
        with open(out_path, "wb") as f:
            f.write(data)
        return f"OK ({len(data)} bytes)"
    except Exception as e:
        return f"FAIL: {e}"


def run(output_dir, workers=20):
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

    with concurrent.futures.ThreadPoolExecutor(max_workers=workers) as executor:
        futures = {executor.submit(download_tile, t, output_dir): t for t in tasks}
        for future in concurrent.futures.as_completed(futures):
            result = future.result()
            done += 1
            if result.startswith("FAIL"):
                failed += 1
                print(f"  {futures[future]}: {result}")
            elif result == "SKIP":
                skipped += 1

            if done % 200 == 0 or done == total:
                elapsed = time.time() - start
                print(f"Progress: {done}/{total} ({skipped} skipped, {failed} failed) [{elapsed:.1f}s]")

    elapsed = time.time() - start
    print(f"\nDone! {done} tiles processed ({skipped} skipped, {failed} failed) in {elapsed:.1f}s")
