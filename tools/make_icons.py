"""Draws the item icons for all Kong Strong variants.

Usage: python3 make_icons.py OUT_DIR [SIZE] [--classic]
  Writes one PNG per item and status effect.
  SIZE defaults to 128 (item icons); Thunderstore wants 256.
  The default style is painted and colour graded to sit next to Valheim's own icons;
  --classic gives the clean, bright first version (kept in Assets/backup-v1).

Fonts: set KONG_FONT_HEAVY (heavy sans) and KONG_FONT_SLAB (bold slab/serif) to
override the default Noto Sans Black and DejaVu Serif Bold paths.

Everything is drawn from simple shapes; the gorilla (gorilla.py) is an own drawing.
"""
import os
import random
import sys
from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageOps

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gorilla import GH, GW, draw_full_face, draw_half_face  # noqa: E402

CLASSIC = False            # set by main(); the drawing helpers are also used by make_textures.py
SIZE = 128
FONT_HEAVY = os.environ.get("KONG_FONT_HEAVY", "/usr/share/fonts/noto/NotoSans-Black.ttf")
FONT_SLAB = os.environ.get("KONG_FONT_SLAB", "/usr/share/fonts/TTF/DejaVuSerif-Bold.ttf")

W = H = 512                # drawn at 512 and downscaled for anti-aliasing

BLACK = (16, 18, 24)
SILVER_DARK = (96, 102, 116)

PALETTES = {
    # panel colour, deep shade for text shadows
    "blue": ((30, 132, 214), (8, 52, 120)),
    "red": ((226, 52, 40), (110, 14, 10)),
    "ice": ((150, 214, 245), (24, 84, 130)),
    "green": ((80, 200, 64), (16, 80, 18)),
    "purple": ((150, 84, 222), (56, 18, 104)),
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
    left, right, top, bottom, lid = 166, 346, 22, 490, (30 if CLASSIC else 50)
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


def grey(layer):
    g = ImageOps.grayscale(layer.convert("RGB"))
    return Image.merge("RGBA", (g, g, g, layer.getchannel("A")))


def draw_empty():
    """Crushed, empty grey can: the Leergut."""
    can = grey(draw_can("blue", "SUGAR\nFREE"))
    can = can.resize((W, int(H * 0.62)), Image.LANCZOS)
    out = new_layer()
    out.alpha_composite(can, (0, (H - can.height) // 2 + 40))
    d = ImageDraw.Draw(out)
    for y in (215, 268, 318):                                               # dents
        d.line([(170, y), (240, y + 10), (300, y - 6), (344, y + 6)], fill=(70, 70, 76, 255), width=7)
    return out.rotate(28, resample=Image.BICUBIC, center=(256, 256))


def draw_cup():
    """Plain Skyr cup: white and blue with a cream swirl, no Kong branding."""
    main, dark = (52, 116, 196), (14, 52, 104)
    top, bottom = 150, 440
    layer = new_layer()
    mask = Image.new("L", (W, H), 0)
    ImageDraw.Draw(mask).polygon([(120, top), (392, top), (352, bottom), (160, bottom)], fill=255)
    art = new_layer()
    ad = ImageDraw.Draw(art)
    ad.rectangle([0, 0, W, H], fill=(246, 246, 240))
    ad.rectangle([0, 270, W, H], fill=main)
    # cream swirl on the blue part
    cream, shadow = (252, 252, 246), (196, 210, 228)
    for (x0, y0, x1, y1, c) in ((180, 352, 332, 404, shadow), (176, 344, 336, 396, cream),
                                (196, 318, 316, 366, shadow), (192, 312, 320, 360, cream),
                                (218, 290, 294, 330, cream)):
        ad.ellipse([x0, y0, x1, y1], fill=c)
    ad.polygon([(244, 268), (268, 300), (232, 304)], fill=cream)
    ad.arc([204, 322, 300, 352], 200, 340, fill=shadow, width=6)
    font = ImageFont.truetype(FONT_HEAVY, 64)
    tb = ad.textbbox((0, 0), "SKYR", font=font)
    ad.text((256 - (tb[2] + tb[0]) // 2, 212 - (tb[3] + tb[1]) // 2), "SKYR", font=font, fill=dark)
    ad.rectangle([0, 262, W, 274], fill=dark)
    layer.paste(art, (0, 0), mask)
    layer = shade_cylinder(layer, 120, 392, 0.4)
    d = ImageDraw.Draw(layer)
    d.ellipse([110, top - 26, 402, top + 26], fill=(214, 220, 230, 255), outline=(120, 126, 140, 255), width=5)
    d.ellipse([150, top - 14, 362, top + 14], fill=(236, 240, 246, 255))
    return layer


def draw_bottle(label_main, label_text, glass=(150, 84, 22)):
    """Mead style bottle with a Kong label."""
    main, deep = PALETTES[label_main]
    layer = new_layer()
    mask = Image.new("L", (W, H), 0)
    md = ImageDraw.Draw(mask)
    md.rounded_rectangle([214, 40, 298, 170], radius=14, fill=255)          # neck
    md.ellipse([146, 130, 366, 260], fill=255)                              # shoulder
    md.rounded_rectangle([146, 190, 366, 476], radius=40, fill=255)         # body
    art = new_layer()
    ad = ImageDraw.Draw(art)
    ad.rectangle([0, 0, W, H], fill=glass)
    split = 250
    ad.rectangle([146, 250, split, 430], fill=main)
    gorilla = draw_half_face(label_main).resize((140, int(140 * GH / GW)), Image.LANCZOS)
    clip = Image.new("L", (W, H), 0)
    ImageDraw.Draw(clip).rectangle([146, 250, split, 430], fill=255)
    tmp = new_layer()
    tmp.alpha_composite(gorilla, (split - 140, 340 - gorilla.height // 2))
    art.paste(tmp, (0, 0), ImageChops.multiply(clip, tmp.getchannel("A")))
    ad.rectangle([split, 250, 366, 430], fill=BLACK)
    word = fit(metallic_text(label_text, main, deep).rotate(90, expand=True), 366 - split - 12, 170)
    art.alpha_composite(word, (split + (366 - split - word.width) // 2, 255))
    layer.paste(art, (0, 0), mask)
    layer = shade_cylinder(layer, 146, 366, 0.5)
    d = ImageDraw.Draw(layer)
    d.rounded_rectangle([206, 22, 306, 62], radius=8, fill=(120, 78, 40, 255), outline=(60, 36, 16, 255), width=4)
    return layer


def draw_barrel():
    """Small wooden keg with a Kong label: the fermenter base of the mead bomb."""
    main, deep = PALETTES["blue"]
    layer = new_layer()
    d = ImageDraw.Draw(layer)
    d.rounded_rectangle([120, 90, 392, 460], radius=90, fill=(150, 96, 50, 255))
    for x in range(150, 392, 44):
        d.line([x, 100, x, 450], fill=(110, 66, 30, 255), width=6)
    for y in (140, 410):
        d.rectangle([116, y - 14, 396, y + 14], fill=(90, 90, 100, 255))
    d.rectangle([150, 210, 362, 340], fill=BLACK)
    d.rectangle([150, 210, 230, 340], fill=main)
    kong = fit(metallic_text("KONG", main, deep), 362 - 236, 110)
    layer.alpha_composite(kong, (236 + (126 - kong.width) // 2, 275 - kong.height // 2))
    return shade_cylinder(layer, 120, 392, 0.45)


def draw_heart():
    """Red heart with a pulse line: heart racing."""
    layer = new_layer()
    d = ImageDraw.Draw(layer)
    d.ellipse([86, 110, 266, 290], fill=(214, 30, 40, 255))
    d.ellipse([246, 110, 426, 290], fill=(214, 30, 40, 255))
    d.polygon([(96, 230), (416, 230), (256, 440)], fill=(214, 30, 40, 255))
    d.ellipse([130, 140, 200, 200], fill=(255, 120, 120, 255))
    d.line([(60, 270), (170, 270), (205, 190), (250, 350), (290, 220), (320, 270), (452, 270)],
           fill=(255, 255, 255, 255), width=22, joint="curve")
    return layer


def valheimify(layer, seed):
    """Paints over the clean drawing: muted, warm colours, light from the top left,
    darker edges and brush texture, so the icons sit next to Valheim's hand painted ones."""
    rng = random.Random(seed)
    alpha = layer.getchannel("A")
    rgb = layer.convert("RGB")

    # muted, slightly warm and less contrasty
    rgb = ImageEnhance.Color(rgb).enhance(0.88)
    rgb = ImageEnhance.Contrast(rgb).enhance(0.92)
    rgb = Image.blend(rgb, ImageChops.multiply(rgb, Image.new("RGB", (W, H), (255, 230, 186))), 0.35)

    # painterly smear: mode filter merges small details into flat strokes
    rgb = Image.blend(rgb, rgb.filter(ImageFilter.ModeFilter(9)), 0.55)

    # light from the top left, shade to the bottom right
    light = Image.linear_gradient("L").rotate(45, expand=False).resize((W, H))
    light = ImageOps.invert(light).point(lambda v: int(150 + v * 0.55))
    rgb = ImageChops.multiply(rgb, Image.merge("RGB", (light, light, light)))
    rgb = ImageEnhance.Brightness(rgb).enhance(1.32)

    # darker rim along the silhouette (fake ambient occlusion)
    inner = alpha.filter(ImageFilter.MinFilter(17)).filter(ImageFilter.GaussianBlur(14))
    rim = ImageOps.invert(inner).point(lambda v: int(255 - v * 0.45))
    rgb = ImageChops.multiply(rgb, Image.merge("RGB", (rim, rim, rim)))

    # soft, blotchy brush texture (large scale, low amplitude) and a few darker strokes
    noise = Image.effect_noise((W // 16, H // 16), 50).resize((W, H), Image.BICUBIC).filter(ImageFilter.GaussianBlur(6))
    noise = noise.point(lambda v: int(236 + (v - 128) * 0.16))
    rgb = ImageChops.multiply(rgb, Image.merge("RGB", (noise, noise, noise)))
    strokes = Image.new("L", (W, H), 255)
    d = ImageDraw.Draw(strokes)
    for _ in range(14):
        x, y = rng.randint(80, W - 80), rng.randint(80, H - 80)
        length, slope = rng.randint(20, 50), rng.uniform(-0.4, 0.4)
        d.line([(x, y), (x + length, y + int(length * slope))], fill=rng.randint(200, 228), width=rng.randint(5, 9))
    strokes = strokes.filter(ImageFilter.GaussianBlur(3))
    rgb = ImageChops.multiply(rgb, Image.merge("RGB", (strokes, strokes, strokes)))

    out = rgb.convert("RGBA")
    out.putalpha(alpha)
    return out


# Valheim shows bottles lying at an angle; degrees clockwise per icon in the painted style.
TILT = {"sugarfree": 28, "classic": 28, "frost": 28, "toxic": 28, "eitr": 28, "skyr": 24, "xxl": 14,
        "metbombe": 34, "skyrcup": 10, "metbombe_base": 6}

# Magic drinks glow a little, like Valheim's resistance and eitr meads.
GLOW = {"frost": (150, 220, 255), "toxic": (140, 255, 120), "eitr": (200, 140, 255)}


def finish(layer, seed=0):
    """Classic: outline and drop shadow. Painted: tilted, no outline, optional glow, like vanilla icons."""
    if not CLASSIC:
        layer = layer.rotate(-TILT.get(seed, 0), resample=Image.BICUBIC, center=(W // 2, H // 2))
        layer = valheimify(layer, seed)
        out = new_layer()
        if seed in GLOW:
            glow = layer.getchannel("A").filter(ImageFilter.MaxFilter(21)).filter(ImageFilter.GaussianBlur(28))
            out.paste(Image.new("RGBA", (W, H), GLOW[seed] + (150,)), (0, 0), glow)
        out.alpha_composite(layer)
        return out.resize((SIZE, SIZE), Image.LANCZOS)

    alpha = layer.getchannel("A")
    outline = alpha.filter(ImageFilter.MaxFilter(13))
    shadow = alpha.filter(ImageFilter.MaxFilter(9)).filter(ImageFilter.GaussianBlur(10))
    out = new_layer()
    out.paste(Image.new("RGBA", (W, H), (0, 0, 0, 150)), (10, 12), shadow)
    out.paste(Image.new("RGBA", (W, H), (12, 10, 8, 255)), (0, 0), outline)
    out.alpha_composite(layer)
    return out.resize((SIZE, SIZE), Image.LANCZOS)


def main():
    global CLASSIC, SIZE
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    CLASSIC = "--classic" in sys.argv
    out_dir = args[0]
    SIZE = int(args[1]) if len(args) > 1 else 128

    icons = (
        ("sugarfree", lambda: draw_can("blue", "SUGAR\nFREE")),
        ("classic", lambda: draw_can("red", "URBAN\nCLASSIC")),
        ("skyr", draw_skyr), ("xxl", draw_xxl), ("crash", draw_crash),
        ("frost", lambda: draw_can("ice", "FROST\nEDITION")),
        ("toxic", lambda: draw_can("green", "TOXIC\nEDITION")),
        ("eitr", lambda: draw_can("purple", "EITR\nEDITION")),
        ("empty", draw_empty), ("skyrcup", draw_cup),
        ("metbombe", lambda: draw_bottle("blue", "MET")), ("metbombe_base", draw_barrel),
        ("heartracing", draw_heart),
    )
    os.makedirs(out_dir, exist_ok=True)
    for name, draw in icons:
        path = os.path.join(out_dir, f"{name}.png")
        finish(draw(), seed=name).save(path)
        print("wrote", path)


if __name__ == "__main__":
    main()
