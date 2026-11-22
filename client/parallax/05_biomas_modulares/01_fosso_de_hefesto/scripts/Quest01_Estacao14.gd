extends Node3D
class_name Quest01Estacao14

## Quest 1 — Calor de Contrabando (Estação de Válvula 14 · Borda do Fosso de Hefesto · Face Voltada)
## Vertical Slice completo e interativo integrando:
## - 3 Zonas da Estação 14 (Casa de Caldeiras -> Câmara de Despacho -> Galeria de Sangria)
## - 3 Olhares Assimétricos ([1] Goliath, [2] Phantom na passarela alta, [3] Cipher + [O] Override Cego)
## - Objetivos Primários: Abrir Porta 10t -> Despachar Contêiner de Acumuladores -> Abrir Válvula 14 & Resistir às Esquadras
## - Objetivos Secundários & Plantas de Lore (lidos direto de res://data/lore/fragments.json):
##   * Fogo Mínimo (0 Linhas Vivas rompidas + LED amarelo de trava nos soldados do Sindicato)
##   * Manômetro 140% com fita amarela
##   * Relógio 23 ANOS riscado a prego & Etiqueta de Cota Familiar Souza-Adeodato
## - Ensino Therma-Tek: Superaquecimento da arma ([F]) + decaimento do Núcleo Térmico (ThermalNode.gd)

enum OlharAtivo {
	GOLIATH = 0,
	PHANTOM = 1,
	CIPHER = 2
}

const MENU_SCENE_PATH: String = "res://01_ui_menu_beta/scenes/MainMenuSalaDeMaquinas.tscn"
const FRAGMENTS_JSON_PATH: String = "res://data/lore/fragments.json"
const MOVE_SPEED: float = 6.8
const MIN_X: float = -16.5
const MAX_X: float = 24.5

var olhar_atual: OlharAtivo = OlharAtivo.GOLIATH
var override_cego_ativo: bool = false
var leak_count: int = 0
var seismic_intensity: float = 0.22

# Estado dos objetivos da Quest 1
var porta_10t_aberta: bool = false
var conteiner_despachado: bool = false
var valvula_14_aberta: bool = false
var fogo_minimo_intacto: bool = true
var manometro_lido: bool = false
var relogio_23_achado: bool = false
var etiqueta_lida: bool = false
var nucleo_coletado: bool = false

# Ensino Therma-Tek: superaquecimento e aquecimento prévio
var arma_calor: float = 18.0
var arma_travada_superaquecimento: bool = false
var nucleo_carga_pct: float = 100.0
var soldado_arma_travada_linha_viva: bool = true

var fragments_by_id: Dictionary = {}

@onready var ortho_camera: Camera3D = $OrthoCamera25D
@onready var beat_sequencer: BeatSequencer = $BeatSequencer
@onready var nucleo_thermal: ThermalNode = $Interativos/NucleoTermicoDrop/ThermalNode

@onready var goliath_node: Node3D = $Operadores/Goliath_3D
@onready var phantom_node: Node3D = $Operadores/Phantom_3D
@onready var cipher_node: Node3D = $Operadores/Cipher_3D
@onready var esquadra_node: Node3D = $Inimigos/EsquadraContencao_01

@onready var porta_10t_node: Node3D = $Interativos/PortaChumbo10t
@onready var conteiner_node: Node3D = $Interativos/ConteinerAcumuladores
@onready var valvula_14_node: Node3D = $Interativos/ValvulaMestra14
@onready var duto_sangria_node: Node3D = $Interativos/DutoSangria
@onready var nucleo_drop_node: Node3D = $Interativos/NucleoTermicoDrop

@onready var goliath_fx: ColorRect = $VisionHUD/GoliathThermalFX
@onready var phantom_fx: ColorRect = $VisionHUD/PhantomOpticFX
@onready var phantom_reticle: TextureRect = $VisionHUD/PhantomReticleOverlay
@onready var cipher_fx: ColorRect = $VisionHUD/CipherNetFX
@onready var override_terminal: Control = $VisionHUD/OverrideTerminalOverlay

@onready var zona_label: Label = $VisionHUD/TopHUD/Margin/VBox/ZonaLabel
@onready var objetivos_label: Label = $VisionHUD/TopHUD/Margin/VBox/ObjetivosLabel
@onready var prompt_label: Label = $VisionHUD/BottomHUD/Margin/VBox/PromptLabel
@onready var radio_log_label: Label = $VisionHUD/BottomHUD/Margin/VBox/RadioLogLabel
@onready var arma_calor_bar: ProgressBar = $VisionHUD/BottomHUD/Margin/VBox/MetersRow/ArmaCalorBar
@onready var nucleo_bar: ProgressBar = $VisionHUD/BottomHUD/Margin/VBox/MetersRow/NucleoCargaBar


func _ready() -> void:
	_carregar_fragmentos_daedalus()
	if beat_sequencer:
		beat_sequencer.beat_pulse.connect(_on_beat_pulse)
	if nucleo_thermal:
		nucleo_thermal.current_kelvin = 680.0
		nucleo_thermal.cooling_rate_k_per_sec = 4.5
	_iniciar_todas_animacoes()
	_aplicar_olhar()
	_atualizar_hud_objetivos()
	radio_log_label.text = "[CANDEIA · RÁDIO]: \"O Setor 7-Baixo faz três dias sem calor. Entra na Estação 14, pega o contêiner e joga no duto.\""


func _carregar_fragmentos_daedalus() -> void:
	if not FileAccess.file_exists(FRAGMENTS_JSON_PATH):
		return
	var raw: String = FileAccess.get_file_as_string(FRAGMENTS_JSON_PATH)
	var parsed: Variant = JSON.parse_string(raw)
	if parsed is Dictionary and parsed.has("fragments"):
		for frag: Variant in parsed["fragments"]:
			if frag is Dictionary and frag.has("id"):
				fragments_by_id[frag["id"]] = frag


func _iniciar_todas_animacoes() -> void:
	var players: Array[Node] = find_children("*", "AnimationPlayer", true, false)
	for node: Node in players:
		var ap: AnimationPlayer = node as AnimationPlayer
		if not ap:
			continue
		var anims: PackedStringArray = ap.get_animation_list()
		if anims.is_empty():
			continue
		var chosen: String = anims[0]
		for pref: String in ["idle_heavy", "idle_stealth", "idle_hack", "idle_motor", "hover_scan", "idle_patrol"]:
			if pref in anims:
				chosen = pref
				break
		var anim_res: Animation = ap.get_animation(chosen)
		if anim_res:
			anim_res.loop_mode = Animation.LOOP_LINEAR
		ap.play(chosen)


func _get_operador_ativo() -> Node3D:
	match olhar_atual:
		OlharAtivo.GOLIATH:
			return goliath_node
		OlharAtivo.PHANTOM:
			return phantom_node
		_:
			return cipher_node


func _process(delta: float) -> void:
	# 1. Movimento lateral 2.5D do operador ativo
	var dir_x: float = 0.0
	if Input.is_key_pressed(KEY_D) or Input.is_key_pressed(KEY_RIGHT):
		dir_x += 1.0
	if Input.is_key_pressed(KEY_A) or Input.is_key_pressed(KEY_LEFT):
		dir_x -= 1.0

	var op: Node3D = _get_operador_ativo()
	var is_moving: bool = false
	if op and absf(dir_x) > 0.01:
		is_moving = true
		var next_x: float = clampf(op.position.x + dir_x * MOVE_SPEED * delta, MIN_X, MAX_X)
		# Bloqueia passagem pela Porta de Chumbo 10t se fechada (exceto Phantom na passarela alta!)
		if not porta_10t_aberta and olhar_atual != OlharAtivo.PHANTOM and op.position.x < 1.2 and next_x >= 1.2:
			next_x = 1.15
		op.position.x = next_x
		op.rotation.y = 0.45 if dir_x > 0.0 else -0.45
	_atualizar_animacao_operador(op, is_moving)

	# Câmera ortográfica 2.5D acompanha suavemente no eixo X
	if ortho_camera and op:
		var target_cam_x: float = clampf(op.position.x + 1.5, -12.0, 20.5)
		ortho_camera.position.x = lerpf(ortho_camera.position.x, target_cam_x, clampf(delta * 5.0, 0.0, 1.0))

	# 2. Resfriamento da arma Therma-Tek & decaimento do Núcleo Térmico
	arma_calor = maxf(0.0, arma_calor - delta * 24.0)
	if arma_travada_superaquecimento and arma_calor <= 30.0:
		arma_travada_superaquecimento = false
	if arma_calor_bar:
		arma_calor_bar.value = arma_calor

	if nucleo_coletado and nucleo_thermal:
		nucleo_carga_pct = nucleo_thermal.get_normalized_heat() * 100.0
	if nucleo_bar:
		nucleo_bar.value = nucleo_carga_pct

	# 3. Pulso sísmico da Batida no Olhar do Goliath
	seismic_intensity = move_toward(seismic_intensity, 0.18, delta * 1.4)
	if goliath_fx and goliath_fx.material is ShaderMaterial:
		(goliath_fx.material as ShaderMaterial).set_shader_parameter("seismic_pulse_intensity", seismic_intensity)

	# 4. Doutrina do Fogo Mínimo: soldado trava a arma perto da Linha Viva (X = 12..19)
	if esquadra_node and op:
		var perto_linha_viva: bool = absf(op.position.x - 15.0) < 4.5
		if perto_linha_viva != soldado_arma_travada_linha_viva:
			soldado_arma_travada_linha_viva = perto_linha_viva
			_atualizar_animacao_esquadra()

	_atualizar_prompt_contextual(op)


func _input(event: InputEvent) -> void:
	if not (event is InputEventKey and event.pressed):
		return
	var k: InputEventKey = event as InputEventKey
	match k.keycode:
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
		KEY_E:
			_interagir_contextual()
		KEY_F:
			_disparar_canhao_thermatek()
		KEY_B:
			if beat_sequencer:
				beat_sequencer.trigger_manual_strike()
		KEY_ESCAPE:
			if ResourceLoader.exists(MENU_SCENE_PATH):
				get_tree().change_scene_to_file(MENU_SCENE_PATH)


func _disparar_canhao_thermatek() -> void:
	if arma_travada_superaquecimento:
		radio_log_label.text = "[THERMA-TEK]: SUPERAQUECIMENTO! Aguarde o resfriamento do cano antes da próxima rajada curta."
		return
	arma_calor = minf(100.0, arma_calor + 28.0)
	_tocar_animacao_no(goliath_node, "fire_slag_cannon", false)
	if arma_calor >= 98.0:
		arma_travada_superaquecimento = true
		radio_log_label.text = "[THERMA-TEK]: CANO NO LIMITE TÉRMICO (100%) — Trava de segurança ativada!"
		return
	var op: Node3D = _get_operador_ativo()
	if op and absf(op.position.x - 15.0) < 3.0 and olhar_atual == OlharAtivo.GOLIATH:
		fogo_minimo_intacto = false
		radio_log_label.text = "[ALERTA LINHA VIVA]: Disparo do Goliath atingiu a faixa amarela! Vapor liberado — bônus Fogo Mínimo perdido."
		_atualizar_hud_objetivos()
	else:
		radio_log_label.text = "[THERMA-TEK · CANHÃO DE ESCÓRIA]: Disparo confirmado (calor: %.0f%%). Esquadra de Contenção travada perto da Linha Viva!" % arma_calor


func _tocar_animacao_no(target_node: Node3D, anim_name: String, loop: bool = true) -> void:
	if not target_node:
		return
	var ap: AnimationPlayer = target_node.find_child("AnimationPlayer", true, false) as AnimationPlayer
	if not ap or not ap.has_animation(anim_name):
		return
	if ap.current_animation == anim_name and ap.is_playing():
		return
	var anim_res: Animation = ap.get_animation(anim_name)
	if anim_res:
		anim_res.loop_mode = Animation.LOOP_LINEAR if loop else Animation.LOOP_NONE
	ap.play(anim_name)


func _atualizar_animacao_operador(op: Node3D, is_moving: bool) -> void:
	if not op:
		return
	match olhar_atual:
		OlharAtivo.GOLIATH:
			_tocar_animacao_no(goliath_node, "walk_heavy" if is_moving else "idle_heavy", true)
		OlharAtivo.PHANTOM:
			_tocar_animacao_no(phantom_node, "run_agile" if is_moving else "idle_stealth", true)
		OlharAtivo.CIPHER:
			if override_cego_ativo:
				_tocar_animacao_no(cipher_node, "override_blind_loop", true)
			else:
				_tocar_animacao_no(cipher_node, "walk_tactical" if is_moving else "idle_hack", true)


func _interagir_contextual() -> void:
	var op: Node3D = _get_operador_ativo()
	if not op:
		return
	var x: float = op.position.x

	# Zona A: Manômetro 140% (X ~ -12.0)
	if absf(x - (-12.0)) < 2.6:
		manometro_lido = true
		var txt: String = fragments_by_id.get("manometro-da-estacao-14", {}).get("text", "Ponteiro cravado em 140% sob fita amarela.")
		radio_log_label.text = "[FRAGMENTO · MANÔMETRO DA ESTAÇÃO 14]: " + txt
		_atualizar_hud_objetivos()
		return

	# Zona A: Relógio 23 ANOS riscado a prego (X ~ -8.5)
	if absf(x - (-8.5)) < 2.6:
		relogio_23_achado = true
		radio_log_label.text = "[PLANTA DE LORE · RELÓGIO DOS FOGUISTAS]: \"23 ANOS\" riscado fundo a prego na chapa da caldeira. O Sindicato diz 140."
		_atualizar_hud_objetivos()
		return

	# Zona A: Etiqueta de Cota Familiar (X ~ -5.2)
	if absf(x - (-5.2)) < 2.6:
		etiqueta_lida = true
		var txt2: String = fragments_by_id.get("etiqueta-de-cota", {}).get("text", "COTA FAMILIAR: SOUZA-ADEODATO. NÃO ABRIR NA NOITE.")
		radio_log_label.text = "[FRAGMENTO · ETIQUETA DE COTA]: " + txt2.replace("\n", " — ")
		_atualizar_hud_objetivos()
		return

	# Zona B: Porta de Chumbo 10t / Terminal Omni-Cripto (X ~ 2.0)
	if not porta_10t_aberta and absf(x - 2.0) < 3.6:
		porta_10t_aberta = true
		_tocar_animacao_no(goliath_node, "lift_door_10t", false)
		if porta_10t_node:
			var leaf: Node3D = porta_10t_node.find_child("Door_Leaf", true, false) as Node3D
			if leaf:
				var tw: Tween = create_tween().set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
				tw.tween_property(leaf, "position:y", 3.4, 1.35)
		radio_log_label.text = "[OBJETIVO 1 CONCLUÍDO]: Câmara de Despacho aberta! (Goliath + Macaco ergueram 10t / Phantom abriu sangria / Cipher destravou)."
		_atualizar_hud_objetivos()
		return

	# Zona B -> C: Empurrar Contêiner de Acumuladores até o Duto de Sangria (X ~ 6.5)
	if porta_10t_aberta and not conteiner_despachado and absf(x - 6.5) < 3.5:
		conteiner_despachado = true
		if conteiner_node and duto_sangria_node:
			var tw2: Tween = create_tween().set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN_OUT)
			tw2.tween_property(conteiner_node, "position:x", duto_sangria_node.position.x - 0.5, 1.8)
		radio_log_label.text = "[OBJETIVO 2 CONCLUÍDO]: Contêiner de Acumuladores posicionado no Duto de Sangria do Setor 7-Baixo!"
		_atualizar_hud_objetivos()
		return

	# Coletar Núcleo Térmico dropado (X ~ 13.5)
	if not nucleo_coletado and absf(x - 13.5) < 2.5:
		nucleo_coletado = true
		if nucleo_drop_node:
			nucleo_drop_node.visible = false
		radio_log_label.text = "[LOOT · NÚCLEO TÉRMICO]: Coletado! Atenção: ele esfria devagar (ThermalNode) — extraia antes de perder carga."
		_atualizar_hud_objetivos()
		return

	# Zona C: Girar Válvula Mestra 14 (X ~ 20.5)
	if conteiner_despachado and not valvula_14_aberta and absf(x - 20.5) < 3.5:
		valvula_14_aberta = true
		_tocar_animacao_no(goliath_node, "turn_valve", false)
		if valvula_14_node:
			var wheel: Node3D = valvula_14_node.find_child("Wheel_Hinge", true, false) as Node3D
			if wheel:
				var tw3: Tween = create_tween().set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
				tw3.tween_property(wheel, "rotation:z", TAU * 1.5, 1.6)
		if duto_sangria_node:
			var hatch: Node3D = duto_sangria_node.find_child("Chute_Hatch", true, false) as Node3D
			if hatch:
				var tw4: Tween = create_tween()
				tw4.tween_property(hatch, "rotation:x", -1.25, 0.9)
		radio_log_label.text = "[QUEST 1 CONCLUÍDA · CALOR DE CONTRABANDO]: Válvula 14 aberta! O calor desce para o Setor 7-Baixo. Recompensa: Canhão de Escória Therma-Tek + Auxiliar Macaco."
		_atualizar_hud_objetivos()


func _atualizar_prompt_contextual(op: Node3D) -> void:
	if not op or not prompt_label:
		return
	var x: float = op.position.x
	var zona: String = "ZONA A · CASA DE CALDEIRAS" if x < 0.0 else ("ZONA B · CÂMARA DE DESPACHO" if x < 11.0 else "ZONA C · GALERIA DE SANGRIA (FACE VOLTADA · NETUNO 48°)")
	if zona_label:
		zona_label.text = "QUEST 1 · ESTAÇÃO DE VÁLVULA 14  |  %s  |  X = %.1fm" % [zona, x]

	if absf(x - (-12.0)) < 2.6:
		prompt_label.text = "[E] Inspecionar MANÔMETRO DA ESTAÇÃO 14 (140% sob fita amarela)"
	elif absf(x - (-8.5)) < 2.6:
		prompt_label.text = "[E] Examinar RELÓGIO RISCADO A PREGO NA CHAPA (23 ANOS)"
	elif absf(x - (-5.2)) < 2.6:
		prompt_label.text = "[E] Ler ETIQUETA DE COTA FAMILIAR (SOUZA-ADEODATO)"
	elif not porta_10t_aberta and absf(x - 2.0) < 3.6:
		prompt_label.text = "[E] Abrir PORTA DE CHUMBO 10t (Câmara de Despacho — 1 a 3 Jogadores + Auxiliares)"
	elif porta_10t_aberta and not conteiner_despachado and absf(x - 6.5) < 3.5:
		prompt_label.text = "[E] Empurrar CONTÊINER DE ACUMULADORES até o Duto de Sangria"
	elif not nucleo_coletado and absf(x - 13.5) < 2.5:
		prompt_label.text = "[E] Coletar NÚCLEO TÉRMICO (esfria com o tempo)"
	elif conteiner_despachado and not valvula_14_aberta and absf(x - 20.5) < 3.5:
		prompt_label.text = "[E] Girar VÁLVULA MESTRA 14 (Número visível no topo pelo Olhar do Phantom [2])"
	else:
		var aviso_trava: String = "  |  [SOLDADO: ARMA TRAVADA PERTO DA LINHA VIVA!]" if soldado_arma_travada_linha_viva else ""
		prompt_label.text = "[A/D] Mover   |   [1] Goliath  [2] Phantom  [3] Cipher  [O] Override   |   [F] Disparar Therma-Tek   |   [ESC] Menu" + aviso_trava


func _atualizar_hud_objetivos() -> void:
	if not objetivos_label:
		return
	var o1: String = "[X]" if porta_10t_aberta else "[ ]"
	var o2: String = "[X]" if conteiner_despachado else "[ ]"
	var o3: String = "[X]" if valvula_14_aberta else "[ ]"
	var s1: String = "INTACTO" if fogo_minimo_intacto else "ROMPIDO"
	var s2: String = "[X]" if manometro_lido else "[ ]"
	var s3: String = "[X]" if relogio_23_achado else "[ ]"
	objetivos_label.text = (
		"PRIMÁRIOS: %s Abrir Câmara 10t   %s Despachar Contêiner   %s Abrir Válvula 14   ||   SECUNDÁRIOS: Fogo Mínimo [%s]   %s Manômetro 140%%   %s Relógio 23a"
		% [o1, o2, o3, s1, s2, s3]
	)


func _atualizar_animacao_esquadra() -> void:
	if not esquadra_node:
		return
	var ap: AnimationPlayer = esquadra_node.find_child("AnimationPlayer", true, false) as AnimationPlayer
	if not ap:
		return
	var target_anim: String = "weapon_lock" if soldado_arma_travada_linha_viva else "idle_patrol"
	if ap.has_animation(target_anim):
		ap.play(target_anim)


func _on_beat_pulse(_step_index: int, is_accent: bool) -> void:
	if is_accent:
		seismic_intensity = 0.88


func _aplicar_olhar() -> void:
	goliath_fx.visible = (olhar_atual == OlharAtivo.GOLIATH)
	phantom_fx.visible = (olhar_atual == OlharAtivo.PHANTOM)
	phantom_reticle.visible = (olhar_atual == OlharAtivo.PHANTOM)
	cipher_fx.visible = (olhar_atual == OlharAtivo.CIPHER)
	override_terminal.visible = (olhar_atual == OlharAtivo.CIPHER and override_cego_ativo)
	if cipher_fx and cipher_fx.material is ShaderMaterial:
		(cipher_fx.material as ShaderMaterial).set_shader_parameter("leak_count", leak_count)
