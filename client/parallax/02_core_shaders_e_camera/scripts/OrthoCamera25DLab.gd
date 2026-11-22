extends Node3D
class_name OrthoCamera25DLab

## Laboratório de Calibração 2.5D & Lentes Assimétricas (Módulo 02 + Módulo 03 — PARALLAX)
## Valida a Câmera Ortográfica 2.5D (3 faixas Z), os modelos 3D reais das 3 classes e 3 Auxiliares
## (.glb exportados do Blender 5.3) e os 3 Shaders de Olhar + Override Cego + Tremor Vermelho.

enum OlharAtivo {
	GOLIATH = 0,
	PHANTOM = 1,
	CIPHER = 2
}

const MENU_SCENE_PATH: String = "res://01_ui_menu_beta/scenes/MainMenuSalaDeMaquinas.tscn"

var olhar_atual: OlharAtivo = OlharAtivo.GOLIATH
var override_cego_ativo: bool = false
var leak_count: int = 1
var seismic_intensity: float = 0.25

@onready var beat_sequencer: BeatSequencer = $BeatSequencer
@onready var valve_thermal: ThermalNode = $PropsCalibracao/ValvulaMestraTeste/ThermalNode
@onready var modelos_root: Node3D = $ModelosModulo03
@onready var goliath_fx: ColorRect = $VisionHUD/GoliathThermalFX
@onready var phantom_fx: ColorRect = $VisionHUD/PhantomOpticFX
@onready var phantom_reticle: TextureRect = $VisionHUD/PhantomReticleOverlay
@onready var cipher_fx: ColorRect = $VisionHUD/CipherNetFX
@onready var override_terminal: Control = $VisionHUD/OverrideTerminalOverlay
@onready var override_leak_label: Label = $VisionHUD/OverrideTerminalOverlay/LeakWarningLabel
@onready var status_label: Label = $VisionHUD/DebugPanel/Margin/VBox/StatusLabel
@onready var telemetry_label: Label = $VisionHUD/DebugPanel/Margin/VBox/TelemetryLabel


func _ready() -> void:
	if beat_sequencer:
		beat_sequencer.beat_pulse.connect(_on_beat_pulse)
	if valve_thermal:
		valve_thermal.current_kelvin = 620.0
	_iniciar_animacoes_modulo03()
	_aplicar_olhar()


func _iniciar_animacoes_modulo03() -> void:
	if not modelos_root:
		return
	var players: Array[Node] = modelos_root.find_children("*", "AnimationPlayer", true, false)
	for node: Node in players:
		var ap: AnimationPlayer = node as AnimationPlayer
		if not ap:
			continue
		var anims: PackedStringArray = ap.get_animation_list()
		if anims.is_empty():
			continue
		var chosen: String = anims[0]
		for pref: String in ["idle_heavy", "idle_stealth", "idle_hack", "idle_motor", "hover_scan"]:
			if pref in anims:
				chosen = pref
				break
		var anim_res: Animation = ap.get_animation(chosen)
		if anim_res:
			anim_res.loop_mode = Animation.LOOP_LINEAR
		ap.play(chosen)


func _process(delta: float) -> void:
	seismic_intensity = move_toward(seismic_intensity, 0.18, delta * 1.4)
	if goliath_fx and goliath_fx.material is ShaderMaterial:
		(goliath_fx.material as ShaderMaterial).set_shader_parameter("seismic_pulse_intensity", seismic_intensity)

	if telemetry_label and valve_thermal:
		telemetry_label.text = (
			"VÁLVULA: %.1f K (%.0f%%)  |  VAZAMENTO ÔMEGA: %d/5  |  OVERRIDE CEGO: %s  |  PULSO BATIDA: %.2f"
			% [
				valve_thermal.current_kelvin,
				valve_thermal.get_normalized_heat() * 100.0,
				leak_count,
				"ATIVO" if override_cego_ativo else "INATIVO",
				seismic_intensity
			]
		)


func _input(event: InputEvent) -> void:
	if not (event is InputEventKey and event.pressed):
		return
	var key_event: InputEventKey = event as InputEventKey
	match key_event.keycode:
		KEY_1:
			olhar_atual = OlharAtivo.GOLIATH
			override_cego_ativo = false
			_aplicar_olhar()
		KEY_2:
			olhar_atual = OlharAtivo.PHANTOM
			override_cego_ativo = false
			_aplicar_olhar()
		KEY_3:
			olhar_atual = OlharAtivo.CIPHER
			_aplicar_olhar()
		KEY_O:
			olhar_atual = OlharAtivo.CIPHER
			override_cego_ativo = not override_cego_ativo
			_aplicar_olhar()
		KEY_L:
			leak_count = (leak_count + 1) % 6
			_aplicar_olhar()
		KEY_B:
			if beat_sequencer:
				beat_sequencer.trigger_manual_strike()
		KEY_H:
			if valve_thermal:
				valve_thermal.apply_heat(180.0)
		KEY_C:
			if valve_thermal:
				valve_thermal.apply_cryo(420.0)
		KEY_ESCAPE:
			if ResourceLoader.exists(MENU_SCENE_PATH):
				get_tree().change_scene_to_file(MENU_SCENE_PATH)


func _on_beat_pulse(_step_index: int, is_accent: bool) -> void:
	if is_accent:
		seismic_intensity = 0.92


func _aplicar_olhar() -> void:
	goliath_fx.visible = (olhar_atual == OlharAtivo.GOLIATH)
	phantom_fx.visible = (olhar_atual == OlharAtivo.PHANTOM)
	phantom_reticle.visible = (olhar_atual == OlharAtivo.PHANTOM)
	cipher_fx.visible = (olhar_atual == OlharAtivo.CIPHER)
	override_terminal.visible = (olhar_atual == OlharAtivo.CIPHER and override_cego_ativo)

	if cipher_fx and cipher_fx.material is ShaderMaterial:
		(cipher_fx.material as ShaderMaterial).set_shader_parameter("leak_count", leak_count)

	if override_leak_label:
		override_leak_label.text = "VAZAMENTO DETECTADO: %d/5  |  SINAL DETECTADO: FONTE P — 26h56m" % leak_count
		override_leak_label.modulate = Color(1.0, 0.15, 0.25) if leak_count >= 3 else Color(0.8, 0.65, 1.0)

	match olhar_atual:
		OlharAtivo.GOLIATH:
			status_label.text = "LENTE ATIVA [1]: OLHAR DO GOLIATH (LUT TÉRMICO + PULSO DA BATIDA + MODELOS 3D REAIS)"
			status_label.modulate = Color(1.0, 0.45, 0.2)
		OlharAtivo.PHANTOM:
			status_label.text = "LENTE ATIVA [2]: OLHAR DO PHANTOM (SOBEL CIANO + REFRAÇÃO N2 + RETÍCULA VERTICAL)"
			status_label.modulate = Color(0.1, 0.92, 1.0)
		OlharAtivo.CIPHER:
			var alerta: String = " [TREMOR VERMELHO ÔMEGA!]" if leak_count >= 3 else ""
			status_label.text = "LENTE ATIVA [3]: OLHAR DO CIPHER (PELE DE REDE + OVERRIDE CEGO [O])" + alerta
			status_label.modulate = Color(1.0, 0.2, 0.35) if leak_count >= 3 else Color(0.76, 0.58, 1.0)
