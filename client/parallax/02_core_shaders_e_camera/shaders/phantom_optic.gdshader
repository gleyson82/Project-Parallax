shader_type canvas_item;

// Olhar do Phantom (phantom_optic.gdshader)
// Realce de bordas Sobel em ciano-gelo (#00E5FF) para pontos altos, cabos de tensão e códigos no teto + refração prismática N2.

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform vec4 optic_edge_color : source_color = vec4(0.0, 0.9, 1.0, 1.0);
uniform float edge_strength : hint_range(0.0, 3.0) = 1.65;
uniform float prism_refraction : hint_range(0.0, 0.01) = 0.0022;

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 px = SCREEN_PIXEL_SIZE;

	// Refração prismática sutil nas bordas (poeira de cristal de nitrogênio Kinesys)
	float edge_dist = length(uv - vec2(0.5));
	vec2 prism_shift = normalize(uv - vec2(0.5) + vec2(0.0001)) * prism_refraction * smoothstep(0.28, 0.72, edge_dist);
	vec3 c = vec3(
		texture(screen_texture, uv + prism_shift).r,
		texture(screen_texture, uv).g,
		texture(screen_texture, uv - prism_shift).b
	);

	vec3 n = texture(screen_texture, uv + vec2(0.0, -px.y)).rgb;
	vec3 s = texture(screen_texture, uv + vec2(0.0, px.y)).rgb;
	vec3 e = texture(screen_texture, uv + vec2(px.x, 0.0)).rgb;
	vec3 w = texture(screen_texture, uv + vec2(-px.x, 0.0)).rgb;

	float edge = length((n - s) + (e - w)) * edge_strength;
	// Foco ótico mais nítido na metade superior (teto, vigas e números das válvulas)
	float upper_boost = mix(1.45, 0.72, uv.y);
	vec3 final_rgb = mix(c * 0.84, optic_edge_color.rgb, clamp(edge * upper_boost, 0.0, 1.0));

	COLOR = vec4(final_rgb, 1.0);
}
