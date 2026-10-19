"""Stylised gorilla head for the icons: the left half of a front-facing face.

The right edge of the image is the centre line of the face, so the black panel of
the can can cover the other half like on the real can.
"""
import math
from PIL import Image, ImageDraw

GW, GH = 1000, 1400        # canvas of the half face; x = GW is the centre line

TONES = {
    # outline, dark fur, mid fur, light skin, highlight
    "blue": ((5, 18, 44), (12, 50, 108), (26, 98, 178), (70, 156, 230), (180, 226, 255)),
    "red": ((36, 4, 4), (98, 12, 10), (160, 28, 22), (230, 88, 66), (255, 200, 180)),
    "ice": ((8, 40, 64), (40, 110, 160), (92, 168, 214), (168, 222, 246), (235, 250, 255)),
    "green": ((6, 34, 8), (20, 86, 24), (44, 150, 46), (120, 214, 96), (214, 255, 190)),
    "purple": ((26, 6, 44), (64, 22, 110), (112, 52, 176), (176, 120, 236), (236, 210, 255)),
}


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(len(a)))


def _pt(u, v):
    return (u * GW, v * GH)


def _catmull(points, closed=True, steps=14):
    """Smooth curve through the points (Catmull-Rom)."""
    pts = list(points)
    n = len(pts)
    out = []
    for i in range(n if closed else n - 1):
        p0 = pts[(i - 1) % n] if closed or i > 0 else pts[i]
        p1, p2 = pts[i], pts[(i + 1) % n]
        p3 = pts[(i + 2) % n] if closed or i + 2 < n else p2
        for s in range(steps):
            t = s / steps
            t2, t3 = t * t, t * t * t
            out.append(tuple(
                0.5 * (2 * p1[k] + (-p0[k] + p2[k]) * t + (2 * p0[k] - 5 * p1[k] + 4 * p2[k] - p3[k]) * t2
                       + (-p0[k] + 3 * p1[k] - 3 * p2[k] + p3[k]) * t3) for k in range(2)))
    if not closed:
        out.append(pts[-1])
    return out


def _shape(d, uv, fill=None, outline=None, width=0):
    pts = _catmull([_pt(u, v) for u, v in uv])
    d.polygon(pts, fill=fill)
    if outline:
        d.line(pts + [pts[0]], fill=outline, width=width, joint="curve")


def _stroke(d, uv, fill, width):
    d.line(_catmull([_pt(u, v) for u, v in uv], closed=False), fill=fill, width=width, joint="curve")


def _strand(d, p0, pc, p1, width, fill):
    """Tapered, curved fur strand along a quadratic Bezier (u/v coordinates)."""
    a, c, b = _pt(*p0), _pt(*pc), _pt(*p1)
    left, right = [], []
    steps = 24
    for i in range(steps + 1):
        t = i / steps
        x = (1 - t) ** 2 * a[0] + 2 * (1 - t) * t * c[0] + t * t * b[0]
        y = (1 - t) ** 2 * a[1] + 2 * (1 - t) * t * c[1] + t * t * b[1]
        dx = 2 * (1 - t) * (c[0] - a[0]) + 2 * t * (b[0] - c[0])
        dy = 2 * (1 - t) * (c[1] - a[1]) + 2 * t * (b[1] - c[1])
        length = math.hypot(dx, dy) or 1
        w = width * math.sin(math.pi * min(1.0, t * 1.15)) ** 0.8 / 2
        left.append((x - dy / length * w, y + dx / length * w))
        right.append((x + dy / length * w, y - dx / length * w))
    d.polygon(left + right[::-1], fill=fill)


# irregular tuft sizes so the mane does not look like a saw blade
_TUFT_DEPTH = (1.0, 0.65, 1.25, 0.8, 1.1, 0.55, 1.2, 0.9, 0.7, 1.15, 0.85, 1.0)


def _shaggy(uv, spikes, depth, start, end, sweep=0.6):
    """Adds swept, pointed fur tufts to the outer contour between two fractions of its length."""
    smooth = _catmull([_pt(u, v) for u, v in uv], steps=30)
    n = len(smooth)
    a, b = int(n * start), int(n * end)
    out = []
    for i, (x, y) in enumerate(smooth):
        if a <= i <= b:
            px, py = smooth[i - 1]
            nx, ny = smooth[(i + 1) % n]
            tx, ty = nx - px, ny - py
            length = math.hypot(tx, ty) or 1
            tx, ty = tx / length, ty / length
            ox, oy = -ty, tx                               # outward normal of this contour
            pos = (i - a) / max(1, b - a) * spikes
            phase = pos % 1.0
            k = depth * _TUFT_DEPTH[int(pos) % len(_TUFT_DEPTH)] * phase ** 2.5   # sharp tip at phase 1
            out.append((x + (ox + tx * sweep) * k, y + (oy + ty * sweep) * k))
        else:
            out.append((x, y))
    return out


def draw_full_face(tone="blue"):
    """Both halves side by side, for places without a split panel."""
    half = draw_half_face(tone)
    full = Image.new("RGBA", (GW * 2, GH), (0, 0, 0, 0))
    full.alpha_composite(half, (0, 0))
    full.alpha_composite(half.transpose(Image.FLIP_LEFT_RIGHT), (GW, 0))
    return full


def draw_half_face(tone="blue"):
    outline, dark, mid, light, hi = TONES[tone]
    img = Image.new("RGBA", (GW, GH), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    line = 20

    # head with a high crest, from the top of the centre line over the left side to the chin
    head = [(1.0, 0.02), (0.8, 0.03), (0.58, 0.09), (0.4, 0.18), (0.26, 0.3), (0.17, 0.45),
            (0.14, 0.6), (0.18, 0.75), (0.28, 0.88), (0.48, 0.97), (1.0, 0.99)]
    mane = _shaggy(head, spikes=10, depth=110, start=0.08, end=0.78)
    d.polygon(mane, fill=outline)
    d.polygon([(x + (GW - x) * 0.045, y + 14) for x, y in mane], fill=dark)

    # curved fur strands following the head
    for p0, pc, p1, w in (
            ((0.92, 0.08), (0.8, 0.1), (0.7, 0.2), 40), ((0.72, 0.1), (0.58, 0.14), (0.5, 0.25), 44),
            ((0.52, 0.17), (0.4, 0.23), (0.34, 0.34), 44), ((0.36, 0.3), (0.26, 0.4), (0.25, 0.52), 42),
            ((0.26, 0.47), (0.2, 0.58), (0.23, 0.7), 40), ((0.26, 0.66), (0.24, 0.78), (0.33, 0.86), 36),
            ((0.84, 0.2), (0.74, 0.24), (0.7, 0.3), 30), ((0.62, 0.22), (0.5, 0.27), (0.46, 0.33), 30)):
        _strand(d, p0, pc, p1, w, mid)

    # bare face skin
    _shape(d, [(1.0, 0.36), (0.7, 0.36), (0.44, 0.39), (0.33, 0.47), (0.32, 0.58), (0.37, 0.69),
               (0.44, 0.81), (0.55, 0.9), (0.72, 0.95), (1.0, 0.96)], fill=mid, outline=outline, width=line)
    _shape(d, [(0.38, 0.5), (0.47, 0.49), (0.51, 0.58), (0.46, 0.67), (0.39, 0.61)],
           fill=lerp(mid, light, 0.35))

    # small, deep-set eye (drawn before the brow, which overlaps it)
    _shape(d, [(0.47, 0.44), (0.62, 0.425), (0.8, 0.46), (0.64, 0.49), (0.5, 0.478)], fill=outline)
    d.ellipse([_pt(0.585, 0.43), _pt(0.7, 0.48)], fill=hi)
    d.ellipse([_pt(0.615, 0.44), _pt(0.675, 0.473)], fill=outline)
    d.ellipse([_pt(0.625, 0.444), _pt(0.645, 0.456)], fill=(255, 255, 255))

    # massive brow ridge, dipping towards the centre for an angry look
    _shape(d, [(1.0, 0.3), (0.72, 0.27), (0.46, 0.28), (0.28, 0.34), (0.24, 0.4), (0.38, 0.43),
               (0.56, 0.44), (0.78, 0.46), (1.0, 0.5)], fill=dark, outline=outline, width=line)
    _stroke(d, [(0.34, 0.335), (0.55, 0.31), (0.82, 0.335), (0.98, 0.37)], mid, 16)

    # wrinkles under the eye
    _stroke(d, [(0.5, 0.505), (0.64, 0.515), (0.78, 0.505)], outline, 11)
    _stroke(d, [(0.55, 0.545), (0.67, 0.555), (0.8, 0.545)], outline, 9)

    # broad, flat nose pad with a big nostril
    _stroke(d, [(0.93, 0.5), (0.9, 0.54), (0.86, 0.575)], outline, 13)
    _shape(d, [(1.0, 0.555), (0.84, 0.56), (0.71, 0.6), (0.66, 0.655), (0.71, 0.705), (0.86, 0.712),
               (1.0, 0.716)], fill=lerp(mid, light, 0.45), outline=outline, width=line)
    _shape(d, [(0.97, 0.66), (0.86, 0.637), (0.75, 0.66), (0.79, 0.692), (0.94, 0.697)], fill=outline)
    _stroke(d, [(0.76, 0.6), (0.86, 0.59)], lerp(light, hi, 0.4), 9)

    # muzzle: wide upper lip, frowning mouth, chin
    _shape(d, [(1.0, 0.716), (0.82, 0.722), (0.64, 0.748), (0.56, 0.8), (0.58, 0.86), (0.72, 0.9),
               (1.0, 0.905)], fill=lerp(mid, light, 0.3), outline=outline, width=14)
    _stroke(d, [(0.57, 0.85), (0.76, 0.838), (1.0, 0.842)], outline, 15)
    _stroke(d, [(0.68, 0.77), (0.84, 0.765)], lerp(light, hi, 0.4), 9)
    _stroke(d, [(0.7, 0.925), (0.86, 0.935), (1.0, 0.93)], outline, 10)

    return img
