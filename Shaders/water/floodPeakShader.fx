float4x4 worldViewProjection;  //���ϱ任����
float4x4 world;                //�������
float4   AmbientLight;         //������
float4   DirectionalLight;     //�����
float4   vecLightDir;          //�����Դ����
float4   materialAmbient;	//���ʷ��价����ϵ��
float4   materialDiffuse;	//����������ϵ��

float4   vecEye;              	//�۲��λ��
float4   materialSpecular;    	//���ʾ��淴��ϵ��

//��ɫ����
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
      Texture=<ColorTexture>;//���������
      MinFilter=LINEAR;//��Сͼ��ʹ�������˲�
      MagFilter=LINEAR;//�Ŵ�ͼ��ʹ�������˲�
      MipFilter=LINEAR;//Mipmapʹ�������˲�
      AddressU=Wrap;//U��V�����ϵ�����Ѱַģʽ������Wrap��ʽ
      AddressV=Wrap;
};

VS_OUTPUT VS(float4 Pos: POSITION,float2 colorTexUV : TEXCOORD0 )         //������ɫ����
{
   VS_OUTPUT Out = (VS_OUTPUT) 0;               
   Out.Pos = mul(Pos, worldViewProjection);                      //���㶥��λ��

   //float4 lightDir  = normalize(vecLightDir);                    //�õ���λ��������
   // float4 normalWorld=normalize(mul(Normal,world));              //�õ���λ��������
   //float4 diff = saturate( dot(normalWorld, lightDir) );	 //A1=N���L=cos��
   //Out.Color =  AmbientLight *materialAmbient+DirectionalLight*diff *materialDiffuse;
   
   Out.colorTexCoord=colorTexUV;

   return Out;
}

float4 PS( VS_OUTPUT vsout ) : COLOR					//������ɫ����
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
