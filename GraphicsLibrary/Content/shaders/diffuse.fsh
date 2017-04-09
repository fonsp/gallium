#version 120
#extension GL_EXT_gpu_shader4 : enable

uniform float worldTime;
uniform int effects;
//uniform float bL;
//uniform float bW;
//uniform vec3 vdirL;
//uniform vec3 vdirW;
uniform vec3 cpos;
uniform mat4 crot;
uniform sampler2D tex;
uniform float fogStart;
uniform float fogEnd;
uniform vec4 fogColor;

varying vec3 pos_rel_to_cam;
varying float intensity;

void main()
{
	vec4 fragout = vec4(vec3(max(sqrt(intensity), 0.0)), 1.0) * gl_LightSource[0].diffuse + gl_LightSource[0].ambient;	// light
	fragout *= texture2D(tex, gl_TexCoord[0].xy);																		// texture
	//fragout *= vec4(vec3(gl_Color.w), 1.0);																				// fog
	fragout *= vec4(gl_Color.xyz, 1.0);																					// color
	//fragout.w = 1.0;

	/*if((effects / 2) % 2 == 1)
	{
		fragout = fragout * vec4(vec3(1.0 / sqrt(1.0 - bL * bL) + dopp), 1.0);
	}
	if((effects / 4) % 2 == 1)
	{
		vec4 shift = vec4(1.0);
		shift.r = 2 * max(0.0, 0.5 - abs(dopp + 0.0)) * fragout.r + 2 * max(0.0, 0.5 - abs(dopp + 0.5)) * fragout.g + 2 * max(0.0, 0.5 - abs(dopp + 1.0)) * fragout.b;
		shift.g = 2 * max(0.0, 0.5 - abs(dopp - 0.5)) * fragout.r + 2 * max(0.0, 0.5 - abs(dopp + 0.0)) * fragout.g + 2 * max(0.0, 0.5 - abs(dopp + 0.5)) * fragout.b;
		shift.b = 2 * max(0.0, 0.5 - abs(dopp - 1.0)) * fragout.r + 2 * max(0.0, 0.5 - abs(dopp - 0.5)) * fragout.g + 2 * max(0.0, 0.5 - abs(dopp + 0.0)) * fragout.b;
		fragout = shift;
	}*/

	float a = max(0.0, fragout.r - 1.0);
	fragout.g += a;
	fragout.b += a;
	a = max(0.0, fragout.g - 1.0);
	fragout.r += a;
	fragout.b += a;
	a = max(0.0, fragout.b - 1.0);
	fragout.r += a;
	fragout.g += a;

	//fragout += vec4(vec3(a), 0.0);

	//float dist = gl_FragCoord.z / gl_FragCoord.w;
	float dist = length(pos_rel_to_cam);
	gl_FragColor = mix(fragout, fogColor, max(0.0, min(1.0, (dist - fogStart) / (fogEnd - fogStart))));
	//gl_FragColor.rgb = pos_rel_to_cam.xyz / 10.0;
	//gl_FragColor = vec4(vec3(), 1.0);
}