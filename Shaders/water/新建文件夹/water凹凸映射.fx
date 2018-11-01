//float4x4 WorldViewProject:	WORLDVIEWPROJECTION;
float4x4 World:	WORLD;   //世界矩阵
float4x4 View:	VIEW;   //视矩阵
float4x4 Projection: PROJECTION; // 投影矩阵
float4x4 ReflectionView; //反射视矩阵

float4 CameraPos;	//相机位置

texture BumpTexture;//水面的凹凸贴图
texture BumpTexture1;//水面的凹凸贴图

texture ReflectTexture;//水面反射纹理
texture HeightTexture;//泡沫纹理

float WaveHeight;                           // 振幅
float WaveLength;                           //波长

//float4 WindDirection;                       //风向
float WindForce;               		    //风力

float     Time;                             // 时间


float3 LightDirection; //光源方向
float4 AmbientColor;        //环境光
float4 DiffuseColor;        //漫反射光
float AmbientIntensity;//材质反射环境光衰减系数
float DiffuseIntensity;//材质漫反射衰减系数

struct VS_OUTPUT
{
    float4 Position                  : POSITION;
    float4 ReflectionMapSamplingPos  : TEXCOORD1;
    float2 BumpMapSamplingPos        : TEXCOORD2;
    float4 HeightSamplingPos         : TEXCOORD3;
    float3 LightDirT		     : TEXCOORD4;
    float3 ViewDirT	             : TEXCOORD5;
    float3 h			     : TEXCOORD6;
}; 

struct PS_OUTPUT
{
	float4 Color:	COLOR;
	
};
sampler ReflectionSampler = sampler_state  //反射采样器
{
	Texture = <ReflectTexture>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
        AddressU = wrap; 
	AddressV = wrap;
};

sampler BumpMapSampler= sampler_state  //凹凸纹理采样器
{ 
	texture = <BumpTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter=LINEAR; 
	AddressU = wrap; 
	AddressV = wrap;
};

sampler BumpMapSampler1= sampler_state  //凹凸纹理采样器
{ 
	texture = <BumpTexture1>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter=LINEAR; 
	AddressU = wrap; 
	AddressV = wrap;
};

sampler HeightSampler = sampler_state  //高度图采样器
{ 
	texture = <HeightTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter=LINEAR; 
	AddressU = wrap; 
	AddressV = wrap;
};

VS_OUTPUT VS ( float4 inPos : POSITION0,float3 inNormal:NORMAL,float3 inTangent:TANGENT, float3 inBinormal:BINORMAL,float2 inTex : TEXCOORD0,float4 inriverdir :POSITION1, float3 h : TEXCOORD1)
{
	float4	WindDirection=inriverdir;
	VS_OUTPUT Out = (VS_OUTPUT)0;

	float4x4 preViewProjection = mul (View,Projection); 
	float4x4 preWorldViewProjection = mul(World,preViewProjection);

	Out.Position = mul(inPos,preWorldViewProjection);

	float4 WorldPosition = mul(inPos,World);
	//计算反射纹理的采样坐标
	float4x4 preReflectionViewProjection = mul(ReflectionView,Projection);
	float4x4 preWorldReflectionViewProjection = mul(World,preReflectionViewProjection);		
	Out.ReflectionMapSamplingPos=mul(inPos,preWorldReflectionViewProjection);
	Out.HeightSamplingPos=Out.ReflectionMapSamplingPos;

	

	//凹凸纹理采样坐标

	
	float3 windDir=normalize(WindDirection);
	float3 normal=normalize(inNormal);
	float3 perpDir=cross(WindDirection,normal);
  
        // 获取经水流方向修正的纹理uv坐标

	float4 absoluteTexCoords=Out.ReflectionMapSamplingPos/Out.ReflectionMapSamplingPos.w/2+0.5;
	float4 rotatedTexCoords = mul(absoluteTexCoords, WindDirection);
    	float2 moveVector = float2(1.25, 1.25); 

	//凹凸纹理采样坐标
	  Out.BumpMapSamplingPos =absoluteTexCoords.xy / WaveLength+ Time * WindForce * moveVector.xy;
	
	//纹理坐标修正
	

	//转换矩阵
	//inBinormal = cross(inTangent,inNormal);
	float3x3 TangentToObject; 
	TangentToObject[0] = normalize(inTangent); 
	TangentToObject[1] = normalize(inBinormal); 
	TangentToObject[2] = normalize(inNormal);
	float3x3 TangentToWorld=(TangentToObject,World);

	Out.LightDirT = mul(TangentToWorld,LightDirection);
	float3 eyeVector=CameraPos-WorldPosition;
	Out.ViewDirT=mul(TangentToWorld,eyeVector);
	Out.h=h;
        return Out;
};
PS_OUTPUT PS(VS_OUTPUT Input)
{
	PS_OUTPUT Out=(PS_OUTPUT)0;
	//凹凸纹理
	float4 bumpColor = tex2D(BumpMapSampler,Input.BumpMapSamplingPos);
	float4 bumpColor1 = tex2D(BumpMapSampler1,Input.BumpMapSamplingPos);

	float2 perturbation = WaveHeight*(bumpColor.rg-0.5f)*2;
	
	//计算反射纹理的采样坐标
	float2 ReflectionMapSamplingPosCoord;
	ReflectionMapSamplingPosCoord.x=Input.ReflectionMapSamplingPos.x/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ReflectionMapSamplingPosCoord.y=-Input.ReflectionMapSamplingPos.y/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	
	
	// 将反射贴图采样坐标从2D屏幕空间映射到纹理坐标    (反射颜色)
        float2 perturbatedTexCoords = ReflectionMapSamplingPosCoord+perturbation;
        float4 reflectionColor = tex2D(ReflectionSampler, perturbatedTexCoords);
	
	
	//光线视线处理
	float3 tanNormalVector;

 	tanNormalVector= ((bumpColor.rgb)-0.5f)*2;


	tanNormalVector=normalize(tanNormalVector);


	float3 tanLightDir= normalize(Input.LightDirT);
	
	float3 tanEyeDir=normalize(Input.ViewDirT);


	//金沙江水质颜色
	float r=124.0f/255.0f;
	float g=129.0f/255.0f;
	float b=100.0f/255.0f;
	//float r=95.0f/255.0f;
	//float g=105.0f/255.0f;
	//float b=166.0f/255.0f;
	//float4 JSJWaterColor =float4(0.3f,0.3f,0.5f,1.0f);
	float4 JSJWaterColor =float4(r,g,b,0.94 );
	//材质颜色
	Out.Color=lerp(reflectionColor,JSJWaterColor,0.77f);


	//光照处理
	float4 ambcolor=float4(AmbientColor.rgb,1.0f);
	float4 diffcolor=float4(DiffuseColor.rgb,1.0f);
	//漫反射
	float diff=-dot(tanNormalVector,tanLightDir);
	float4 diffuse=0.0f;
	float4 specular=0.0f;
	if(diff > 0)
	{
		//计算漫反射光
		diffuse = 0.3*JSJWaterColor * DiffuseColor * diff;
		//高光处理
		float3 refLight = reflect(tanLightDir,tanNormalVector);
		float specFactor = pow(max(dot(refLight,tanEyeDir),0.f),25);
		specular =  specFactor;
	}

	
float a=1;
	
if(Input.h.x<4)
{
a=Input.h.x/4;
}

	Out.Color=   Out.Color   //环境光处理	
	        +diffuse+specular;//漫反射处理
//Out.Color+=specular;
	
	return Out;
};
technique WaterRender
{   pass P0
      {
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_2_0 PS();
      }
}