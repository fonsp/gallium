#version 120
#extension GL_EXT_gpu_shader4 : enable

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

varying vec3 cameradir;

void main()
{
	vec4 v = gl_Vertex;
	v.xyz = v.xyz - cpos;
	v = gl_ModelViewMatrix * v;
	
	v = crot * v;
    gl_Position = gl_ProjectionMatrix * v;
	gl_FrontColor = gl_Color;
	gl_TexCoord[0] = gl_MultiTexCoord0;

	

	cameradir = normalize(gl_Vertex.xyz);

}