extends SceneTree

# 校验转场 shader：真实编译 + 端点契约。
#
# 用法（必须带真实渲染器，--headless 是 dummy renderer，出不了像素也测不了端点）：
#   MegaDot_v4.5.1-stable_mono_win64_console.exe --path . --rendering-driver opengl3 \
#       -s res://tools/_validate_shader.gd -- <shader路径...>
#
# 端点契约来自 NTransition：threshold=0 ⇒ alpha 全 0；threshold=1 ⇒ alpha 全 1。
# falloff 被噪波扰动后必须 clamp 回 [0,1]，否则端点会漏色。

const DEFAULT_TARGETS := [
	"res://Fgo/shaders/fgo_transition.gdshader",
	"res://Fgo/shaders/fgo_void_sea_transition.gdshader",
]

const REQUIRED_UNIFORMS := ["transitionTex", "threshold"]

# 每个 shader 用哪张溶解图，与各自材质里的 transitionTex 保持一致。
# 加载失败必须直接判失败 —— 曾经因为源图丢失导致 load() 返回 null，
# shader 采到默认白，threshold=0.5 也整屏不透明，却照样「通过」了。
const MASK_TEX := {
	"res://Fgo/shaders/fgo_transition.gdshader":
		"res://Fgo/images/charui/fgo_transition.webp",
	"res://Fgo/shaders/fgo_void_sea_transition.gdshader":
		"res://Fgo/images/transitions/void_sea_dissolve.png",
}

var _vp: SubViewport
var _rect: ColorRect
var _mat: ShaderMaterial
var _steps: Array = []
var _i := -1
var _wait := 0
var _failed := 0
var _dir := {}


func _initialize() -> void:
	_vp = SubViewport.new()
	_vp.size = Vector2i(640, 264)
	_vp.transparent_bg = true
	_vp.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(_vp)

	_rect = ColorRect.new()
	_rect.size = Vector2(640, 264)
	_vp.add_child(_rect)

	var args := OS.get_cmdline_user_args()
	var targets: Array = args if args.size() > 0 else DEFAULT_TARGETS

	for path in targets:
		_check_uniforms(path)
		_queue_endpoint_tests(path)

	_next()


func _check_uniforms(path: String) -> void:
	var shader := load(path) as Shader
	if shader == null:
		print("[FAIL] %s 无法加载" % path)
		_failed += 1
		return
	var names: Array = []
	for u in shader.get_shader_uniform_list():
		names.append(u["name"])
	for req in REQUIRED_UNIFORMS:
		if not names.has(req):
			print("[FAIL] %s 缺少必需 uniform: %s" % [path, req])
			_failed += 1
	print("[ OK ] %s 编译通过，%d 个 uniform" % [path, names.size()])


func _queue_endpoint_tests(path: String) -> void:
	for th in [0.0, 0.5, 1.0]:
		_steps.append([path, th])
	# 方向档：threshold 由高到低时，应先揭开遮罩图「falloff 大」的一侧。
	_steps.append([path, 0.8, "dir"])
	_steps.append([path, 0.2, "dir"])


func _next() -> void:
	_i += 1
	if _i >= _steps.size():
		_report()
		return
	var shader := load(_steps[_i][0]) as Shader
	var tex_path: String = MASK_TEX.get(_steps[_i][0], "")
	var tex := load(tex_path) as Texture2D
	if tex == null:
		print("[FAIL] %s 的溶解遮罩图加载失败: %s" % [_steps[_i][0], tex_path])
		_failed += 1
		return
	# 每次重建材质，确保参数从干净状态出发
	_mat = ShaderMaterial.new()
	_mat.shader = shader
	_mat.set_shader_parameter("transitionTex", tex)
	_rect.material = _mat
	_mat.set_shader_parameter("threshold", _steps[_i][1])
	_wait = 8


func _alpha_range() -> Vector2:
	var img := _vp.get_texture().get_image()
	var mn := 1.0
	var mx := 0.0
	for iy in range(0, 17):
		for ix in range(0, 33):
			var x := int(float(ix) / 32.0 * float(img.get_width() - 1))
			var y := int(float(iy) / 16.0 * float(img.get_height() - 1))
			var a := img.get_pixel(x, y).a
			mn = min(mn, a)
			mx = max(mx, a)
	return Vector2(mn, mx)


# 左右各 1/3 的平均 alpha：用于判断溶解方向
func _alpha_sides() -> Vector2:
	var img := _vp.get_texture().get_image()
	var w := img.get_width()
	var h := img.get_height()
	var l := 0.0
	var r := 0.0
	var ln := 0
	var rn := 0
	for iy in range(0, 17):
		var y := int(float(iy) / 16.0 * float(h - 1))
		for ix in range(0, 9):
			var x := int(float(ix) / 8.0 * float(w - 1))
			l += img.get_pixel(x, y).a
			ln += 1
		for ix in range(24, 33):
			var x2 := int(float(ix) / 32.0 * float(w - 1))
			r += img.get_pixel(x2, y).a
			rn += 1
	return Vector2(l / float(ln), r / float(rn))


func _process(_delta: float) -> bool:
	if _wait > 0:
		_wait -= 1
		return false
	var path: String = _steps[_i][0]
	var th: float = _steps[_i][1]
	var is_dir: bool = _steps[_i].size() > 2
	var r := _alpha_range()

	if is_dir:
		var s := _alpha_sides()
		_dir[path] = _dir.get(path, {}) as Dictionary
		(_dir[path] as Dictionary)[str(th)] = s
		print("[ .. ] %s threshold=%.2f -> 左=%.4f 右=%.4f（方向采样）" % [path, th, s.x, s.y])
	else:
		if is_zero_approx(th) and r.y > 0.002:
			print("[FAIL] %s threshold=0 时 alpha 应全 0，实测 max=%.4f" % [path, r.y])
			_failed += 1
		elif is_equal_approx(th, 1.0) and r.x < 0.998:
			print("[FAIL] %s threshold=1 时 alpha 应全 1，实测 min=%.4f" % [path, r.x])
			_failed += 1
		elif th > 0.0 and th < 1.0 and (r.y - r.x) < 0.5:
			print("[FAIL] %s threshold=%.2f 应为部分覆盖，实测 min=%.4f max=%.4f（全屏同值 = 遮罩图没生效？）"
				% [path, th, r.x, r.y])
			_failed += 1
		else:
			print("[ OK ] %s threshold=%.2f -> alpha min=%.4f max=%.4f" % [path, th, r.x, r.y])
	_next()
	return false


func _check_direction(path: String) -> void:
	var d: Dictionary = _dir.get(path, {})
	if not d.has("0.8") or not d.has("0.2"):
		return
	var hi: Vector2 = d["0.8"]     # 遮罩仍多
	var lo: Vector2 = d["0.2"]     # 已被揭开较多
	var dl := hi.x - lo.x
	var dr := hi.y - lo.y
	# 虚数之海遮罩图是「左暗右亮」⇒ falloff 左大右小 ⇒ 左侧先被揭开。
	# 这也正是原版角色选人转场的推进方向（左→右）取反后的镜像方向。
	if dl <= dr:
		print("[FAIL] %s 溶解方向反了：0.8→0.2 期间左仅降 %.4f、右降 %.4f，应左侧先揭开" % [path, dl, dr])
		_failed += 1
	else:
		print("[ OK ] %s 溶解方向正确（左→右揭开，左降 %.4f > 右降 %.4f）" % [path, dl, dr])


func _report() -> void:
	for path in _dir.keys():
		_check_direction(path)
	print("")
	print("SHADER_VALIDATION_FAILED=%d" % _failed)
	quit(0 if _failed == 0 else 1)
