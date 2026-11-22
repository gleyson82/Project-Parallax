shader_type spatial;
render_mode blend_add, depth_draw_never, cull_disabled, unshaded;

// Ondas de calor e vapor sulfúrico subindo pelas grelhas do Fosso de Hefesto (Quest 1)
uniform vec4 heat_color : source_color = vec4(1.0, 0.42, 0.12, 0.35);
uniform float rise_speed : hint_range(0.1, 3.0) = 0.65;
uniform float shimmer_freq : hint_range(2.0, 24.0) = 10.0;

void fragment() {
	vec2 uv = UV;
	float wave1 = sin(uv.x * shimmer_freq + TIME * 2.2) * 0.5 + 0.5;
	float wave2 = sin((uv.y - TIME * rise_speed) * 14.0 + uv.x * 6.0) * 0.5 + 0.5;
	float vertical_fade = smoothstep(0.0, 0.25, uv.y) * smoothstep(1.0, 0.15, uv.y);
	float edge_fade = smoothstep(0.0, 0.15, uv.x) * smoothstep(1.0, 0.85, uv.x);
	float alpha = wave1 * wave2 * vertical_fade * edge_fade * heat_color.a;
	ALBEDO = heat_color.rgb;
	ALPHA = alpha;
}
