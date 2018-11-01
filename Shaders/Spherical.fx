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

float4 Interpolation(float3 IN, float percentage);
float4 Interpolation(float3 IN, float percentage)
{
   float4 color1,color2; 
   color1 = tex3D(VolumeSampler, IN);
   color2 = tex3D(VolumeSampler2, IN);
   color1 = lerp(color1, color2, percentage);
   return color1;
}

PS_OUTPUT PS(PS_INPUT IN)
{
    PS_OUTPUT OUT;
	//tmin=float3(0,0,0);
    //tmax=float3(400,250,1000);
    //dmin=float3(0,0,0);
    //dmax=float3(200,300,1000);
    //permutation=float3(1,0,2);
    //clip=float3(1,0,1);

    //float pi=3.14159;
	//float3 cartesian=permute(IN.texturecoord.xyz);
	//float3 spherical;
	
	//cartesian=dmax.z * (cartesian * 2.0 - 1.0);
	//spherical.z=length(cartesian.xyz);
	
	//float4 intensity=float4(0,0,0,0);
	//if(spherical.z > dmax.z || spherical.z < dmin.z)
	//{
	//   OUT.color=float4(0,1,0,1);
	//}
	//else
	//{
	   //spherical.x = asin(clamp(cartesian.y / length(cartesian.yz), -1.0, 1.0));
	  // spherical.y = acos(clamp(cartesian.x / spherical.z, -1.0, 1.0));
	  // if(cartesian.z >= 0.0)
	  // {
	  //    if(spherical.x < 0.0) spherical.x += 2.0*pi;
	  // }
	  // else
	  // {
	  //    spherical.x = pi - spherical.x;
	  // }
	   
	  // spherical.y = (spherical.y / (pi));
	 //  spherical.x = (spherical.x / (2.0*pi));
	  // if ((clip.y && (spherical.y < dmin.y || spherical.y > dmax.y)) || (clip.x && (spherical.x < dmin.x || spherical.x > dmax.x)))
	  // {
	  //    OUT.color=float4(1,0,0,1);
	  // }
	  // else
	  // {
	  //    spherical = (spherical - dmin) * ((tmax - tmin) / (dmax - dmin))+tmin;
	//	  intensity = float4(tex3D(VolumeSampler, invPermute(spherical)));
	//	  OUT.color=float4(tex1D(ColorMapSampler, intensity.x));
	  // }
	//}
    float4 color1,color2;
	color1 = tex3D( VolumeSampler, IN.texturecoord );
	color2 = tex3D( VolumeSampler2, IN.texturecoord );
	color1 = lerp( color1, color2, FramePercentage);
	if(color1.r<=0.01) OUT.color=float4(0,0,0,0);
	else OUT.color = tex1D(ColorMapSampler, color1.r);
	OUT.color = float4(IN.texturecoord.x*100,IN.texturecoord.y,IN.texturecoord.z,1);
	OUT.color = float4(1,0,0,1);
	return OUT;
}

technique Spherical
{
    pass Pass0
    {
        PixelShader  = compile ps_3_0 PS();
    }
}