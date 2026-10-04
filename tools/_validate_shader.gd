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
const MASK_TEX := "res://Fgo/images/charui/fgo_transition.webp"

var _vp: SubViewport
var _rect: ColorRect
var _mat: ShaderMaterial
var _steps: Array = []
var _i := -1
var _wait := 0
var _failed := 0


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


func _next() -> void:
	_i += 1
	if _i >= _steps.size():
		_report()
		return
	var shader := load(_steps[_i][0]) as Shader
	# 每次重建材质，确保参数从干净状态出发
	_mat = ShaderMaterial.new()
	_mat.shader = shader
	_mat.set_shader_parameter("transitionTex", load(MASK_TEX))
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


func _process(_delta: float) -> bool:
	if _wait > 0:
		_wait -= 1
		return false
	var path: String = _steps[_i][0]
	var th: float = _steps[_i][1]
	var r := _alpha_range()
	if is_zero_approx(th) and r.y > 0.002:
		print("[FAIL] %s threshold=0 时 alpha 应全 0，实测 max=%.4f" % [path, r.y])
		_failed += 1
	elif is_equal_approx(th, 1.0) and r.x < 0.998:
		print("[FAIL] %s threshold=1 时 alpha 应全 1，实测 min=%.4f" % [path, r.x])
		_failed += 1
	else:
		print("[ OK ] %s threshold=%.2f -> alpha min=%.4f max=%.4f" % [path, th, r.x, r.y])
	_next()
	return false


func _report() -> void:
	print("")
	print("SHADER_VALIDATION_FAILED=%d" % _failed)
	quit(0 if _failed == 0 else 1)
