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

varying vec3 cameradir;

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
	
	vec3 sundir = normalize(gl_LightSource[0].position.xyz);


	float sun_angle_cos = dot(sundir, cameradir);
	float sunsq = (sun_angle_cos > 0.0 ? sun_angle_cos * sun_angle_cos : 0.0);
	float rayleighphase = (1.0 + sunsq) / 2.0;
	float irradiance = dot(sundir, vec3(0.0, 1.0, 0.0)) + 0.3;

	float optical_depth = 1000.0;

	fragout.rgb = vec3(0.0);

	float the_integral = 0.0;

	vec3 cn = normalize(cameradir);
	for(float h = 0.0; h < 12345.0; h += 1000.0)
	{
		the_integral += exp(-(h*cn.y) / optical_depth);
	}

	fragout.r = .10 * rayleighphase * the_integral;
	fragout.g = .15 * rayleighphase * the_integral;
	fragout.b = .25 * rayleighphase * the_integral;
	

	fragout.r = 0.4 * sunsq / (0.5 + the_integral * 0.05) + 0.4 * (the_integral + 2.0) * .17 * irradiance;
	fragout.g = 0.4 * sunsq / (0.5 + the_integral * 0.15) + 0.5 * (the_integral + 2.0) * .17 * irradiance;
	fragout.b = 0.4 * sunsq / (0.5 + the_integral * 0.35) + 0.8 * (the_integral + 2.0) * .17 * irradiance;

	if (cameradir.y < 0) {
		fragout.rgb = vec3(0.2, 0.25, 0.3) * irradiance;
	}

	//fragout.xyz = rainbow(fragout.x);
	//fragout.xyz = cameradir;
	//fragout.xyz = vec3(rayleighphase)/2.0;
	//fragout.xyz = vec3(the_integral * 0.35);
	gl_FragColor = fragout;
}