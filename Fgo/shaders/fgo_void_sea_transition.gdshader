shader_type canvas_item;
render_mode blend_mix, unshaded;

// ════════════════════════════════════════════════════════════════════
//  〔虚数之海〕进入转场遮罩
//  与 fgo_transition.tres 同接口：同样是 ShaderMaterial + threshold。
//  由 FgoVoidSeaLayer 场景内的 AnimationPlayer 驱动 threshold 1 → 0，
//  于是遮罩从「完全覆盖」溶解消散，露出底下的虚数之海。
//
//  契约：
//    uniform sampler2D transitionTex;  推进遮罩的 falloff 场
//    uniform float     threshold;      1 = 完全覆盖，0 = 完全透明
//  falloff 做了噪波扰动，必须 clamp 回 [0,1] 才能保证端点不漏色。
// ════════════════════════════════════════════════════════════════════

uniform sampler2D transitionTex;
uniform float threshold : hint_range(0,1);

// ── 画面比例（虚数之海图层约 2881×1350 ⇒ 2.13）────────────────
uniform float aspect : hint_range(0.5,4.0) = 2.13;

// ── 阴森配色：幽蓝 / 暗紫 / 深渊黑 ─────────────────────────────
uniform vec4 abyss   : source_color = vec4(0.043, 0.031, 0.078, 1.0); // 深渊底
uniform vec4 deepblue: source_color = vec4(0.055, 0.176, 0.361, 1.0); // 幽蓝
uniform vec4 violet  : source_color = vec4(0.271, 0.106, 0.443, 1.0); // 暗紫
uniform vec4 glowblue: source_color = vec4(0.278, 0.639, 0.949, 1.0); // 裂隙冷光

// ── 分量强度 ─────────────────────────────────────────────────
uniform float crack_gain  : hint_range(0.0,2.0) = 1.00; // 裂纹
uniform float surge_gain  : hint_range(0.0,2.0) = 1.00; // 深渊涌动
uniform float eye_gain    : hint_range(0.0,2.0) = 1.00; // 深渊之眼
uniform float noise_gain  : hint_range(0.0,2.0) = 1.00; // 侵蚀噪波

// ── 侵蚀强度：falloff 被噪波撕开的幅度 ───────────────────────
uniform float erode : hint_range(0.0,0.5) = 0.16;
uniform float noise_scale : hint_range(0.5,8.0) = 2.4;

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

float fbm(vec2 p) {
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 5; i++) {
        v += a * vnoise(p);
        p = p * 2.03 + vec2(17.1, 9.7);
        a *= 0.5;
    }
    return v;
}

// 脊状噪波：形成裂纹的骨架
float ridged(vec2 p) {
    float n = fbm(p);
    return 1.0 - abs(n * 2.0 - 1.0);
}

void fragment() {
    float th = clamp(threshold, 0.0, 1.0);

    vec2 p = (UV - vec2(0.5)) * vec2(aspect, 1.0);
    float r = length(p);
    float ang = atan(p.y, p.x);

    // ── 侵蚀噪波：域扭曲让边缘像被撕咬 ────────────────────
    vec2 q = p * noise_scale;
    float warp = fbm(q + vec2(TIME * 0.05, -TIME * 0.04));
    float n = fbm(q + vec2(warp * 1.6, warp * -1.1) + vec2(TIME * 0.03, 0.0));

    // ── 推进遮罩：原接口 + 噪波侵蚀，clamp 保端点 ──────────
    float falloff = 1.0 - texture(transitionTex, UV).r;
    falloff = clamp(falloff + (n - 0.5) * erode * noise_gain, 0.0, 1.0);
    float remap = mix(-0.1, 1.1, th);
    float cover = step(falloff, remap);

    // ── 深渊涌动：反向旋转的浓雾涌动 ──────────────────────
    float swirl = fbm(p * 1.7 + vec2(-TIME * 0.07, TIME * 0.05));
    float surge = fbm(p * 2.6 + vec2(swirl * 2.2, -TIME * 0.09));
    surge = smoothstep(0.35, 0.95, surge) * (0.55 + 0.45 * sin(TIME * 0.9 + r * 5.0));

    // ── 裂纹：脊状噪波取高次幂成细线 ──────────────────────
    float ck = ridged(q * 1.3 + vec2(TIME * 0.02, TIME * 0.015));
    ck = pow(clamp(ck, 0.0, 1.0), 9.0);
    float ck2 = ridged(q * 2.7 - vec2(TIME * 0.03, 0.0));
    ck2 = pow(clamp(ck2, 0.0, 1.0), 16.0) * 0.6;
    float cracks = (ck + ck2) * exp(-r * 0.9);

    // ── 深渊之眼：中心冷光核，随阈值收缩 ──────────────────
    float eye_r = 0.16 + 0.10 * (1.0 - th);
    float eye = exp(-pow(r / eye_r, 2.0));
    float eye_ring = 1.0 - smoothstep(0.0, 0.012, abs(r - eye_r * 1.35));

    // ── 边缘幽蓝描边：贴着推进锋面 ────────────────────────
    float edge = 1.0 - smoothstep(0.0, 0.07, abs(falloff - remap));
    float halo = exp(-abs(falloff - remap) * 7.0);

    // ── 合成 ──────────────────────────────────────────────
    vec3 col = abyss.rgb;
    // 纵向深度：上暗下更沉
    col = mix(col, abyss.rgb * 0.55, UV.y);
    // 涌动雾
    col += mix(violet.rgb, deepblue.rgb, 0.5 + 0.5 * swirl) * surge * 0.55 * surge_gain;
    // 裂纹冷光
    col += glowblue.rgb * cracks * 0.85 * crack_gain;
    // 之眼
    col += mix(glowblue.rgb, vec3(0.85, 0.95, 1.0), 0.4) * eye * 0.55 * eye_gain;
    col += glowblue.rgb * eye_ring * 0.50 * eye_gain;
    // 锋面
    col += glowblue.rgb * edge * 0.70;
    col += violet.rgb * halo * 0.35;

    // alpha 必须严格等于 cover：threshold=1 ⇒ 全 1（完全遮蔽），threshold=0 ⇒ 全 0。
    // 入场的淡入淡出交给图层自身的 modulate 交叉淡入，不要在这里再乘任何系数——
    // 早先版本用 (0.72 + 0.85*glow) 提亮发光处，结果 threshold=1 时 alpha 只能到 0.72，
    // 端点契约直接破了（实测 min=0.7216）。发光强度全部放在 rgb 上表达。
    COLOR = vec4(col, cover);
}
