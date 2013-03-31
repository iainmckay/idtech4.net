float4 g_VertexColorModulate;
float4 g_VertexColorAdd;
float4 g_Color;
float4 g_ModelViewProjectionMatrixX;
float4 g_ModelViewProjectionMatrixY;
float4 g_ModelViewProjectionMatrixZ;
float4 g_ModelViewProjectionMatrixW;
float4 g_TextureMatrixS;
float4 g_TextureMatrixT;
float4 g_TextureCoordinates0S;
float4 g_TextureCoordinates0T;
float4 g_TextureCoordinates0Enabled;
float4 g_AlphaTest;

texture g_Texture0;
sampler2D g_TextureSampler0 = sampler_state {
    Texture = g_Texture0;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float4 Normal   : NORMAL0;
	float4 Tangent  : TANGENT0;
	float4 Color    : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position   : POSITION0;
	float2 TexCoord   : TEXCOORD0;
	float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.Position.x = dot(input.Position, g_ModelViewProjectionMatrixX);
	output.Position.y = dot(input.Position, g_ModelViewProjectionMatrixY);
	output.Position.z = dot(input.Position, g_ModelViewProjectionMatrixZ);
	output.Position.w = dot(input.Position, g_ModelViewProjectionMatrixW);
	
	if(g_TextureCoordinates0Enabled.x > 0.0) {
		output.TexCoord.x = dot(input.Position , g_TextureCoordinates0S);
		output.TexCoord.y = dot(input.Position , g_TextureCoordinates0T);
	} else {
		output.TexCoord.x = dot(input.TexCoord.xy , g_TextureMatrixS);
		output.TexCoord.y = dot(input.TexCoord.xy , g_TextureMatrixT);
	}

	float4 vertexColor = (input.Color * g_VertexColorModulate) + g_VertexColorAdd;
	output.Color = vertexColor * g_Color;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(g_TextureSampler0 , input.TexCoord) * input.Color;
	saturate(color.a - g_AlphaTest.x);
	
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