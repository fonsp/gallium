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
uniform mat4 shadowProj;
uniform sampler2D shadowTex;

varying vec3 pos_rel_to_cam;

varying float intensity;
varying vec3 shadowCoord;

float smoothslap(float x) {
	if (x < .02) {
		return 10 * x;
	}
	if (x < .1) {
		return 2.5*x + 0.15;
	}
	if (x < .5) {
		return x + 0.3;
	}
	return 0.4*x + 0.6;
	
	return sqrt(x);
}

float smoothslaps(float x) {
	//return x;
	if (x < 0) {
		return -smoothslap(-x);
	}
	return smoothslap(x);
}

void main()
{
	//intensity = (dot(gl_LightSource[0].position.xyz, (gl_ModelViewMatrix * vec4(gl_Normal, 0.0)).xyz) + 1.0) / 2.0;
	intensity = dot(gl_LightSource[0].position.xyz, normalize((gl_ModelViewMatrix * vec4(gl_Normal, 0.0)).xyz));

	vec4 v = gl_Vertex;
	v.xyz = v.xyz - cpos;
	v = gl_ModelViewMatrix * v;
	/*if(bL > 0.001)
	{
		dopp = (dot(v.xyz, vdirL) / length(v.xyz)) * (bL / sqrt(1.0 - bL * bL));
	} else {
		dopp = 0.0;
	}
	if(bW > 0.001)
	{
		if((effects / 1) % 2 == 1)
		{
			float oldlength = length(v.xyz);
			v.xyz = v.xyz + vdirW * bW * length(v.xyz);
			v.xyz = v.xyz * (oldlength / length(v.xyz));
		}
	}*/
	pos_rel_to_cam = v.xyz;
	v = crot * v;
    gl_Position = gl_ProjectionMatrix * v;
	


	gl_FrontColor = gl_Color;
	gl_TexCoord[0] = gl_MultiTexCoord0;

	shadowCoord = (shadowProj * (gl_ModelViewMatrix * gl_Vertex)).xyz;
	shadowCoord.x = smoothslaps(shadowCoord.x);
	shadowCoord.y = smoothslaps(shadowCoord.y);
	shadowCoord.xyz = shadowCoord.xyz * 0.5 + vec3(0.5);
}

