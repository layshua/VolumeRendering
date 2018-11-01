//float4x4 WorldViewProject:	WORLDVIEWPROJECTION;
float4x4 World:	WORLD;   //世界矩阵
float4x4 View:	VIEW;   //视矩阵
float4x4 Projection: PROJECTION; // 投影矩阵
float4x4 ReflectionView; //反射视矩阵

float4 CameraPos;	//相机位置

texture ReflectTexture;//水面反射纹理

float4 S=float4(50,20,30,15);//四个正弦波的波速
float4 A=float4(6.2,4.7,3.9,2.9);//四个正弦波的振幅
float4 L=float4(270,290,350,200);//四个正弦波的波长
float2 D0=float2(-0.3,0.5);
float2 D1=float2(-1.4,1.7f);
float2 D2=float2(0.6,-0.7f);
float2 D3=float2(-1.3,-0.5f);//以上四个为四个正弦波的方向

float4 Q=float4(1.0,0.8,0.9,0.5);

float     Time;                             // 时间

float PI=3.14;

float3 LightDirection; //光源方向
float4 AmbientColor;        //环境光
float4 DiffuseColor;        //漫反射光
float AmbientIntensity;//材质反射环境光衰减系数
float DiffuseIntensity;//材质漫反射衰减系数

struct VS_OUTPUT
{
    float4 Position                  : POSITION;
    float4 PosWS  		     : TEXCOORD1;
    float4 ReflectionMapSamplingPos  : TEXCOORD2;
    float3x3 TangentToWorld 	     : TEXCOORD3;
    float3 light		     : TEXCOORD6;
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


VS_OUTPUT VS ( float4 inPos : POSITION)
{
	
	VS_OUTPUT Out = (VS_OUTPUT)0;

	float4x4 preViewProjection = mul (View,Projection); 
	float4x4 preWorldViewProjection = mul(World,preViewProjection);

	Out.Position = mul(inPos,preWorldViewProjection);

	
	//计算反射纹理的采样坐标
	float4x4 preReflectionViewProjection = mul(ReflectionView,Projection);
	float4x4 preWorldReflectionViewProjection = mul(World,preReflectionViewProjection);		
	Out.ReflectionMapSamplingPos=mul(inPos,preWorldReflectionViewProjection);
	
	//计算波
	float4 dotProducts;
        dotProducts.x=dot(D0,inPos.xz);
     	dotProducts.y=dot(D1,inPos.xz);
     	dotProducts.z=dot(D2,inPos.xz);
     	dotProducts.w=dot(D3,inPos.xz);   
    	float4 arg=dotProducts*2*PI/L+Time*S*2*PI/L;
	
	float4 D2X=float4(D0.x*D0.x,D1.x*D1.x,D2.x*D2.x,D3.x*D3.x);
	float4 D2Y=float4(D0.y*D0.y,D1.y*D1.y,D2.y*D2.y,D3.y*D3.y);
	float4 DXDY=float4(D0.x*D0.y,D1.x*D1.y,D2.x*D2.y,D3.x*D3.y);
	float4 DX=float4(D0.x,D1.x,D2.x,D3.x);
	float4 DY=float4(D0.y,D1.y,D2.y,D3.y);

	float4 heightsX=Q*A*DX*cos(arg);
	
    	float4 heightsY=A*sin(arg);
	
	float4 heightsZ=Q*A*DY*cos(arg);
    	
	float4 finalPos=inPos;
     	finalPos.x+=heightsX.x;
    	finalPos.x+=heightsX.y;
     	finalPos.x+=heightsX.z;
     	finalPos.x+=heightsX.w;
	
	finalPos.y+=heightsY.x;
    	finalPos.y+=heightsY.y;
     	finalPos.y+=heightsY.z;
     	finalPos.y+=heightsY.w;

	finalPos.z+=heightsZ.x;
    	finalPos.z+=heightsZ.y;
     	finalPos.z+=heightsZ.z;
     	finalPos.z+=heightsZ.w;
	
	Out.Position=mul(finalPos,preWorldViewProjection);

	Out.PosWS = mul(finalPos,World);
	
	float4 derivativesCOS=A*2*PI*cos(arg)/L;
	float4 derivativesSIN=A*2*PI*sin(arg)/L;
	
	//B
        
        float deviationsBX=Q.x*D2X.x*derivativesSIN.x;
	deviationsBX+=Q.y*D2X.y*derivativesSIN.y;
	deviationsBX+=Q.z*D2X.z*derivativesSIN.z;
	deviationsBX+=Q.w*D2X.w*derivativesSIN.w;
	deviationsBX =1-deviationsBX;
	
	float deviationsBY=DX.x*derivativesCOS.x;
	deviationsBY+=DX.y*derivativesCOS.y;
	deviationsBY+=DX.z*derivativesCOS.z;
	deviationsBY+=DX.w*derivativesCOS.w;
	deviationsBY=deviationsBY;

        float deviationsBZ=Q.x*DXDY.x*derivativesSIN.x;
	deviationsBZ+=Q.y*DXDY.y*derivativesSIN.y;
	deviationsBZ+=Q.z*DXDY.z*derivativesSIN.z;
	deviationsBZ+=Q.w*DXDY.w*derivativesSIN.w;
	deviationsBZ=-deviationsBZ;

	
	//T
		
        float deviationsTX=Q.x*DXDY.x*derivativesSIN.x;
	deviationsTX+=Q.y*DXDY.y*derivativesSIN.y;
	deviationsTX+=Q.z*DXDY.z*derivativesSIN.z;
	deviationsTX+=Q.w*DXDY.w*derivativesSIN.w;
	deviationsTX=-deviationsTX;

	float deviationsTY=DY.x*derivativesCOS.x;
	deviationsTY+=DY.y*derivativesCOS.y;
	deviationsTY+=DY.z*derivativesCOS.z;
	deviationsTY+=DY.w*derivativesCOS.w;
	deviationsTY=deviationsTY;
	
	float deviationsTZ=Q.x*D2Y.x*derivativesSIN.x;
	deviationsTZ+=Q.y*D2Y.y*derivativesSIN.y;
	deviationsTZ+=Q.z*D2Y.z*derivativesSIN.z;
	deviationsTZ+=Q.w*D2Y.w*derivativesSIN.w;
	deviationsTZ =1-deviationsTZ;
	//N
	
	float deviationsNX=DX.x*derivativesCOS.x;
	deviationsNX+=DX.y*derivativesCOS.y;
	deviationsNX+=DX.z*derivativesCOS.z;
	deviationsNX+=DX.w*derivativesCOS.w;
	deviationsNX=-deviationsNX;

	float deviationsNY=Q.x*derivativesSIN.x;
	deviationsNY+=Q.y*derivativesSIN.y;
	deviationsNY+=Q.z*derivativesSIN.z;
	deviationsNY+=Q.w*derivativesSIN.w;
	deviationsNY=1-deviationsNY;
	
	float deviationsNZ=DY.x*derivativesCOS.x;
	deviationsNZ+=DY.y*derivativesCOS.y;
	deviationsNZ+=DY.z*derivativesCOS.z;
	deviationsNZ+=DY.w*derivativesCOS.w;
	deviationsNZ =-deviationsNZ;

	float3 Tangent=float3(deviationsTX,deviationsTY,deviationsTZ);
	float3 Binormal=float3(deviationsBX,deviationsBY,deviationsBZ);
	float3 Normal=float3(deviationsNX,deviationsNY,deviationsNZ);
	
	float3x3 tangentToObject;
	tangentToObject[0]=normalize(Tangent);
	tangentToObject[1]=normalize(Binormal);
	tangentToObject[2]=normalize(Normal);
	Out.TangentToWorld=mul(tangentToObject,World);
	float3 templight=mul(LightDirection,World);
	Out.light=normalize(templight-Out.PosWS);
        return Out;
};
PS_OUTPUT PS(VS_OUTPUT Input)
{
	PS_OUTPUT Out=(PS_OUTPUT)0;
	
	
	//计算反射纹理的采样坐标
	float2 ReflectionMapSamplingPosCoord;
	ReflectionMapSamplingPosCoord.x=Input.ReflectionMapSamplingPos.x/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ReflectionMapSamplingPosCoord.y=-Input.ReflectionMapSamplingPos.y/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	
	
	// 将反射贴图采样坐标从2D屏幕空间映射到纹理坐标    (反射颜色)
        float2 perturbatedTexCoords = ReflectionMapSamplingPosCoord;
        float4 reflectionColor = tex2D(ReflectionSampler, perturbatedTexCoords);
	
	


	//金沙江水质颜色
	float r=124.0f/255.0f;
	float g=129.0f/255.0f;
	float b=100.0f/255.0f;
	//float r=95.0f/255.0f;
	//float g=105.0f/255.0f;
	//float b=166.0f/255.0f;

	float4 JSJWaterColor =float4(r,g,b,0.94 );
	
	float diff=saturate(dot(Input.light,Input.TangentToWorld[2]));
	float diffuse=0;
	Out.Color=lerp(reflectionColor,JSJWaterColor,0.77f);
	if(diff > 0)
	{
		//计算漫反射光
		diffuse =Out.Color*DiffuseColor*diff ;
		
	}

	//材质颜色
	
	Out.Color+=   //环境光处理	
	        0.3*diffuse;
	
	return Out;
};
technique WaterRender
{   pass P0
      {
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_2_0 PS();
      }
}