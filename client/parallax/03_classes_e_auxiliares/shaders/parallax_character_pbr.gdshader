shader_type spatial;
render_mode cull_back, diffuse_burley, specular_schlick_ggx;

// Shader de Look-Dev 2.5D para Personagens de PARALLAX (Godot 4.6+)
// Garante recorte imediato de silhueta em câmera ortográfica (Rim Light direcional por classe)
// e estado dinâmico de Calor / Geada ligado ao ThermalNode.gd (38 K -> 750 K).

uniform vec4 albedo_tint : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float metallic_val : hint_range(0.0, 1.0) = 0.65;
uniform float roughness_val : hint_range(0.0, 1.0) = 0.42;

uniform vec4 rim_color : source_color = vec4(1.0, 0.55, 0.22, 1.0);
uniform float rim_power : hint_range(1.0, 6.0) = 2.8;
uniform float rim_strength : hint_range(0.0, 3.0) = 0.85;

// Estado térmico físico: -1.0 = congelado a 38 K (geada), 0.0 = neutro, +1.0 = superaquecido
uniform float thermal_state : hint_range(-1.0, 1.0) = 0.0;
uniform vec4 frost_tint : source_color = vec4(0.55, 0.90, 1.0, 1.0);
uniform vec4 overheat_tint : source_color = vec4(1.0, 0.32, 0.05, 1.0);

void fragment() {
	vec3 base_col = albedo_tint.rgb;
	float frost_mask = clamp(-thermal_state, 0.0, 1.0);
	float heat_mask = clamp(thermal_state, 0.0, 1.0);

	// Acúmulo de geada nas superfícies voltadas para cima/bordas a 38 K
	float fresnel = pow(1.0 - clamp(dot(NORMAL, VIEW), 0.0, 1.0), rim_power);
	base_col = mix(base_col, frost_tint.rgb, frost_mask * (0.35 + fresnel * 0.55));

	ALBEDO = base_col;
	METALLIC = mix(metallic_val, 0.15, frost_mask * 0.7);
	ROUGHNESS = mix(roughness_val, 0.22, frost_mask * 0.6);

	// Rim light de recorte 2.5D + incandescência térmica quando superaquecido
	vec3 rim_emit = rim_color.rgb * fresnel * rim_strength;
	vec3 heat_emit = overheat_tint.rgb * heat_mask * (0.4 + fresnel * 1.4);
	EMISSION = rim_emit + heat_emit;
}
