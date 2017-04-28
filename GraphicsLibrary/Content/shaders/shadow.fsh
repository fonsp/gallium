#version 120
#extension GL_EXT_gpu_shader4 : enable

uniform sampler2D tex;

void main()
{
	//gl_FragColor = vec4(.1,.3,.7,1.0);
	//gl_FragColor = texture2D(tex, gl_TexCoord[0].xy);
}