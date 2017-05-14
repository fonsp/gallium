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
uniform sampler2D shadowTex;
uniform float fogStart;
uniform float fogEnd;
uniform vec4 fogColor;

varying vec3 pos_rel_to_cam;
varying float intensity;
varying vec3 shadowCoord;

uniform int enableTextures;

float scale(float x){
	float z = abs(x*2.0 - 1.0);
	if(z < 0.02){
		return 10;
	}
	if(z < 0.1){
		return 2.5;
	}
	if(z < 0.5){
		return 1.0;
	}
	return .4;
}

void main()
{
	const vec2 poissonDisk[4] = vec2[](
		vec2( -0.94201624, -0.39906216 ),
		vec2( 0.94558609, -0.76890725 ),
		vec2( -0.094184101, -0.92938870 ),
		vec2( 0.34495938, 0.29387760 )
	);
	vec4 fragout =  gl_LightSource[0].ambient;	// light
	vec4 texcolor;
	if(enableTextures > 0)
	{
		texcolor = texture2D(tex, gl_TexCoord[0].xy);
	} else {
		texcolor = vec4(1.0);
	}
	fragout *= texcolor;																									// texture
	
	
	vec4 diffuse = vec4(vec3(pow(intensity,1)), 1.0) * gl_LightSource[0].diffuse * texcolor;
	if(intensity > 0){
		if(shadowCoord.x <= 1.0 && shadowCoord.x >= 0.0 && shadowCoord.y <= 1.0 && shadowCoord.y >= 0.0)
		{
			float realDepth = shadowCoord.z;
			
			/*int litCount = 0;
			for(int i = 0; i < 4; i++){
				float shadowDepth = texture2D(shadowTex, shadowCoord.xy + poissonDisk[i]/vec2(scale(shadowCoord.x), scale(shadowCoord.y))/5000.0).r;
				if(realDepth - shadowDepth < .002/min(scale(shadowCoord.y),scale(shadowCoord.x))){
					litCount++;
				}
			}
			fragout.xyz += diffuse.xyz * litCount / 4.0;*/

			float shadowDepth = texture2D(shadowTex, shadowCoord.xy).r;
			if(realDepth - shadowDepth < .0008 / min(scale(shadowCoord.x), scale(shadowCoord.y))){
				fragout.xyz += diffuse.xyz;
			}
		} else {
			fragout.xyz += diffuse.xyz;
		}
	}
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
	
	
	fragout.xyz *= gl_Color.xyz;
	fragout.a = gl_Color.a;

	//float dist = gl_FragCoord.z / gl_FragCoord.w;
	float dist = length(pos_rel_to_cam);
	gl_FragColor.xyz = mix(fragout.xyz, fogColor.xyz, max(0.0, min(1.0, (dist - fogStart) / (fogEnd - fogStart))));
	gl_FragColor.a = fragout.a;
	//gl_FragColor.rgb = pos_rel_to_cam.xyz / 10.0;
	//gl_FragColor = vec4(vec3(), 1.0);
	//gl_FragColor = mix(fragout, texture2D(shadowTex, gl_TexCoord[0].xy) - vec4(.5), .5);// + vec4(.5);
	//gl_FragColor = vec4(vec3(atan(texture2D(shadowTex, gl_TexCoord[0].xy).r)),1);
	//gl_FragColor = texture2D(shadowTex, gl_TexCoord[0].xy);
	//gl_FragColor = fragout;
	
	
}