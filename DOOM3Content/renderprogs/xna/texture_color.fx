float4 g_VertexColorModulate;
float4 g_VertexColorAdd;
float4 g_Color;
float4x4 g_ModelViewProjectionMatrix;
float4 g_TextureMatrixS;
float4 g_TextureMatrixT;
float4 g_TextureCoordinates0S;
float4 g_TextureCoordinates0T;
float4 g_TextureCoordinates0Enabled;
float4 g_AlphaTest;

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
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float4 Color    : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.Position = mul(input.Position , g_ModelViewProjectionMatrix);
	
	if(g_TextureCoordinates0Enabled.x > 0.0) {
		output.TexCoord.x = mul(input.Position.x , g_TextureCoordinates0S);
		output.TexCoord.y = mul(input.Position.y, g_TextureCoordinates0T);
	} else {
		output.TexCoord.x = mul(input.TexCoord.xy , g_TextureMatrixS);
		output.TexCoord.y = mul(input.TexCoord.xy , g_TextureMatrixT);
	}

	float4 vertexColor = (/*input.Color*/ float4(1,1,1,1) * g_VertexColorModulate) + g_VertexColorAdd;
	output.Color = vertexColor * g_Color;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(g_TextureSampler0 , input.TexCoord) * input.Color;
	clip(color.a - g_AlphaTest.x);

	return color;
}

technique main
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}