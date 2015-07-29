#version 120
uniform float worldTime;
uniform float width;
uniform float height;
uniform sampler2D tex;
uniform sampler2D depthTex;

#define blurN 7

void main()
{
	float dx = 4.0 / 1920.0;
	float dy = 4.0 / 1080.0;
	
	vec4 sum = vec4(0.0);
	vec4 fin;

#if (blurN == 7)
	float[] pascal = float[blurN](1.0, 6.0, 15.0, 20.0, 15.0, 6.0, 1.0);
#endif	
#if (blurN == 9)
	float[] pascal = float[blurN](1.0, 8.0, 28.0, 56.0, 70.0, 56.0, 28.0, 8.0, 1.0);
#endif	

	for(int x = 0; x < blurN; x++)
	{
		for(int y = 0; y < blurN; y++)
		{
			fin = texture2D(tex, gl_TexCoord[0].xy + vec2((x - (blurN + 1) / 2) * dx, (y - (blurN - 1) / 2) * dy)) - vec4(0.9, 0.9, 0.9, 0.0);
			fin.r = max(0.0, fin.r);
			fin.g = max(0.0, fin.g);
			fin.b = max(0.0, fin.b);
			sum = sum + fin * pascal[x] * pascal[y];
		}
	}

#if (blurN == 7)
	sum = 10.0 * sum / 4096.0;
#endif
#if (blurN == 9)
	sum = 10.0 * sum / 65536.0;
#endif

	gl_FragColor = texture2D(tex, gl_TexCoord[0].xy);

	if(gl_FragColor.r < 0.9) {
		gl_FragColor.r += sum.r;
	}
	if(gl_FragColor.g < 0.9) {
		gl_FragColor.g += sum.g;
	}
	if(gl_FragColor.b < 0.9) {
		gl_FragColor.b += sum.b;
	}
	//gl_FragColor = sum;
}