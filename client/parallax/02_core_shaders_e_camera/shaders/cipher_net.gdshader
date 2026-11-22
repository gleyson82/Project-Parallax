shader_type canvas_item;

// Olhar do Cipher (cipher_net.gdshader)
// Desatura o mundo físico em 75%, realça a Pele de Rede em violeta (#B388FF) e dispara o Tremor Vermelho quando leak_count >= 3.

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform sampler2D glitch_noise : repeat_enable, filter_nearest;
uniform bool use_glitch_texture = false;
uniform int leak_count : hint_range(0, 5) = 1;
uniform vec4 net_color : source_color = vec4(0.7, 0.53, 1.0, 1.0);
uniform vec4 omega_error_color : source_color = vec4(1.0, 0.09, 0.27, 1.0);

void fragment() {
	vec2 uv = SCREEN_UV;
	float omega_active = float(leak_count >= 3);
	float leak_severity = clamp(float(leak_count - 2) / 3.0, 0.0, 1.0) * omega_active;

	float block_glitch = 0.0;
	float tear_band = 0.0;
	if (use_glitch_texture && omega_active > 0.5) {
		vec3 gn = texture(glitch_noise, vec2(uv.x, fract(uv.y + floor(TIME * 12.0) * 0.13))).rgb;
		block_glitch = step(0.78, gn.r) * leak_severity;
		tear_band = (gn.g - 0.5) * 0.018 * leak_severity;
	}

	// Tremor Vermelho da Diretriz Ômega quando a Contagem de Vazamento chega a 3+
	float glitch_offset = (sin(TIME * 42.0 + uv.y * 120.0) * 0.006 * omega_active) + tear_band;

	float r = texture(screen_texture, uv + vec2(glitch_offset, 0.0)).r;
	float g = texture(screen_texture, uv).g;
	float b = texture(screen_texture, uv - vec2(glitch_offset, 0.0)).b;
	vec3 raw = vec3(r, g, b);

	float luma = dot(raw, vec3(0.299, 0.587, 0.114));
	vec3 desaturated = mix(vec3(luma), raw, 0.25);
	vec3 net_tinted = mix(desaturated, net_color.rgb * luma * 1.38, 0.36);

	// Scanline vermelha + blocos corrompidos se a Ômega estiver invadindo a interface
	float scanline = step(0.92, fract(uv.y * 180.0 + TIME * 4.0)) * omega_active * 0.38;
	vec3 final_rgb = mix(net_tinted, omega_error_color.rgb, clamp(scanline + block_glitch * 0.45, 0.0, 0.85));

	COLOR = vec4(final_rgb, 1.0);
}
