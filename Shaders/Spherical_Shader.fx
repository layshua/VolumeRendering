texture TransferMap;
texture VolumeTex;
texture VolumeTex2;
float FramePercentage;
float3 tmin;
float3 tmax;
float3 dmin;
float3 dmax;
float3 permutation;
float2 clip;

sampler VolumeSampler = sampler_state
{  
    Texture   = (VolumeTex);  
    MagFilter = Anisotropic;     
    MinFilter = Anisotropic;
	MipFilter = Anisotropic;
    AddressU  = CLAMP;  
    AddressV  = CLAMP;  
    AddressW  = CLAMP;
};

sampler VolumeSampler2 = sampler_state
{  
    Texture   = (VolumeTex2);  
    MagFilter = Anisotropic;     
    MinFilter = Anisotropic;
	MipFilter = Anisotropic;
    AddressU  = CLAMP;  
    AddressV  = CLAMP;  
    AddressW  = CLAMP;
};

sampler ColorMapSampler = sampler_state
{  
    Texture   = (TransferMap);  
    MagFilter = Anisotropic;     
    MinFilter = Anisotropic;
	MipFilter = Anisotropic;
    AddressU  = CLAMP;  
    AddressV  = CLAMP;  
    AddressW  = CLAMP;
};

struct PS_INPUT
{
    float4 hposition     : POSITION;  
    float4 texturecoord  : TEXCOORD0;  
    float4 color         : COLOR0;
};

struct PS_OUTPUT
{
    float4 color : COLOR;
};

PS_OUTPUT PS(PS_INPUT IN)
{
    PS_OUTPUT OUT;
	
    float4 color1,color2;
	color1 = tex3D( VolumeSampler, IN.texturecoord );
	color2 = tex3D( VolumeSampler2, IN.texturecoord );
	color1 = lerp( color1, color2, FramePercentage);
	if(color1.r<=0.01) OUT.color=float4(0,0,0,0);
	else OUT.color = tex1D(ColorMapSampler, color1.r);
	return OUT;
}

technique Spherical
{
    pass Pass0
    {
        PixelShader  = compile ps_2_0 PS();
    }
}