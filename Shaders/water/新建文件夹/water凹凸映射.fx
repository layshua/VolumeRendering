//float4x4 WorldViewProject:	WORLDVIEWPROJECTION;
float4x4 World:	WORLD;   //�������
float4x4 View:	VIEW;   //�Ӿ���
float4x4 Projection: PROJECTION; // ͶӰ����
float4x4 ReflectionView; //�����Ӿ���

float4 CameraPos;	//���λ��

texture BumpTexture;//ˮ��İ�͹��ͼ
texture BumpTexture1;//ˮ��İ�͹��ͼ

texture ReflectTexture;//ˮ�淴������
texture HeightTexture;//��ĭ����

float WaveHeight;                           // ���
float WaveLength;                           //����

//float4 WindDirection;                       //����
float WindForce;               		    //����

float     Time;                             // ʱ��


float3 LightDirection; //��Դ����
float4 AmbientColor;        //������
float4 DiffuseColor;        //�������
float AmbientIntensity;//���ʷ��价����˥��ϵ��
float DiffuseIntensity;//����������˥��ϵ��

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
sampler ReflectionSampler = sampler_state  //���������
{
	Texture = <ReflectTexture>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
        AddressU = wrap; 
	AddressV = wrap;
};

sampler BumpMapSampler= sampler_state  //��͹���������
{ 
	texture = <BumpTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter=LINEAR; 
	AddressU = wrap; 
	AddressV = wrap;
};

sampler BumpMapSampler1= sampler_state  //��͹���������
{ 
	texture = <BumpTexture1>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter=LINEAR; 
	AddressU = wrap; 
	AddressV = wrap;
};

sampler HeightSampler = sampler_state  //�߶�ͼ������
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
	//���㷴������Ĳ�������
	float4x4 preReflectionViewProjection = mul(ReflectionView,Projection);
	float4x4 preWorldReflectionViewProjection = mul(World,preReflectionViewProjection);		
	Out.ReflectionMapSamplingPos=mul(inPos,preWorldReflectionViewProjection);
	Out.HeightSamplingPos=Out.ReflectionMapSamplingPos;

	

	//��͹�����������

	
	float3 windDir=normalize(WindDirection);
	float3 normal=normalize(inNormal);
	float3 perpDir=cross(WindDirection,normal);
  
        // ��ȡ��ˮ����������������uv����

	float4 absoluteTexCoords=Out.ReflectionMapSamplingPos/Out.ReflectionMapSamplingPos.w/2+0.5;
	float4 rotatedTexCoords = mul(absoluteTexCoords, WindDirection);
    	float2 moveVector = float2(1.25, 1.25); 

	//��͹�����������
	  Out.BumpMapSamplingPos =absoluteTexCoords.xy / WaveLength+ Time * WindForce * moveVector.xy;
	
	//������������
	

	//ת������
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
	//��͹����
	float4 bumpColor = tex2D(BumpMapSampler,Input.BumpMapSamplingPos);
	float4 bumpColor1 = tex2D(BumpMapSampler1,Input.BumpMapSamplingPos);

	float2 perturbation = WaveHeight*(bumpColor.rg-0.5f)*2;
	
	//���㷴������Ĳ�������
	float2 ReflectionMapSamplingPosCoord;
	ReflectionMapSamplingPosCoord.x=Input.ReflectionMapSamplingPos.x/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ReflectionMapSamplingPosCoord.y=-Input.ReflectionMapSamplingPos.y/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	
	
	// ��������ͼ���������2D��Ļ�ռ�ӳ�䵽��������    (������ɫ)
        float2 perturbatedTexCoords = ReflectionMapSamplingPosCoord+perturbation;
        float4 reflectionColor = tex2D(ReflectionSampler, perturbatedTexCoords);
	
	
	//�������ߴ���
	float3 tanNormalVector;

 	tanNormalVector= ((bumpColor.rgb)-0.5f)*2;


	tanNormalVector=normalize(tanNormalVector);


	float3 tanLightDir= normalize(Input.LightDirT);
	
	float3 tanEyeDir=normalize(Input.ViewDirT);


	//��ɳ��ˮ����ɫ
	float r=124.0f/255.0f;
	float g=129.0f/255.0f;
	float b=100.0f/255.0f;
	//float r=95.0f/255.0f;
	//float g=105.0f/255.0f;
	//float b=166.0f/255.0f;
	//float4 JSJWaterColor =float4(0.3f,0.3f,0.5f,1.0f);
	float4 JSJWaterColor =float4(r,g,b,0.94 );
	//������ɫ
	Out.Color=lerp(reflectionColor,JSJWaterColor,0.77f);


	//���մ���
	float4 ambcolor=float4(AmbientColor.rgb,1.0f);
	float4 diffcolor=float4(DiffuseColor.rgb,1.0f);
	//������
	float diff=-dot(tanNormalVector,tanLightDir);
	float4 diffuse=0.0f;
	float4 specular=0.0f;
	if(diff > 0)
	{
		//�����������
		diffuse = 0.3*JSJWaterColor * DiffuseColor * diff;
		//�߹⴦��
		float3 refLight = reflect(tanLightDir,tanNormalVector);
		float specFactor = pow(max(dot(refLight,tanEyeDir),0.f),25);
		specular =  specFactor;
	}

	
float a=1;
	
if(Input.h.x<4)
{
a=Input.h.x/4;
}

	Out.Color=   Out.Color   //�����⴦��	
	        +diffuse+specular;//�����䴦��
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