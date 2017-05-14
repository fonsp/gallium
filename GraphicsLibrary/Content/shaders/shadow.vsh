#version 120

uniform mat4 depthProj;

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
	//gl_Position = depthProj * gl_Vertex;
	
	vec4 v = gl_Vertex;
	v = gl_ModelViewMatrix * v;

	gl_Position = gl_ProjectionMatrix * v;

	gl_Position.x = smoothslaps(gl_Position.x);
	gl_Position.y = smoothslaps(gl_Position.y);
	
	//gl_FrontColor = gl_Color;
	//gl_TexCoord[0] = gl_MultiTexCoord0;
}

