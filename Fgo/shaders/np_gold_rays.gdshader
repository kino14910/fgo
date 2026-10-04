shader_type canvas_item;
render_mode blend_add, unshaded;

// NP 按钮金色光效层。全部在片元里解析生成，不依赖任何贴图。
//
// 由内到外的层次（叠加顺序即此顺序）：
//   core    光核       正中心最亮，接近白色
//   halo    辉光       大范围柔和衰减，承接光核
//   streak  横向拉丝   极扁的横向亮线，强化"中心最亮"的镜头感
//   rays    径向光芒   极坐标切格得到的长短参差光束
//   stars   星星粒子   极坐标环带上的四角星，从光心持续向外扩散
//
// 关键约定：所有 uniform 都由 FgoNpBar 每帧覆写（Scripts/UI/FgoNpBar.cs 的 NpGlowTuning），
// 改数值下一帧即生效，不需要重载场景。
//
// 注意: TAU 是 Godot 着色语言内置常量，不能再自己 const 一遍，否则报 "Redefinition of 'TAU'"。

// ── 坐标系 ─────────────────────────────────────────────────────────────
// 光心在 UV 里的位置。FgoNpBar 把光效层对齐到按钮中心，所以默认就是 rect 正中。
uniform vec2  origin    = vec2(0.5, 0.5);
uniform float aspect    = 1.30;   // 控件宽/高，把 UV 还原成等比空间，避免光斑被拉扁
uniform float intensity = 1.0;    // 总强度，代码用它做淡入淡出与状态调制（可 > 1）

// ── 颜色（外缘深金 → 辉光金 → 中心近白）────────────────────────────────
uniform vec4 ray_color   : source_color = vec4(1.000, 0.700, 0.210, 1.0);
uniform vec4 glow_color  : source_color = vec4(1.000, 0.780, 0.300, 1.0);
uniform vec4 core_color  : source_color = vec4(1.000, 0.965, 0.855, 1.0);
uniform vec4 spark_color : source_color = vec4(1.000, 0.930, 0.700, 1.0);

// ── 呼吸（脉动）────────────────────────────────────────────────────────
// 整层强度做一次缓慢正弦起伏，周期 = 1 / breathe_speed 秒。
uniform float breathe_speed  = 0.30;
uniform float breathe_amount = 0.18;

// ── 光核 / 辉光 / 拉丝 ────────────────────────────────────────────────
uniform float core_size    = 0.070;  // 光核半径（等比空间）
uniform float core_soft    = 2.20;   // 光核边缘锐度，越大越"实"
uniform float core_gain    = 1.00;
uniform float halo_size    = 0.26;   // 辉光半径
uniform float halo_soft    = 2.80;
uniform float halo_gain    = 0.55;
uniform float streak_gain  = 0.30;   // 横向拉丝强度，0 = 关闭

// ── 径向光芒 ──────────────────────────────────────────────────────────
uniform float ray_count   = 30.0;    // 光束数量
uniform float ray_width   = 0.30;    // 单束角宽（0..0.5）
uniform float ray_sharp   = 1.60;    // 边缘硬度
uniform float ray_reach   = 0.30;   // 光芒长度（等比空间）
uniform float ray_falloff = 1.50;    // 沿半径衰减指数
uniform float ray_gain    = 0.42;
uniform float ray_shimmer = 0.20;    // 每束独立闪烁幅度

// ── 星星粒子 ──────────────────────────────────────────────────────────
uniform float star_sectors = 22.0;   // 角向分格数（× 径向环数 = 粒子槽位上限）
uniform float star_rings   = 7.0;    // 径向环数
uniform float star_density = 0.42;   // 槽位点亮比例，0..1（"密度"主控）
uniform float star_size    = 0.34;   // 星星占所在格子的比例（"尺寸"主控）
uniform float star_arm     = 2.60;   // 星芒长度 / 星核半径
uniform float star_sharp   = 1.50;   // 星芒对比度
uniform float star_reach   = 0.30;   // 粒子活动半径（等比空间）
uniform float star_inner   = 0.05;   // 内圈淡出半径，避免中心糊成一坨
uniform float star_speed   = 0.30;   // 扩散速度（格/秒）
uniform float star_gain    = 0.55;
uniform float star_twinkle = 0.45;   // 闪烁幅度
uniform float star_dim     = 0.70;   // 径向变暗混合量 0..1：0=不衰减，1=向外明显变暗

// 无参数 1D 哈希：给径向光芒的每束光一个稳定随机种子。
float hash_1(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

// 2D 哈希：给极坐标格子 (ai, ri) 里的星星一组稳定随机数。
float hash_f(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

// 四角星（十字星芒 + 圆核）。d = 片元到星心的等比空间偏移。
// 返回值已归一化到峰值 1，尺寸完全由 size 控制，便于统一调亮度。
float sparkle(vec2 d, float size, float arm, float sharp) {
    float s2 = size * size;
    float ax = 2.0 * s2 * arm * arm;   // 沿星芒方向的方差（拉长）
    float ay = 2.0 * s2 * 0.10;        // 垂直星芒方向的方差（决定星芒粗细）
    float gx = exp(-(d.x * d.x) / ax - (d.y * d.y) / ay);
    float gy = exp(-(d.y * d.y) / ax - (d.x * d.x) / ay);
    float g0 = exp(-dot(d, d) / (2.0 * s2 * 0.22));
    return pow(max(max(gx, gy), g0), sharp);
}

// 单个极坐标槽位（角向格 ai × 径向环 ri）里的那一颗星。
// 星星在本格内从内缘游走到外缘，亮度在抵达外缘前已归零再回到内缘，所以看不到跳变。
float starCell(float ai, float ri, vec2 p, float cell_r, float cell_a) {
    float ida = ai * 57.0;
    float idr = ri * 91.0;

    float h1 = hash_f(vec2(ida, idr));               // 点亮判定
    if (h1 > star_density) {
        return 0.0;
    }

    float h2 = hash_f(vec2(ida + 13.0, idr + 7.0));   // 速度 / 尺寸差异
    float h3 = hash_f(vec2(ida + 29.0, idr + 41.0));  // 生命周期相位
    float h4 = hash_f(vec2(ida + 71.0, idr + 53.0));  // 格内角向位置

    float life = fract(TIME * star_speed * (0.55 + 0.90 * h2) + h3);
    float rad = (ri + 0.22 + 0.56 * life) * cell_r;

    // 角向必须与 fragment() 里 ai 的取格方式严格同约定。
    // ai = floor((ang / TAU + 0.5) * sectors) 是"以 -π 为起点"的编号，
    // 因此还原成角度时要减掉半个圆（sectors * 0.5 * cell_a == π）。
    // 早前漏了这个偏移，星星被画在所属格的对面（差 180°），
    // 3×3 邻域自然找不到它 —— 表现为整片只剩中心零星几点、亮像素数为 0。
    float ang = (ai - star_sectors * 0.5 + 0.18 + 0.64 * h4) * cell_a;

    // 尺寸基准：径向格宽为主，角向只做「轻微」收窄。
    // 不能直接用 min(cell_r, rad * cell_a) —— 极坐标的角向格宽在光心处趋近 0，
    // 会把内侧星星压成亚像素而完全看不见（实测亮像素数为 0）。
    // 这里给角向项加一个 cell_r 的下限，最多只收窄到一半。
    float basis = min(cell_r, rad * cell_a * 0.5 + cell_r * 0.5);
    float size = star_size * basis * mix(0.55, 1.0, h2);

    vec2 d = p - vec2(cos(ang), sin(ang)) * rad;

    // 包围球剔除：绝大多数槽位在这一步就返回 0，不进 exp。
    float bound = size * star_arm * 1.8;
    if (dot(d, d) > bound * bound) {
        return 0.0;
    }

    float env = smoothstep(0.0, 0.16, life) * (1.0 - smoothstep(0.58, 1.0, life));
    float twinkle = 1.0 - star_twinkle * (0.5 + 0.5 * sin(TIME * 6.0 + h3 * TAU + h1 * 9.0));

    // 径向变暗：用 sqrt 而非直接幂，保证外圈仍在衰减而不是被压成 0。
    // pow(1-t, 1.5) 在 t>0.7 时已经很小，星星会集体消失；sqrt 在 t=1 时才归零。
    float dim = sqrt(clamp(1.0 - rad / max(star_reach, 1e-4), 0.0, 1.0));
    dim = mix(1.0, dim, star_dim);
    float inner = smoothstep(star_inner * 0.4, star_inner, rad);

    return sparkle(d, size, star_arm, star_sharp) * env * twinkle * dim * inner;
}

void fragment() {
    // 进入时 COLOR = modulate × ColorRect 的 color；取出来在最后回乘，保证外部还能用 modulate 压暗。
    vec4 tint = COLOR;

    vec2 p = (UV - origin) * vec2(aspect, 1.0);
    float r = max(length(p), 1e-5);
    float ang = atan(p.y, p.x);

    // 呼吸：1 → 1-amount → 1，周期 1/breathe_speed 秒。
    float breath = 1.0 - breathe_amount * (0.5 - 0.5 * cos(TIME * TAU * breathe_speed));

    // ── 光核 / 辉光 / 拉丝 ────────────────────────────────────────────
    float core = pow(clamp(1.0 - r / max(core_size, 1e-4), 0.0, 1.0), core_soft) * core_gain;
    float halo = pow(clamp(1.0 - r / max(halo_size, 1e-4), 0.0, 1.0), halo_soft) * halo_gain;
    // 拉丝横向长度跟随 halo_size（略长一点），避免调小辉光时还留一条超长的横线。
    float streak = streak_gain
                 * pow(clamp(1.0 - abs(p.y) / (core_size * 0.34), 0.0, 1.0), 2.0)
                 * pow(clamp(1.0 - abs(p.x) / (aspect * halo_size * 1.15), 0.0, 1.0), 1.5);

    // ── 径向光芒：极坐标下按角度切格，每格一束 ────────────────────────
    float k = ang / TAU * ray_count;
    float cell = floor(k);
    float local = fract(k) - 0.5;                 // 0 = 光束中心，±0.5 = 相邻光束之间
    float seed = hash_1(cell + 7.13);

    float beam = 1.0 - smoothstep(0.0, ray_width, abs(local));
    beam = pow(beam, ray_sharp);

    float ray_len = mix(0.45, 1.0, seed);         // 每束长短参差
    float radial = pow(clamp(1.0 - r / max(ray_reach * ray_len, 1e-4), 0.0, 1.0), ray_falloff);
    float twinkle = 1.0 - ray_shimmer * (0.5 + 0.5 * sin(TIME * 2.1 + seed * 61.7));
    float rays = beam * radial * mix(0.55, 1.0, seed) * twinkle * ray_gain;

    // ── 星星粒子：只需求值 3×3 邻域（星芒可跨格，故不能只取本格）────────
    float cell_a = TAU / max(star_sectors, 1.0);
    float cell_r = star_reach / max(star_rings, 1.0);
    float ri = floor(clamp(r / max(star_reach, 1e-4), 0.0, 0.99999) * star_rings);
    float ai = floor((ang / TAU + 0.5) * star_sectors);

    float stars = 0.0;
    for (int j = -1; j <= 1; j++) {
        float rr = ri + float(j);
        if (rr < 0.0 || rr >= star_rings) {
            continue;
        }
        for (int i = -1; i <= 1; i++) {
            // 角向首尾相接：mod 保证跨 0/TAU 接缝处不漏星、不重星。
            float aa = mod(ai + float(i), max(star_sectors, 1.0));
            stars += starCell(aa, rr, p, cell_r, cell_a);
        }
    }
    stars *= star_gain;

    // ── 合成：外缘深金 → 越靠中心越白 ──────────────────────────────────
    float lum = core + halo + streak + rays + stars;
    // heat 越高越接近"白热"，这是"中心最亮"的关键。
    float heat = clamp(core * 1.20 + stars * 0.90 + streak * 0.50, 0.0, 1.0);

    vec3 warm = ray_color.rgb * (rays + stars)
              + glow_color.rgb * (halo + streak);
    vec3 col = mix(warm, core_color.rgb * lum, heat * 0.80);

    col *= breath * intensity;

    // blend_add 的实际贡献 = COLOR.rgb * COLOR.a。
    //
    // alpha 这里只做"整层开关/淡入淡出"，亮度一律由 rgb 表达。
    // 早前把 lum 塞进 alpha 是个陷阱：贡献会变成 col × lum，对本就稀疏的星星
    // 相当于把亮度平方（0.31 → 0.096），视觉上直接消失。实测 stars 层亮像素数为 0。
    COLOR = vec4(col * tint.rgb, tint.a);
}
