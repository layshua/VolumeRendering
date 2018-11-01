// -------------------------------------------------------------
// Those settings are (and must be) set by the host program
// -------------------------------------------------------------
//#define BLOOM_STEPS 2
//#define BLOOM_STEPS 3
//#define BLOOM_STEPS 4

// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
texture texBloom0, texBloom1, texBloom2, texBloom3;
sampler sampBloom[BLOOM_STEPS] =
{
	sampler_state {
		Texture = (texBloom0);
		AddressU = CLAMP;
		AddressV = CLAMP;	
	},
	sampler_state {
		Texture = (texBloom1);
		AddressU = CLAMP;
		AddressV = CLAMP;	
	},
	#if BLOOM_STEPS >= 3
		sampler_state {
			Texture = (texBloom2);
			AddressU = CLAMP;
			AddressV = CLAMP;	
		},	
		#if BLOOM_STEPS == 4
			sampler_state {
				Texture = (texBloom3);
				AddressU = CLAMP;
				AddressV = CLAMP;	
			}
		#endif
	#endif
};

texture texFrameBuffer;
sampler sampFrameBuffer = sampler_state {
	Texture = (texFrameBuffer);
};

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_INPUT
{
	float4 position : POSITION;
	float2 texCoord : TEXCOORD;
};
struct VS_OUTPUT
{
	float4 position : POSITION;
	float2 texCoord : TEXCOORD;	
};
#define PS_INPUT VS_OUTPUT

// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
float bloomFactor = 1;
float bloomWidth = 1;
float sceneIntensity = 1;
float2 bloomTexCoordOffset[BLOOM_STEPS];
float2 frameBufferTexCoordOffset;

// -------------------------------------------------------------
// Vertex Shader
// -------------------------------------------------------------
VS_OUTPUT VS(const VS_INPUT IN)
{
	VS_OUTPUT OUT;
	
	OUT.position = IN.position;
	OUT.texCoord = IN.texCoord;

	return OUT;
}

// -------------------------------------------------------------
// Pixel Shaders
// -------------------------------------------------------------
float4 PS(const PS_INPUT IN) : COLOR
{
	// The following "large" and "small" weights are exponential
	// They could be set via parameters if needed...
	#if BLOOM_STEPS == 2
		float2x3 bloomSamples;	
		float2 weights = lerp(
				float2(2, 1),
				float2(1, 2),
				bloomWidth) / 3.0f;
	#endif
	#if BLOOM_STEPS == 3
		float3x3 bloomSamples;	
		float3 weights = lerp(
				float3(4, 2, 1),
				float3(1, 2, 4),
				bloomWidth) / 7.0f;
	#endif
	#if BLOOM_STEPS == 4
		float4x3 bloomSamples;	
		float4 weights = lerp(
				float4(8, 4, 2, 1),
				float4(1, 2, 4, 8),
				bloomWidth) / 15.0f;
	#endif
	
	weights *= bloomFactor;
	
	for(int i=0; i<BLOOM_STEPS; i++)
		bloomSamples[i] = tex2D(sampBloom[i], IN.texCoord * (1 - bloomTexCoordOffset[i] * 2) + bloomTexCoordOffset[i]).rgb;
	
	float3 combinedBloom = mul(weights, bloomSamples);
	
	float3 frameBufferSample = tex2D(sampFrameBuffer, IN.texCoord + frameBufferTexCoordOffset).rgb;
		  
	return float4(lerp(combinedBloom, combinedBloom + frameBufferSample, sceneIntensity), 1);
}

// -------------------------------------------------------------
// Techniques
// -------------------------------------------------------------
technique TSM3
{
	pass P
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}
technique TSM2a
{
	pass P
	{
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_a PS();
	}
}
technique TSM2b
{
	pass P
	{
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_b PS();
	}
}
technique TSM2
{
	pass P
	{
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_0 PS();
	}
}