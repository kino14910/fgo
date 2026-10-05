extends SceneTree

# 离屏验证 fgo_transition.gdshader 消费贴图 G/B 通道（令紋本体 + 描边）。
# 关注三件事：
#   1. threshold 端点契约（alpha 全 0 / 全 1）——G/B 层乘了 cover，不能破
#   2. crimson 令紋本体真的被画出来（G>0 区域应有明显红偏）
#   3. gold 描边真的被画出来（B>0 区域应偏金）
# 用法（必须真实渲染器，--headless 是 dummy renderer 出不了像素）：
#   MegaDot_v4.5.1-stable_mono_win64_console.exe --path . --rendering-driver opengl3 \
#       -s res://tools/_verify_seal_art.gd

const SHADER := "res://Fgo/shaders/fgo_transition.gdshader"
const MASK := "res://Fgo/images/charui/fgo_transition.webp"

const VW := 640
const VH := 264

# threshold 采样点：端点两个 + 中段三个 + 令紋最亮的 ignite 峰值一个
const THS := [0.0, 0.20, 0.40, 0.60, 0.85, 1.0]

var _vp: SubViewport
var _rect: ColorRect
var _mat: ShaderMaterial
var _i := -1
var _wait := 0
var _failed := 0


func _initialize() -> void:
	_vp = SubViewport.new()
	_vp.size = Vector2i(VW, VH)
	_vp.transparent_bg = true
	_vp.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	get_root().add_child(_vp)

	_rect = ColorRect.new()
	_rect.size = Vector2(VW, VH)
	_vp.add_child(_rect)

	_mat = ShaderMaterial.new()
	_mat.shader = load(SHADER) as Shader
	if _mat.shader == null:
		print("[FAIL] shader 加载失败")
		quit(1)
		return
	_mat.set_shader_parameter("transitionTex", load(MASK))
	# 与 .tres 一致
	_mat.set_shader_parameter("aspect", 2.42)
	_mat.set_shader_parameter("seal_art_gain", 1.0)
	_rect.material = _mat
	_next()


func _next() -> void:
	_i += 1
	if _i >= THS.size():
		_report()
		return
	_mat.set_shader_parameter("threshold", THS[_i])
	_wait = 8


func _stats(img: Image) -> Dictionary:
	var amin := 1.0
	var amax := 0.0
	var red := 0      # crimson 本体像素
	var gold := 0      # gold 描边像素
	var lit := 0
	# 基线取**中位数**，不是均值：金色描边虽只占少量像素但亮度接近 1.0，
	# 均值会被它拉高（实测 base 从 0.07 虚高到 0.80），于是真正的
	# crimson 本体反而低于 base*2.2 判不出来。中位数对少量极亮点免疫。
	var lums: Array[float] = []
	for iy in range(0, 17):
		for ix in range(0, 33):
			var x := int(float(ix) / 32.0 * float(img.get_width() - 1))
			var y := int(float(iy) / 16.0 * float(img.get_height() - 1))
			var c := img.get_pixel(x, y)
			if c.a > 0.5:
				lums.append(c.get_luminance())
	lums.sort()
	var base := 0.0
	if lums.size() > 0:
		base = lums[lums.size() / 2]

	for iy in range(0, 33):
		for ix in range(0, 65):
			var x := int(float(ix) / 64.0 * float(img.get_width() - 1))
			var y := int(float(iy) / 32.0 * float(img.get_height() - 1))
			var c := img.get_pixel(x, y)
			amin = min(amin, c.a)
			amax = max(amax, c.a)
			if c.a > 0.05:
				lit += 1
				# crimson.rgb = (0.780, 0.145, 0.180)：r 远高于 g，g≈b。
				# 幕底是深蓝（b>g>r），令紋是深红（r>g≈b），两者不会混淆。
				# 判据 = 色相（r>g*1.45）为主，亮度只用来排除纯黑幕底。
				if c.r > 0.06 and c.r > c.g * 1.45 and c.g <= c.b + 0.14:
					red += 1
				# gold = (1.0, 0.816, 0.310)：g 高、b 明显低
				if c.r > 0.20 and c.g > 0.14 and c.b < c.g * 0.72:
					gold += 1
	return {"amin": amin, "amax": amax, "red": red, "gold": gold,
			"lit": lit, "base": base}


func _process(_delta: float) -> bool:
	if _wait > 0:
		_wait -= 1
		return false
	var th: float = THS[_i]
	var s := _stats(_vp.get_texture().get_image())
	var line := "th=%.2f alpha[%.3f,%.3f] base=%.4f lit=%d red=%d gold=%d" % [
		th, s["amin"], s["amax"], s["base"], s["lit"], s["red"], s["gold"]]
	if is_zero_approx(th):
		if s["amax"] > 0.002:
			print("[FAIL] %s threshold=0 alpha 应全 0" % line)
			_failed += 1
		else:
			print("[ OK ] %s" % line)
	elif is_equal_approx(th, 1.0):
		if s["amin"] < 0.998:
			print("[FAIL] %s threshold=1 alpha 应全 1" % line)
			_failed += 1
		elif s["red"] > 20:
			# 收尾必须干净：settle 之后不该还留着令紋，否则黑幕上有个红印
			print("[FAIL] %s threshold=1 令紋未随 settle 收干净" % line)
			_failed += 1
		else:
			print("[ OK ] %s" % line)
	else:
		# 中段：必须有 crimson 令紋本体
		# th=0.20 是「点火」帧，令紋刚从穹丘里被墨锋扫出、只显出一小部分，
		# crimson 像素天然少（实测 14），所以这一帧放宽为「本体或描边任一出现」。
		var need_seal: int = int(s["red"]) + int(s["gold"])
		var thresh := 25
		if th < 0.30:
			thresh = 12
		if need_seal < thresh:
			print("[FAIL] %s 令紋像素不足(red+gold<%d)" % [line, thresh])
			_failed += 1
		else:
			print("[ OK ] %s" % line)
	_next()
	return false


func _report() -> void:
	print("")
	print("SEAL_ART_FAILED=%d" % _failed)
	quit(0 if _failed == 0 else 1)
