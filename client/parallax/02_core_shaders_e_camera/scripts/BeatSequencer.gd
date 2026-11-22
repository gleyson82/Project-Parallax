extends Node
class_name BeatSequencer

## Sequenciador de "A Batida" dos Foguistas (PARALLAX)
## Sincronizado ao _physics_process para precisão rítmica em puzzles sonoros nos dutos
## e feedback visual/sísmico no Olhar do Goliath (goliath_thermal.gdshader).

signal beat_pulse(step_index: int, is_accent: bool)

@export var bpm: float = 96.0
@export var pattern: Array[bool] = [true, false, true, true, false, false, true, false]
@export var active: bool = true

var _accumulator: float = 0.0
var _step_index: int = 0


func _physics_process(delta: float) -> void:
	if not active or pattern.is_empty():
		return
	var step_duration: float = 60.0 / maxf(1.0, bpm) * 0.5
	_accumulator += delta
	if _accumulator >= step_duration:
		_accumulator -= step_duration
		var is_accent: bool = pattern[_step_index]
		beat_pulse.emit(_step_index, is_accent)
		_step_index = (_step_index + 1) % pattern.size()


func trigger_manual_strike() -> void:
	beat_pulse.emit(_step_index, true)
