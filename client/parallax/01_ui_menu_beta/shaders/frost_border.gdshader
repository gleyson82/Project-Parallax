shader_type canvas_item;

// Shader de Geada nas Bordas (A Sala de Máquinas - PARALLAX)
// Quanto maior 'frost_progress' (0.0 a 1.0), mais o frio de -235°C avança das bordas para o centro.

uniform float frost_progress : hint_range(0.0, 1.0) = 0.25;
uniform vec4 frost_color : source_color = vec4(0.65, 0.92, 1.0, 0.85);

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

void fragment() {
	vec2 uv = UV;
	// Distância até a borda mais próxima (0.0 na borda, 0.5 no centro)
	float dist_edge = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
	float crystal_noise = hash(floor(uv * 160.0)) * 0.08;
	float threshold = frost_progress * 0.35 + crystal_noise;
	float mask = smoothstep(threshold, threshold - 0.06, dist_edge);
	COLOR = vec4(frost_color.rgb, mask * frost_color.a * frost_progress);
}
