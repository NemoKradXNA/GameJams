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

float _MinDeepSeaDepth = 0;
float _MinSeaDepth = .241f;
float _MinShoreDepth  = .328f;
float _MinLand  = .65;
float _MinHill  = .63;

float4 _DeepSea = float4(0, 0, 1, 1);
float4 _SnowCap = float4(1, 1, 1, 1);
float4 _Sea = float4(.38f,.41f,1,1);
float4 _Shore = float4(.04f,.52f,.65f,1);
float4 _Land = float4(0, .5, .16, 1);
float4 _Hills = float4(.42f, .15f, .05f, 1);

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

    float h = texCUBElod(heightMapSampler, float4(input.normal,1)).r;
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
    
    float4 col = 0;

    if (r <= _MinSeaDepth)
        col = lerp(_DeepSea, _Sea, (r - _MinDeepSeaDepth) / _MinSeaDepth);
    else if (r <= _MinShoreDepth)
        col = lerp(_Sea, _Shore, (r - _MinSeaDepth) / _MinShoreDepth);
    else if (r < _MinLand)
        col = _Land;
    else if (r < _MinHill)
        col = _Hills;
    else
        col = _SnowCap;
    
    output.Color = col;
    
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