shader_type canvas_item;

varying vec2 local_position;
uniform vec4 color;
uniform float radius;
uniform float alpha_mul_min;
uniform float flicker_frequency;

void vertex() {
	local_position = VERTEX;
}

void fragment() {
	COLOR = color;
	COLOR.a = 1.0 - length(local_position) / radius;
	COLOR *= 1.4;
	COLOR.a *= ((1.0 - alpha_mul_min) / 2.0) * cos(flicker_frequency * TIME) + ((1.0 + alpha_mul_min) / 2.0);
}
