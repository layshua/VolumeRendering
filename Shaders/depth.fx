float4x4 modelViewProj;
float4x4 matWorld;
float4 eyePos;

struct PS_OUTPUT
{   
    float4 color : COLOR0;
};

struct VS_INPUT
{
    float4 position : POSITION;
};

struct VS_OUTPUT
{
    float4 pos : POSITION;
    float  depth: TEXCOORD0;
};

VS_OUTPUT VS_Lighting(VS_INPUT IN)
{
    VS_OUTPUT OUT=(VS_OUTPUT)0;
    OUT.pos=mul(IN.position,modelViewProj);
    OUT.depth=OUT.pos.z;
    return OUT;
}

PS_OUTPUT PS_Lighting(VS_OUTPUT IN)
{  
   PS_OUTPUT OUT=(PS_OUTPUT)0;
   float depth = IN.depth;
   OUT.color=float4(depth, depth, depth, depth);
   return OUT;
}

technique depthBuffer
{  
    pass Pass0  
    {   
        VertexShader = compile vs_2_0 VS_Lighting();
        PixelShader  = compile ps_2_0 PS_Lighting();   
    }
}