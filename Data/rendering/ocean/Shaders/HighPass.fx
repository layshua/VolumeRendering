// -------------------------------------------------------------
// Textures & Samplers
// -------------------------------------------------------------
texture texSource;
sampler sampSource = sampler_state {
	Texture = (texSource);
};

// -------------------------------------------------------------
// Constants
// -------------------------------------------------------------
// The Y luminance transformation used follows that used by TIFF and JPEG (Rec 601-1)
const float3 luminanceFilter = { 0.2989, 0.5866, 0.1145 };

// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
float threshold = 0.5f;

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
	float normalizationFactor = 1 / (1 - threshold);

	float3 sample = tex2D(sampSource, IN.texCoord).rgb;
	float greyLevel = mul(sample, luminanceFilter);
	float3 desaturated = lerp(sample, greyLevel.rrr, threshold);
			  
	return float4((desaturated - threshold) * normalizationFactor, 1);
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