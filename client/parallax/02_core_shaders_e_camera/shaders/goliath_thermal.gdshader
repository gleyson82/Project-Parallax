shader_type canvas_item;

// Olhar do Goliath (goliath_thermal.gdshader)
// Realça calor e pressão usando LUT térmico (38 K -> 140% Therma-Tek), ondas sísmicas no piso e vinheta de capacete pesado.

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform sampler2D thermal_lut : repeat_disable, filter_linear;
uniform bool use_thermal_lut = false;
uniform float thermal_contrast : hint_range(0.5, 2.5) = 1.45;
uniform float seismic_pulse_intensity : hint_range(0.0, 1.0) = 0.25;
uniform vec4 hot_color : source_color = vec4(1.0, 0.34, 0.13, 1.0);
uniform vec4 cold_color : source_color = vec4(0.07, 0.11, 0.18, 1.0);

void fragment() {
	vec2 uv = SCREEN_UV;
	// Ondas sísmicas concêntricas no terço inferior (vibração do piso e A Batida)
	float floor_mask = smoothstep(0.52, 1.0, uv.y);
	float dist_bottom = length(vec2((uv.x - 0.5) * 1.77, uv.y - 1.0));
	float ring_wave = sin(dist_bottom * 48.0 - TIME * 10.0) * 0.0035 * seismic_pulse_intensity * floor_mask;
	vec4 base = texture(screen_texture, uv + vec2(ring_wave, ring_wave * 0.5));

	float luma = dot(base.rgb, vec3(0.299, 0.587, 0.114));
	float heat_factor = pow(clamp(luma * thermal_contrast, 0.0, 1.0), 1.25);

	vec3 thermal_view;
	if (use_thermal_lut) {
		thermal_view = texture(thermal_lut, vec2(heat_factor, 0.5)).rgb;
	} else {
		thermal_view = mix(cold_color.rgb, hot_color.rgb, heat_factor);
	}

	vec3 final_rgb = mix(base.rgb, thermal_view, 0.52);

	// Pulso visual alaranjado na borda inferior quando A Batida ressoa
	float pulse_glow = max(0.0, sin(dist_bottom * 48.0 - TIME * 10.0)) * floor_mask * seismic_pulse_intensity * 0.22;
	final_rgb += vec3(1.0, 0.55, 0.12) * pulse_glow;

	// Vinheta superior (capacete pesado do Goliath limita visão do teto)
	float top_blindness = smoothstep(0.0, 0.28, uv.y);
	final_rgb *= mix(0.32, 1.0, top_blindness);

	COLOR = vec4(final_rgb, 1.0);
}
