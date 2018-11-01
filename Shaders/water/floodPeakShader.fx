float4x4 worldViewProjection;  //复合变换矩阵
float4x4 world;                //世界矩阵
float4   AmbientLight;         //环境光
float4   DirectionalLight;     //定向光
float4   vecLightDir;          //定向光源方向
float4   materialAmbient;	//材质反射环境光系数
float4   materialDiffuse;	//材质漫反射系数

float4   vecEye;              	//观察点位置
float4   materialSpecular;    	//材质镜面反射系数

//颜色纹理
texture ColorTexture;

struct VS_OUTPUT
{
    float4 Pos:           POSITION;
    float2 colorTexCoord: TEXCOORD0;
    
};

struct PS_OUTPUT
{
	float4 Color:   COLOR;
	
};

sampler ColorTextureSampler=sampler_state
{
      Texture=<ColorTexture>;//纹理采样器
      MinFilter=LINEAR;//缩小图形使用线性滤波
      MagFilter=LINEAR;//放大图形使用线性滤波
      MipFilter=LINEAR;//Mipmap使用线性滤波
      AddressU=Wrap;//U、V方向上的纹理寻址模式都采用Wrap方式
      AddressV=Wrap;
};

VS_OUTPUT VS(float4 Pos: POSITION,float2 colorTexUV : TEXCOORD0 )         //顶点着色方法
{
   VS_OUTPUT Out = (VS_OUTPUT) 0;               
   Out.Pos = mul(Pos, worldViewProjection);                      //计算顶点位置

   //float4 lightDir  = normalize(vecLightDir);                    //得到单位光照向量
   // float4 normalWorld=normalize(mul(Normal,world));              //得到单位法线向量
   //float4 diff = saturate( dot(normalWorld, lightDir) );	 //A1=N点乘L=cosα
   //Out.Color =  AmbientLight *materialAmbient+DirectionalLight*diff *materialDiffuse;
   
   Out.colorTexCoord=colorTexUV;

   return Out;
}

float4 PS( VS_OUTPUT vsout ) : COLOR					//像素着色方法
{   
    float4 textureColor=tex2D(ColorTextureSampler, vsout.colorTexCoord);
    //float4 resultColor=lerp(textureColor,vsout.Color,0.6f);
    //textureColor*0.8+vsout.Color*0.2;
    //float4 resultColor=textureColor*vsout.Color;
    return textureColor;
}


technique RenderFloodPeak
{   pass P0
    {   
                VertexShader = compile vs_3_0 VS();
    		PixelShader=compile ps_3_0 PS();   
    }
}
