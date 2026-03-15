#!/usr/bin/env python3
"""OryxBot Road Extractor — detect roads on minimap images and output coordinates."""

import argparse
import os


def get_default_data_dir():
    return os.path.join(os.path.dirname(__file__), '..', 'map-viewer', 'data')


def cmd_extract(args):
    from road_extractor.batch import run
    run(
        data_dir=args.data_dir,
        output_path=args.output,
        workers=args.workers,
        debug=args.debug,
        single_id=args.single,
    )


def main():
    default_data = get_default_data_dir()
    default_output = os.path.join(default_data, 'roadPaths.json')

    parser = argparse.ArgumentParser(
        prog='road-extractor',
        description='Detect roads on Albion Online minimap images and extract coordinates.',
    )
    subparsers = parser.add_subparsers(dest='command', required=True)

    p = subparsers.add_parser('extract', help='Extract road coordinates from minimap images')
    p.add_argument('--data-dir', default=default_data,
                   help=f'Map viewer data directory (default: {default_data})')
    p.add_argument('--output', '-o', default=default_output,
                   help=f'Output JSON file (default: {default_output})')
    p.add_argument('--workers', type=int, default=8,
                   help='Number of parallel workers (default: 8)')
    p.add_argument('--debug', action='store_true',
                   help='Save debug visualization images')
    p.add_argument('--single', type=str, default=None,
                   help='Process only this location ID (for debugging)')
    p.set_defaults(func=cmd_extract)

    args = parser.parse_args()
    args.data_dir = os.path.abspath(args.data_dir)
    args.output = os.path.abspath(args.output)
    args.func(args)


if __name__ == '__main__':
    main()
