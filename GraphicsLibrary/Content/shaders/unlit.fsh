#version 120

uniform float worldTime;
uniform int effects;
uniform float bL;
uniform float bW;
uniform vec3 vdirL;
uniform vec3 vdirW;
uniform vec3 cpos;
uniform mat4 crot;
varying float dopp;
uniform sampler2D tex;
uniform sampler2D shadowTex;

vec3 rainbow(float x)
{
	/*
	Target colors
	=============

	L  x   color
	0  0.0 vec4(1.0, 0.0, 0.0, 1.0);
	1  0.2 vec4(1.0, 0.5, 0.0, 1.0);
	2  0.4 vec4(1.0, 1.0, 0.0, 1.0);
	3  0.6 vec4(0.0, 0.5, 0.0, 1.0);
	4  0.8 vec4(0.0, 0.0, 1.0, 1.0);
	5  1.0 vec4(0.5, 0.0, 0.5, 1.0);
	*/
	if (x < .25) {
		return mix(vec3(0.0, 0.0, 0.0), vec3(1.0, 0.0, 0.0), x * 4.0);
	}
	if (x < .5) {
		return mix(vec3(1.0, 0.0, 0.0), vec3(0.0, 1.0, 0.0), x * 4.0 - 1.0);
	}
	if (x < .75) {
		return mix(vec3(0.0, 1.0, 0.0), vec3(0.0, 0.0, 1.0), x * 4.0 - 2.0);
	}
	return mix(vec3(0.0, 0.0, 1.0), vec3(1.0, 1.0, 1.0), x * 4.0 - 3.0);
	
}

void main()
{
	vec4 fragout = texture2D(tex, gl_TexCoord[0].xy) * gl_Color;
	/*if((effects / 2) % 2 == 1)
	{
		fragout = fragout * vec4(vec3(1.0 / sqrt(1.0 - bL * bL) + dopp), 1.0);
	}
	if((effects / 4) % 2 == 1)
	{
		vec4 shift = vec4(1.0);
		shift.r = 2 * max(0, 0.5 - abs(dopp + 0.0)) * fragout.r + 2 * max(0, 0.5 - abs(dopp + 0.5)) * fragout.g + 2 * max(0, 0.5 - abs(dopp + 1.0)) * fragout.b;
		shift.g = 2 * max(0, 0.5 - abs(dopp - 0.5)) * fragout.r + 2 * max(0, 0.5 - abs(dopp + 0.0)) * fragout.g + 2 * max(0, 0.5 - abs(dopp + 0.5)) * fragout.b;
		shift.b = 2 * max(0, 0.5 - abs(dopp - 1.0)) * fragout.r + 2 * max(0, 0.5 - abs(dopp - 0.5)) * fragout.g + 2 * max(0, 0.5 - abs(dopp + 0.0)) * fragout.b;
		fragout = shift;
	}*/
	//fragout.xyz = rainbow(fragout.x);
	gl_FragColor = fragout;
}