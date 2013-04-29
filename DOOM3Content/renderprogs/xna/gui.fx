float4x4 g_ModelViewProjectionMatrix;

texture g_Texture0;
sampler2D g_TextureSampler0 = sampler_state {
    Texture = g_Texture0;
	mipfilter = LINEAR; 
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float4 Normal   : NORMAL0;
	float4 Tangent  : TANGENT0;
	float4 Color    : COLOR0;
	float4 Color2   : COLOR1;
};

struct VertexShaderOutput
{
    float4 Position  : POSITION0;
	float2 TexCoord0 : TEXCOORD0;
	float4 TexCoord1 : TEXCOORD1;
	float4 Color     : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.Position  = mul(input.Position , g_ModelViewProjectionMatrix);
	output.TexCoord0 = input.TexCoord;
	output.TexCoord1 = (input.Color2 * 2) - 1;
	output.Color     = input.Color;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = (tex2D(g_TextureSampler0 , input.TexCoord0)) /** input.Color) + input.TexCoord1*/;
	color.xyz    = color.xyz * color.w;
	color.w      = color.w;
	
	return color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}