extends Node
class_name ThermalNode

## Componente Termodinâmico Padrão de PARALLAX (Godot 4.4+)
## Gerencia temperatura em Kelvin (ambiente externo em Tritão = 38.0 K / -235 °C),
## superaquecimento Therma-Tek, congelamento Kinesys, decaimento de 2% das Células Térmicas
## e o evento de Choque Térmico (superaquecido + impacto criogênico).

signal temperature_changed(new_kelvin: float, normalized_heat: float)
signal overheated(current_kelvin: float)
signal frozen(current_kelvin: float)
signal thermal_shock_triggered(delta_kelvin: float)

const TRITON_AMBIENT_KELVIN: float = 38.0
const CELULA_DECAY_PER_DAY: float = 0.02

@export var current_kelvin: float = 290.0
@export var ambient_kelvin: float = TRITON_AMBIENT_KELVIN
@export var overheat_threshold_k: float = 750.0
@export var freeze_threshold_k: float = 95.0
@export var thermal_shock_min_drop_k: float = 380.0
@export var cooling_rate_k_per_sec: float = 6.0
@export var passive_cooling_enabled: bool = true

var is_overheated: bool = false
var is_frozen: bool = false


func _physics_process(delta: float) -> void:
	if not passive_cooling_enabled:
		return
	if absf(current_kelvin - ambient_kelvin) > 0.1:
		var next_k: float = move_toward(current_kelvin, ambient_kelvin, cooling_rate_k_per_sec * delta)
		_set_temperature(next_k)


func apply_heat(delta_kelvin: float) -> void:
	_set_temperature(current_kelvin + absf(delta_kelvin))


func apply_cryo(delta_kelvin: float) -> void:
	var drop: float = absf(delta_kelvin)
	var was_overheated: bool = is_overheated
	_set_temperature(maxf(TRITON_AMBIENT_KELVIN, current_kelvin - drop))
	if was_overheated and drop >= thermal_shock_min_drop_k:
		thermal_shock_triggered.emit(drop)


func get_normalized_heat() -> float:
	return clampf((current_kelvin - TRITON_AMBIENT_KELVIN) / maxf(1.0, overheat_threshold_k - TRITON_AMBIENT_KELVIN), 0.0, 1.0)


static func compute_thermal_cell_charge(initial_charge: float, elapsed_days: float) -> float:
	return maxf(0.0, initial_charge * pow(1.0 - CELULA_DECAY_PER_DAY, maxf(0.0, elapsed_days)))


func _set_temperature(new_k: float) -> void:
	current_kelvin = maxf(TRITON_AMBIENT_KELVIN, new_k)
	var now_overheated: bool = current_kelvin >= overheat_threshold_k
	var now_frozen: bool = current_kelvin <= freeze_threshold_k

	if now_overheated and not is_overheated:
		overheated.emit(current_kelvin)
	if now_frozen and not is_frozen:
		frozen.emit(current_kelvin)

	is_overheated = now_overheated
	is_frozen = now_frozen
	temperature_changed.emit(current_kelvin, get_normalized_heat())
