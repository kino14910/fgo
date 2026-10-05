shader_type canvas_item;
render_mode blend_mix, unshaded;

// ════════════════════════════════════════════════════════════════════
//  〔虚数之海〕进入转场遮罩
//
//  推进数学与原版 res://shaders/texture_transition.gdshader **逐字一致**：
//      falloff = 1.0 - texture(transitionTex, UV).r;
//      remap   = mix(-0.1, 1.1, threshold);
//      cover   = step(falloff, remap);
//  遮罩图 void_sea_dissolve.png 是「左暗右亮 + 周期 fbm 噪声」，
//  故 falloff 左大右小；threshold 由 1 降到 0 时 remap 向下扫过 falloff，
//  揭盖条件 remap < falloff ⇒ 左侧先揭开 ⇒ 虚数之海自左向右溶解显现。
//
//  端点契约：threshold=1 ⇒ alpha 全 1（完全覆盖）；threshold=0 ⇒ alpha 全 0。
//
//  **无闪烁的关键**：cover 只由 threshold 决定，噪声烘在贴图里而不是每帧用 TIME 算。
//  以前把 fbm(TIME) 叠进 falloff，溶解锋面会随时间抖动，1.9s 的溶解里一直在闪。
//  现在 TIME 只驱动极慢的颜色层涌动，不参与覆盖度判定。
// ════════════════════════════════════════════════════════════════════

uniform sampler2D transitionTex;
uniform float threshold : hint_range(0,1);

// ── 画面比例（虚数之海图层 2881×1350 ⇒ 2.13，与遮罩图同比例）──────
uniform float aspect : hint_range(0.5,4.0) = 2.13;

// ── 阴森配色：幽蓝 / 暗紫 / 深渊黑 ─────────────────────────────
uniform vec4 abyss   : source_color = vec4(0.043, 0.031, 0.078, 1.0); // 深渊底
uniform vec4 deepblue: source_color = vec4(0.055, 0.176, 0.361, 1.0); // 幽蓝
uniform vec4 violet  : source_color = vec4(0.271, 0.106, 0.443, 1.0); // 暗紫
uniform vec4 glowblue: source_color = vec4(0.278, 0.639, 0.949, 1.0); // 裂隙冷光

// ── 分量强度（0 = 关闭该层）────────────────────────────────────
uniform float crack_gain : hint_range(0.0,2.0) = 1.00; // 裂纹
uniform float surge_gain : hint_range(0.0,2.0) = 0.85; // 深渊涌动
uniform float eye_gain   : hint_range(0.0,2.0) = 0.90; // 深渊之眼
uniform float edge_gain  : hint_range(0.0,2.0) = 1.00; // 溶解锋面辉光
uniform float edge_soft  : hint_range(0.005,0.30) = 0.055;

// 静态哈希：裂纹与涌动都用它，**不取 TIME**，保证溶解过程绝对稳定。
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float vnoise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// 静态 2 倍频脊状噪波 → 裂纹骨架
float cracks_at(vec2 q) {
    float n1 = 1.0 - abs(vnoise(q) * 2.0 - 1.0);
    float n2 = 1.0 - abs(vnoise(q * 2.3 + 5.1) * 2.0 - 1.0);
    return pow(clamp(n1, 0.0, 1.0), 7.0) + pow(clamp(n2, 0.0, 1.0), 12.0) * 0.55;
}

void fragment() {
    float th = clamp(threshold, 0.0, 1.0);

    vec2 p = (UV - vec2(0.5)) * vec2(aspect, 1.0);
    float r = length(p);

    // ── 推进遮罩：与原版一致，噪声来自贴图本身 ─────────────────
    float falloff = 1.0 - texture(transitionTex, UV).r;
    float remap = mix(-0.1, 1.1, th);
    float cover = step(falloff, remap);

    // ── 溶解锋面：沿噪声撕出的锯齿线发光（只乘 cover，不越界）────
    float edge = 1.0 - smoothstep(0.0, edge_soft, abs(falloff - remap));
    float halo = exp(-abs(falloff - remap) * 9.0) * cover;

    // ── 裂纹 / 之眼：静态，位置固定，不随时间抖 ────────────────
    float cracks = cracks_at(p * 3.2) * exp(-r * 0.85);
    float eye_r = 0.17 + 0.10 * (1.0 - th);
    float eye = exp(-pow(r / eye_r, 2.0));
    float eye_ring = 1.0 - smoothstep(0.0, 0.012, abs(r - eye_r * 1.35));

    // ── 涌动：极慢的大尺度呼吸，只影响颜色，不影响覆盖度 ─────────
    float swirl = vnoise(p * 1.15);
    float surge = smoothstep(0.38, 0.92, vnoise(p * 2.3 + vec2(swirl * 1.8, 0.0)))
                * (0.55 + 0.45 * sin(TIME * 0.55 + r * 4.0));

    // ── 合成 ──────────────────────────────────────────────────
    vec3 col = mix(abyss.rgb, abyss.rgb * 0.55, UV.y);
    col += mix(violet.rgb, deepblue.rgb, 0.5 + 0.5 * swirl) * surge * 0.50 * surge_gain;
    col += glowblue.rgb * cracks * 0.80 * crack_gain;
    col += mix(glowblue.rgb, vec3(0.85, 0.95, 1.0), 0.4) * eye * 0.50 * eye_gain;
    col += glowblue.rgb * eye_ring * 0.45 * eye_gain;
    col += glowblue.rgb * edge * 0.75 * edge_gain;
    col += violet.rgb * halo * 0.35 * edge_gain;

    COLOR = vec4(col, cover);
}
