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

varying vec3 pos_rel_to_cam;

varying float intensity;

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
	

	gl_FrontColor = vec4(gl_Color.xyz, 1.0 / (gl_Position.w / 2000.0 + 0.9));
	gl_TexCoord[0] = gl_MultiTexCoord0;
}