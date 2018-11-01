texture VolumeTex;
texture VolumeTex2;
texture TransferMap;
float FramePercentage;
float kd;
float ka;
float ks;
float expS;
float xcull;
float ycull;

float3 lightDirection;
float3 dimensions;
float4x4 viewMatrix;
float4x4 WorldViewProj;
float4x4 worldMatrix;
float4x4 normalMat;

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

sampler TransferMapSampler = sampler_state
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
    float4 color          : COLOR0;
};

struct PS_OUTPUT
{
    float4 color : COLOR;
};

struct VS_INPUT
{
    float4 position      : POSITION;
    float4 texturecoord  : TEXCOORD0;
    float4 color         : COLOR0;
};

struct VS_OUTPUT
{
    float4 position      : POSITION;
    float4 texturecoord  : TEXCOORD0;
    float4 color         : COLOR0;
    float4 ecPosition    : COLOR1;
};

VS_OUTPUT VS_Lighting(VS_INPUT IN)
{
    VS_OUTPUT OUT;
    OUT.position= mul(IN.position,WorldViewProj);
    OUT.ecPosition= mul(mul(worldMatrix,viewMatrix),IN.position);
    OUT.color=IN.color;
    OUT.texturecoord=IN.texturecoord;
    return OUT;
}

float3 Gradient(VS_OUTPUT IN);
PS_OUTPUT PS_Lighting(VS_OUTPUT IN)
{  
    ka=kd=0.5f;
    ks=0.5f;
    expS=100.0f;
 
    PS_OUTPUT    OUT; 
    float4 color1,color2; 
    color1 = tex3D(VolumeSampler, IN.texturecoord.xyz);
    color2 = tex3D(VolumeSampler2, IN.texturecoord.xyz);
    color1 = lerp( color1, color2, FramePercentage);

    OUT.color = tex1D(TransferMapSampler, color1.x);
    //OUT.color=color1;
       
    float3x3 normalMat33=normalMat;

    float4 lightColor = float4(1.0f,1.0f,1.0f,1.0f);
    float3 gradient = normalize(mul(normalMat33,Gradient(IN)));

    float3 lightVec=normalize(lightDirection);
    float3 halfv=reflect(-lightVec,gradient);
    float3 viewVec=normalize(-IN.ecPosition).xyz;
   
    float diffuse  = abs(dot(lightVec, gradient));
       
    float specular = 0.0;
    if(diffuse>0.0)
    {
        specular = pow(abs(dot(halfv, viewVec)), expS);
    }
    diffuse=kd*diffuse;
    specular=ks*specular;
    OUT.color.rgb=OUT.color.rgb *(ka+diffuse)+specular*lightColor.rgb;
	if(IN.texturecoord.y<ycull || IN.texturecoord.x>xcull) OUT.color.a=0.0f;
	
    return OUT;
}

float4 Interpolation(float3 IN, float percentage);
float4 Interpolation(float3 IN, float percentage)
{
   float4 color1,color2;
   color1 = tex3D(VolumeSampler, IN);
   color2 = tex3D(VolumeSampler2, IN);
   color1 = lerp(color1, color2, percentage);
   return color1;
}

//--------------------------------------------
//Compute the gradient using the data
//--------------------------------------------
float3 Gradient(VS_OUTPUT IN)
{
    float3 gradient;
    float dx=0.5f/(dimensions.x);
    float dy=0.5f/(dimensions.y);
    float dz=0.5f/(dimensions.z);
   
    float3 a0;
    float3 a1;
    
	a0.x=Interpolation(IN.texturecoord.xyz+float3(dx,0,0), FramePercentage).x;
	a1.x=Interpolation(IN.texturecoord.xyz+float3(-dx,0,0), FramePercentage).x;
    
	a0.y=Interpolation(IN.texturecoord.xyz+float3(0,dy,0), FramePercentage).x;
	a1.y=Interpolation(IN.texturecoord.xyz+float3(0,-dy,0), FramePercentage).x;
   
	a0.z=Interpolation(IN.texturecoord.xyz+float3(0,0,dz), FramePercentage).x;
    a1.z=Interpolation(IN.texturecoord.xyz+float3(0,0,-dz), FramePercentage).x;
    
    gradient=(a1-a0)/2.0;
    return gradient;
}

PS_OUTPUT PS(PS_INPUT IN)
{
	PS_OUTPUT    OUT;
	float4 color1,color2;
	color1 = tex3D(VolumeSampler, IN.texturecoord);
	color2 = tex3D(VolumeSampler2, IN.texturecoord);
	color1 = lerp(color1, color2, FramePercentage);
	//use transfer
	if(color1.r<=0.01) OUT.color=float4(0,0,0,0);
	else OUT.color = tex1D(TransferMapSampler,color1.r);
        if(IN.texturecoord.y<ycull && IN.texturecoord.x>xcull) OUT.color.a=0.0f;
	return OUT;
}

technique TextureTD
{
    pass Pass0
    {
        PixelShader  = compile ps_2_0 PS();   
    }
}

technique TextureTD_Lighting
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 VS_Lighting();
        PixelShader  = compile ps_2_0 PS_Lighting();
    }
}