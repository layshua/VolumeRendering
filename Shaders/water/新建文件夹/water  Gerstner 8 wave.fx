//float4x4 WorldViewProject:	WORLDVIEWPROJECTION;
float4x4 World:	WORLD;   //世界矩阵
float4x4 View:	VIEW;   //视矩阵
float4x4 Projection: PROJECTION; // 投影矩阵
float4x4 ReflectionView; //反射视矩阵

float4 CameraPos;	//相机位置

texture ReflectTexture;//水面反射纹理




float     Time;                             // 时间

float PI=3.14;

float4 LightDirection; //光源方向
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
    float4 light		     : TEXCOORD6;
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

		float S[8];//正弦波的波速
	float A[8];//正弦波的振幅
	float L[8];//正弦波的波长
	float2 D[8];//正弦波的方向
	float Q[8];//控制波的尖锐程度
	S[0] = 6.0f;
	S[1] = 6.3f;
	S[2] = 4.6f;
	S[3] = 1.7f;
	S[4] = 7.9f;
	S[5] = 4.0f;
	S[6] = 3.1f;
	S[7] = 2.4f;

	A[0]= 4.2f;
	A[1]= 3.4f;
	A[2]= 3.6f;
	A[3]= 2.8f;
	A[4]= 3.0f;
	A[5]= 4.2f;
	A[6]= 2.4f;
	A[7]= 5.6f;
	
	L[0]= 320.0f;
	L[1]= 250.0f;
	L[2]= 260.0f;
	L[3]= 270.0f;
	L[4]= 280.0f;
	L[5]= 190.0f;
	L[6]= 200.0f;
	L[7]= 240.0f;

	D[0]=float2(-0.5f, -0.5f);
	D[1]=float2(0.3f, 0.4f);
	D[2]=float2(0.2f, -0.3f);
	D[3]=float2(0.4f, 0.9f);
	D[4]=float2(-0.3f, 0.5f);
	D[5]=float2(0.8f, 0.2f);
	D[6]=float2(0.9f, -0.1f);
	D[7]=float2(0.6f, 0.4f);
	
	Q[0]= 0.8f;
	Q[1]= 0.7f;
	Q[2]= 0.5f;
	Q[3]= 0.4f;
	Q[4]= 0.5f;
	Q[5]= 0.6f;
	Q[6]= 0.7f;
	Q[7]= 0.8f;

	
	VS_OUTPUT Out = (VS_OUTPUT)0;

	float4x4 preViewProjection = mul (View,Projection); 
	float4x4 preWorldViewProjection = mul(World,preViewProjection);

	Out.Position = mul(inPos,preWorldViewProjection);

	
	//计算反射纹理的采样坐标
	float4x4 preReflectionViewProjection = mul(ReflectionView,Projection);
	float4x4 preWorldReflectionViewProjection = mul(World,preReflectionViewProjection);		
	Out.ReflectionMapSamplingPos=mul(inPos,preWorldReflectionViewProjection);
	
	//计算波
	float dotProducts[8];
	int i=0;
	for(i=0;i<8;i++)
	{
	   dotProducts[i]=dot(D[i].xy,inPos.xz);
	}
  	
    	float arg[8];
	for(i=0;i<8;i++)
	{
	   arg[i]=dotProducts[i]*2*PI/L[i]+Time*S[i]*2*PI/L[i];
	}
	  float D2X[8];
	  float D2Y[8];
	  float DXDY[8];
	  float DX[8];
	  float DY[8];
	for(i=0;i<8;i++)
	{  
	   D2X[i]=D[i].x*D[i].x;
	   D2Y[i]=D[i].y*D[i].y;
	   DXDY[i]=D[i].x*D[i].y;
	   DX[i]=D[i].x;
	   DY[i]=D[i].y;
	}
	float4 finalPos=inPos;
	for(i=0;i<8;i++)
	{  
	  finalPos.x+=Q[i]*A[i]*DX[i]*cos(arg[i]); 	
	
	  finalPos.y+=A[i]*sin(arg[i]);

	  finalPos.z+=Q[i]*A[i]*DY[i]*cos(arg[i]);	
	
	}

    	
	
	Out.Position=mul(finalPos,preWorldViewProjection);

	Out.PosWS = mul(finalPos,World);
	
	float derivativesCOS[8];
	float derivativesSIN[8];

	for(i=0;i<8;i++)
	{  
	  derivativesCOS[i]=A[i]*2*PI*cos(arg[i])/L[i];
	  derivativesSIN[i]=A[i]*2*PI*sin(arg[i])/L[i];
	}

	
	
	//B
        float deviationsBX=0;
	float deviationsBY=0;
	float deviationsBZ=0;
	for(i=0;i<8;i++)
	{  
	  deviationsBX+=Q[i]*D2X[i]*derivativesSIN[i];
	  deviationsBY+=DX[i]*derivativesCOS[i];
	  deviationsBZ+=Q[i]*DXDY[i]*derivativesSIN[i];
	}

	deviationsBX =1-deviationsBX;
	
	deviationsBY=deviationsBY;

	deviationsBZ=-deviationsBZ;

	
	//T
		
	float deviationsTX=0;
	float deviationsTY=0;
	float deviationsTZ=0;
	for(i=0;i<8;i++)
	{  
	  deviationsTX+=Q[i]*DXDY[i]*derivativesSIN[i];
	  deviationsTY+=DY[i]*derivativesCOS[i];
	  deviationsTZ+=Q[i]*D2Y[i]*derivativesSIN[i];
	}
      
	deviationsTX=-deviationsTX;
	
	deviationsTY=deviationsTY;
		
	deviationsTZ =1-deviationsTZ;
	//N
	
	float deviationsNX=0;
	float deviationsNY=0;
	float deviationsNZ=0;
	for(i=0;i<8;i++)
	{  
	  deviationsNX+=DX[i]*derivativesCOS[i];
	  deviationsNY+=Q[i]*derivativesSIN[i];
	  deviationsNZ+=DY[i]*derivativesCOS[i];
	}

	deviationsNX=-deviationsNX;

	deviationsNY=1-deviationsNY;

	deviationsNZ =-deviationsNZ;

	float3 Tangent=float3(deviationsTX,deviationsTY,deviationsTZ);
	float3 Binormal=float3(deviationsBX,deviationsBY,deviationsBZ);
	float3 Normal=float3(deviationsNX,deviationsNY,deviationsNZ);
	//float3 Normal=cross(Tangent,Binormal);
	
	float3x3 tangentToObject;
	tangentToObject[0]=normalize(Tangent);
	tangentToObject[1]=normalize(Binormal);
	tangentToObject[2]=normalize(Normal);
	Out.TangentToWorld=mul(tangentToObject,World);
	float4 templight=mul(LightDirection,World);
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
		diffuse ==Out.Color* DiffuseColor*diff ;
		
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