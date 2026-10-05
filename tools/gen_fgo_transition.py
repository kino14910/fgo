"""重绘选角→进战斗转场遮罩 fgo_transition.webp，贴合 FGO 美术风格并融入令紋。

用法：
    C:\\Users\\14910\\.workbuddy\\binaries\\python\\envs\\default\\Scripts\\python.exe tools\\gen_fgo_transition.py

产物（两份保持同步，md5 必须相同）：
    Fgo/images/transitions/fgo_transition.webp
    Fgo/images/charui/fgo_transition.webp

─── 这张图不是美术图，是 falloff 场 ───────────────────────────────
fgo_transition.gdshader 只读红通道：
    falloff = 1.0 - texture(transitionTex, UV).r
    remap   = mix(-0.1, 1.1, threshold)
    cover   = step(falloff, remap)          ← 唯一决定 alpha 的量
原图是「左白右黑」斜向刷痕：r 左高右低 ⇒ falloff 左小右大 ⇒ 自左向右烧开。
端点契约由 r∈[0,1] 自动满足：th=0 时 remap=-0.0 < falloff ⇒ alpha 全 0；
th=1 时 remap=1.1 ≥ falloff ⇒ alpha 全 1。**所以别让 r 越界。**

─── 令紋怎么融进来 ───────────────────────────────────────────────
不是把令紋画在幕上（alpha 由 cover 决定，画了也看不见），而是让
**falloff 场的局部极大值长成令紋形状**：
    R = 斜向水墨梯度 + 令紋穹丘 + fbm 撕口 + 干笔飞白
穹丘使场在令紋核心处隆起成局部极大值，于是 remap 扫过时，等值线
先在令紋内部闭合成环、再沿字形外扩 —— shader 的 rim 顺着头像描出的
金边**自动勾出令紋轮廓**，推进锋面被令紋调制，而不是一块矩形刷子。

通道分工：
    R = falloff 场（契约通道，动它就改溶解方向/时序）
    G = 令紋本体（shader 上 crimson 令紋）
    B = 令紋描边（shader 上 gold 轮廓，与 rim 的推进锋面区分开）
原文件是有损 VP8，色度子采样会毁掉 G/B 的独立信息，故必须存无损。

─── 尺寸与比例 ───────────────────────────────────────────────────
纹理 2560×1200（2.133）被拉伸到宿主 ColorRect 约 3200×1320（2.42）。
令紋若按纹素画圆，屏上会被拉成 1.134 倍的椭圆，故令紋在**p 空间**
（p=(UV-0.5)*vec2(2.42,1.0)，与 shader 同构）里定义，屏上才是正圆。
"""

import os

import numpy as np
from PIL import Image

# ── 输出 ────────────────────────────────────────────────────────────
W, H = 2560, 1200
ASPECT = 2.42                     # 与 shader 的 aspect uniform 一致

# ── 斜向水墨梯度（左白右黑）────────────────────────────────────────
TILT = np.radians(13.0)           # 刷痕倾角，给梯度一点斜势
R_HI = 0.88                       # 最左：最先覆盖
R_LO = 0.05                       # 最右：最后覆盖
R_EASE = 0.90                     # <1 = 左侧铺得快，右侧收尾留时间

# ── 令紋穹丘 ───────────────────────────────────────────────────────
SEAL_SRC = os.path.join("Fgo", "images", "ui", "CommandSpell", "CommandSpell3.png")
SEAL_R = 0.40                     # 令紋半宽（p 空间）——比 shader 内绘的 r=0.150
                                  # 大得多：本图是转场主视觉，要压得住 3200px 宽的幕
DOME_AMP = 0.58                   # 穹丘高度。需 > 梯度在令紋半宽内的落差(≈0.23)，
                                  # 否则令紋只是缓坡，闭不出等值线环
SEAL_UP = 8                       # 令紋上采样倍数（源 128px → 1024px，边缘不毛躁）
SEAL_BLUR = 30                    # 令�高斯模糊半径（1024 空间）：让等值线平滑可读

# ── 水墨质感 ───────────────────────────────────────────────────────
FBM_OCT = 5
NOISE_AMP = 0.085                 # 有机撕口
STREAK_AMP = 0.035                # 干笔飞白：横向拉长的竖丝
FINE_AMP = 0.004                  # 细粒度，消掉程序化平滑感。
                                  # 无损 WebP 下这是体积杀手：全分辨率白噪不可压缩，
                                  # 从 0.012 降到 0.004 后整图 1.36MB → 约 0.5MB，
                                  # 观感几乎无损（水墨质感由中频 fbm 承担）。
NOISE_FREQ = (7.0, 5.0)           # 横向略多于纵向 ⇒ 噪声偏高瘦，像侵蚀的竖向撕裂
STREAK_FREQ = (34.0, 2.0)         # 横向高频、纵向低频 = 竖直飞白
SEED = 20261005

OUTS = [
    os.path.join("Fgo", "images", "transitions", "fgo_transition.webp"),
    os.path.join("Fgo", "images", "charui", "fgo_transition.webp"),
]


# ── 噪声 ────────────────────────────────────────────────────────────
def _value_noise(h, w, fx, fy, seed):
    """双轴周期性 value noise。UV 是 0..1 直映射，噪声跨边界不连续会在
    幕布上留下竖直硬缝，故两轴都取模环绕。"""
    rng = np.random.default_rng(seed)
    g = rng.random((fy, fx)).astype(np.float32)
    yy = np.arange(h, dtype=np.float32) / h * fy
    xx = np.arange(w, dtype=np.float32) / w * fx
    y0 = np.floor(yy).astype(np.int32)
    x0 = np.floor(xx).astype(np.int32)
    ty = yy - y0
    tx = xx - x0
    ty = ty * ty * (3.0 - 2.0 * ty)
    tx = tx * tx * (3.0 - 2.0 * tx)
    y0m, y1m = y0 % fy, (y0 + 1) % fy
    x0m, x1m = x0 % fx, (x0 + 1) % fx
    a = g[np.ix_(y0m, x0m)]
    b = g[np.ix_(y0m, x1m)]
    c = g[np.ix_(y1m, x0m)]
    d = g[np.ix_(y1m, x1m)]
    top = a + (b - a) * tx[None, :]
    bot = c + (d - c) * tx[None, :]
    return top + (bot - top) * ty[:, None]


def _fbm(h, w, freq, octaves, seed):
    total = np.zeros((h, w), dtype=np.float32)
    norm = 0.0
    amp = 0.5
    for o in range(octaves):
        fx = max(2, int(round(freq[0] * (2 ** o))))
        fy = max(2, int(round(freq[1] * (2 ** o))))
        total += amp * _value_noise(h, w, fx, fy, seed + o * 1013)
        norm += amp
        amp *= 0.5
    return total / norm


def _box_blur(a, r):
    """可分离盒式模糊 ×3 ≈ 高斯。numpy cumsum 实现，避开 scipy 依赖。"""
    if r < 1:
        return a
    for _ in range(3):
        pad = np.pad(a, ((0, 0), (r, r)), mode="edge")
        cs = np.cumsum(pad, axis=1, dtype=np.float32)
        cs = np.concatenate([np.zeros((a.shape[0], 1), np.float32), cs], axis=1)
        a = (cs[:, 2 * r + 1:2 * r + 1 + a.shape[1]] - cs[:, :a.shape[1]]) / (2 * r + 1)
        pad = np.pad(a, ((r, r), (0, 0)), mode="edge")
        cs = np.cumsum(pad, axis=0, dtype=np.float32)
        cs = np.concatenate([np.zeros((1, a.shape[1]), np.float32), cs], axis=0)
        a = (cs[2 * r + 1:2 * r + 1 + a.shape[0], :] - cs[:a.shape[0], :]) / (2 * r + 1)
    return a


# ── 令紋 ────────────────────────────────────────────────────────────
def _load_seal(root):
    """把令紋 alpha 抬到高分辨率，并记下「p 空间原点 ↔ 源像素」的映射。

    源图 128px 的 alpha 包围盒是 x∈[1,126]、y∈[11,124]，视觉中心在
    (63.5, 67.5) 而非 (64,64)；三臂对称的真中心取包围盒中心。
    """
    im = Image.open(os.path.join(root, SEAL_SRC)).convert("RGBA")
    a = np.asarray(im.split()[3], dtype=np.float32) / 255.0
    n = 128 * SEAL_UP
    a = np.asarray(
        Image.fromarray((a * 255.0).astype(np.uint8)).resize((n, n), Image.LANCZOS),
        dtype=np.float32,
    ) / 255.0
    cx, cy = 63.5 * SEAL_UP, 67.5 * SEAL_UP
    # 令紋半宽 62.5 源像素 ↦ SEAL_R（p 空间）
    k = SEAL_R / (62.5 * SEAL_UP)
    return a, cx, cy, k


def _seal_field(root, h, w):
    """在 p 空间采样令紋，返回 (穹丘 dome, 锐利本体 sig, 锐利描边 edge)，均为 0..1。

    逐纹素做 p→源坐标 的双线性采样：p=(sx,sy) ⇒ 源 (cx+sx/k, cy+sy/k)，
    于是令紋在 2.42 比例的幕上呈正圆而非被拉宽的椭圆。

    刻意分「锐利」与「模糊」两份，用途不同：
      - 模糊 → 塞进 R 场当穹丘，让等值线平滑闭合（毛刺的等值线会碎成噪点）
      - 锐利 → 进 G/B 通道，shader 拿上 crimson 本体 + gold 轮廓。
        若把模糊版塞进 G，shader 画出来是一团糊 blob，令紋的三臂与中心柱全丢了。
    """
    a, cx, cy, k = _load_seal(root)
    n = a.shape[0]

    u = (np.arange(w, dtype=np.float32) + 0.5) / w
    v = (np.arange(h, dtype=np.float32) + 0.5) / h
    sx = (u - 0.5) * ASPECT                       # (W,)
    sy = (v - 0.5)                                # (H,)
    fx = cx + sx[None, :] / k                     # (1,W) 源坐标
    fy = cy + sy[:, None] / k                     # (H,1)

    x0 = np.floor(fx).astype(np.int32)
    y0 = np.floor(fy).astype(np.int32)
    tx = fx - x0
    ty = fy - y0

    def fetch(yi, xi):
        yc = np.clip(yi, 0, n - 1)
        xc = np.clip(xi, 0, n - 1)
        return a[yc, xc]

    sig = (fetch(y0, x0) * (1 - tx) + fetch(y0, x0 + 1) * tx) * (1 - ty) + \
          (fetch(y0 + 1, x0) * (1 - tx) + fetch(y0 + 1, x0 + 1) * tx) * ty

    sig = np.broadcast_to(sig, (h, w)).astype(np.float32).copy()
    sig = np.clip(sig, 0.0, 1.0)

    # 锐利本体：轻微对比拉伸，去掉 LANCZOS  ringing 带来的半透明边缘，
    # 让 shader 上的 crimson 是实心色块而不是雾。
    # 只拉伸不模糊 —— 令紋的识别特征就是那道锐边，糊了就不像令紋了。
    # 边缘的抗锯齿交给上面的描边通道（gold 线）去补。
    sig = np.clip((sig - 0.06) / 0.88, 0.0, 1.0)

    # 锐利描边：梯度模按 99.5 百分位归一化（写死系数会随参数漂移）
    gx = np.gradient(sig, axis=1)
    gy = np.gradient(sig, axis=0)
    grad = np.hypot(gx, gy)
    full = float(np.percentile(grad, 99.5))
    if full > 1e-6:
        grad = grad / full
    edge = np.clip(grad, 0.0, 1.0) * (sig > 0.02)
    # 轻微模糊：梯度模是逐像素差分，直接用会在幕上切出锯齿硬边
    # （shader 里乘上去就是一圈黑色锯齿）。模糊半径给得小，只为断掉锯齿。
    edge = _box_blur(edge, 4)
    edge = np.clip(edge * 1.15, 0.0, 1.0)

    # 穹丘：模糊版，喂给 R 场
    dome = np.clip(_box_blur(sig, SEAL_BLUR), 0.0, 1.0)
    return dome, sig, edge


# ── 主构建 ──────────────────────────────────────────────────────────
def _build(root):
    u = ((np.arange(W, dtype=np.float32) + 0.5) / W)[None, :]
    v = ((np.arange(H, dtype=np.float32) + 0.5) / H)[:, None]
    uu = np.broadcast_to(u, (H, W))
    vv = np.broadcast_to(v, (H, W))

    # 1. 斜向梯度：t 归一化到 0..1，左 0 右 1
    t = uu * np.cos(TILT) + vv * np.sin(TILT)
    t0 = min(0.0, np.sin(TILT))
    t1 = np.cos(TILT) + np.sin(TILT)
    tn = (t - t0) / (t1 - t0)
    base = R_HI + (R_LO - R_HI) * np.power(np.clip(tn, 0.0, 1.0), R_EASE)

    # 2. 令紋穹丘：局部极大值 ⇒ 等值线闭合成令紋轮廓
    dome, seal, edge = _seal_field(root, H, W)

    # 3. 水墨撕口 + 干笔飞白 + 细粒度
    n = _fbm(H, W, NOISE_FREQ, FBM_OCT, SEED)
    streak = _fbm(H, W, STREAK_FREQ, 2, SEED + 7717)
    rng = np.random.default_rng(SEED + 31)
    fine = rng.random((H, W)).astype(np.float32) - 0.5

    r = base + DOME_AMP * dome + (n - 0.5) * 2.0 * NOISE_AMP + \
        (streak - 0.5) * 2.0 * STREAK_AMP + fine * 2.0 * FINE_AMP

    # 端点契约：r 必须落在 [0,1]
    r = np.clip(r, 0.0, 1.0)
    return r, seal, edge


def _write_webp(path, r, seal, edge):
    """存**无损** WebP。有损 VP8 的色度子采样会毁掉 G/B 的独立信息，
    而 G/B 正是令紋的 crimson 本体与 gold 描边。"""
    img = np.empty((H, W, 3), dtype=np.uint8)
    img[:, :, 0] = np.clip(r * 255.0 + 0.5, 0, 255).astype(np.uint8)
    img[:, :, 1] = np.clip(seal * 255.0 + 0.5, 0, 255).astype(np.uint8)
    img[:, :, 2] = np.clip(edge * 255.0 + 0.5, 0, 255).astype(np.uint8)
    Image.fromarray(img, "RGB").save(path, "WEBP", lossless=True, quality=100, method=6)


def main():
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    r, seal, edge = _build(root)

    for rel in OUTS:
        _write_webp(os.path.join(root, rel), r, seal, edge)

    # ── 自检 ────────────────────────────────────────────────────────
    mid = H // 2
    left = r[mid, :W // 50].mean()
    right = r[mid, -W // 50:].mean()
    core = r[mid, W // 2]
    corners = [r[2, 2], r[2, -3], r[-3, 2], r[-3, -3]]
    print("wrote:")
    for rel in OUTS:
        p = os.path.join(root, rel)
        print("  %s  %d bytes" % (rel, os.path.getsize(p)))
    print("  中线左端 r=%.3f  右端 r=%.3f  (要求 左 > 右 ⇒ 自左向右溶解)" % (left, right))
    print("  令紋核心 r=%.3f  角落 r=%s" % (core, " ".join("%.3f" % c for c in corners)))
    print("  r 全局 [%.4f, %.4f]" % (r.min(), r.max()))

    assert left > right, "溶解方向反了：红通道应左亮右暗"
    assert r.min() >= 0.0 and r.max() <= 1.0, "r 越界，端点契约会漏色"
    assert core > r[mid, W // 4], "令紋核心没有隆起，等值线闭不出令紋轮廓"
    assert seal.max() > 0.9 and edge.max() > 0.5, "令紋通道 G/B 是空的"
    print("  OK")


if __name__ == "__main__":
    main()
