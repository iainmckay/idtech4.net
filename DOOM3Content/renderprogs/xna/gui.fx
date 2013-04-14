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
	float2 TexCoord1 : TEXCOORD0;
	float4 TexCoord2 : TEXCOORD1;
	float4 Color     : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	//input.Color = float4(0, 0, 1, 1);

    VertexShaderOutput output;
	output.Position  = mul(input.Position , g_ModelViewProjectionMatrix);
	output.TexCoord1  = input.TexCoord;
	output.TexCoord2 = (input.Color * 2) - 1;
	output.Color     = input.Color;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = (tex2D(g_TextureSampler0 , input.TexCoord1) * input.Color) + input.TexCoord2;
	color.xyz = color.xyz * color.w;

	//color = tex2D(g_TextureSampler0 , input.TexCoord1);

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