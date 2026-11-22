extends Control
class_name MainMenuSalaDeMaquinas

## Menu Principal Beta — A Sala de Máquinas & O Quadro de Válvulas (PARALLAX)
## Implementa:
## 1. Abertura com Logo PARALLAX em duas chapas desalinhadas (+12px) que travam ao primeiro input.
## 2. Avanço de geada por inatividade (recua com qualquer input).
## 3. Alternância das 3 camadas de leitura em Paralaxe (Física/Goliath, Espacial/Phantom, Lógica/Cipher).
## 4. Esquema visual de dutos de Tritão com os 4 tipos de válvulas SVG, emblemas e retratos de rádio.
## 5. Confirmação "Três Mãos" (0.4s hold) e atalho [F2] para o Laboratório 2.5D (Módulo 02).

enum CamadaOlhar {
	FISICA_GOLIATH = 0,
	ESPACIAL_PHANTOM = 1,
	LOGICA_CIPHER = 2
}

const HOLD_TIME_REQUIRED: float = 0.4
const FROST_IDLE_SPEED: float = 0.055
const LAB_25D_SCENE_PATH: String = "res://02_core_shaders_e_camera/scenes/OrthoCamera25D.tscn"
const QUEST01_SCENE_PATH: String = "res://05_biomas_modulares/01_fosso_de_hefesto/scenes/Quest01_Estacao14.tscn"

const TEX_CANDEIA: Texture2D = preload("res://01_ui_menu_beta/assets/portrait_candeia_radio.png")
const TEX_BRANDT: Texture2D = preload("res://01_ui_menu_beta/assets/portrait_brandt_radio.png")
const TEX_LOGO_THERMA: Texture2D = preload("res://01_ui_menu_beta/assets/logo_therma_tek.svg")
const TEX_LOGO_KINESYS: Texture2D = preload("res://01_ui_menu_beta/assets/logo_kinesys.svg")
const TEX_LOGO_OMNI: Texture2D = preload("res://01_ui_menu_beta/assets/logo_omni_cripto.svg")
const TEX_LOGO_CIFRA: Texture2D = preload("res://01_ui_menu_beta/assets/logo_cifra.svg")

@export var camada_atual: CamadaOlhar = CamadaOlhar.FISICA_GOLIATH

var idle_frost: float = 0.16
var hold_timer: float = 0.0
var is_holding_confirm: bool = false
var selected_node_index: int = 0
var logo_alinhado: bool = false

var missoes_valvulas: Array[Dictionary] = [
	{
		"id": "quest_01",
		"titulo": "QUEST 1 · CALOR DE CONTRABANDO",
		"bioma": "Fosso de Hefesto · Estação de Válvula 14 (Face Voltada)",
		"tipo": "Volante Simples · Onboarding Goliath / Therma-Tek",
		"briefing": "\"O Setor 7-Baixo faz três dias sem calor. Na Estação 14 tem um contêiner de acumuladores que o Sindicato chama de reserva estratégica. A gente chama de jantar. Entra, pega e joga no duto.\" — Candeia",
		"leitura_goliath": "[CAMADA FÍSICA · GOLIATH]: Pressão do duto em 140% (Sobrecarga oculta). Porta de chumbo de 10 t na Câmara de Despacho.",
		"leitura_phantom": "[CAMADA ESPACIAL · PHANTOM]: Passarela superior sobre a casa de caldeiras; sangria secundária acessível por cabo.",
		"leitura_cipher": "[CAMADA LÓGICA · CIPHER]: Trava lógica simples na Câmara de Despacho; Linhas Vivas ativas no corredor.",
		"recompensa": "Núcleos Térmicos · 1ª Arma Therma-Tek · Auxiliar Macaco",
		"retrato": "candeia",
		"transmissor": "CANAL CIFRA // CANDEIA (ESTAÇÃO OCULTA)",
		"corporacao": "therma",
		"quarta_marca": false
	},
	{
		"id": "quest_02",
		"titulo": "QUEST 2 · LINHA DE VISADA",
		"bioma": "Os Estilhaços · Farol 3-Gelo (Superfície -235 °C)",
		"tipo": "Trilho & Escolta · Onboarding Phantom / Kinesys",
		"briefing": "\"O Cruzador Vagalume está parado numa tempestade com duzentas pessoas dentro. O Farol 3 saiu do alinhamento depois do último tremor. Sobe, alinha o espelho e me dá um caminho antes que o gelo escolha por nós.\" — Candeia",
		"leitura_goliath": "[CAMADA FÍSICA · GOLIATH]: Mastro instável sob tremor; requer ancoragem pesada na base durante o alinhamento.",
		"leitura_phantom": "[CAMADA ESPACIAL · PHANTOM]: Topo do mastro a 100 m; rota vertical entre colunas de gêiseres de 8 km.",
		"leitura_cipher": "[CAMADA LÓGICA · CIPHER]: Fratura de Nervo a 1,4 km; anomalia espectral 'FONTE P' repetindo a cada 26h56.",
		"recompensa": "Rifle Criogênico Kinesys · Cabo de Tensão · Auxiliar Pipa",
		"retrato": "candeia",
		"transmissor": "CANAL CIFRA // CANDEIA (RETRANSMISSÃO FAROL 3)",
		"corporacao": "kinesys",
		"quarta_marca": false
	},
	{
		"id": "quest_03",
		"titulo": "QUEST 3 · CHAVE TRIPLA",
		"bioma": "Krios-Prime · Nó Cinza (Face Cega · Céu sem Netuno)",
		"tipo": "Volante Simples · Onboarding Cipher / Omni-Cripto",
		"briefing": "\"O Sindicato mantém uma lista. Quem entra em uma Arca, quando e de qual setor. Ela passa por um nó em Krios-Prime. A porta pede três coisas ao mesmo tempo. Agora vocês são três.\" — Candeia",
		"leitura_goliath": "[CAMADA FÍSICA · GOLIATH]: Fechadura 1 (Therma-Tek): Volante de alta resistência térmica.",
		"leitura_phantom": "[CAMADA ESPACIAL · PHANTOM]: Fechadura 2 (Kinesys): Trava estrutural suspensa sob a redoma invertida.",
		"leitura_cipher": "[CAMADA LÓGICA · CIPHER]: Fechadura 3 (Omni-Cripto): Janela de Sincronia aberta + 4º Receptáculo sem selo.",
		"recompensa": "Equipamento Omni-Cripto · Manifesto das Arcas · Desbloqueia Raid 1",
		"retrato": "candeia",
		"transmissor": "CANAL CIFRA // CANDEIA (FREQUÊNCIA CRIPTOGRAFADA)",
		"corporacao": "omni",
		"quarta_marca": true
	},
	{
		"id": "raid_01",
		"titulo": "RAID 1 · SANGRIA DE HEFESTO",
		"bioma": "Fosso de Hefesto · Reator Central (Face Voltada)",
		"tipo": "Volante Duplo · Raid Cooperativa (1 a 3 Jogadores)",
		"briefing": "\"Se abrir essas válvulas sem autorização da Diretoria, o reator despressuriza em três minutos. Vocês não entendem o custo do calor que roubam. Saiam do meu piso.\" — Engenheiro-Chefe Brandt (Interceptação)",
		"leitura_goliath": "[CAMADA FÍSICA · GOLIATH]: 9 válvulas no Pátio; cadência da Batida nos canos dita a ordem; Porta de Pressão no Coração.",
		"leitura_phantom": "[CAMADA ESPACIAL · PHANTOM]: Números cobertos de fuligem visíveis apenas das vigas altas; placas do Mestre de Fornalha prontas para Choque Térmico.",
		"leitura_cipher": "[CAMADA LÓGICA · CIPHER]: Sensor sísmico na Galeria das Plataformas; Livro-Razão de Brandt oculta produção real de 140%.",
		"recompensa": "Arsenal Therma-Tek Grau Alto · Segredo: O Coração de Hefesto (Grau-Zero)",
		"retrato": "brandt",
		"transmissor": "INTERCEPTAÇÃO THERMA-TEK // ENG.-CHEFE BRANDT",
		"corporacao": "therma",
		"quarta_marca": true
	},
	{
		"id": "raid_02",
		"titulo": "RAID 2 · META ATINGIDA (SETOR 9-N)",
		"bioma": "Cemitério das Máquinas · Setor Nulo Soterrado (Face Cega)",
		"tipo": "Cruz de Sangria · Raid de Sobrevivência & Terror",
		"briefing": "\"A fornalha do Setor Nove-N trabalha há dezenove anos sem ninguém vivo para aquecer. A Diretriz Ômega ainda está lá dentro contando vazamentos. Desçam em silêncio e tragam esse calor.\" — Candeia",
		"leitura_goliath": "[CAMADA FÍSICA · GOLIATH]: Ar morno anômalo; carcaças exigem Modo Brasa; manômetro mecânico desmente a rede.",
		"leitura_phantom": "[CAMADA ESPACIAL · PHANTOM]: 5 Anéis de Escuta acústica; pontos de instalação de Surdinas e Placa de Turno 11 no teto.",
		"leitura_cipher": "[CAMADA LÓGICA · CIPHER]: ALERTA: A Ômega falsifica 1 leitura por rodada. Meta do Ciclo 4.113 vinculada ao número de vivos.",
		"recompensa": "Arsenal Omni-Cripto Grau Alto · Segredo: O Código-Fonte Ômega (Grau-Zero)",
		"retrato": "candeia",
		"transmissor": "CANAL CIFRA // CANDEIA (SINAL FRACO — SETOR 9-N)",
		"corporacao": "cifra",
		"quarta_marca": true
	}
]

@onready var frost_overlay: ColorRect = $FrostOverlay
@onready var logo_camada_b: TextureRect = $MainLayout/HeaderBar/LogoParallaxStack/LogoCamadaB
@onready var layer_badge: Label = $MainLayout/HeaderBar/CamadaAtivaLabel
@onready var parallax_bg_fisica: Control = $ParallaxContainer/CamadaFisica
@onready var parallax_bg_espacial: Control = $ParallaxContainer/CamadaEspacial
@onready var parallax_bg_logica: Control = $ParallaxContainer/CamadaLogica
@onready var portrait_rect: TextureRect = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/RadioHeader/PortraitFrame/RadioPortrait
@onready var transmissor_label: Label = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/RadioHeader/MetaVBox/TransmissorLabel
@onready var corp_logo_rect: TextureRect = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/RadioHeader/CorpLogoRect
@onready var quarta_marca_rect: TextureRect = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/RadioHeader/QuartaMarcaRect
@onready var ficha_titulo: Label = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/RadioHeader/MetaVBox/TituloMissao
@onready var ficha_bioma: Label = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/RadioHeader/MetaVBox/BiomaLabel
@onready var ficha_tipo: Label = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/TipoValvulaLabel
@onready var ficha_briefing: Label = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/BriefingText
@onready var ficha_leitura_olhar: Label = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/LeituraOlharBox/LeituraOlharLabel
@onready var ficha_recompensa: Label = $MainLayout/ContentSplit/FichaDeIncursaoPanel/Margin/VBox/RecompensaLabel
@onready var tres_maos_bar: ProgressBar = $MainLayout/FooterBar/FooterRow/ProgressVBox/TresMaosProgress
@onready var status_lancamento: Label = $MainLayout/FooterBar/FooterRow/ProgressVBox/StatusLancamentoLabel
@onready var valve_buttons: Array[Button] = [
	$MainLayout/ContentSplit/MapaDutosPanel/Margin/ValvulasList/BtnQuest01,
	$MainLayout/ContentSplit/MapaDutosPanel/Margin/ValvulasList/BtnQuest02,
	$MainLayout/ContentSplit/MapaDutosPanel/Margin/ValvulasList/BtnQuest03,
	$MainLayout/ContentSplit/MapaDutosPanel/Margin/ValvulasList/BtnRaid01,
	$MainLayout/ContentSplit/MapaDutosPanel/Margin/ValvulasList/BtnRaid02
]


func _ready() -> void:
	# Logo inicia com as duas chapas desalinhadas em 12px (metafora da Paralaxe)
	if logo_camada_b:
		logo_camada_b.position = Vector2(12.0, -4.0)
	_atualizar_interface_completa()


func _process(delta: float) -> void:
	# 1. Geada avança pelas bordas se o menu ficar parado
	idle_frost = clampf(idle_frost + FROST_IDLE_SPEED * delta, 0.08, 0.95)
	if frost_overlay and frost_overlay.material is ShaderMaterial:
		(frost_overlay.material as ShaderMaterial).set_shader_parameter("frost_progress", idle_frost)

	# 2. Gesto "Três Mãos": segurar confirmar por 0.4s para girar a válvula e lançar
	if is_holding_confirm:
		hold_timer += delta
		tres_maos_bar.value = (hold_timer / HOLD_TIME_REQUIRED) * 100.0
		if hold_timer >= HOLD_TIME_REQUIRED:
			_lancar_missao_selecionada()
			is_holding_confirm = false
			hold_timer = 0.0
	else:
		hold_timer = maxf(0.0, hold_timer - delta * 2.5)
		tres_maos_bar.value = (hold_timer / HOLD_TIME_REQUIRED) * 100.0


func _input(event: InputEvent) -> void:
	var has_user_input: bool = (
		event is InputEventKey
		or event is InputEventMouseMotion
		or event is InputEventMouseButton
		or event is InputEventJoypadButton
	)
	if has_user_input:
		idle_frost = maxf(0.08, idle_frost - 0.12)
		if not logo_alinhado and (event is InputEventKey or event is InputEventMouseButton or event is InputEventJoypadButton):
			_travar_chapas_logo()

	if event is InputEventKey and event.pressed and event.keycode == KEY_F2:
		abrir_laboratorio_25d()
		return

	if event.is_action_pressed("ui_focus_next") or (event is InputEventKey and event.pressed and event.keycode == KEY_TAB):
		alternar_camada_paralaxe()
	elif event.is_action_pressed("ui_down"):
		selecionar_valvula((selected_node_index + 1) % missoes_valvulas.size())
	elif event.is_action_pressed("ui_up"):
		selecionar_valvula((selected_node_index - 1 + missoes_valvulas.size()) % missoes_valvulas.size())
	elif event.is_action_pressed("ui_accept"):
		is_holding_confirm = true
		status_lancamento.text = "GIRANDO VÁLVULA MESTRA (TRÊS MÃOS: GOLIATH + PHANTOM + CIPHER)..."
	elif event.is_action_released("ui_accept"):
		is_holding_confirm = false
		status_lancamento.text = "[TAB] Alternar Olhar   |   [SETAS] Escolher Válvula   |   [SEGURAR ENTER 0.4s] Três Mãos   |   [F2] Laboratório 2.5D"


func _travar_chapas_logo() -> void:
	logo_alinhado = true
	if logo_camada_b:
		var tw: Tween = create_tween().set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
		tw.tween_property(logo_camada_b, "position", Vector2.ZERO, 0.28)


func alternar_camada_paralaxe() -> void:
	camada_atual = ((int(camada_atual) + 1) % 3) as CamadaOlhar
	_atualizar_interface_completa()


func selecionar_valvula(index: int) -> void:
	selected_node_index = clampi(index, 0, missoes_valvulas.size() - 1)
	_atualizar_interface_completa()


func abrir_laboratorio_25d() -> void:
	if ResourceLoader.exists(LAB_25D_SCENE_PATH):
		get_tree().change_scene_to_file(LAB_25D_SCENE_PATH)
	else:
		status_lancamento.text = "ERRO: Cena de calibração 2.5D não encontrada em " + LAB_25D_SCENE_PATH


func _atualizar_interface_completa() -> void:
	var missao: Dictionary = missoes_valvulas[selected_node_index]
	ficha_titulo.text = missao["titulo"]
	ficha_bioma.text = "SETOR: " + missao["bioma"]
	ficha_tipo.text = "VÁLVULA: " + missao["tipo"]
	ficha_briefing.text = missao["briefing"]
	ficha_recompensa.text = "EXTRAÇÃO PREVISTA: " + missao["recompensa"]
	transmissor_label.text = missao["transmissor"]

	# Retrato de rádio diegético (Candeia vs. Brandt)
	if portrait_rect:
		portrait_rect.texture = TEX_BRANDT if missao["retrato"] == "brandt" else TEX_CANDEIA

	# Emblema corporativo e vestígio discreto da Marca de Quatro Hastes
	if corp_logo_rect:
		match missao["corporacao"]:
			"therma":
				corp_logo_rect.texture = TEX_LOGO_THERMA
			"kinesys":
				corp_logo_rect.texture = TEX_LOGO_KINESYS
			"omni":
				corp_logo_rect.texture = TEX_LOGO_OMNI
			_:
				corp_logo_rect.texture = TEX_LOGO_CIFRA

	if quarta_marca_rect:
		quarta_marca_rect.visible = bool(missao["quarta_marca"])

	# Realce visual da válvula selecionada
	for i: int in range(valve_buttons.size()):
		if valve_buttons[i]:
			valve_buttons[i].modulate = Color(1.18, 1.12, 0.96, 1.0) if i == selected_node_index else Color(0.76, 0.80, 0.85, 0.92)

	# Deslocamento suave de Paralaxe entre as 3 camadas de dutos
	var tw: Tween = create_tween().set_parallel(true).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	match camada_atual:
		CamadaOlhar.FISICA_GOLIATH:
			layer_badge.text = "CAMADA ATIVA: [1/3] FÍSICA & PRESSÃO (GOLIATH · THERMA-TEK)"
			layer_badge.modulate = Color(1.0, 0.42, 0.21)
			ficha_leitura_olhar.text = missao["leitura_goliath"]
			ficha_leitura_olhar.modulate = Color(1.0, 0.55, 0.35)
			tw.tween_property(parallax_bg_fisica, "position", Vector2(0, 0), 0.25)
			tw.tween_property(parallax_bg_espacial, "position", Vector2(-22, -12), 0.25)
			tw.tween_property(parallax_bg_logica, "position", Vector2(22, 12), 0.25)
			tw.tween_property(parallax_bg_fisica, "modulate:a", 1.0, 0.25)
			tw.tween_property(parallax_bg_espacial, "modulate:a", 0.32, 0.25)
			tw.tween_property(parallax_bg_logica, "modulate:a", 0.32, 0.25)
		CamadaOlhar.ESPACIAL_PHANTOM:
			layer_badge.text = "CAMADA ATIVA: [2/3] ESPACIAL & ROTAS ALTAS (PHANTOM · KINESYS)"
			layer_badge.modulate = Color(0.2, 0.9, 1.0)
			ficha_leitura_olhar.text = missao["leitura_phantom"]
			ficha_leitura_olhar.modulate = Color(0.4, 0.95, 1.0)
			tw.tween_property(parallax_bg_fisica, "position", Vector2(20, 14), 0.25)
			tw.tween_property(parallax_bg_espacial, "position", Vector2(0, 0), 0.25)
			tw.tween_property(parallax_bg_logica, "position", Vector2(-20, -14), 0.25)
			tw.tween_property(parallax_bg_fisica, "modulate:a", 0.32, 0.25)
			tw.tween_property(parallax_bg_espacial, "modulate:a", 1.0, 0.25)
			tw.tween_property(parallax_bg_logica, "modulate:a", 0.32, 0.25)
		CamadaOlhar.LOGICA_CIPHER:
			layer_badge.text = "CAMADA ATIVA: [3/3] LÓGICA & PELE DE REDE (CIPHER · OMNI-CRIPTO)"
			layer_badge.modulate = Color(0.75, 0.55, 1.0)
			ficha_leitura_olhar.text = missao["leitura_cipher"]
			ficha_leitura_olhar.modulate = Color(0.85, 0.7, 1.0)
			tw.tween_property(parallax_bg_fisica, "position", Vector2(-18, 16), 0.25)
			tw.tween_property(parallax_bg_espacial, "position", Vector2(18, -16), 0.25)
			tw.tween_property(parallax_bg_logica, "position", Vector2(0, 0), 0.25)
			tw.tween_property(parallax_bg_fisica, "modulate:a", 0.32, 0.25)
			tw.tween_property(parallax_bg_espacial, "modulate:a", 0.32, 0.25)
			tw.tween_property(parallax_bg_logica, "modulate:a", 1.0, 0.25)


func _lancar_missao_selecionada() -> void:
	var missao: Dictionary = missoes_valvulas[selected_node_index]
	if missao["id"] == "quest_01" and ResourceLoader.exists(QUEST01_SCENE_PATH):
		get_tree().change_scene_to_file(QUEST01_SCENE_PATH)
		return
	status_lancamento.text = "VÁLVULA ABERTA -> INICIANDO INCURSÃO: " + missao["titulo"] + " [EM PRODUÇÃO — USE QUEST 1 PARA JOGAR VERTICAL SLICE]"
