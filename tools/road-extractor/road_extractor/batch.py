"""Batch processing of all minimap images."""

import json
import os
from concurrent.futures import ProcessPoolExecutor, as_completed
from datetime import datetime, timezone

from .pipeline import process_location


def _process_one(args):
    """Worker function for parallel processing."""
    location, cache_dir, debug_dir = args
    try:
        return process_location(location, cache_dir, debug_dir)
    except Exception as e:
        return {'locationId': location['id'], 'paths': [], 'error': str(e)}


def run(data_dir, output_path, workers=8, debug=False, single_id=None):
    """Process all minimap images and write road paths JSON.

    Args:
        data_dir: Path to map-viewer data directory.
        output_path: Path to write output JSON.
        workers: Number of parallel workers.
        debug: If True, save debug images.
        single_id: If set, only process this location ID.
    """
    locations_path = os.path.join(data_dir, 'albionLocations.json')
    cache_dir = os.path.join(os.path.dirname(output_path), 'originals_cache')
    debug_dir = os.path.join(os.path.dirname(output_path), 'road_debug') if debug else None

    os.makedirs(cache_dir, exist_ok=True)

    with open(locations_path) as f:
        all_locations = json.load(f)

    # Filter to locations with 2+ exits and an image file
    tasks = []
    for loc in all_locations:
        if single_id and loc['id'] != single_id:
            continue
        exits = loc.get('exits', [])
        if len(exits) < 2:
            continue
        if not loc.get('imageFile'):
            continue
        if not loc.get('minimapBoundsMin') or not loc.get('minimapBoundsMax'):
            continue

        tasks.append((loc, cache_dir, debug_dir))

    print(f'Found {len(tasks)} locations with 2+ exits')

    roads = {}
    errors = 0
    with_roads = 0

    if workers <= 1 or len(tasks) == 1:
        for i, task in enumerate(tasks):
            result = _process_one(task)
            loc_id = result['locationId']
            if 'error' in result:
                print(f'  [{i+1}/{len(tasks)}] {loc_id}: ERROR - {result["error"]}')
                errors += 1
            else:
                path_count = len(result['paths'])
                roads[loc_id] = {'paths': result['paths']}
                if path_count > 0:
                    with_roads += 1
                if (i + 1) % 20 == 0 or i == len(tasks) - 1:
                    print(f'  [{i+1}/{len(tasks)}] processed')
    else:
        with ProcessPoolExecutor(max_workers=workers) as executor:
            futures = {executor.submit(_process_one, t): t[0]['id'] for t in tasks}
            done = 0
            for future in as_completed(futures):
                done += 1
                result = future.result()
                loc_id = result['locationId']
                if 'error' in result:
                    print(f'  {loc_id}: ERROR - {result["error"]}')
                    errors += 1
                else:
                    path_count = len(result['paths'])
                    roads[loc_id] = {'paths': result['paths']}
                    if path_count > 0:
                        with_roads += 1
                if done % 50 == 0 or done == len(tasks):
                    print(f'  [{done}/{len(tasks)}] processed')

    output = {
        'extractedAt': datetime.now(timezone.utc).isoformat(),
        'version': 1,
        'totalLocations': len(tasks),
        'locationsWithRoads': with_roads,
        'roads': roads,
    }

    os.makedirs(os.path.dirname(os.path.abspath(output_path)), exist_ok=True)
    with open(output_path, 'w') as f:
        json.dump(output, f, separators=(',', ':'))

    print(f'\nDone! {with_roads}/{len(tasks)} locations with roads, {errors} errors')
    print(f'Output: {output_path}')
