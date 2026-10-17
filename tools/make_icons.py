"""Draws the item icons for all Kong Strong variants.

Usage: python3 make_icons.py OUT_DIR [SIZE]
  Writes sugarfree.png, classic.png, skyr.png, xxl.png and crash.png.
  SIZE defaults to 128 (item icons); Thunderstore wants 256.

Fonts: set KONG_FONT_HEAVY (heavy sans) and KONG_FONT_SLAB (bold slab/serif) to
override the default Noto Sans Black and DejaVu Serif Bold paths.

Everything is drawn from simple shapes; the gorilla (gorilla.py) is an own drawing.
"""
import os
import sys
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont, ImageOps

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gorilla import GH, GW, draw_full_face, draw_half_face  # noqa: E402

OUT_DIR = sys.argv[1]
SIZE = int(sys.argv[2]) if len(sys.argv) > 2 else 128
FONT_HEAVY = os.environ.get("KONG_FONT_HEAVY", "/usr/share/fonts/noto/NotoSans-Black.ttf")
FONT_SLAB = os.environ.get("KONG_FONT_SLAB", "/usr/share/fonts/TTF/DejaVuSerif-Bold.ttf")

W = H = 512                # drawn at 512 and downscaled for anti-aliasing

BLACK = (16, 18, 24)
SILVER_DARK = (96, 102, 116)

PALETTES = {
    # panel colour, deep shade for text shadows
    "blue": ((30, 132, 214), (8, 52, 120)),
    "red": ((226, 52, 40), (110, 14, 10)),
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

    spec = new_layer()
    sx = left + int((right - left) * 0.3)
    ImageDraw.Draw(spec).rectangle([sx, 0, sx + (right - left) // 14, H], fill=(255, 255, 255, 70))
    spec = spec.filter(ImageFilter.GaussianBlur(6))
    spec.putalpha(ImageChops.multiply(spec.getchannel("A"), layer.getchannel("A")))
    out.alpha_composite(spec)
    return out


def metallic_text(text, outline, shadow, size=220, stroke=16):
    """Silver lettering with a coloured outline and an offset shadow, like the can."""
    font = ImageFont.truetype(FONT_HEAVY, size)
    probe = ImageDraw.Draw(Image.new("L", (1, 1)))
    box = probe.textbbox((0, 0), text, font=font, stroke_width=stroke)
    off = stroke // 2 + 2
    w, h = box[2] - box[0] + off + 8, box[3] - box[1] + off + 8
    origin = (-box[0] + 4, -box[1] + 4)

    def mask(stroke_width, shift=(0, 0)):
        m = Image.new("L", (w, h), 0)
        ImageDraw.Draw(m).text((origin[0] + shift[0], origin[1] + shift[1]), text, font=font, fill=255,
                               stroke_width=stroke_width, stroke_fill=255)
        return m

    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    out.paste(Image.new("RGBA", (w, h), shadow + (255,)), (0, 0), mask(stroke, (off, off)))
    out.paste(Image.new("RGBA", (w, h), outline + (255,)), (0, 0), mask(stroke))

    # silver gradient across the letter height
    grad = Image.new("RGBA", (w, h))
    gd = ImageDraw.Draw(grad)
    stops = ((0.0, (252, 253, 255)), (0.42, (206, 214, 226)), (0.52, (244, 247, 252)), (1.0, (168, 178, 194)))
    top, bottom = origin[1] + (box[1] - box[1]), h - off - 4
    for y in range(h):
        t = min(1.0, max(0.0, (y - top) / max(1, bottom - top)))
        for (t0, c0), (t1, c1) in zip(stops, stops[1:]):
            if t0 <= t <= t1:
                gd.line([(0, y), (w, y)], fill=lerp(c0, c1, (t - t0) / (t1 - t0)) + (255,))
                break
    out.paste(grad, (0, 0), mask(0))
    return out


def fit(img, max_w, max_h):
    scale = min(max_w / img.width, max_h / img.height)
    return img.resize((max(1, int(img.width * scale)), max(1, int(img.height * scale))), Image.LANCZOS)


def slab_text(d, xy, text, size, fill, stroke=0, stroke_fill=None, anchor="la", max_w=None):
    """Slab lettering; shrinks the font until the text fits into max_w."""
    font = ImageFont.truetype(FONT_SLAB, size)
    while max_w and size > 6:
        box = d.textbbox((0, 0), text, font=font, stroke_width=stroke)
        if box[2] - box[0] <= max_w:
            break
        size -= 1
        font = ImageFont.truetype(FONT_SLAB, size)
    d.text(xy, text, font=font, fill=fill, stroke_width=stroke, stroke_fill=stroke_fill, anchor=anchor)


def draw_can(palette, label):
    main, deep = PALETTES[palette]
    left, right, top, bottom, lid = 166, 346, 22, 490, 30
    split = left + int((right - left) * 0.48)

    art = new_layer()
    ad = ImageDraw.Draw(art)
    ad.rectangle([left, top, split, bottom], fill=main)

    # big gorilla, its face centre on the split like on the real can
    gw = int((split - left) * 1.5)
    gorilla = draw_half_face(palette).resize((gw, int(gw * GH / GW)), Image.LANCZOS)
    art.alpha_composite(gorilla, (split - gw, int(top + (bottom - top) * 0.6 - gorilla.height / 2)))

    # variety label top left
    lines = label.split("\n")
    for i, line in enumerate(lines):
        slab_text(ad, (left + 10, top + 34 + i * 22), line, 22, lerp(main, (255, 255, 255), 0.55), 3, BLACK,
                  max_w=split - left - 14)
    slab_text(ad, (left + 10, top + 38 + len(lines) * 22), "ENERGY DRINK", 13, BLACK, max_w=split - left - 14)

    # black panel with KONG / STRONG / WILD POWER
    ad.rectangle([split, top, right, bottom], fill=BLACK)
    panel = right - split
    kong = metallic_text("KONG", main, deep).rotate(90, expand=True)
    kong = kong.resize((int(panel * 0.7), int((bottom - top) * 0.76)), Image.LANCZOS)
    art.alpha_composite(kong, (split + 3, top + 34))
    strong = metallic_text("STRONG", main, deep, size=160, stroke=14).rotate(90, expand=True)
    strong = strong.resize((int(panel * 0.26), int((bottom - top) * 0.5)), Image.LANCZOS)
    art.alpha_composite(strong, (split + 3 + kong.width, top + 34))
    slab_text(ad, ((split + right) // 2 + 1, bottom - 30), "WILD POWER", 18, main, anchor="mm",
              max_w=panel - 12)

    can_mask = Image.new("L", (W, H), 0)
    md = ImageDraw.Draw(can_mask)
    md.rectangle([left, top + lid // 2, right, bottom - lid // 2], fill=255)
    md.ellipse([left, bottom - lid, right, bottom], fill=255)
    body = new_layer()
    body.paste(art, (0, 0), can_mask)
    body = shade_cylinder(body, left, right)

    d = ImageDraw.Draw(body)
    d.ellipse([left, top, right, top + lid], fill=(205, 210, 220, 255), outline=SILVER_DARK, width=5)
    d.ellipse([left + 16, top + 5, right - 16, top + lid - 5], fill=(160, 166, 178, 255))
    d.rounded_rectangle([left + 66, top + 8, left + 114, top + 21], radius=6,
                        fill=(220, 224, 232, 255), outline=SILVER_DARK, width=3)
    d.arc([left, bottom - lid, right, bottom], 0, 180, fill=(170, 176, 188, 255), width=8)
    return body


def draw_skyr():
    """Milky protein drink bottle with a blue sleeve and the gorilla face."""
    main, dark = (70, 140, 214), (14, 52, 104)
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
    gorilla = draw_full_face("blue")
    gw = right - left + 20
    gorilla = gorilla.resize((gw, int(gw * gorilla.height / gorilla.width)), Image.LANCZOS)
    # centred on the bottle, in the blue part below the band (the mane may touch the edges)
    art.alpha_composite(gorilla, ((left + right - gorilla.width) // 2, 336 - gorilla.height // 2))
    ad.rectangle([0, 0, W, 205], fill=(246, 246, 240))                      # white shoulder
    font = ImageFont.truetype(FONT_HEAVY, 74)
    tb = ad.textbbox((0, 0), "SKYR", font=font)
    ad.text((256 - (tb[2] + tb[0]) // 2, 150 - (tb[3] + tb[1]) // 2), "SKYR", font=font, fill=(16, 22, 34))
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
    main, deep = PALETTES["blue"]
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

    # label: blue part with the gorilla, black part with the lettering
    lt, lb = top + 120, bottom - 30
    lsplit = left + 120
    label = new_layer()
    ld = ImageDraw.Draw(label)
    ld.rectangle([left + 12, lt, lsplit, lb], fill=main)
    gw = int((lsplit - left - 12) * 1.35)
    gorilla = draw_half_face("blue").resize((gw, int(gw * GH / GW)), Image.LANCZOS)
    label.alpha_composite(gorilla, (lsplit - gw, (lt + lb) // 2 - gorilla.height // 2 + 10))
    clip = Image.new("L", (W, H), 0)
    ImageDraw.Draw(clip).rectangle([left + 12, lt, lsplit, lb], fill=255)
    jug.paste(label, (0, 0), ImageChops.multiply(clip, label.getchannel("A")))

    jd.rectangle([lsplit, lt, right - 12, lb], fill=BLACK)
    kong = fit(metallic_text("KONG", main, deep), right - 12 - lsplit - 24, (lb - lt) * 0.55)
    jug.alpha_composite(kong, (lsplit + (right - 12 - lsplit - kong.width) // 2, lt + 14))
    xb = jd.textbbox((0, 0), "XXL", font=ImageFont.truetype(FONT_HEAVY, 46))
    jd.text((right - 24 - xb[2], lb - 8 - xb[3]), "XXL", font=ImageFont.truetype(FONT_HEAVY, 46),
            fill=(255, 214, 0))
    jug = shade_cylinder(jug, left, right, 0.4)
    layer.alpha_composite(jug)
    return layer


def draw_crash():
    """Empty grey can lying on its side, used for the sugar crash debuff."""
    red = draw_can("red", "URBAN\nCLASSIC")
    grey = ImageOps.grayscale(red.convert("RGB"))
    can = Image.merge("RGBA", (grey, grey, grey, red.getchannel("A")))
    can = can.rotate(-75, resample=Image.BICUBIC, center=(256, 256))
    d = ImageDraw.Draw(can)
    d.text((300, 40), "z", font=ImageFont.truetype(FONT_HEAVY, 70), fill=(235, 235, 245, 255),
           stroke_width=6, stroke_fill=(30, 30, 40))
    d.text((360, 0), "z", font=ImageFont.truetype(FONT_HEAVY, 50), fill=(235, 235, 245, 255),
           stroke_width=5, stroke_fill=(30, 30, 40))
    return can


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


os.makedirs(OUT_DIR, exist_ok=True)
for name, layer in (("sugarfree", draw_can("blue", "SUGAR\nFREE")),
                    ("classic", draw_can("red", "URBAN\nCLASSIC")),
                    ("skyr", draw_skyr()), ("xxl", draw_xxl()), ("crash", draw_crash())):
    path = os.path.join(OUT_DIR, f"{name}.png")
    finish(layer).save(path)
    print("wrote", path)
