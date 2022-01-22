#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_5_0
    #define PS_SHADERMODEL ps_5_0
#endif

float4x4 world : World;
float4x4 wvp : WorldViewProjection;
float4x4 itw : WorldInverseTranspose;
float4x4 lightViewProjection : LightViewProjection;

float displacemntMag = 1;

float _MinSeaDepth;

texture heightTexture;

samplerCUBE heightMapSampler = sampler_state
{
    Texture = (heightTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct vIn
{
    float4 pos : SV_POSITION;
    float2 texCoords : TEXCOORD0;
    float3 normal : NORMAL0;
    float3 tangent : TANGENT0;
    float4 color : COLOR0;
};

struct vOut
{
    float4 pos : SV_POSITION;
    float2 texCoords : TEXCOORD0;
    float3 normal : NORMAL0;
    float3 tangent : TANGENT0;
    float4 color : COLOR0;
};
struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

vOut VertexShaderFunction(vIn input)
{
    vOut output;

    float h = 0;
    
    #if !OPENGL
        h = texCUBElod(heightMapSampler, float4(input.normal, 1)).r;
    #endif
    
    input.pos.xyz += input.normal * h * displacemntMag;

    output.pos = mul(input.pos, wvp);
    output.normal = input.normal;
    output.texCoords = input.texCoords;
    output.tangent = input.tangent;
    output.color = input.color;
    
    return output;
}

PixelShaderOutput PixelShaderFunction(vOut input) : Color
{
    PixelShaderOutput output = (PixelShaderOutput) 0;

    float r = texCUBE(heightMapSampler, input.normal).r;
    
    
    float4 color = input.color * r;
    
    output.Color = float4(color.rgb * float3(input.texCoords, 1), 1);
    output.Color = float4(color.rgb, 1);
    

    return output;
}

technique VertColor
{
    pass Pass1
    {
		//FILLMODE = WIREFRAME;
		//CULLMODE = NONE;
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}