//float4x4 WorldViewProject:	WORLDVIEWPROJECTION;
float4x4 World:	WORLD;   //世界矩阵
float4x4 View:	VIEW;   //视矩阵
float4x4 Projection: PROJECTION; // 投影矩阵
float4x4 ReflectionView; //反射视矩阵

float4 CameraPos;	//相机位置

texture ReflectTexture;//水面反射纹理

float4 S1=float4(20,40,35,35);//四个正弦波的波速
float4 A1=float4(1.76,1.74,1.66,1.66);//四个正弦波的振幅
float4 L1=float4(250,160,240,170);//四个正弦波的波长
float4 Q1=float4(0.45,0.35,0.3,0.4);

float4 S2=float4(14,17,32,28);//四个正弦波的波速
float4 A2=float4(0.86,0.63,0.86,0.62);//四个正弦波的振幅
float4 L2=float4(250,250,250,250);//四个正弦波的波长
float4 Q2=float4(0.3,0.39,0.5,0.4);

float2 D0=float2(-0.03,-0.9);
float2 D1=float2(-0.5,0.5);
float2 D2=float2(-0.7,0);
float2 D3=float2(-0.6,-0.54);//以上四个为四个正弦波的方向
float2 D4=float2(0,-0.8);
float2 D5=float2(0.6,-0.64);
float2 D6=float2(0.92,0);
float2 D7=float2(0.46,0.51);//以上四个为四个正弦波的方向

float k=0.15;

float     Time;                             // 时间
float     bumpT;
float PI=3.14;
float Factor=0.01;
float3 LightDirection1; //光源方向
float3 LightDirection2; //光源方向
float4 AmbientColor;        //环境光
float4 DiffuseColor;        //漫反射光
float AmbientIntensity;//材质反射环境光衰减系数
float DiffuseIntensity;//材质漫反射衰减系数

//流向纹理
texture directionTexture;
float direCoordUmove=1.0;//水流基本速度

//高差纹理

texture waterHeadTexture;
//bool showWaterHead=false;

//凹凸
texture BumpTexture;//水面的凹凸贴图
float WaveHeight;                           // 振幅
float WaveLength;                           //波长
float4 WindDirection;                       //风向
float WindForce;               		    //风力


float r1;
float g1;
float b1;
float a1;

texture perm_2d_tex;
texture grad3_perm_tex;
texture FoamTexture;

//=================运行时常量=======================
float4 xWindDirection = float4(1.0f,0.0f,0.0f,0.0f);
float xWindForce = 10.0f;
float xWaveHeight = 0.2;//0.5f;
float xWaveLength = 0.8f;
float xDrawMode = 1.0f;
float xDullBlendFactor = 0.1f;
int xFresnelMode = 2;
float xSpecPerturb = 12.0f;//高光范围宽度
float xSpecPower = 20.0f;  //高光强度
//=======End=======运行时常量============End========

int SpecMode=1;
struct VS_OUTPUT
{
    float4 Position                  : POSITION;
    float4 PosWS  		     : TEXCOORD1;
    float4 ReflectionMapSamplingPos  : TEXCOORD2;
    float3x3 TanToWorld 	     	     : TEXCOORD3;
    float4 lighth		     : TEXCOORD6;
    float2 BumpMapSamplingPos 	     	     : TEXCOORD7;
    float2 directionTex              : TEXCOORD8;
    
}; 
struct VS_OUTPUTWaterHead
{
    float4 Position                  : POSITION;
    float4 PosWS  		     : TEXCOORD1;
    float4 ReflectionMapSamplingPos  : TEXCOORD2;
    float3x3 TanToWorld 	     	     : TEXCOORD3;
    float4 lighth		     : TEXCOORD6;
    float2 BumpMapSamplingPos 	     	     : TEXCOORD7;
    float2 directionTex              : TEXCOORD8;
    float2 waterHeadTex              : TEXCOORD9;
    
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
sampler DirectionSampler = sampler_state  //河流流向纹理采样器
{
	Texture = <directionTexture>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
        AddressU = wrap; 
	AddressV = wrap;
};

sampler WaterHeadSampler = sampler_state  //河流流向纹理采样器
{
	Texture = <waterHeadTexture>;
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


VS_OUTPUTWaterHead VSWithWaterHead ( float4 inPos : POSITION,float3 h : TEXCOORD0,float2 direction : TEXCOORD1,float2 waterHead : TEXCOORD2)
{
	
	VS_OUTPUTWaterHead Out = (VS_OUTPUTWaterHead)0;

	float4x4 preViewProjection = mul (View,Projection); 
	float4x4 preWorldViewProjection = mul(World,preViewProjection);

	Out.Position = mul(inPos,preWorldViewProjection);

	
	//计算反射纹理的采样坐标
	float4x4 preReflectionViewProjection = mul(ReflectionView,Projection);
	float4x4 preWorldReflectionViewProjection = mul(World,preReflectionViewProjection);		
	Out.ReflectionMapSamplingPos=mul(inPos,preWorldReflectionViewProjection);
	
	//=================================暂时不考虑风向的Bump采样======================================
    //bump采样
    
    float4 absoluteTexCoords = Out.ReflectionMapSamplingPos/Out.ReflectionMapSamplingPos.w; //使用全局坐标
    float4 rotatedTexCoords = mul(absoluteTexCoords, xWindDirection);
    float2 moveVector = float2(1.25, 1.25); 
    // moving the water 
    //Out.BumpMapSamplingPos = rotatedTexCoords.xy / xWaveLength + Time/100 * xWindForce * moveVector.xy;//使用带风向的bump采样
    Out.BumpMapSamplingPos =absoluteTexCoords.xy / xWaveLength + Time/100 * xWindForce * moveVector.xy;
    //=================================暂时不考虑风向的Bump采样======================================

	//计算波
	float4 dotProducts1;
	float4 dotProducts2;
	
        dotProducts1.x=dot(D0,inPos.xz);
     	dotProducts1.y=dot(D1,inPos.xz);
     	dotProducts1.z=dot(D2,inPos.xz);
     	dotProducts1.w=dot(D3,inPos.xz); 

	dotProducts2.x=dot(D4,inPos.xz);
     	dotProducts2.y=dot(D5,inPos.xz);
     	dotProducts2.z=dot(D6,inPos.xz);
     	dotProducts2.w=dot(D7,inPos.xz);
  
    	float4 arg1=dotProducts1*2*PI/L1+Time*S1*2*PI/L1;
	float4 arg2=dotProducts2*2*PI/L2+Time*S2*2*PI/L2;

	float4 D2X1=float4(D0.x*D0.x,D1.x*D1.x,D2.x*D2.x,D3.x*D3.x);
	float4 D2Y1=float4(D0.y*D0.y,D1.y*D1.y,D2.y*D2.y,D3.y*D3.y);
	float4 DXDY1=float4(D0.x*D0.y,D1.x*D1.y,D2.x*D2.y,D3.x*D3.y);
	float4 DX1=float4(D0.x,D1.x,D2.x,D3.x);
	float4 DY1=float4(D0.y,D1.y,D2.y,D3.y);

	float4 D2X2=float4(D4.x*D4.x,D5.x*D5.x,D6.x*D6.x,D7.x*D7.x);
	float4 D2Y2=float4(D4.y*D4.y,D5.y*D5.y,D6.y*D6.y,D7.y*D7.y);
	float4 DXDY2=float4(D4.x*D4.y,D5.x*D5.y,D6.x*D6.y,D7.x*D7.y);
	float4 DX2=float4(D4.x,D5.x,D6.x,D7.x);
	float4 DY2=float4(D4.y,D5.y,D6.y,D7.y);

	float4 heightsX1=Q1*A1*DX1*cos(arg1);
	float4 heightsX2=Q2*A2*DX2*cos(arg2);
	
    	float4 heightsY1=A1*sin(arg1);
	float4 heightsY2=A2*sin(arg2);
	
	float4 heightsZ1=Q1*A1*DY1*cos(arg1);
    	float4 heightsZ2=Q2*A2*DY2*cos(arg2);

 	//推移==================================================
	float4 movesX1=k*(DX1+DX1* S1);
	float4 movesX2=k*(DX2+DX2* S2);	
	float4 movesY1=k*(DY1+DY1* S1);
	float4 movesY2=k*(DY2+DY2* S2);
	
	//float4 movesX1=k*(DX1+ Time*S1)*2*PI/L1/L1;
	//float4 movesX2=k*(DX2+Time*S2)*2*PI/L2/L2;	
	//float4 movesY1=k*(DY1+Time*S1)*2*PI/L1/L1;
	//float4 movesY2=k*(DY2+Time*S2)*2*PI/L2/L2;
	////X方向
	//if(arg1.x>PI/L1.x)
	//{
	//	movesX1.x=k*L1.x*L1.x/(DX1.x+ Time*S1.x)/2/PI;
	//}
	//if(arg1.y>PI/L1.y)
	//{
	//	movesX1.y=k*L1.y*L1.y/(DX1.y+ Time*S1.y)/2/PI;
	//}
	//if(arg1.z>PI/L1.z)
	//{
	//	movesX1.z=k*L1.z*L1.z/(DX1.z+ Time*S1.z)/2/PI;
	//}
	//if(arg1.x>PI/L1.w)
	//{
	//	movesX1.w=k*L1.w*L1.w/(DX1.w+ Time*S1.w)/2/PI;
	//}
	
	//if(arg2.x>PI/L2.x)
	//{
	//	movesX2.x=k*L2.x*L2.x/(DX2.x+ Time*S2.x)/2/PI;
	//}
	//if(arg2.y>PI/L2.y)
	//{
	//	movesX2.y=k*L2.y*L2.y/(DX2.y+ Time*S2.y)/2/PI;
	//}
	//if(arg2.z>PI/L2.z)
	//{
	//	movesX2.z=k*L2.z*L2.z/(DX2.z+ Time*S2.z)/2/PI;
	//}
	//if(arg2.x>PI/L2.w)
	//{
	//	movesX2.w=k*L2.w*L2.w/(DX2.w+ Time*S2.w)/2/PI;
	//}
	//Y方向
	//if(arg1.x>PI/L1.x)
	//{
	//	movesY1.x=k*L1.x*L1.x/(DY1.x+ Time*S1.x)/2/PI;
	//}
	//if(arg1.y>PI/L1.y)
	//{
	//	movesY1.y=k*L1.y*L1.y/(DY1.y+ Time*S1.y)/2/PI;
	//}
	//if(arg1.z>PI/L1.z)
	//{
	//	movesY1.z=k*L1.z*L1.z/(DY1.z+ Time*S1.z)/2/PI;
	//}
	//if(arg1.x>PI/L1.w)
	//{
	//	movesY1.w=k*L1.w*L1.w/(DY1.w+ Time*S1.w)/2/PI;
	//}
	
	//if(arg2.x>PI/L2.x)
	//{
	//	movesY2.x=k*L2.x*L2.x/(DY2.x+ Time*S2.x)/2/PI;
	//}
	//if(arg2.y>PI/L2.y)
	//{
	//	movesY2.y=k*L2.y*L2.y/(DY2.y+ Time*S2.y)/2/PI;
	//}
	//if(arg2.z>PI/L2.z)
	//{
	//	movesY2.z=k*L2.z*L2.z/(DY2.z+ Time*S2.z)/2/PI;
	//}
	//if(arg2.x>PI/L2.w)
	//{
	//	movesY2.w=k*L2.w*L2.w/(DY2.w+ Time*S2.w)/2/PI;
	//}
	//==================================================
	float4 finalPos=inPos;
     	finalPos.x+=heightsX1.x+movesX1.x;
    	finalPos.x+=heightsX1.y+movesX1.y;
     	finalPos.x+=heightsX1.z+movesX1.z;
     	finalPos.x+=heightsX1.w+movesX1.w;
	
	finalPos.x+=heightsX2.x+movesX2.x;
    	finalPos.x+=heightsX2.y+movesX2.y;
     	finalPos.x+=heightsX2.z+movesX2.z;
     	finalPos.x+=heightsX2.w+movesX2.w;

	finalPos.y+=heightsY1.x;
    	finalPos.y+=heightsY1.y;
    	finalPos.y+=heightsY1.z;
     	finalPos.y+=heightsY1.w;
	
	finalPos.y+=heightsY2.x;
    	finalPos.y+=heightsY2.y;
     	finalPos.y+=heightsY2.z;
     	finalPos.y+=heightsY2.w;

	finalPos.z+=heightsZ1.x+movesY1.x;
    	finalPos.z+=heightsZ1.y+movesY1.y;
     	finalPos.z+=heightsZ1.z+movesY1.z;
     	finalPos.z+=heightsZ1.w+movesY1.w;
	
	finalPos.z+=heightsZ2.x+movesY2.x;
    	finalPos.z+=heightsZ2.y+movesY2.y;
     	finalPos.z+=heightsZ2.z+movesY2.z;
     	finalPos.z+=heightsZ2.w+movesY2.w;

	Out.Position=mul(finalPos,preWorldViewProjection);

	Out.PosWS = mul(finalPos,World);
	
	float4 derivativesCOS1=A1*2*PI*cos(arg1)/L1;
	float4 derivativesCOS2=A2*2*PI*cos(arg2)/L2;
	
	float4 derivativesSIN1=A1*2*PI*sin(arg1)/L1;
	float4 derivativesSIN2=A2*2*PI*sin(arg2)/L2;
	//B
        
        float deviationsBX=Q1.x*D2X1.x*derivativesSIN1.x;
	deviationsBX+=Q1.y*D2X1.y*derivativesSIN1.y;
	deviationsBX+=Q1.z*D2X1.z*derivativesSIN1.z;
	deviationsBX+=Q1.w*D2X1.w*derivativesSIN1.w;
	
	deviationsBX+=Q2.x*D2X2.x*derivativesSIN2.x;
	deviationsBX+=Q2.y*D2X2.y*derivativesSIN2.y;
	deviationsBX+=Q2.z*D2X2.z*derivativesSIN2.z;
	deviationsBX+=Q2.w*D2X2.w*derivativesSIN2.w;

	deviationsBX =1-deviationsBX;
	
	float deviationsBY=DX1.x*derivativesCOS1.x;
	deviationsBY+=DX1.y*derivativesCOS1.y;
	deviationsBY+=DX1.z*derivativesCOS1.z;
	deviationsBY+=DX1.w*derivativesCOS1.w;

	deviationsBY+=DX2.x*derivativesCOS2.x;
	deviationsBY+=DX2.y*derivativesCOS2.y;
	deviationsBY+=DX2.z*derivativesCOS2.z;
	deviationsBY+=DX2.w*derivativesCOS2.w;
	deviationsBY=deviationsBY;

        float deviationsBZ=Q1.x*DXDY1.x*derivativesSIN1.x;
	deviationsBZ+=Q1.y*DXDY1.y*derivativesSIN1.y;
	deviationsBZ+=Q1.z*DXDY1.z*derivativesSIN1.z;
	deviationsBZ+=Q1.w*DXDY1.w*derivativesSIN1.w;
	
	deviationsBZ+=Q2.x*DXDY2.x*derivativesSIN2.x;
	deviationsBZ+=Q2.y*DXDY2.y*derivativesSIN2.y;
	deviationsBZ+=Q2.z*DXDY2.z*derivativesSIN2.z;
	deviationsBZ+=Q2.w*DXDY2.w*derivativesSIN2.w;

	deviationsBZ=-deviationsBZ;

	
	//T
		
        float deviationsTX=Q1.x*DXDY1.x*derivativesSIN1.x;
	deviationsTX+=Q1.y*DXDY1.y*derivativesSIN1.y;
	deviationsTX+=Q1.z*DXDY1.z*derivativesSIN1.z;
	deviationsTX+=Q1.w*DXDY1.w*derivativesSIN1.w;

	deviationsTX+=Q2.x*DXDY2.x*derivativesSIN2.x;
	deviationsTX+=Q2.y*DXDY2.y*derivativesSIN2.y;
	deviationsTX+=Q2.z*DXDY2.z*derivativesSIN2.z;
	deviationsTX+=Q2.w*DXDY2.w*derivativesSIN2.w;
	deviationsTX=-deviationsTX;

	float deviationsTY=DY1.x*derivativesCOS1.x;
	deviationsTY+=DY1.y*derivativesCOS1.y;
	deviationsTY+=DY1.z*derivativesCOS1.z;
	deviationsTY+=DY1.w*derivativesCOS1.w;

	deviationsTY+=DY2.x*derivativesCOS2.x;
	deviationsTY+=DY2.y*derivativesCOS2.y;
	deviationsTY+=DY2.z*derivativesCOS2.z;
	deviationsTY+=DY2.w*derivativesCOS2.w;

	deviationsTY=deviationsTY;
	
	float deviationsTZ=Q1.x*D2Y1.x*derivativesSIN1.x;
	deviationsTZ+=Q1.y*D2Y1.y*derivativesSIN1.y;
	deviationsTZ+=Q1.z*D2Y1.z*derivativesSIN1.z;
	deviationsTZ+=Q1.w*D2Y1.w*derivativesSIN1.w;
	
	deviationsTZ+=Q2.x*D2Y2.x*derivativesSIN2.x;
	deviationsTZ+=Q2.y*D2Y2.y*derivativesSIN2.y;
	deviationsTZ+=Q2.z*D2Y2.z*derivativesSIN2.z;
	deviationsTZ+=Q2.w*D2Y2.w*derivativesSIN2.w;
	deviationsTZ =1-deviationsTZ;

	//N
	
	float deviationsNX=DX1.x*derivativesCOS1.x;
	deviationsNX+=DX1.y*derivativesCOS1.y;
	deviationsNX+=DX1.z*derivativesCOS1.z;
	deviationsNX+=DX1.w*derivativesCOS1.w;

	deviationsNX+=DX2.x*derivativesCOS2.x;
	deviationsNX+=DX2.y*derivativesCOS2.y;
	deviationsNX+=DX2.z*derivativesCOS2.z;
	deviationsNX+=DX2.w*derivativesCOS2.w;	

	deviationsNX=-deviationsNX;

	float deviationsNY=Q1.x*derivativesSIN1.x;
	deviationsNY+=Q1.y*derivativesSIN1.y;
	deviationsNY+=Q1.z*derivativesSIN1.z;
	deviationsNY+=Q1.w*derivativesSIN1.w;
	
	deviationsNY+=Q2.x*derivativesSIN2.x;
	deviationsNY+=Q2.y*derivativesSIN2.y;
	deviationsNY+=Q2.z*derivativesSIN2.z;
	deviationsNY+=Q2.w*derivativesSIN2.w;

	deviationsNY=1-deviationsNY;
	
	float deviationsNZ=DY1.x*derivativesCOS1.x;
	deviationsNZ+=DY1.y*derivativesCOS1.y;
	deviationsNZ+=DY1.z*derivativesCOS1.z;
	deviationsNZ+=DY1.w*derivativesCOS1.w;
	
	deviationsNZ+=DY2.x*derivativesCOS2.x;
	deviationsNZ+=DY2.y*derivativesCOS2.y;
	deviationsNZ+=DY2.z*derivativesCOS2.z;
	deviationsNZ+=DY2.w*derivativesCOS2.w;

	deviationsNZ =-deviationsNZ;

	float3 Tangent=float3(deviationsTX,deviationsTY,deviationsTZ);
	float3 Binormal=float3(deviationsBX,deviationsBY,deviationsBZ);
	//float3 Normal=float3(deviationsNX,deviationsNY,deviationsNZ);
	float3 Normal=cross(Tangent,Binormal);
	Tangent=cross(Normal,Binormal);
	float3x3 tangentToObject;
	Normal=normalize(Normal);
	tangentToObject[0]=normalize(Tangent);
	tangentToObject[1]=normalize(Binormal);
	tangentToObject[2]=normalize(Normal);
	float3x3 TangentToWorld=mul(tangentToObject,World);
	Out.lighth.xyz=normalize(LightDirection1-Out.PosWS);
	Out.lighth.w=(h+heightsY1.x+heightsY1.y+heightsY1.z+heightsY1.w+heightsY2.x+heightsY2.y+heightsY2.z+heightsY2.w);
	Out.TanToWorld=TangentToWorld;

	Out.directionTex=direction;
        Out.waterHeadTex=waterHead;
 
	
        return Out;
};

VS_OUTPUT VSNoWaterHead ( float4 inPos : POSITION,float3 h : TEXCOORD0,float2 direction : TEXCOORD1)
{
	
	VS_OUTPUT Out = (VS_OUTPUT)0;

	float4x4 preViewProjection = mul (View,Projection); 
	float4x4 preWorldViewProjection = mul(World,preViewProjection);

	Out.Position = mul(inPos,preWorldViewProjection);

	
	//计算反射纹理的采样坐标
	float4x4 preReflectionViewProjection = mul(ReflectionView,Projection);
	float4x4 preWorldReflectionViewProjection = mul(World,preReflectionViewProjection);		
	Out.ReflectionMapSamplingPos=mul(inPos,preWorldReflectionViewProjection);
	
	//=================================暂时不考虑风向的Bump采样======================================
    //bump采样
    
    float4 absoluteTexCoords = Out.ReflectionMapSamplingPos/Out.ReflectionMapSamplingPos.w; //使用全局坐标
    float4 rotatedTexCoords = mul(absoluteTexCoords, xWindDirection);
    float2 moveVector = float2(1.25, 1.25); 
    // moving the water 
    //Out.BumpMapSamplingPos = rotatedTexCoords.xy / xWaveLength + Time/100 * xWindForce * moveVector.xy;//使用带风向的bump采样
    Out.BumpMapSamplingPos =absoluteTexCoords.xy / xWaveLength + Time/100 * xWindForce * moveVector.xy;
    //=================================暂时不考虑风向的Bump采样======================================

	//计算波
	float4 dotProducts1;
	float4 dotProducts2;
	
        dotProducts1.x=dot(D0,inPos.xz);
     	dotProducts1.y=dot(D1,inPos.xz);
     	dotProducts1.z=dot(D2,inPos.xz);
     	dotProducts1.w=dot(D3,inPos.xz); 

	dotProducts2.x=dot(D4,inPos.xz);
     	dotProducts2.y=dot(D5,inPos.xz);
     	dotProducts2.z=dot(D6,inPos.xz);
     	dotProducts2.w=dot(D7,inPos.xz);
  
    	float4 arg1=dotProducts1*2*PI/L1+Time*S1*2*PI/L1;
	float4 arg2=dotProducts2*2*PI/L2+Time*S2*2*PI/L2;

	float4 D2X1=float4(D0.x*D0.x,D1.x*D1.x,D2.x*D2.x,D3.x*D3.x);
	float4 D2Y1=float4(D0.y*D0.y,D1.y*D1.y,D2.y*D2.y,D3.y*D3.y);
	float4 DXDY1=float4(D0.x*D0.y,D1.x*D1.y,D2.x*D2.y,D3.x*D3.y);
	float4 DX1=float4(D0.x,D1.x,D2.x,D3.x);
	float4 DY1=float4(D0.y,D1.y,D2.y,D3.y);

	float4 D2X2=float4(D4.x*D4.x,D5.x*D5.x,D6.x*D6.x,D7.x*D7.x);
	float4 D2Y2=float4(D4.y*D4.y,D5.y*D5.y,D6.y*D6.y,D7.y*D7.y);
	float4 DXDY2=float4(D4.x*D4.y,D5.x*D5.y,D6.x*D6.y,D7.x*D7.y);
	float4 DX2=float4(D4.x,D5.x,D6.x,D7.x);
	float4 DY2=float4(D4.y,D5.y,D6.y,D7.y);

	float4 heightsX1=Q1*A1*DX1*cos(arg1);
	float4 heightsX2=Q2*A2*DX2*cos(arg2);
	
    	float4 heightsY1=A1*sin(arg1);
	float4 heightsY2=A2*sin(arg2);
	
	float4 heightsZ1=Q1*A1*DY1*cos(arg1);
    	float4 heightsZ2=Q2*A2*DY2*cos(arg2);

 	//推移==================================================
	float4 movesX1=k*(DX1+DX1* S1);
	float4 movesX2=k*(DX2+DX2* S2);	
	float4 movesY1=k*(DY1+DY1* S1);
	float4 movesY2=k*(DY2+DY2* S2);
	
	//float4 movesX1=k*(DX1+ Time*S1)*2*PI/L1/L1;
	//float4 movesX2=k*(DX2+Time*S2)*2*PI/L2/L2;	
	//float4 movesY1=k*(DY1+Time*S1)*2*PI/L1/L1;
	//float4 movesY2=k*(DY2+Time*S2)*2*PI/L2/L2;
	////X方向
	//if(arg1.x>PI/L1.x)
	//{
	//	movesX1.x=k*L1.x*L1.x/(DX1.x+ Time*S1.x)/2/PI;
	//}
	//if(arg1.y>PI/L1.y)
	//{
	//	movesX1.y=k*L1.y*L1.y/(DX1.y+ Time*S1.y)/2/PI;
	//}
	//if(arg1.z>PI/L1.z)
	//{
	//	movesX1.z=k*L1.z*L1.z/(DX1.z+ Time*S1.z)/2/PI;
	//}
	//if(arg1.x>PI/L1.w)
	//{
	//	movesX1.w=k*L1.w*L1.w/(DX1.w+ Time*S1.w)/2/PI;
	//}
	
	//if(arg2.x>PI/L2.x)
	//{
	//	movesX2.x=k*L2.x*L2.x/(DX2.x+ Time*S2.x)/2/PI;
	//}
	//if(arg2.y>PI/L2.y)
	//{
	//	movesX2.y=k*L2.y*L2.y/(DX2.y+ Time*S2.y)/2/PI;
	//}
	//if(arg2.z>PI/L2.z)
	//{
	//	movesX2.z=k*L2.z*L2.z/(DX2.z+ Time*S2.z)/2/PI;
	//}
	//if(arg2.x>PI/L2.w)
	//{
	//	movesX2.w=k*L2.w*L2.w/(DX2.w+ Time*S2.w)/2/PI;
	//}
	//Y方向
	//if(arg1.x>PI/L1.x)
	//{
	//	movesY1.x=k*L1.x*L1.x/(DY1.x+ Time*S1.x)/2/PI;
	//}
	//if(arg1.y>PI/L1.y)
	//{
	//	movesY1.y=k*L1.y*L1.y/(DY1.y+ Time*S1.y)/2/PI;
	//}
	//if(arg1.z>PI/L1.z)
	//{
	//	movesY1.z=k*L1.z*L1.z/(DY1.z+ Time*S1.z)/2/PI;
	//}
	//if(arg1.x>PI/L1.w)
	//{
	//	movesY1.w=k*L1.w*L1.w/(DY1.w+ Time*S1.w)/2/PI;
	//}
	
	//if(arg2.x>PI/L2.x)
	//{
	//	movesY2.x=k*L2.x*L2.x/(DY2.x+ Time*S2.x)/2/PI;
	//}
	//if(arg2.y>PI/L2.y)
	//{
	//	movesY2.y=k*L2.y*L2.y/(DY2.y+ Time*S2.y)/2/PI;
	//}
	//if(arg2.z>PI/L2.z)
	//{
	//	movesY2.z=k*L2.z*L2.z/(DY2.z+ Time*S2.z)/2/PI;
	//}
	//if(arg2.x>PI/L2.w)
	//{
	//	movesY2.w=k*L2.w*L2.w/(DY2.w+ Time*S2.w)/2/PI;
	//}
	//==================================================
	float4 finalPos=inPos;
     	finalPos.x+=heightsX1.x+movesX1.x;
    	finalPos.x+=heightsX1.y+movesX1.y;
     	finalPos.x+=heightsX1.z+movesX1.z;
     	finalPos.x+=heightsX1.w+movesX1.w;
	
	finalPos.x+=heightsX2.x+movesX2.x;
    	finalPos.x+=heightsX2.y+movesX2.y;
     	finalPos.x+=heightsX2.z+movesX2.z;
     	finalPos.x+=heightsX2.w+movesX2.w;

	finalPos.y+=heightsY1.x;
    	finalPos.y+=heightsY1.y;
    	finalPos.y+=heightsY1.z;
     	finalPos.y+=heightsY1.w;
	
	finalPos.y+=heightsY2.x;
    	finalPos.y+=heightsY2.y;
     	finalPos.y+=heightsY2.z;
     	finalPos.y+=heightsY2.w;

	finalPos.z+=heightsZ1.x+movesY1.x;
    	finalPos.z+=heightsZ1.y+movesY1.y;
     	finalPos.z+=heightsZ1.z+movesY1.z;
     	finalPos.z+=heightsZ1.w+movesY1.w;
	
	finalPos.z+=heightsZ2.x+movesY2.x;
    	finalPos.z+=heightsZ2.y+movesY2.y;
     	finalPos.z+=heightsZ2.z+movesY2.z;
     	finalPos.z+=heightsZ2.w+movesY2.w;

	Out.Position=mul(finalPos,preWorldViewProjection);

	Out.PosWS = mul(finalPos,World);
	
	float4 derivativesCOS1=A1*2*PI*cos(arg1)/L1;
	float4 derivativesCOS2=A2*2*PI*cos(arg2)/L2;
	
	float4 derivativesSIN1=A1*2*PI*sin(arg1)/L1;
	float4 derivativesSIN2=A2*2*PI*sin(arg2)/L2;
	//B
        
        float deviationsBX=Q1.x*D2X1.x*derivativesSIN1.x;
	deviationsBX+=Q1.y*D2X1.y*derivativesSIN1.y;
	deviationsBX+=Q1.z*D2X1.z*derivativesSIN1.z;
	deviationsBX+=Q1.w*D2X1.w*derivativesSIN1.w;
	
	deviationsBX+=Q2.x*D2X2.x*derivativesSIN2.x;
	deviationsBX+=Q2.y*D2X2.y*derivativesSIN2.y;
	deviationsBX+=Q2.z*D2X2.z*derivativesSIN2.z;
	deviationsBX+=Q2.w*D2X2.w*derivativesSIN2.w;

	deviationsBX =1-deviationsBX;
	
	float deviationsBY=DX1.x*derivativesCOS1.x;
	deviationsBY+=DX1.y*derivativesCOS1.y;
	deviationsBY+=DX1.z*derivativesCOS1.z;
	deviationsBY+=DX1.w*derivativesCOS1.w;

	deviationsBY+=DX2.x*derivativesCOS2.x;
	deviationsBY+=DX2.y*derivativesCOS2.y;
	deviationsBY+=DX2.z*derivativesCOS2.z;
	deviationsBY+=DX2.w*derivativesCOS2.w;
	deviationsBY=deviationsBY;

        float deviationsBZ=Q1.x*DXDY1.x*derivativesSIN1.x;
	deviationsBZ+=Q1.y*DXDY1.y*derivativesSIN1.y;
	deviationsBZ+=Q1.z*DXDY1.z*derivativesSIN1.z;
	deviationsBZ+=Q1.w*DXDY1.w*derivativesSIN1.w;
	
	deviationsBZ+=Q2.x*DXDY2.x*derivativesSIN2.x;
	deviationsBZ+=Q2.y*DXDY2.y*derivativesSIN2.y;
	deviationsBZ+=Q2.z*DXDY2.z*derivativesSIN2.z;
	deviationsBZ+=Q2.w*DXDY2.w*derivativesSIN2.w;

	deviationsBZ=-deviationsBZ;

	
	//T
		
        float deviationsTX=Q1.x*DXDY1.x*derivativesSIN1.x;
	deviationsTX+=Q1.y*DXDY1.y*derivativesSIN1.y;
	deviationsTX+=Q1.z*DXDY1.z*derivativesSIN1.z;
	deviationsTX+=Q1.w*DXDY1.w*derivativesSIN1.w;

	deviationsTX+=Q2.x*DXDY2.x*derivativesSIN2.x;
	deviationsTX+=Q2.y*DXDY2.y*derivativesSIN2.y;
	deviationsTX+=Q2.z*DXDY2.z*derivativesSIN2.z;
	deviationsTX+=Q2.w*DXDY2.w*derivativesSIN2.w;
	deviationsTX=-deviationsTX;

	float deviationsTY=DY1.x*derivativesCOS1.x;
	deviationsTY+=DY1.y*derivativesCOS1.y;
	deviationsTY+=DY1.z*derivativesCOS1.z;
	deviationsTY+=DY1.w*derivativesCOS1.w;

	deviationsTY+=DY2.x*derivativesCOS2.x;
	deviationsTY+=DY2.y*derivativesCOS2.y;
	deviationsTY+=DY2.z*derivativesCOS2.z;
	deviationsTY+=DY2.w*derivativesCOS2.w;

	deviationsTY=deviationsTY;
	
	float deviationsTZ=Q1.x*D2Y1.x*derivativesSIN1.x;
	deviationsTZ+=Q1.y*D2Y1.y*derivativesSIN1.y;
	deviationsTZ+=Q1.z*D2Y1.z*derivativesSIN1.z;
	deviationsTZ+=Q1.w*D2Y1.w*derivativesSIN1.w;
	
	deviationsTZ+=Q2.x*D2Y2.x*derivativesSIN2.x;
	deviationsTZ+=Q2.y*D2Y2.y*derivativesSIN2.y;
	deviationsTZ+=Q2.z*D2Y2.z*derivativesSIN2.z;
	deviationsTZ+=Q2.w*D2Y2.w*derivativesSIN2.w;
	deviationsTZ =1-deviationsTZ;

	//N
	
	float deviationsNX=DX1.x*derivativesCOS1.x;
	deviationsNX+=DX1.y*derivativesCOS1.y;
	deviationsNX+=DX1.z*derivativesCOS1.z;
	deviationsNX+=DX1.w*derivativesCOS1.w;

	deviationsNX+=DX2.x*derivativesCOS2.x;
	deviationsNX+=DX2.y*derivativesCOS2.y;
	deviationsNX+=DX2.z*derivativesCOS2.z;
	deviationsNX+=DX2.w*derivativesCOS2.w;	

	deviationsNX=-deviationsNX;

	float deviationsNY=Q1.x*derivativesSIN1.x;
	deviationsNY+=Q1.y*derivativesSIN1.y;
	deviationsNY+=Q1.z*derivativesSIN1.z;
	deviationsNY+=Q1.w*derivativesSIN1.w;
	
	deviationsNY+=Q2.x*derivativesSIN2.x;
	deviationsNY+=Q2.y*derivativesSIN2.y;
	deviationsNY+=Q2.z*derivativesSIN2.z;
	deviationsNY+=Q2.w*derivativesSIN2.w;

	deviationsNY=1-deviationsNY;
	
	float deviationsNZ=DY1.x*derivativesCOS1.x;
	deviationsNZ+=DY1.y*derivativesCOS1.y;
	deviationsNZ+=DY1.z*derivativesCOS1.z;
	deviationsNZ+=DY1.w*derivativesCOS1.w;
	
	deviationsNZ+=DY2.x*derivativesCOS2.x;
	deviationsNZ+=DY2.y*derivativesCOS2.y;
	deviationsNZ+=DY2.z*derivativesCOS2.z;
	deviationsNZ+=DY2.w*derivativesCOS2.w;

	deviationsNZ =-deviationsNZ;

	float3 Tangent=float3(deviationsTX,deviationsTY,deviationsTZ);
	float3 Binormal=float3(deviationsBX,deviationsBY,deviationsBZ);
	//float3 Normal=float3(deviationsNX,deviationsNY,deviationsNZ);
	float3 Normal=cross(Tangent,Binormal);
	Tangent=cross(Normal,Binormal);
	float3x3 tangentToObject;
	Normal=normalize(Normal);
	tangentToObject[0]=normalize(Tangent);
	tangentToObject[1]=normalize(Binormal);
	tangentToObject[2]=normalize(Normal);
	float3x3 TangentToWorld=mul(tangentToObject,World);
	Out.lighth.xyz=normalize(LightDirection1-Out.PosWS);
	Out.lighth.w=(h+heightsY1.x+heightsY1.y+heightsY1.z+heightsY1.w+heightsY2.x+heightsY2.y+heightsY2.z+heightsY2.w);
	Out.TanToWorld=TangentToWorld;

	Out.directionTex=direction; 
	
        return Out;
};
PS_OUTPUT PSNoWaterHead(VS_OUTPUT Input)
{
	PS_OUTPUT Out=(PS_OUTPUT)0;
	
	//计算反射纹理的采样坐标
	float2 ReflectionMapSamplingPosCoord;
	ReflectionMapSamplingPosCoord.x=Input.ReflectionMapSamplingPos.x/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ReflectionMapSamplingPosCoord.y=-Input.ReflectionMapSamplingPos.y/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
    
	//凹凸纹理
	  float4 bumpColor = tex2D(BumpMapSampler,Input.BumpMapSamplingPos); 
	
	float2 perturbation = xWaveHeight * (bumpColor.rg - 0.5f); 

	
	
	
	// 将反射贴图采样坐标从2D屏幕空间映射到纹理坐标    (反射颜色)
         float2 perturbatedTexCoords = ReflectionMapSamplingPosCoord+perturbation;
        float4 reflectionColor = tex2D(ReflectionSampler, perturbatedTexCoords);

     Out.Color = reflectionColor;
     
	float3 finalNormal=mul((bumpColor.rgb - 0.5f)*2,Input.TanToWorld);
	finalNormal=normalize(finalNormal);
	finalNormal=normalize(Input.TanToWorld[2]/1.2+finalNormal);
	float3 finalNormal2=normalize(Input.TanToWorld[2]);
	float3 tempfinalNormal=mul((bumpColor.rgb - 0.5f)*2,Input.TanToWorld);
	//finalNormal=finalNormal2;
	//=========================================DullColor===========================================
    
    //float4 dullColor = float4(0.1f, 0.1f, 0.2f, 1.0f); 
    //float dullBlendFactor = xDullBlendFactor;     
   // Out.Color = (dullBlendFactor*dullColor + (1-dullBlendFactor) * Out.Color); 
    
    //====================End==================DullColor====================End====================
    
    
    //========================================添加水的颜色==========================================
	float x1=Out.Color.r/(Out.Color.r  +  r1);
	float x2=Out.Color.g/(Out.Color.g  +  g1);
	float x3=Out.Color.b/(Out.Color.b  +  b1);
    Out.Color.r = Out.Color.r * 0.23 +  0.77*r1;
    Out.Color.g = Out.Color.g * 0.23 +  0.77*g1;
    Out.Color.b = Out.Color.b * 0.23 + 0.77*b1;
    Out.Color.a = a1;
	// Out.Color=float4(r1,g1,b1,0.94);
    //=========================End============添加水的颜色====================End===================

	float3 eyeVector=normalize(CameraPos-Input.PosWS);
	

    //==========================================Fresel=============================================
    
   
   float3 xNormalVector = normalize(finalNormal);
    ///////////////////////////////////////////////// 
    // FRESNEL TERM APPROXIMATION 
    ///////////////////////////////////////////////// 
    float fresnelTerm = (float)0; 
 
    if ( xFresnelMode == 0 ) 
    { 
      fresnelTerm = 1-dot(eyeVector, xNormalVector)*1.3f; 
    } 
    else if ( xFresnelMode == 1 ) 
    { 
      fresnelTerm = 0.02+0.97f*pow((1-dot(eyeVector, xNormalVector)),5); 
    } 
    else if ( xFresnelMode == 2 ) 
    { 
      float fangle = 1.0f+dot(eyeVector, xNormalVector); 
      fangle = pow(fangle,5);   
      // fresnelTerm = fangle*50; 
      fresnelTerm = 1/fangle; 
    } 
 
    //fresnelTerm = (1/pow((fresnelTerm+1.0f),5))+0.2f; //  
       
    //Hardness factor - user input 
    fresnelTerm = fresnelTerm * xDrawMode; 
     
    //just to be sure that the value is between 0 and 1; 
    fresnelTerm = fresnelTerm < 0? 0 : fresnelTerm; 
    fresnelTerm = fresnelTerm > 1? 1 : fresnelTerm; 
    
 
    // creating the combined color 
	float4 watercolor=float4(r1,g1,b1,0.8f);
   // Out.Color =  watercolor*(1-fresnelTerm)  +   Out.Color*(fresnelTerm); 
    
    //======================End=================Fresel=================End=========================

    //========================================漫反射==========================================
	float diff=saturate(-dot(-finalNormal,LightDirection1));
	float diff2=saturate(dot(finalNormal2,LightDirection1));
	float4 diffuse;
	float4 diffcolor=float4(1.f,1.f,1.f,1.0f);
	//计算漫反射光
	diffuse = Out.Color  * diff;
	float4 diffuse2=diffcolor  * diff2;
	
    //=========================End=============漫反射======================End======================= 

    //=========================================高光================================================
    float4 specular;
    if(SpecMode==1)
     {		
	float3 refLight = reflect(LightDirection2,tempfinalNormal);
	refLight=normalize(refLight);
	float specFactor = pow(dot(refLight,normalize(eyeVector)),190);
	float4 speclight=float4(0.98f,0.97f,0.7f,1.f);
	specular =  specFactor*speclight*0.95;
     }	
    else if (SpecMode==2)
     {
	float3 light=normalize(LightDirection1);
	float ln=max(dot(light,tempfinalNormal),0);
   	float3 tanVector=normalize(cross(eyeVector,tempfinalNormal));
	bool back=(dot(tempfinalNormal,eyeVector)>0)&&(dot(light,tempfinalNormal)) ;
	//高光处理
	specular=float4(0,0,0,0);
        if(back)
	{	
	float LdotT=dot(light,tanVector);
	float VdotT=dot(tempfinalNormal,tanVector);
	float a1=sqrt(1-LdotT*LdotT);
	float a2=sqrt(1-VdotT*VdotT);
	float a3=LdotT*VdotT;
	float a4=a1*a2-a3;
	float4 Ii=float4(0.98f,0.97f,0.7f,1.f);
	//float4 Ii=float4(1.f,1.f,1.f,1.f);
	specular = pow(a4,55)*Ii*ln;
	}	
     }
     else if (SpecMode==3)
     {
	float3 halfVector=normalize(eyeVector+LightDirection2);
	float specFactor = pow(dot(halfVector,normalize(tempfinalNormal)),250);
	float4 speclight=float4(0.98f,0.97f,0.7f,1.f);
	specular =  specFactor*speclight*0.75;
     }
    //=========================End=============高光======================End======================= 
 
	float4 directionTexColor = tex2D(DirectionSampler,Input.directionTex);
        
	Out.Color =(Out.Color*0.9+diffuse*0.5+specular*0.0)*0.8+directionTexColor*0.2;

//float a=0.54;
	
if(Input.lighth.w<5)
{
 	//a1=Input.lighth.w/5;
	//Out.Color.a=a1;
}
	
	//float4 s=float4(r1,g1,b1,1.f);
	//Out.Color=s;
	return Out;
};

PS_OUTPUT PSWithWaterHead(VS_OUTPUTWaterHead Input)
{
	PS_OUTPUT Out=(PS_OUTPUT)0;
	
	//计算反射纹理的采样坐标
	float2 ReflectionMapSamplingPosCoord;
	ReflectionMapSamplingPosCoord.x=Input.ReflectionMapSamplingPos.x/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ReflectionMapSamplingPosCoord.y=-Input.ReflectionMapSamplingPos.y/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
    
	//凹凸纹理
	  float4 bumpColor = tex2D(BumpMapSampler,Input.BumpMapSamplingPos); 
	
	float2 perturbation = xWaveHeight * (bumpColor.rg - 0.5f); 

	
	
	
	// 将反射贴图采样坐标从2D屏幕空间映射到纹理坐标    (反射颜色)
         float2 perturbatedTexCoords = ReflectionMapSamplingPosCoord+perturbation;
        float4 reflectionColor = tex2D(ReflectionSampler, perturbatedTexCoords);

     Out.Color = reflectionColor;
     
	float3 finalNormal=mul((bumpColor.rgb - 0.5f)*2,Input.TanToWorld);
	finalNormal=normalize(finalNormal);
	finalNormal=normalize(Input.TanToWorld[2]/1.2+finalNormal);
	float3 finalNormal2=normalize(Input.TanToWorld[2]);
	float3 tempfinalNormal=mul((bumpColor.rgb - 0.5f)*2,Input.TanToWorld);
	//finalNormal=finalNormal2;
	//=========================================DullColor===========================================
    
    //float4 dullColor = float4(0.1f, 0.1f, 0.2f, 1.0f); 
    //float dullBlendFactor = xDullBlendFactor;     
   // Out.Color = (dullBlendFactor*dullColor + (1-dullBlendFactor) * Out.Color); 
    
    //====================End==================DullColor====================End====================
    
    
    //========================================添加水的颜色==========================================
	float x1=Out.Color.r/(Out.Color.r  +  r1);
	float x2=Out.Color.g/(Out.Color.g  +  g1);
	float x3=Out.Color.b/(Out.Color.b  +  b1);
    Out.Color.r = Out.Color.r * 0.23 +  0.77*r1;
    Out.Color.g = Out.Color.g * 0.23 +  0.77*g1;
    Out.Color.b = Out.Color.b * 0.23 + 0.77*b1;
    Out.Color.a = a1;
	// Out.Color=float4(r1,g1,b1,0.94);
    //=========================End============添加水的颜色====================End===================

	float3 eyeVector=normalize(CameraPos-Input.PosWS);
	

    //==========================================Fresel=============================================
    
   
   float3 xNormalVector = normalize(finalNormal);
    ///////////////////////////////////////////////// 
    // FRESNEL TERM APPROXIMATION 
    ///////////////////////////////////////////////// 
    float fresnelTerm = (float)0; 
 
    if ( xFresnelMode == 0 ) 
    { 
      fresnelTerm = 1-dot(eyeVector, xNormalVector)*1.3f; 
    } 
    else if ( xFresnelMode == 1 ) 
    { 
      fresnelTerm = 0.02+0.97f*pow((1-dot(eyeVector, xNormalVector)),5); 
    } 
    else if ( xFresnelMode == 2 ) 
    { 
      float fangle = 1.0f+dot(eyeVector, xNormalVector); 
      fangle = pow(fangle,5);   
      // fresnelTerm = fangle*50; 
      fresnelTerm = 1/fangle; 
    } 
 
    //fresnelTerm = (1/pow((fresnelTerm+1.0f),5))+0.2f; //  
       
    //Hardness factor - user input 
    fresnelTerm = fresnelTerm * xDrawMode; 
     
    //just to be sure that the value is between 0 and 1; 
    fresnelTerm = fresnelTerm < 0? 0 : fresnelTerm; 
    fresnelTerm = fresnelTerm > 1? 1 : fresnelTerm; 
    
 
    // creating the combined color 
	float4 watercolor=float4(r1,g1,b1,0.8f);
   // Out.Color =  watercolor*(1-fresnelTerm)  +   Out.Color*(fresnelTerm); 
    
    //======================End=================Fresel=================End=========================

    //========================================漫反射==========================================
	float diff=saturate(-dot(-finalNormal,LightDirection1));
	float diff2=saturate(dot(finalNormal2,LightDirection1));
	float4 diffuse;
	float4 diffcolor=float4(1.f,1.f,1.f,1.0f);
	//计算漫反射光
	diffuse = Out.Color  * diff;
	float4 diffuse2=diffcolor  * diff2;
	
    //=========================End=============漫反射======================End======================= 

    //=========================================高光================================================
    float4 specular;
    if(SpecMode==1)
     {		
	float3 refLight = reflect(LightDirection2,tempfinalNormal);
	refLight=normalize(refLight);
	float specFactor = pow(dot(refLight,normalize(eyeVector)),190);
	float4 speclight=float4(0.98f,0.97f,0.7f,1.f);
	specular =  specFactor*speclight*0.95;
     }	
    else if (SpecMode==2)
     {
	float3 light=normalize(LightDirection1);
	float ln=max(dot(light,tempfinalNormal),0);
   	float3 tanVector=normalize(cross(eyeVector,tempfinalNormal));
	bool back=(dot(tempfinalNormal,eyeVector)>0)&&(dot(light,tempfinalNormal)) ;
	//高光处理
	specular=float4(0,0,0,0);
        if(back)
	{	
	float LdotT=dot(light,tanVector);
	float VdotT=dot(tempfinalNormal,tanVector);
	float a1=sqrt(1-LdotT*LdotT);
	float a2=sqrt(1-VdotT*VdotT);
	float a3=LdotT*VdotT;
	float a4=a1*a2-a3;
	float4 Ii=float4(0.98f,0.97f,0.7f,1.f);
	//float4 Ii=float4(1.f,1.f,1.f,1.f);
	specular = pow(a4,55)*Ii*ln;
	}	
     }
     else if (SpecMode==3)
     {
	float3 halfVector=normalize(eyeVector+LightDirection2);
	float specFactor = pow(dot(halfVector,normalize(tempfinalNormal)),250);
	float4 speclight=float4(0.98f,0.97f,0.7f,1.f);
	specular =  specFactor*speclight*0.75;
     }
    //=========================End=============高光======================End======================= 
 
	float4 directionTexColor = tex2D(DirectionSampler,Input.directionTex);
        
	Out.Color =(Out.Color*0.9+diffuse*0.5+specular*0.0)*0.8+directionTexColor*0.2;
       
        float4 waterHeadColor=tex2D(WaterHeadSampler,Input.waterHeadTex);
        Out.Color =Out.Color*0.5+waterHeadColor*0.5;

	

//float a=0.54;
	
if(Input.lighth.w<5)
{
 	//a1=Input.lighth.w/5;
	//Out.Color.a=a1;
}
	
	//float4 s=float4(r1,g1,b1,1.f);
	//Out.Color=s;
	return Out;
};

technique WaterRenderWithHead
{   pass P0
      { 
           VertexShader = compile vs_3_0 VSWithWaterHead();
           PixelShader  = compile ps_3_0 PSWithWaterHead();

       }
      
}
technique WaterRender
{   
    pass P0    
       {   VertexShader = compile vs_3_0 VSNoWaterHead();
           PixelShader  = compile ps_3_0 PSNoWaterHead();
       } 
      

}