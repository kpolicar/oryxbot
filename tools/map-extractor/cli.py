#!/usr/bin/env python3
"""OryxBot Map Extractor — download and color-grade Albion Online map assets."""

import argparse
import os
import sys


def get_default_output_dir():
    return os.path.join(os.path.dirname(__file__), "..", "map-viewer", "data")


def cmd_download_tiles(args):
    from commands.download_tiles import run
    run(output_dir=args.output_dir, workers=args.workers)


def cmd_download_minimaps(args):
    from commands.download_minimaps import run
    run(output_dir=args.output_dir, workers=args.workers)


def cmd_grade_tiles(args):
    from commands.grade_tiles import run
    run(output_dir=args.output_dir, workers=args.workers)


def cmd_grade_minimaps(args):
    from commands.grade_minimaps import run
    run(output_dir=args.output_dir, workers=args.workers)


def cmd_all(args):
    from commands.download_tiles import run as dl_tiles
    from commands.download_minimaps import run as dl_minimaps
    from commands.grade_tiles import run as grade_tiles
    from commands.grade_minimaps import run as grade_minimaps

    print("=== Step 1/4: Downloading tiles ===\n")
    dl_tiles(output_dir=args.output_dir, workers=args.workers)

    print("\n=== Step 2/4: Downloading minimaps ===\n")
    dl_minimaps(output_dir=args.output_dir, workers=args.workers)

    print("\n=== Step 3/4: Grading tiles ===\n")
    grade_tiles(output_dir=args.output_dir, workers=args.grade_workers)

    print("\n=== Step 4/4: Grading minimaps ===\n")
    grade_minimaps(output_dir=args.output_dir, workers=args.grade_workers)

    print("\n=== All done! ===")


def main():
    default_output = get_default_output_dir()

    parser = argparse.ArgumentParser(
        prog="map-extractor",
        description="Download and color-grade Albion Online map assets for the OryxBot Map Viewer.",
    )
    parser.add_argument(
        "--output-dir", "-o",
        default=default_output,
        help=f"Output directory for map data (default: {default_output})",
    )

    subparsers = parser.add_subparsers(dest="command", required=True)

    # download-tiles
    p = subparsers.add_parser("download-tiles", help="Download world map tiles from CDN")
    p.add_argument("--workers", type=int, default=20, help="Concurrent download threads (default: 20)")
    p.set_defaults(func=cmd_download_tiles)

    # download-minimaps
    p = subparsers.add_parser("download-minimaps", help="Download minimap overlay images from CDN")
    p.add_argument("--workers", type=int, default=20, help="Concurrent download threads (default: 20)")
    p.set_defaults(func=cmd_download_minimaps)

    # grade-tiles
    p = subparsers.add_parser("grade-tiles", help="Apply OryxBot color grading to world map tiles")
    p.add_argument("--workers", type=int, default=8, help="Concurrent processing threads (default: 8)")
    p.set_defaults(func=cmd_grade_tiles)

    # grade-minimaps
    p = subparsers.add_parser("grade-minimaps", help="Apply OryxBot color grading to minimap overlays")
    p.add_argument("--workers", type=int, default=8, help="Concurrent processing threads (default: 8)")
    p.set_defaults(func=cmd_grade_minimaps)

    # all
    p = subparsers.add_parser("all", help="Run full pipeline: download + grade everything")
    p.add_argument("--workers", type=int, default=20, help="Concurrent download threads (default: 20)")
    p.add_argument("--grade-workers", type=int, default=8, help="Concurrent grading threads (default: 8)")
    p.set_defaults(func=cmd_all)

    args = parser.parse_args()
    args.output_dir = os.path.abspath(args.output_dir)
    args.func(args)


if __name__ == "__main__":
    main()
