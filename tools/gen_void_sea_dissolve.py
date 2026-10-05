"""生成〔虚数之海〕进入转场用的溶解遮罩纹理。

用法：
    C:\\Users\\14910\\.workbuddy\\binaries\\python\\versions\\3.13.12\\python.exe tools\\gen_void_sea_dissolve.py

产物：Fgo/images/transitions/void_sea_dissolve.png（8bit 灰度，纯 Python 写 PNG）

约定（与原版 res://images/ui/transitions/*.png 完全一致）：
    原版推进数学   falloff = 1.0 - texture(transitionTex, UV).r
                   remap   = mix(-0.1, 1.1, threshold)
                   cover   = step(falloff, remap)
    原版贴图是「左白右黑」的水平渐变，所以 falloff 左小右大；
    threshold 上升时遮罩自左向右铺开。

本图取**反向**：红通道「左暗右亮」+ 周期性 fbm 噪声，于是
    falloff = 1 - r  左大右小
threshold 由 1 降到 0 时，remap 向下扫过 falloff，
    揭���条件是 remap < falloff  ⇒  falloff 大的左侧先被揭开
即虚数之海**自左向右**显现，溶解锋面被噪声撕成有机锯齿。

必须做成横向无缝（周期噪声）：采样是 UV 0..1 直接映射，横向首尾相接，
出现接缝就会在屏幕左右边缘各留一条不溶解的硬线。
"""

import math
import struct
import zlib

W, H = 1024, 480          # 2.1333，与虚数之海图层 2881×1350 同比例
UP = 8                    # fbm 在 1/8 分辨率的格点上算，再平滑上采样
GRADIENT = 0.0            # 0 = 纯梯度（左暗右亮）；>0 时按噪声阈值化成块状
NOISE_AMP = 0.30          # 噪声撕裂幅度（相对整幅 0..1 的跨度）
FINE_AMP = 0.045          # 细粒度，消掉上采样带来的"塑料感"
FBM_OCTAVES = 4
SEED = 20261005


def _hash(ix, iy, period, seed):
    """周期性整数哈希：ix/iy 先对 period 取模，保证左右无缝。"""
    ix %= period
    iy %= period
    n = (ix * 374761393 + iy * 668265263 + seed * 1442695041) & 0xFFFFFFFF
    n = (n ^ (n >> 13)) * 1274126177 & 0xFFFFFFFF
    n ^= n >> 16
    return (n & 0xFFFFFF) / float(0xFFFFFF)


def _smooth(t):
    return t * t * (3.0 - 2.0 * t)


def _value_noise(x, y, period, seed):
    ix, iy = math.floor(x), math.floor(y)
    fx, fy = _smooth(x - ix), _smooth(y - iy)
    a = _hash(ix, iy, period, seed)
    b = _hash(ix + 1, iy, period, seed)
    c = _hash(ix, iy + 1, period, seed)
    d = _hash(ix + 1, iy + 1, period, seed)
    return (a * (1 - fx) + b * fx) * (1 - fy) + (c * (1 - fx) + d * fx) * fy


def _fbm(x, y, period, seed, octaves):
    total, amp, freq = 0.0, 0.5, 1.0
    norm = 0.0
    for o in range(octaves):
        # 每个倍频的周期必须是整数，否则 _hash 里的 % 会拿到 float
        p = max(1, int(round(period * freq)))
        total += amp * _value_noise(x * freq, y * freq, p, seed + o * 1013)
        norm += amp
        amp *= 0.5
        freq *= 2.0
    return total / norm


def _build():
    # ── 1. 低分辨率 fbm（横向周期 = 格点数，纵向不强制周期）──────────
    cw, ch = W // UP, H // UP
    coarse = [[0.0] * cw for _ in range(ch)]
    for j in range(ch):
        v = (j + 0.5) / ch
        for i in range(cw):
            u = (i + 0.5) / cw
            # 横向拉伸：噪声在宽图上要显得「高而窄」，才像侵蚀的竖向撕裂
            coarse[j][i] = _fbm(u * 6.0, v * 6.0 * (H / W) * 2.0, 6, SEED, FBM_OCTAVES)

    # ── 2. 提升到全分辨率（格点间 smoothstep 双线性）────────────────
    out = [[0.0] * W for _ in range(H)]
    for y in range(H):
        gy = y / float(UP) - 0.5
        j0 = int(math.floor(gy))
        fy = _smooth(gy - j0)
        # Python 的 % 对负数返回非负，故 i0/j0 为负时也能正确环绕
        row_a = coarse[j0 % ch]
        row_b = coarse[(j0 + 1) % ch]
        for x in range(W):
            gx = x / float(UP) - 0.5
            i0 = int(math.floor(gx))
            fx = _smooth(gx - i0)
            a = row_a[i0 % cw] * (1 - fx) + row_a[(i0 + 1) % cw] * fx
            b = row_b[i0 % cw] * (1 - fx) + row_b[(i0 + 1) % cw] * fx
            out[y][x] = a * (1 - fy) + b * fy

    # ── 3. 合成梯度 + 噪声 → 红通道（0..255，左暗右亮）───────────────
    rows = []
    for y in range(H):
        v = y / float(H - 1)
        row = bytearray()
        for x in range(W):
            u = x / float(W - 1)
            base = u
            if GRADIENT > 0.0:
                n = out[y][x]
                base = u + GRADIENT * (0.5 - n)
            val = base + (out[y][x] - 0.5) * NOISE_AMP
            # 细粒度：全分辨率 hash，打散上采样的平滑感
            val += (_hash(x, y, 1 << 30, SEED + 77) - 0.5) * FINE_AMP
            val = 0.0 if val < 0.0 else (1.0 if val > 1.0 else val)
            g = int(val * 255.0 + 0.5)
            row += bytes((g, g, g))
        rows.append(bytes(row))
    return rows


def _write_png(path, rows):
    raw = b"".join(b"\x00" + r for r in rows)          # 每行前置 filter 字节 0
    comp = zlib.compress(raw, 9)

    def chunk(tag, data):
        c = struct.pack(">I", len(data)) + tag + data
        return c + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

    ihdr = struct.pack(">IIBBBBB", W, H, 8, 2, 0, 0, 0)  # 8bit, truecolor RGB
    png = (b"\x89PNG\r\n\x1a\n"
           + chunk(b"IHDR", ihdr)
           + chunk(b"IDAT", comp)
           + chunk(b"IEND", b""))
    with open(path, "wb") as f:
        f.write(png)


def main():
    import os
    out = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                       "Fgo", "images", "transitions", "void_sea_dissolve.png")
    os.makedirs(os.path.dirname(out), exist_ok=True)
    rows = _build()
    _write_png(out, rows)

    # 自检：红通道必须单调不下降地「左暗右亮」，否则溶解方向会反。
    left = sum(rows[H // 2][x * 3] for x in range(0, 40)) / 40.0
    right = sum(rows[H // 2][x * 3] for x in range(W - 40, W)) / 40.0
    print("wrote %s  (%dx%d)" % (out, W, H))
    print("  中线左端 r=%.1f  右端 r=%.1f  (要求 左 < 右 ⇒ 自左向右溶解)" % (left, right))
    assert left < right, "遮罩方向反了：红通道应左暗右亮"
    print("  OK")


if __name__ == "__main__":
    main()
