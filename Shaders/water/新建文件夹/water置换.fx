//float4x4 WorldViewProject:	WORLDVIEWPROJECTION;
float4x4 World:	WORLD;   //�������
float4x4 View:	VIEW;   //�Ӿ���
float4x4 Projection: PROJECTION; // ͶӰ����
float4x4 ReflectionView; //�����Ӿ���

float4 CameraPos;	//���λ��

texture BumpTexture;//ˮ��İ�͹��ͼ
texture ReflectTexture;//ˮ�淴������
texture HeightTexture;//��ĭ����

float WaveHeight;                           // ���
float WaveLength;                           //����
float4 WindDirection;                       //����
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
    float3 normal					 : TEXCOORD2;
    float4 HeightSamplingPos         : TEXCOORD3;
    float3 LightDirT				   : TEXCOORD4;
    float3 ViewDirT	             : TEXCOORD5;
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

sampler BumpMapSampler = sampler_state  //��͹���������
{ 
	texture = <BumpTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter=LINEAR; 
	AddressU = wrap; 
	AddressV = wrap;
};

sampler HeightSampler = sampler_state  //��͹���������
{ 
	texture = <HeightTexture>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter=LINEAR; 
	AddressU = wrap; 
	AddressV = wrap;
};

float Height(float4 h)
{
return	(0.2990*h.x + 0.5870*h.y + 0.1140*h.z);
}
float off=1.0/255;
VS_OUTPUT VS ( float4 inPos : POSITION0,float4 inriverdir :POSITION1)
{
	//WindDirection=inriverdir;
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

	//����߶�ͼ�Ĳ�������
	float2 	HeightSamplingPosCoord;
	HeightSamplingPosCoord=Out.HeightSamplingPos/Out.HeightSamplingPos.w/2.0f + 0.5f;
	float4 hij=tex2Dlod(HeightSampler, float4(HeightSamplingPosCoord.xy,0,0));
	float4 hi1j=tex2Dlod(HeightSampler, float4((HeightSamplingPosCoord.x+off),HeightSamplingPosCoord.y,0,0));
	float4 hij1=tex2Dlod(HeightSampler, float4(HeightSamplingPosCoord.x,(HeightSamplingPosCoord.y+off),0,0));
	//�����пռ�
	float3 tangent=normalize(float3(1,0,Height(hi1j)-Height(hij)));
	float3 binormal=normalize(float3(0,1,Height(hij1)-Height(hij)));
	float3 normal=cross(tangent,binormal);
	Out.Position.y+=hij*30;
	Out.normal=normalize(normal);;
	float3x3 TangentToObject; 
	TangentToObject[0] = tangent; 
	TangentToObject[1] = binormal; 
	TangentToObject[2] = normal;
	float3x3 TangentToWorld=(TangentToObject,World);

	Out.LightDirT = mul(TangentToWorld,LightDirection);
	float3 eyeVector=CameraPos-WorldPosition;
	Out.ViewDirT=mul(TangentToWorld,eyeVector);
        return Out;
};
PS_OUTPUT PS(VS_OUTPUT Input)
{
	PS_OUTPUT Out=(PS_OUTPUT)0;
	//��͹����

	float2 perturbation =Input.normal.xy/2;

	//���㷴������Ĳ�������
	float2 ReflectionMapSamplingPosCoord;
	ReflectionMapSamplingPosCoord.x=Input.ReflectionMapSamplingPos.x/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	ReflectionMapSamplingPosCoord.y=-Input.ReflectionMapSamplingPos.y/Input.ReflectionMapSamplingPos.w/2.0f + 0.5f;
	
	
	// ��������ͼ���������2D��Ļ�ռ�ӳ�䵽��������    (������ɫ)
        float2 perturbatedTexCoords = ReflectionMapSamplingPosCoord+perturbation;
        float4 reflectionColor = tex2D(ReflectionSampler, perturbatedTexCoords);
	


	//�������ߴ���
	float3 tanNormalVector;
 	tanNormalVector= Input.normal;
	
	tanNormalVector=normalize(tanNormalVector);

	float3 tanLightDir= normalize(Input.LightDirT);
	
	float3 tanEyeDir=normalize(Input.ViewDirT);


	//��ɳ��ˮ����ɫ
	float r=124.0f/255.0f;
	float g=129.0f/255.0f;
	float b=100.0f/255.0f;
	//float4 JSJWaterColor =float4(0.3f,0.3f,0.5f,1.0f);
	float4 JSJWaterColor =float4(r,g,b,0.94 );
	//������ɫ
	Out.Color=lerp(reflectionColor,JSJWaterColor,0.77);


	//���մ���
	float4 ambcolor=float4(AmbientColor.rgb,1.0f);
	float4 diffcolor=float4(DiffuseColor.rgb,1.0f);
	//������
	float diff=saturate(dot(tanLightDir,tanNormalVector));
	


	//�߹⴦��
	float3 reflectionVector = -reflect(tanLightDir,tanNormalVector);
	reflectionVector=normalize(reflectionVector);
	float specular=dot(reflectionVector,tanEyeDir);
	specular = pow(specular,256);
	//float3 reflectionVector = normalize(2*diff*tanNormalVector-tanLightDir);
	//float specular=normalize(saturate(dot(reflectionVector,tanEyeDir)));
	//specular = pow(specular,256);
	


	Out.Color=   Out.Color   //�����⴦��	
	        +0.2*diffcolor*diff;//�����䴦��
//Out.Color+=specular;
	
	return Out;
};
technique WaterRender
{   pass P0
      {
       VertexShader = compile vs_3_0 VS();
       PixelShader  = compile ps_3_0 PS();
 	
      }
}