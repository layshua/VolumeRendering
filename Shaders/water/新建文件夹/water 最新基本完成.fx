//float4x4 WorldViewProject:	WORLDVIEWPROJECTION;
float4x4 World:	WORLD;   //世界矩阵
float4x4 View:	VIEW;   //视矩阵
float4x4 Projection: PROJECTION; // 投影矩阵
float4x4 ReflectionView; //反射视矩阵

float4 CameraPos;	//相机位置
float xSpecPerturb = 10.0f;//高光范围宽度

texture ReflectTexture;//水面反射纹理

float4 S1=float4(20,40,35,35);//四个正弦波的波速
float4 A1=float4(0.36,0.74,0.66,0.66);//四个正弦波的振幅
float4 L1=float4(192,180,216,219);//四个正弦波的波长
float4 Q1=float4(0.45,0.35,0.3,0.4);

float4 S2=float4(14,17,32,28);//四个正弦波的波速
float4 A2=float4(0.46,0.63,0.86,0.62);//四个正弦波的振幅
float4 L2=float4(185,283,229,204);//四个正弦波的波长
float4 Q2=float4(0.3,0.39,0.5,0.4);

float2 D0=float2(0.9,0);
float2 D1=float2(-0.5,0.5);
float2 D2=float2(-0.7,0);
float2 D3=float2(-0.6,-0.54);//以上四个为四个正弦波的方向
float2 D4=float2(0,-0.8);
float2 D5=float2(0.6,-0.64);
float2 D6=float2(0.92,0);
float2 D7=float2(0.46,0.51);//以上四个为四个正弦波的方向


float     Time;                             // 时间
float     bumpT;
float PI=3.14;
float Factor=0.01;
float3 LightDirection; //光源方向
float4 AmbientColor;        //环境光
float4 DiffuseColor;        //漫反射光
float AmbientIntensity;//材质反射环境光衰减系数
float DiffuseIntensity;//材质漫反射衰减系数

//凹凸
texture BumpTexture;//水面的凹凸贴图
float WaveHeight;                           // 振幅
float WaveLength;                           //波长
float4 WindDirection;                       //风向
float WindForce;               		    //风力


float r1;
float g1;
float b1;

struct VS_OUTPUT
{
    float4 Position                  : POSITION;
    float4 PosWS  		     : TEXCOORD1;
    float4 ReflectionMapSamplingPos  : TEXCOORD2;
    float3 normal 	     	     : TEXCOORD3;
    float4 lighth		     : TEXCOORD4;
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


VS_OUTPUT VS ( float4 inPos : POSITION,float3 h : TEXCOORD0)
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

	float4 finalPos=inPos;
     	finalPos.x+=heightsX1.x;
    	finalPos.x+=heightsX1.y;
     	finalPos.x+=heightsX1.z;
     	finalPos.x+=heightsX1.w;
	
	finalPos.x+=heightsX2.x;
    	finalPos.x+=heightsX2.y;
     	finalPos.x+=heightsX2.z;
     	finalPos.x+=heightsX2.w;

	finalPos.y+=heightsY1.x;
    	finalPos.y+=heightsY1.y;
     	finalPos.y+=heightsY1.z;
     	finalPos.y+=heightsY1.w;
	
	finalPos.y+=heightsY2.x;
    	finalPos.y+=heightsY2.y;
     	finalPos.y+=heightsY2.z;
     	finalPos.y+=heightsY2.w;

	finalPos.z+=heightsZ1.x;
    	finalPos.z+=heightsZ1.y;
     	finalPos.z+=heightsZ1.z;
     	finalPos.z+=heightsZ1.w;
	
	finalPos.z+=heightsZ2.x;
    	finalPos.z+=heightsZ2.y;
     	finalPos.z+=heightsZ2.z;
     	finalPos.z+=heightsZ2.w;

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
	Out.lighth.xyz=normalize(LightDirection-Out.PosWS);
	Out.lighth.w=(h+heightsY1.x+heightsY1.y+heightsY1.z+heightsY1.w+heightsY2.x+heightsY2.y+heightsY2.z+heightsY2.w);

	float2 absoluteTexCoords=Out.ReflectionMapSamplingPos/Out.ReflectionMapSamplingPos.w/2+0.5;
        
	//凹凸纹理采样坐标
	float2 BumpMapSamplingPos =absoluteTexCoords;


	//法线
	float2 bumpcood1 = BumpMapSamplingPos+float2(Time*0.04,Time*0.04);
	float2 bumpcood2 = BumpMapSamplingPos*3.0f+float2(Time*0.04,Time*0.04);
	float2 bumpcood3 = BumpMapSamplingPos/2+float2(Time*0.02,Time*0.02);
	
	float4 bumptemp1=tex2Dlod(BumpMapSampler,float4(bumpcood1.xy,0,0));
	float4 bumptemp2=tex2Dlod(BumpMapSampler,float4(bumpcood2.xy,0,0));
	float4 bumptemp3=tex2Dlod(BumpMapSampler,float4(bumpcood3.xy,0,0));
	
	float3 bumpNormalA = normalize((float3(bumptemp1.x,bumptemp1.y,bumptemp1.z)-0.5f)*2);
	float3 bumpNormalB = normalize((float3(bumptemp2.x,bumptemp2.y,bumptemp2.z)-0.5f)*2);
	float3 bumpNormalC = normalize((float3(bumptemp3.x,bumptemp3.y,bumptemp3.z)-0.5f)*2);

	float3 finalNormal=normalize((Normal+normalize(mul((bumpNormalA+bumpNormalB+bumpNormalC),TangentToWorld))/0.9));

	Out.Position=dot(finalNormal,Normal)*Out.Position;

	Out.normal=finalNormal;
        return Out;
};
PS_OUTPUT PS(VS_OUTPUT Input)
{
	PS_OUTPUT Out=(PS_OUTPUT)0;
	
	//凹凸纹理
	
	
	float2 perturbation = 0.59*Input.normal.xz;

	//计算反射纹理的采样坐标
	float2 ReflectionMapSamplingPosCoord;
	ReflectionMapSamplingPosCoord.x=Input.ReflectionMapSamplingPos.x/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ReflectionMapSamplingPosCoord.y=-Input.ReflectionMapSamplingPos.y/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	
	
	// 将反射贴图采样坐标从2D屏幕空间映射到纹理坐标    (反射颜色)
         float2 perturbatedTexCoords = ReflectionMapSamplingPosCoord+perturbation;
        float4 reflectionColor = tex2D(ReflectionSampler, perturbatedTexCoords);
	
	
	float3 finalNormal=Input.normal;
	//金沙江水质颜色

	
	
	float r2=r1+0.01f;
	float g2=g1+0.01f;
	float b2=b1+0.01f;
	//float r1=128.0/255.0f;
	//float g1=108.0/255.0f;
	//float b1=184.0/255.0f;
	
	//float r2=129.0/255.0f;
	//float g2=110.0/255.0f;
	//float b2=186.0/255.0f;
	
	float3 eye_dir=normalize(CameraPos-Input.PosWS);
	float3 light_dir = normalize(LightDirection);
	float4 WaterColorShallow =float4(r2,g2,b2,0.8 );
	float4 WaterColorDeep =float4(r1,g1,b1,0.8 );
	float facing =dot(light_dir,eye_dir);
	
	float4 waterbody_color=lerp(WaterColorShallow,WaterColorDeep,facing);
		
	float4 ambient = waterbody_color * 0.2f;
	float4 diffuse = waterbody_color;
	
	
	
	float4 temp = 0;
   	float3 reflectV = 2 * cross(dot(light_dir , finalNormal),finalNormal)-light_dir;
   	reflectV  = normalize(reflectV + float3( perturbation.x * xSpecPerturb,perturbation.y * xSpecPerturb , 0 )); 
 
 	temp = pow(dot(reflectV,eye_dir),250 ); 
   	float4 speccolor = float4(0.98f,0.97f,0.7f,0.6f);  
	
    	speccolor = speccolor * temp; 
   	speccolor = float4(speccolor.x * speccolor.w, speccolor.y * speccolor.w, speccolor.z * speccolor.w, 0 ); 

	
	
	
	
	//float diff=abs(dot(Input.lighth.xyz,finalNormal));
	float diff=saturate(-dot(normalize(light_dir),finalNormal));

	

	float4 allcolor=lerp(reflectionColor,waterbody_color,0.87);
	
	diffuse=allcolor*(diff);
	

float a=1;
	
if(Input.lighth.w<4)
{
a=Input.lighth.w/4;
}
	
	
	 Out.Color = ambient+diffuse;
	Out.Color.a=a;

	return Out;
};
technique WaterRender
{   pass P0
      {
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
      }
}