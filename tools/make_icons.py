"""Draws the item icons for all Kong Strong variants.

Usage: python3 make_icons.py OUT_DIR [SIZE] [FONT.ttf]
  Writes sugarfree.png, classic.png, skyr.png, xxl.png and crash.png.
  SIZE defaults to 128 (item icons); Thunderstore wants 256.
  FONT should be a heavy sans, defaults to Noto Sans Black.

Everything is drawn from simple shapes; the gorilla is an own stylised drawing.
"""
import math
import os
import sys
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont, ImageOps

OUT_DIR = sys.argv[1]
SIZE = int(sys.argv[2]) if len(sys.argv) > 2 else 128
FONT = sys.argv[3] if len(sys.argv) > 3 else "/usr/share/fonts/noto/NotoSans-Black.ttf"

W = H = 512                # drawn at 512 and downscaled for anti-aliasing

BLACK = (18, 20, 26)
SILVER_DARK, SILVER_LIGHT = (96, 102, 116), (238, 242, 248)

PALETTES = {
    # main colour, darker shade for the gorilla, light accent for the eye/outline
    "blue": ((30, 132, 214), (8, 58, 120), (150, 210, 255)),
    "red": ((226, 52, 40), (110, 14, 10), (255, 170, 150)),
}


def new_layer():
    return Image.new("RGBA", (W, H), (0, 0, 0, 0))


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(len(a)))


def shade_cylinder(layer, left, right, strength=0.55):
    """Multiplies a horizontal light profile over the layer so it reads as round."""
    shade = Image.new("RGB", (W, H), (255, 255, 255))
    px = shade.load()
    for x in range(left, right):
        u = (x - left) / max(1, right - left - 1)
        light = 1.0 - strength * (abs(u - 0.38) / 0.62) ** 1.8
        v = int(255 * max(0.0, light))
        for y in range(H):
            px[x, y] = (v, v, v)
    rgb = ImageChops.multiply(layer.convert("RGB"), shade)
    out = Image.merge("RGBA", (*rgb.split(), layer.getchannel("A")))

    # thin specular stripe
    spec = new_layer()
    sx = left + int((right - left) * 0.3)
    ImageDraw.Draw(spec).rectangle([sx, 0, sx + (right - left) // 14, H], fill=(255, 255, 255, 70))
    spec = spec.filter(ImageFilter.GaussianBlur(6))
    spec.putalpha(ImageChops.multiply(spec.getchannel("A"), layer.getchannel("A")))
    out.alpha_composite(spec)
    return out


def vertical_text(text, height, fill, outline, stroke):
    """Renders text rotated so it reads bottom-to-top, scaled to the given height."""
    font = ImageFont.truetype(FONT, 200)
    probe = ImageDraw.Draw(Image.new("L", (1, 1)))
    box = probe.textbbox((0, 0), text, font=font, stroke_width=stroke)
    img = Image.new("RGBA", (box[2] - box[0] + 4, box[3] - box[1] + 4), (0, 0, 0, 0))
    ImageDraw.Draw(img).text((-box[0] + 2, -box[1] + 2), text, font=font, fill=fill,
                             stroke_width=stroke, stroke_fill=outline)
    img = img.rotate(90, expand=True)
    scale = height / img.height
    return img.resize((max(1, int(img.width * scale)), height), Image.LANCZOS)


def draw_gorilla(d, cx, cy, s, fur, accent):
    """Half a gorilla face looking right, cut off by whatever is drawn over its right side."""
    def p(x, y):
        return (cx + x * s, cy + y * s)

    # shaggy head silhouette
    head = [p(0.9, -1.25), p(0.1, -1.3), p(-0.45, -1.05), p(-0.8, -1.15), p(-0.75, -0.75),
            p(-1.05, -0.6), p(-0.85, -0.3), p(-1.1, -0.05), p(-0.8, 0.2), p(-1.0, 0.5),
            p(-0.6, 0.65), p(-0.55, 1.0), p(-0.1, 0.95), p(0.3, 1.25), p(0.9, 1.25)]
    d.polygon(head, fill=fur)
    # face plate
    d.polygon([p(0.9, -0.55), p(0.15, -0.6), p(-0.3, -0.3), p(-0.35, 0.25), p(-0.1, 0.75),
               p(0.4, 0.95), p(0.9, 0.95)], fill=lerp(fur, accent, 0.25))
    # heavy brow
    d.polygon([p(-0.45, -0.45), p(0.15, -0.72), p(0.9, -0.62), p(0.9, -0.38), p(0.2, -0.42),
               p(-0.3, -0.22)], fill=lerp(fur, (0, 0, 0), 0.45))
    # angry eye
    d.polygon([p(-0.05, -0.3), p(0.55, -0.36), p(0.45, -0.12), p(0.05, -0.12)], fill=accent)
    d.ellipse([p(0.22, -0.3), p(0.4, -0.14)], fill=(10, 10, 14))
    # nose and mouth
    d.ellipse([p(0.35, 0.15), p(0.62, 0.32)], fill=lerp(fur, (0, 0, 0), 0.5))
    d.line([p(0.0, 0.62), p(0.45, 0.72), p(0.9, 0.66)], fill=lerp(fur, (0, 0, 0), 0.5),
           width=max(2, int(s * 0.08)))


def finish(layer):
    """Dark outline and soft drop shadow, like vanilla Valheim icons."""
    alpha = layer.getchannel("A")
    outline = alpha.filter(ImageFilter.MaxFilter(13))
    shadow = alpha.filter(ImageFilter.MaxFilter(9)).filter(ImageFilter.GaussianBlur(10))
    out = new_layer()
    out.paste(Image.new("RGBA", (W, H), (0, 0, 0, 150)), (10, 12), shadow)
    out.paste(Image.new("RGBA", (W, H), (12, 10, 8, 255)), (0, 0), outline)
    out.alpha_composite(layer)
    return out.resize((SIZE, SIZE), Image.LANCZOS)


def draw_can(palette_name):
    main, dark, accent = PALETTES[palette_name]
    left, right, top, bottom, lid = 168, 344, 28, 482, 30
    split = left + int((right - left) * 0.46)

    body = new_layer()
    d = ImageDraw.Draw(body)
    mask = Image.new("L", (W, H), 0)
    md = ImageDraw.Draw(mask)
    md.rectangle([left, top + lid // 2, right, bottom - lid // 2], fill=255)
    md.ellipse([left, bottom - lid, right, bottom], fill=255)

    art = new_layer()
    ad = ImageDraw.Draw(art)
    ad.rectangle([left, top, split, bottom], fill=main)
    draw_gorilla(ad, split - 50, 300, 56, dark, accent)
    ad.rectangle([split, top, right, bottom], fill=BLACK)
    word = vertical_text("KONG", 340, (240, 244, 250), main, 10)
    art.alpha_composite(word, (split + (right - split - word.width) // 2 + 4, 100))
    body.paste(art, (0, 0), mask)

    body = shade_cylinder(body, left, right)
    d = ImageDraw.Draw(body)
    # silver lid and bottom rim
    d.ellipse([left, top, right, top + lid], fill=(205, 210, 220, 255), outline=SILVER_DARK, width=5)
    d.ellipse([left + 16, top + 5, right - 16, top + lid - 5], fill=(160, 166, 178, 255))
    d.rounded_rectangle([left + 74, top + 8, left + 122, top + 21], radius=6,
                        fill=(220, 224, 232, 255), outline=SILVER_DARK, width=3)
    d.arc([left, bottom - lid, right, bottom], 0, 180, fill=(170, 176, 188, 255), width=8)
    return body


def draw_skyr():
    """Milky protein drink bottle with a blue sleeve."""
    main, dark, accent = (70, 140, 214), (14, 52, 104), (200, 228, 255)
    left, right = 150, 362
    layer = new_layer()
    mask = Image.new("L", (W, H), 0)
    md = ImageDraw.Draw(mask)
    md.rounded_rectangle([200, 70, 312, 130], radius=10, fill=255)          # neck
    md.rounded_rectangle([left, 110, right, 472], radius=70, fill=255)      # body
    md.rectangle([left + 16, 300, right - 16, 340], fill=0)                 # waist
    md.rounded_rectangle([left + 10, 280, right - 10, 360], radius=40, fill=255)

    art = new_layer()
    ad = ImageDraw.Draw(art)
    ad.rectangle([0, 0, W, H], fill=main)
    ad.rectangle([0, 0, W, 205], fill=(246, 246, 240))                      # white shoulder
    draw_gorilla(ad, 300, 340, 92, dark, accent)
    font = ImageFont.truetype(FONT, 74)
    tb = ad.textbbox((0, 0), "SKYR", font=font)
    ad.text((256 - (tb[2] + tb[0]) // 2, 150 - (tb[3] + tb[1]) // 2), "SKYR", font=font,
            fill=(16, 22, 34))
    ad.rectangle([0, 205, W, 222], fill=dark)
    layer.paste(art, (0, 0), mask)
    layer = shade_cylinder(layer, left, right, 0.45)

    d = ImageDraw.Draw(layer)
    d.rounded_rectangle([190, 24, 322, 86], radius=12, fill=(250, 250, 250, 255),
                        outline=(150, 150, 150, 255), width=4)               # cap
    for x in range(204, 312, 14):
        d.line([x, 32, x, 78], fill=(205, 205, 205, 255), width=4)
    return layer


def draw_xxl():
    """18.9 litre canister: yellow drink, handle, blue/black label."""
    main, dark, accent = PALETTES["blue"]
    left, right, top, bottom = 96, 416, 120, 480
    layer = new_layer()
    d = ImageDraw.Draw(layer)

    # handle and cap on top
    d.rounded_rectangle([250, 40, 380, 150], radius=40, outline=(214, 170, 60, 255), width=26)
    d.rounded_rectangle([140, 64, 236, 130], radius=10, fill=(245, 245, 245, 255),
                        outline=(150, 150, 150, 255), width=4)

    jug = new_layer()
    jd = ImageDraw.Draw(jug)
    jd.rounded_rectangle([left, top, right, bottom], radius=46, fill=(222, 170, 40, 255))
    for i in range(14):                                                     # bubbles
        bx = left + 30 + (i * 53) % (right - left - 60)
        by = top + 30 + (i * 37) % 80
        jd.ellipse([bx, by, bx + 10, by + 10], fill=(255, 226, 140, 255))
    # label
    lt, lb = top + 120, bottom - 30
    lsplit = left + 120
    jd.rectangle([left + 12, lt, lsplit, lb], fill=main)
    jd.rectangle([lsplit, lt, right - 12, lb], fill=BLACK)
    draw_gorilla(jd, lsplit - 50, (lt + lb) // 2 + 10, 56, dark, accent)
    jd.rectangle([lsplit, lt, right - 12, lb], fill=BLACK)
    font = ImageFont.truetype(FONT, 66)
    tb = jd.textbbox((0, 0), "KONG", font=font, stroke_width=6)
    jd.text((lsplit + (right - 12 - lsplit - (tb[2] - tb[0])) // 2 - tb[0], (lt + lb) // 2 - (tb[3] + tb[1]) // 2),
            "KONG", font=font, fill=(240, 244, 250), stroke_width=6, stroke_fill=main)
    xb = jd.textbbox((0, 0), "XXL", font=ImageFont.truetype(FONT, 46))
    jd.text((right - 24 - xb[2], lb - 8 - xb[3]), "XXL", font=ImageFont.truetype(FONT, 46),
            fill=(255, 214, 0))
    jug = shade_cylinder(jug, left, right, 0.4)
    layer.alpha_composite(jug)
    return layer


def draw_crash():
    """Empty grey can lying on its side, used for the sugar crash debuff."""
    can = ImageOps.grayscale(draw_can("red").convert("RGB"))
    can = Image.merge("RGBA", (can, can, can, draw_can("red").getchannel("A")))
    can = can.rotate(-75, resample=Image.BICUBIC, center=(256, 256))
    d = ImageDraw.Draw(can)
    font = ImageFont.truetype(FONT, 70)
    d.text((300, 40), "z", font=font, fill=(235, 235, 245, 255), stroke_width=6, stroke_fill=(30, 30, 40))
    d.text((360, 0), "z", font=ImageFont.truetype(FONT, 50), fill=(235, 235, 245, 255),
           stroke_width=5, stroke_fill=(30, 30, 40))
    return can


os.makedirs(OUT_DIR, exist_ok=True)
for name, layer in (("sugarfree", draw_can("blue")), ("classic", draw_can("red")),
                    ("skyr", draw_skyr()), ("xxl", draw_xxl()), ("crash", draw_crash())):
    path = os.path.join(OUT_DIR, f"{name}.png")
    finish(layer).save(path)
    print("wrote", path)
