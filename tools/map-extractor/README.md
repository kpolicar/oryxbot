# OryxBot Map Extractor

CLI tool for downloading and color-grading Albion Online map assets for the [Map Viewer](../map-viewer/).

## Setup

```bash
pip install -r requirements.txt
```

## Usage

All commands output to `../map-viewer/data/` by default. Override with `--output-dir`.

### Full pipeline (download + grade everything)

```bash
python cli.py all
```

### Individual commands

```bash
# Download world map tiles from CDN
python cli.py download-tiles

# Download minimap overlay images from CDN
# (requires albionLocations.json in the output directory)
python cli.py download-minimaps

# Apply OryxBot color grading to tiles (modifies in-place)
python cli.py grade-tiles

# Apply OryxBot color grading to minimaps (modifies in-place)
python cli.py grade-minimaps
```

### Options

```
--output-dir, -o    Output directory (default: ../map-viewer/data/)
--workers           Concurrent threads for downloads (default: 20)
--grade-workers     Concurrent threads for grading (default: 8, only for `all`)
```

## Pipeline order

The commands must run in this order (handled automatically by `all`):

1. `download-tiles` — fetch raw tiles from CDN
2. `download-minimaps` — fetch minimap overlays (needs `albionLocations.json`)
3. `grade-tiles` — apply color grading to tiles
4. `grade-minimaps` — apply color grading to minimaps
