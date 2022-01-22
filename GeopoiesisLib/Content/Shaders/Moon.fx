#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_5_0
    #define PS_SHADERMODEL ps_5_0
#endif


#include "GenericHeader.fxh"

float4x4 world : World;
float4x4 wvp : WorldViewProjection;
float4x4 itw : WorldInverseTranspose;
float4x4 lightViewProjection : LightViewProjection;

float displacemntMag = .1f;
float3 lightDirection;

float _MinDeepSeaDepth = 0;
float _MinSeaDepth = .241f;
float _MinShoreDepth  = .328f;
float _MinLand  = .65;
float _MinHill  = .63;

texture heightTexture;
float2 res;

samplerCUBE heightMapSampler = sampler_state
{
    Texture = (heightTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

vOut VertexShaderFunction(vIn input)
{
    vOut output;

    float h = 0;
    
    #if !OPENGL
        h = texCUBElod(heightMapSampler, float4(input.normal, 1)).r;
    #endif  
    
    //h = (h * 2) -1;
    input.pos.xyz += input.normal * h * displacemntMag;

    output.pos = mul(input.pos, wvp);
    output.normal = input.normal;
    output.texCoords = input.texCoords;  
   
    float3 n = normalize(mul(input.pos.xyz, (float3x3) world));
    output.normal2 = 0;
    output.tangent[0] = normalize(mul(input.tangent, (float3x3) world));
    output.tangent[1] = normalize(mul(cross(input.tangent, n), (float3x3) world));
    output.tangent[2] = normalize(n);
    
    output.color = input.color;
    
    return output;
}

PixelShaderOutput PixelShaderFunction(vOut input) : Color
{
    PixelShaderOutput output = (PixelShaderOutput) 0;

    float r = texCUBE(heightMapSampler, input.normal).r;
    
    float3 n = 2.0 * (getNormalMap(heightMapSampler, input.normal, res, .1).rgb * .5 + .5) - 1.0;
    n = mul(n, input.tangent);
    
    float3 lightVector = normalize(-lightDirection);
    float NdL = saturate(dot(-n, lightVector)) + ambientPower;
    
    float4 col = r;
    
    output.Color = (col * NdL);
    
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