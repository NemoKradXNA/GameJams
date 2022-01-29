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


float3 EyePosition : CameraPosition;

// I used https://sourceforge.net/projects/spacescape/ to gen my space cube map.
texture textureMap;

samplerCUBE textureMapSampler = sampler_state
{
    Texture = (textureMap);
    MinFilter = Point;
    MagFilter = Point;
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
    float3 normal : NORMAL0;
    float3 viewDirection : NORMAL1;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0; 
};

vOut VertexShaderFunction(vIn input)
{
    vOut output;
    
    output.pos = mul(input.pos, wvp);
    output.normal = input.normal;
    output.viewDirection = EyePosition - mul(input.pos.xyz, (float3x3) world);
    
    return output;
}

PixelShaderOutput PixelShaderFunction(vOut input) : Color
{
    PixelShaderOutput output = (PixelShaderOutput) 0;

    float4 col = texCUBE(textureMapSampler, input.viewDirection);
    
    output.Color = col;
    
    return output;
}

technique VertColor
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}