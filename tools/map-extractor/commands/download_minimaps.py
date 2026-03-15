"""Download minimap overlay images from albionfreemarket CDN."""

import os
import json
import urllib.request
import concurrent.futures
import time

BASE_URL = "https://cdn.albionfreemarket.com/AlbionWorld/map/images"

HEADERS = {
    "Referer": "https://albionfreemarket.com/",
    "User-Agent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
}


def download_image(image_file, output_dir):
    webp_name = image_file.replace(".png", ".webp")
    url = f"{BASE_URL}/{webp_name}"
    out_path = os.path.join(output_dir, "tiles", "images", webp_name)

    if os.path.exists(out_path) and os.path.getsize(out_path) > 0:
        return "SKIP"

    try:
        req = urllib.request.Request(url, headers=HEADERS)
        with urllib.request.urlopen(req) as resp:
            data = resp.read()
        with open(out_path, "wb") as f:
            f.write(data)
        return "OK"
    except Exception as e:
        return f"FAIL: {e}"


def run(output_dir, workers=20):
    images_dir = os.path.join(output_dir, "tiles", "images")
    os.makedirs(images_dir, exist_ok=True)

    locations_file = os.path.join(output_dir, "albionLocations.json")
    with open(locations_file) as f:
        locations = json.load(f)

    image_files = list({
        loc["imageFile"]
        for loc in locations
        if loc.get("worldmapposition") and loc.get("imageFile")
    })

    print(f"Downloading {len(image_files)} minimap images...")
    start = time.time()
    ok = fail = skip = 0

    with concurrent.futures.ThreadPoolExecutor(max_workers=workers) as executor:
        futures = {executor.submit(download_image, f, output_dir): f for f in image_files}
        for future in concurrent.futures.as_completed(futures):
            result = future.result()
            if result == "OK":
                ok += 1
            elif result == "SKIP":
                skip += 1
            else:
                fail += 1
                print(f"  {futures[future]}: {result}")

    elapsed = time.time() - start
    print(f"Done: {ok} downloaded, {skip} skipped, {fail} failed in {elapsed:.1f}s")
