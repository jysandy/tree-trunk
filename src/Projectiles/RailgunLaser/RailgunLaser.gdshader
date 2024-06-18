shader_type canvas_item;

uniform float beam_width;
uniform vec4 beam_color;
uniform float alpha_mul_min;
varying vec2 local_vertex;
uniform float flicker_frequency;

void vertex() {
	local_vertex = VERTEX;
}

void fragment() {
	COLOR = beam_color;
	COLOR.a = 1.0 - (abs(local_vertex.y) / (beam_width / 2.0));
	COLOR.rgb *= 1.4;
	COLOR.a *= ((1.0 - alpha_mul_min) / 2.0) * cos(flicker_frequency * TIME) + ((1.0 + alpha_mul_min) / 2.0);
}
