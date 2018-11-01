float4x4 modelViewProj;

struct VS_OUTPUT
{
    float4 pos  : POSITION;
    float4 color : COLOR0;
};

VS_OUTPUT VS(float4 position : POSITION, float4 color : COLOR0)
{
    VS_OUTPUT OUT = (VS_OUTPUT)0;
    OUT.pos=mul(position,modelViewProj);
	OUT.color=color;
    return OUT;
}

float4 PS(float4 inColor:COLOR0):COLOR0
{  
   float4 color=inColor;
   return color;
}

technique Coord
{  
    pass Pass0  
    {   
        VertexShader = compile vs_2_0 VS(); 
		PixelShader  = compile ps_2_0 PS();
    }
}