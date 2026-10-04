shader_type canvas_item;
render_mode blend_mix, unshaded;

// ════════════════════════════════════════════════════════════════════
//  FGO 选角 → 进战斗 转场遮罩
//  令紋（Command Seal）/ 萨博・阿尔托莉雅 / 藤丸立香
//
//  契约（勿改名、勿删）：
//    uniform sampler2D transitionTex;  推进遮罩的 falloff 场
//    uniform float     threshold;      由 NTransition 在 0→1 补间
//  端点语义：threshold=0 → alpha 全 0；threshold=1 → alpha 全 1。
//  falloff 必须 clamp 回 [0,1]，否则噪波扰动会让端点漏色。
// ════════════════════════════════════════════════════════════════════

uniform sampler2D transitionTex;
uniform float threshold : hint_range(0,1);

// ── 画面比例 ────────────────────────────────────────────────
// 宿主 GameTransitionRect 是 ColorRect，约 3200×1320 ⇒ 2.42
uniform float aspect : hint_range(0.5,4.0) = 2.42;

// ── FGO 配色 ────────────────────────────────────────────────
uniform vec4 gold    : source_color = vec4(1.000, 0.816, 0.310, 1.0); // 阿尔托莉雅金 / Excalibur
uniform vec4 azure   : source_color = vec4(0.204, 0.451, 0.855, 1.0); // 迦勒底夜蓝
uniform vec4 crimson : source_color = vec4(0.780, 0.145, 0.180, 1.0); // 令紋 / 藤丸立香
uniform vec4 deep    : source_color = vec4(0.024, 0.028, 0.063, 1.0); // 幕底

// ── 分量强度（0 = 关闭该层）──────────────────────────────────
uniform float seal_gain     : hint_range(0.0,2.0) = 1.00; // 令紋
uniform float circle_gain   : hint_range(0.0,2.0) = 0.90; // 魔術迴路
uniform float beam_gain     : hint_range(0.0,2.0) = 1.00; // 圣剑之光 + 集中線
uniform float sparkle_gain  : hint_range(0.0,2.0) = 1.00; // 星芒
uniform float rim_gain      : hint_range(0.0,2.0) = 1.00; // 推进锋面

// ── 旋转速率（负数 = 反向）──────────────────────────────────
uniform float spin : hint_range(-2.0,2.0) = 1.0;

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

// 圆环：半径 rr、线宽 w
float ring_band(float r, float rr, float w) {
    return 1.0 - smoothstep(w, w + 0.0018, abs(r - rr));
}

// 圆弧带：半径 rr、角半宽 halfang、角中心 ac
float arc_band(float r, float ang, float rr, float w, float halfang, float ac) {
    float da = abs(ang - ac);
    da = min(da, TAU - da);
    return ring_band(r, rr, w) * (1.0 - smoothstep(halfang - 0.035, halfang, da));
}

// 等边三角形边：把角度按 120° 折叠，边即 x = R/2 的那条线段
float tri_edge(float r, float ang, float rot, float R, float w) {
    float k = TAU / 3.0;
    float a3 = mod(ang - rot + k * 0.5, k) - k * 0.5;
    float d = abs(cos(a3) * r - R * 0.5);
    return (1.0 - smoothstep(w, w + 0.0016, d)) * step(r, R * 1.02);
}

void fragment() {
    float th = clamp(threshold, 0.0, 1.0);

    // ── 原有推进逻辑：贴图红通道作 falloff ──────────────────
    float falloff = 1.0 - texture(transitionTex, UV).r;
    float remap = mix(-0.1, 1.1, th);
    float cover = step(falloff, remap);

    vec2 p = (UV - vec2(0.5)) * vec2(aspect, 1.0);
    float r = length(p);
    float ang = atan(p.y, p.x);

    // 点火 → 全屏推进 → 收尾只留黑幕
    float ignite = smoothstep(0.015, 0.26, th) * (1.0 - smoothstep(0.62, 0.96, th));
    float settle = 1.0 - smoothstep(0.72, 1.0, th);

    // ── 幕底：迦勒底夜色（上暗下泛蓝）─────────────────────
    vec3 col = mix(deep.rgb, deep.rgb * 0.55, UV.y);
    col += azure.rgb * 0.045 * (1.0 - UV.y);
    col += gold.rgb * 0.10 * exp(-r * 3.2) * ignite;

    // ── 魔術迴路（召唤阵）：双环 + 刻度 + 正反三角 ────────────
    float spin1 = TIME * 0.16 * spin;
    float spin2 = -TIME * 0.11 * spin;
    float circle = ring_band(r, 0.470, 0.0016) * 0.90;
    circle += ring_band(r, 0.430, 0.0008) * 0.50;
    circle += ring_band(r, 0.300, 0.0012) * 0.70;
    float tick_a = mod(ang + spin1, TAU / 48.0) - TAU / 96.0;
    float tick = (1.0 - smoothstep(0.0, 0.008, abs(tick_a)))
               * smoothstep(0.300, 0.316, r)
               * (1.0 - smoothstep(0.420, 0.432, r));
    circle += tick * 0.75;
    circle += tri_edge(r, ang, spin2, 0.400, 0.0016) * 0.85;
    circle += tri_edge(r, ang, spin2 + PI / 3.0, 0.400, 0.0016) * 0.85;
    circle *= ignite * circle_gain;
    col += mix(azure.rgb, gold.rgb, 0.35) * circle * 0.85;

    // ── 令紋（命令紋章）：外环 + 三臂弧 + 核 ────────────────
    float seal = ring_band(r, 0.150, 0.0022);
    float sa = spin2 * 0.6;
    seal += arc_band(r, ang, 0.098, 0.0026, 0.62, sa + PI * 0.5);
    seal += arc_band(r, ang, 0.098, 0.0026, 0.62, sa + PI * 0.5 + TAU / 3.0);
    seal += arc_band(r, ang, 0.098, 0.0026, 0.62, sa + PI * 0.5 + TAU * 2.0 / 3.0);
    seal += ring_band(r, 0.030, 0.0030) * 0.90;
    seal *= ignite * seal_gain;
    col += mix(crimson.rgb, gold.rgb, 0.30) * seal * 1.15;
    col += crimson.rgb * exp(-r * 9.0) * 0.30 * ignite * seal_gain;

    // ── 圣剑之光（Excalibur 竖直金柱）+ 集中線 ───────────────
    float beam_win = smoothstep(0.05, 0.34, th) * (1.0 - smoothstep(0.58, 0.95, th));
    float beam_w = 0.085 + 0.045 * smoothstep(0.0, 0.5, th);
    float beam = exp(-pow(abs(p.x) / beam_w, 1.7)) * (1.0 - smoothstep(0.0, 0.62, r));
    float core = exp(-pow(abs(p.x) / (beam_w * 0.28), 2.0)) * (1.0 - smoothstep(0.0, 0.50, r));
    col += gold.rgb * beam * 0.55 * beam_win * beam_gain;
    col += vec3(1.0, 0.965, 0.860) * core * 0.85 * beam_win * beam_gain;

    float sc = 84.0;
    float sf = fract(ang / TAU * sc) - 0.5;
    float ssd = hash11(floor(ang / TAU * sc) + 3.17);
    float spokes = pow(1.0 - smoothstep(0.0, 0.40, abs(sf)), 2.2)
                 * (0.30 + 0.70 * ssd)
                 * smoothstep(0.12, 0.55, r)
                 * (1.0 - smoothstep(0.55, 1.05, r));
    col += gold.rgb * spokes * 0.30 * beam_win * beam_gain;

    // ── 星芒 ───────────────────────────────────────────────
    vec2 g = UV * vec2(aspect, 1.0) * 26.0;
    vec2 gf = fract(g) - 0.5;
    float gh = hash11(dot(floor(g), vec2(12.9898, 78.233)) + 0.5);
    float tw = 0.5 + 0.5 * sin(TIME * 2.6 + gh * 43.0);
    float star = step(0.955, gh)
               * (1.0 - smoothstep(0.0, 0.30, length(gf)))
               * (0.35 + 0.65 * tw);
    col += mix(gold.rgb, vec3(1.0), 0.55) * star * ignite * sparkle_gain;

    // ── 推进锋面：金色热边（乘 cover，绝不越界）─────────────
    float rim = (1.0 - smoothstep(0.0, 0.055, abs(falloff - remap))) * cover;
    float rim_glow = exp(-abs(falloff - remap) * 9.0) * cover;
    vec3 rim_col = mix(gold.rgb, vec3(1.0, 0.980, 0.920), 0.55);
    col += rim_col * rim * 1.35 * rim_gain;
    col += gold.rgb * rim_glow * 0.30 * rim_gain;

    // 收尾压暗：threshold=1 时回到干净黑幕
    col = mix(col, deep.rgb * 0.60, settle);

    COLOR = vec4(col, cover);
}
