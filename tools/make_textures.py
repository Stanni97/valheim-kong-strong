"""Draws the textures of the 3D models (tex_*.png).

Usage: python3 make_textures.py OUT_DIR

The UV layout has to match the meshes in src/Models.cs: lathe models (cans, bottles, cup,
barrel, canister) wrap u once around and use v from bottom (0) to top (1); the pieces use
atlases whose regions are listed next to each function. Uses the drawing helpers of
make_icons.py, so the models look like their icons.
"""
import math
import os
import sys

from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageOps

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import make_icons as mi  # noqa: E402
from gorilla import GH, GW, draw_full_face, draw_half_face  # noqa: E402

SILVER = (196, 200, 208)
SILVER_DARK = (120, 124, 134)


def y_of(v, height):
    """Pixel row of texture coordinate v (Unity samples v = 0 at the bottom of the PNG)."""
    return int(round((1.0 - v) * height))


def grade(img):
    """The textures get lit by the game, so only the colour grading of the icons, no fake light."""
    rgb = img.convert("RGB")
    rgb = ImageEnhance.Color(rgb).enhance(0.88)
    rgb = Image.blend(rgb, ImageChops.multiply(rgb, Image.new("RGB", rgb.size, (255, 232, 196))), 0.3)
    noise = Image.effect_noise((rgb.width // 8, rgb.height // 8), 40).resize(rgb.size, Image.BICUBIC)
    noise = noise.filter(ImageFilter.GaussianBlur(3)).point(lambda v: int(240 + (v - 128) * 0.12))
    return ImageChops.multiply(rgb, Image.merge("RGB", (noise, noise, noise)))


def silver_band(d, w, y0, y1):
    for y in range(y0, y1):
        t = (y - y0) / max(1, y1 - y0 - 1)
        c = mi.lerp(SILVER, (238, 240, 244), 0.5 - 0.5 * math.cos(t * math.pi * 2))
        d.line([(0, y), (w, y)], fill=c)


def paste_clipped(dst, src, pos, box):
    clip = Image.new("L", dst.size, 0)
    ImageDraw.Draw(clip).rectangle(box, fill=255)
    layer = Image.new("RGBA", dst.size, (0, 0, 0, 0))
    layer.alpha_composite(src, pos)
    dst.paste(layer, (0, 0), ImageChops.multiply(clip, layer.getchannel("A")))


# ---------------------------------------------------------------- can: 512 x 384
# v: 0-0.08 bottom, 0.08-0.88 label, 0.88-1 shoulder and lid. u: colour panel 0-0.46, black 0.46-1.
CAN_W, CAN_H = 512, 384
CAN_LABEL = (0.08, 0.88)


def tex_can(palette, label):
    main, deep = mi.PALETTES[palette]
    img = Image.new("RGBA", (CAN_W, CAN_H), SILVER + (255,))
    d = ImageDraw.Draw(img)
    top, bottom = y_of(CAN_LABEL[1], CAN_H), y_of(CAN_LABEL[0], CAN_H)
    silver_band(d, CAN_W, 0, top)
    silver_band(d, CAN_W, bottom, CAN_H)
    d.line([(0, y_of(0.97, CAN_H)), (CAN_W, y_of(0.97, CAN_H))], fill=SILVER_DARK, width=3)

    split = int(CAN_W * 0.46)
    d.rectangle([0, top, split, bottom], fill=main)
    gh = int((bottom - top) * 0.82)
    gw = int(gh * GW / GH)
    face = draw_half_face(palette).resize((gw, gh), Image.LANCZOS)
    paste_clipped(img, face, (split - gw, bottom - gh), [0, top, split, bottom])
    lines = label.split("\n")
    for i, line in enumerate(lines):
        mi.slab_text(d, (12, top + 10 + i * 24), line, 22, mi.lerp(main, (255, 255, 255), 0.55), 3, mi.BLACK,
                     max_w=split - 24)
    mi.slab_text(d, (12, top + 14 + len(lines) * 24), "ENERGY DRINK", 13, mi.BLACK, max_w=split - 24)

    d.rectangle([split, top, CAN_W, bottom], fill=mi.BLACK)
    height = bottom - top
    kong = mi.metallic_text("KONG", main, deep).rotate(90, expand=True).resize((150, int(height * 0.82)), Image.LANCZOS)
    img.alpha_composite(kong, (split + 24, top + 8))
    strong = mi.metallic_text("STRONG", main, deep, size=160, stroke=14).rotate(90, expand=True)
    strong = strong.resize((56, int(height * 0.52)), Image.LANCZOS)
    img.alpha_composite(strong, (split + 24 + 150 + 6, top + 8))
    mi.slab_text(d, ((split + CAN_W) // 2, bottom - 14), "WILD POWER", 20, main, anchor="mm", max_w=CAN_W - split - 20)
    return img


def tex_empty():
    """Grey version of the can: intact, so the deposit machine can scan the barcode."""
    return ImageOps.grayscale(tex_can("blue", "SUGAR\nFREE").convert("RGB")).convert("RGBA")


# ---------------------------------------------------------------- Skyr bottle: 512 x 512
# v: 0-0.58 blue body with the gorilla, 0.58-0.61 band, 0.61-0.86 white shoulder "SKYR",
# 0.86-0.9 neck, 0.9-1 white cap. The front is at u = 0.25 and again at u = 0.75.
def tex_skyr():
    w = h = 512
    main, dark = (70, 140, 214), (14, 52, 104)
    img = Image.new("RGBA", (w, h), (246, 246, 240, 255))
    d = ImageDraw.Draw(img)
    d.rectangle([0, y_of(0.58, h), w, h], fill=main)
    d.rectangle([0, y_of(0.61, h), w, y_of(0.58, h)], fill=dark)
    face = draw_full_face("blue")
    fh = int(h * 0.42)
    face = face.resize((int(fh * face.width / face.height), fh), Image.LANCZOS)
    font = ImageFont.truetype(mi.FONT_HEAVY, 60)
    for cu in (0.25, 0.75):
        cx = int(cu * w)
        img.alpha_composite(face, (cx - face.width // 2, y_of(0.30, h) - fh // 2))
        tb = d.textbbox((0, 0), "SKYR", font=font)
        d.text((cx - (tb[2] + tb[0]) // 2, y_of(0.735, h) - (tb[3] + tb[1]) // 2), "SKYR", font=font, fill=(16, 22, 34))
    for x in range(0, w, 12):                            # ribbed cap
        d.line([(x, 0), (x, y_of(0.9, h))], fill=(214, 214, 214, 255), width=3)
    return img


# ---------------------------------------------------------------- Skyr cup: 512 x 384
# v: 0-0.05 bottom, 0.05-0.6 blue with cream swirl, 0.6-0.95 white with "SKYR", 0.95-1 foil lid.
def tex_skyrcup():
    w, h = 512, 384
    main, dark = (52, 116, 196), (14, 52, 104)
    img = Image.new("RGBA", (w, h), (246, 246, 240, 255))
    d = ImageDraw.Draw(img)
    d.rectangle([0, y_of(0.6, h), w, y_of(0.05, h)], fill=main)
    d.rectangle([0, y_of(0.63, h), w, y_of(0.6, h)], fill=dark)
    silver_band(d, w, 0, y_of(0.95, h))
    font = ImageFont.truetype(mi.FONT_HEAVY, 56)
    for cu in (0.25, 0.75):
        cx = int(cu * w)
        tb = d.textbbox((0, 0), "SKYR", font=font)
        d.text((cx - (tb[2] + tb[0]) // 2, y_of(0.79, h) - (tb[3] + tb[1]) // 2), "SKYR", font=font, fill=dark)
        cy = y_of(0.32, h)
        for (dx0, dy0, dx1, dy1) in ((-60, 10, 60, 50), (-46, -16, 46, 22), (-28, -38, 28, -6)):
            d.ellipse([cx + dx0, cy + dy0, cx + dx1, cy + dy1], fill=(252, 252, 246))
    return img


# ---------------------------------------------------------------- mead bomb bottle: 512 x 512
# v: 0-0.2 amber glass, 0.2-0.62 Kong label (front u 0.1-0.4), 0.62-0.92 glass, 0.92-1 cork.
def tex_metbombe():
    w = h = 512
    main, deep = mi.PALETTES["blue"]
    img = Image.new("RGBA", (w, h), (150, 84, 22, 255))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, w, y_of(0.92, h)], fill=(120, 78, 40))
    top, bottom = y_of(0.62, h), y_of(0.2, h)
    d.rectangle([0, top, w, bottom], fill=mi.BLACK)
    x0, split, x1 = int(0.06 * w), int(0.22 * w), int(0.44 * w)
    d.rectangle([x0, top, split, bottom], fill=main)
    face = draw_half_face("blue").resize((110, int(110 * GH / GW)), Image.LANCZOS)
    paste_clipped(img, face, (split - 110, (top + bottom) // 2 - face.height // 2), [x0, top, split, bottom])
    word = mi.fit(mi.metallic_text("MET", main, deep).rotate(90, expand=True), x1 - split - 16, bottom - top - 16)
    img.alpha_composite(word, (split + (x1 - split - word.width) // 2, top + 8))
    for x in (x1 + 40, x1 + 160):
        d.line([(x, top + 30), (x, bottom - 30)], fill=main, width=10)
    return img


# ---------------------------------------------------------------- barrel: 512 x 512
# Planks around, iron bands at v 0.12 and 0.88, Kong label on the front (u 0.1-0.4, v 0.36-0.64).
def tex_barrel():
    w = h = 512
    main, deep = mi.PALETTES["blue"]
    img = Image.new("RGBA", (w, h), (150, 96, 50, 255))
    d = ImageDraw.Draw(img)
    for i, x in enumerate(range(0, w, 40)):
        d.rectangle([x, 0, x + 38, h], fill=mi.lerp((150, 96, 50), (122, 74, 36), (i % 3) / 2))
        d.line([(x, 0), (x, h)], fill=(90, 54, 24), width=3)
    for v in (0.12, 0.88):
        d.rectangle([0, y_of(v, h) - 16, w, y_of(v, h) + 16], fill=(84, 86, 94))
    top, bottom = y_of(0.64, h), y_of(0.36, h)
    x0, split, x1 = int(0.1 * w), int(0.18 * w), int(0.4 * w)
    d.rectangle([x0, top, x1, bottom], fill=mi.BLACK)
    d.rectangle([x0, top, split, bottom], fill=main)
    word = mi.fit(mi.metallic_text("KONG", main, deep), x1 - split - 12, bottom - top - 20)
    img.alpha_composite(word, (split + (x1 - split - word.width) // 2, (top + bottom) // 2 - word.height // 2))
    return img


# ---------------------------------------------------------------- XXL canister: 512 x 512
# Rounded square; the front face is u 0.0-0.25 (between two corners). v: 0-0.86 body,
# 0.86-1 cap and handle area (white cap at u 0.6-1, the handle samples u 0.5).
def tex_xxl():
    w = h = 512
    main, deep = mi.PALETTES["blue"]
    img = Image.new("RGBA", (w, h), (222, 170, 40, 255))
    d = ImageDraw.Draw(img)
    for i in range(40):
        x, y = (i * 97) % w, 20 + (i * 53) % 120
        d.ellipse([x, y, x + 8, y + 8], fill=(250, 214, 120))
    d.rectangle([int(0.6 * w), 0, w, y_of(0.86, h)], fill=(240, 240, 240))
    top, bottom = y_of(0.72, h), y_of(0.16, h)
    x0, split, x1 = int(0.15 * w), int(0.22 * w), int(0.35 * w)
    d.rectangle([x0, top, x1, bottom], fill=mi.BLACK)
    d.rectangle([x0, top, split, bottom], fill=main)
    face = draw_half_face("blue").resize((70, int(70 * GH / GW)), Image.LANCZOS)
    paste_clipped(img, face, (split - 70, (top + bottom) // 2 - face.height // 2), [x0, top, split, bottom])
    word = mi.fit(mi.metallic_text("KONG", main, deep).rotate(90, expand=True), x1 - split - 10, bottom - top - 60)
    img.alpha_composite(word, (split + (x1 - split - word.width) // 2, top + 8))
    xxl = ImageFont.truetype(mi.FONT_HEAVY, 30)
    d.text((split + 10, bottom - 44), "XXL", font=xxl, fill=(255, 214, 0))
    return img


# ---------------------------------------------------------------- fridge atlas: 512 x 512
# door front x 0-256 (full height), side x 256-512 y 0-256, top x 256-384 y 256-512,
# inside x 384-512 y 256-512.
def tex_fridge():
    w = h = 512
    main, deep = mi.PALETTES["blue"]
    img = Image.new("RGBA", (w, h), (232, 234, 236, 255))
    d = ImageDraw.Draw(img)
    # door
    d.rectangle([0, 0, 255, 511], fill=(236, 238, 240))
    d.rectangle([12, 40, 243, 300], fill=mi.BLACK)
    d.rectangle([12, 40, 110, 300], fill=main)
    face = draw_half_face("blue").resize((130, int(130 * GH / GW)), Image.LANCZOS)
    paste_clipped(img, face, (110 - 130, 170 - face.height // 2), [12, 40, 110, 300])
    word = mi.fit(mi.metallic_text("KONG", main, deep).rotate(90, expand=True), 243 - 110 - 20, 240)
    img.alpha_composite(word, (110 + (133 - word.width) // 2, 50))
    mi.slab_text(d, (128, 330), "ICE COLD", 26, main, anchor="mm")
    d.rounded_rectangle([210, 360, 232, 480], radius=8, fill=(150, 154, 164), outline=(90, 94, 104), width=3)
    d.line([(0, 0), (0, 511)], fill=(180, 184, 190), width=4)
    # side
    d.rectangle([256, 0, 511, 255], fill=(226, 228, 232))
    d.rectangle([256, 150, 511, 190], fill=main)
    d.rectangle([256, 190, 511, 200], fill=mi.BLACK)
    # top
    d.rectangle([256, 256, 383, 511], fill=(214, 216, 220))
    # inside: dark blue with shelves and cans
    d.rectangle([384, 256, 511, 511], fill=(18, 34, 62))
    for y in (320, 384, 448):
        d.rectangle([384, y, 511, y + 6], fill=(170, 190, 210))
        for x in range(394, 500, 22):
            d.rectangle([x, y - 34, x + 14, y - 2], fill=main if (x // 22) % 2 else (226, 52, 40))
    return img


# ---------------------------------------------------------------- deposit machine atlas: 512 x 512
# front x 0-256 (full height), side x 256-512 y 0-256, top x 256-512 y 256-512.
# ---------------------------------------------------------------- deposit machine atlas: 1024 x 1024
# Sized to the 1.0 x 1.9 x 0.8 m body so nothing is stretched:
#   front  x 0-512,  full height (aspect 0.5, the box is 1.0 : 1.9)
#   side   x 512-942, full height (aspect 0.42, the box is 0.8 : 1.9), also used for the back
#   flat   x 960-1024 plain grey, used for top and bottom
AUTOMAT_FRONT = (0, 0, 512, 1024)
AUTOMAT_SIDE = (512, 0, 942, 1024)


def tex_automat():
    w = h = 1024
    main, deep = mi.PALETTES["blue"]
    body, edge = (52, 58, 70), (22, 26, 32)
    img = Image.new("RGBA", (w, h), (64, 68, 78, 255))
    d = ImageDraw.Draw(img)

    # ---- front
    d.rectangle([0, 0, 511, 1023], fill=body)
    d.rectangle([0, 0, 511, 1023], outline=edge, width=8)
    d.rectangle([24, 24, 487, 190], fill=(250, 214, 60), outline=edge, width=6)           # PFAND sign
    mi.slab_text(d, (256, 107), "PFAND", 110, mi.BLACK, anchor="mm", max_w=430)
    d.rectangle([24, 214, 487, 654], fill=mi.BLACK, outline=edge, width=6)                # advert panel
    d.rectangle([24, 214, 200, 654], fill=main)
    face = draw_half_face("blue")
    fh = 420
    face = face.resize((int(fh * GW / GH), fh), Image.LANCZOS)
    paste_clipped(img, face, (200 - face.width, 654 - fh - 4), [24, 214, 200, 654])
    word = mi.fit(mi.metallic_text("KONG", main, deep).rotate(90, expand=True), 250, 400)
    img.alpha_composite(word, (200 + (250 - word.width) // 2 + 18, 224))
    mi.slab_text(d, (356, 636), "WILD POWER", 26, main, anchor="mm", max_w=280)
    # scanner: dark slot with a green light and a laser line
    d.rounded_rectangle([96, 690, 416, 800], radius=54, fill=(10, 10, 14), outline=(160, 164, 174), width=8)
    d.rounded_rectangle([196, 726, 316, 764], radius=19, fill=(30, 200, 90))
    d.line([(120, 745), (392, 745)], fill=(255, 70, 60), width=4)
    mi.slab_text(d, (256, 840), "LEERGUT", 52, (226, 230, 236), anchor="mm", max_w=430)
    # return tray with a flap
    d.rectangle([64, 880, 448, 984], fill=(14, 16, 20), outline=(160, 164, 174), width=6)
    d.rectangle([84, 900, 428, 964], fill=(30, 34, 42))
    d.line([(84, 932), (428, 932)], fill=(80, 86, 98), width=4)

    # ---- side (also the back)
    d.rectangle([512, 0, 941, 1023], fill=main)
    d.rectangle([512, 0, 941, 1023], outline=edge, width=8)
    d.rectangle([700, 0, 941, 1023], fill=mi.BLACK)
    long_word = mi.fit(mi.metallic_text("KONG", main, deep).rotate(90, expand=True), 220, 760)
    img.alpha_composite(long_word, (700 + (241 - long_word.width) // 2, 120))
    mi.slab_text(d, (820, 930), "WILD POWER", 34, main, anchor="mm", max_w=220)
    d.rectangle([512, 0, 941, 60], fill=(250, 214, 60))
    d.rectangle([512, 964, 941, 1023], fill=(250, 214, 60))

    # ---- flat grey for top and bottom
    d.rectangle([960, 0, 1023, 1023], fill=(74, 78, 88))
    return img


def tex_wood():
    w = h = 256
    img = Image.new("RGBA", (w, h), (128, 84, 46, 255))
    d = ImageDraw.Draw(img)
    for i, y in enumerate(range(0, h, 32)):
        d.rectangle([0, y, w, y + 30], fill=mi.lerp((128, 84, 46), (104, 66, 34), (i % 3) / 2))
        d.line([(0, y), (w, y)], fill=(70, 44, 20), width=3)
    return img


# ---------------------------------------------------------------- piece icons (512 layers for make_icons)
def _box_icon(front, side, top_color, x0=150, y0=110, x1=330, y1=470, dx=70, dy=-50):
    """Simple 3/4 view of a box: front image, sheared side image, flat top."""
    layer = Image.new("RGBA", (512, 512), (0, 0, 0, 0))
    layer.alpha_composite(front.resize((x1 - x0, y1 - y0), Image.LANCZOS).convert("RGBA"), (x0, y0))
    sw, sh = dx, y1 - y0
    side = side.resize((sw, sh), Image.LANCZOS).convert("RGBA")
    sheared = side.transform((sw, sh - dy), Image.AFFINE, (1, 0, 0, -dy / sw, 1, dy), resample=Image.BICUBIC)
    shade = Image.new("RGBA", sheared.size, (0, 0, 0, 70))
    sheared.alpha_composite(Image.composite(shade, Image.new("RGBA", sheared.size, (0, 0, 0, 0)), sheared.getchannel("A")))
    layer.alpha_composite(sheared, (x1, y0 + dy))
    d = ImageDraw.Draw(layer)
    d.polygon([(x0, y0), (x0 + dx, y0 + dy), (x1 + dx, y0 + dy), (x1, y0)], fill=top_color)
    return layer


def icon_fridge():
    tex = tex_fridge()
    return _box_icon(tex.crop((0, 0, 256, 512)), tex.crop((256, 0, 512, 256)), (214, 216, 220, 255), x0=170, x1=330)


def icon_automat():
    tex = tex_automat()
    return _box_icon(tex.crop(AUTOMAT_FRONT), tex.crop(AUTOMAT_SIDE), (74, 78, 88, 255), x0=160, x1=340)


def icon_pyramid():
    layer = Image.new("RGBA", (512, 512), (0, 0, 0, 0))
    palettes = [("blue", "SUGAR\nFREE"), ("red", "URBAN\nCLASSIC"), ("ice", "FROST\nEDITION"), ("green", "TOXIC\nEDITION")]
    can_w, can_h = 88, 116
    for row in range(4):
        count = 4 - row
        can = mi.draw_can(*palettes[row]).crop((150, 10, 362, 500)).resize((can_w, can_h), Image.LANCZOS)
        for i in range(count):
            x = 256 + int((i - (count - 1) / 2) * (can_w + 4)) - can_w // 2
            y = 470 - (row + 1) * can_h
            layer.alpha_composite(can, (x, y))
    return layer


def icon_tap():
    layer = Image.new("RGBA", (512, 512), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    wood, dark = (128, 84, 46, 255), (90, 58, 30, 255)
    for x in (130, 360):
        d.rectangle([x, 330, x + 24, 490], fill=wood, outline=dark, width=4)
    d.rectangle([110, 316, 400, 344], fill=wood, outline=dark, width=4)
    can = mi.draw_xxl().resize((300, 300), Image.LANCZOS)
    layer.alpha_composite(can, (106, 40))
    d.rectangle([236, 300, 276, 318], fill=(200, 200, 205, 255), outline=(90, 90, 96, 255), width=3)
    d.rectangle([248, 316, 264, 340], fill=(200, 200, 205, 255), outline=(90, 90, 96, 255), width=3)
    return layer


PIECE_ICONS = {"fridge": icon_fridge, "automat": icon_automat, "pyramid": icon_pyramid, "tap": icon_tap}


TEXTURES = {
    "tex_can_sugarfree": lambda: tex_can("blue", "SUGAR\nFREE"),
    "tex_can_classic": lambda: tex_can("red", "URBAN\nCLASSIC"),
    "tex_can_frost": lambda: tex_can("ice", "FROST\nEDITION"),
    "tex_can_toxic": lambda: tex_can("green", "TOXIC\nEDITION"),
    "tex_can_eitr": lambda: tex_can("purple", "EITR\nEDITION"),
    "tex_empty": tex_empty,
    "tex_skyr": tex_skyr,
    "tex_skyrcup": tex_skyrcup,
    "tex_metbombe": tex_metbombe,
    "tex_barrel": tex_barrel,
    "tex_xxl": tex_xxl,
    "tex_fridge": tex_fridge,
    "tex_automat": tex_automat,
    "tex_wood": tex_wood,
}


def main():
    out_dir = sys.argv[1]
    os.makedirs(out_dir, exist_ok=True)
    for name, draw in TEXTURES.items():
        path = os.path.join(out_dir, name + ".png")
        grade(draw()).save(path, optimize=True)
        print("wrote", path)


if __name__ == "__main__":
    main()
