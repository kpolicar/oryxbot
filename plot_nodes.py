import json
from PIL import Image, ImageDraw

data = json.load(open('app/web/OryxBot.MapViewer/data/world-graph.json'))
img = Image.open('app/web/OryxBot.MapViewer/data/tiles/worldmap_upscaled.png').convert("RGBA")
draw = ImageDraw.Draw(img)

# Let's guess the map bounds. 
# Center is 0,0. Range is probably -256 to 256 for X, -512 to 512 for Y
for c in data['clusters'].values():
    wx, wy = c.get('worldX'), c.get('worldY')
    if wx is not None and wy is not None:
        # Convert world coord to pixel coord
        # Let's assume the texture maps to exactly 512x1024 units, centered at 0,0
        # If worldX goes up to 256, then pixelX = worldX + 256.
        # But wy usually goes positive North (up), while images go positive Y (down).
        px = wx + 256
        py = -wy + 512

        draw.rectangle([px-2, py-2, px+2, py+2], fill="red")

img.save('test_plot.png')
print("Done. Saved test_plot.png")
