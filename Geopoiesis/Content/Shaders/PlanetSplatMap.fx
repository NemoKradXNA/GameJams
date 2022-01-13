﻿#if OPENGL
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
float3 lightDirection;

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

texture splatTexture;

samplerCUBE splatMapSampler = sampler_state
{
    Texture = (splatTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture normalTexture;

samplerCUBE normalMapSampler = sampler_state
{
    Texture = (normalTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture sandTexture;

sampler2D sandSampler = sampler_state
{
    Texture = (sandTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture grassTexture;

sampler2D grassSampler = sampler_state
{
    Texture = (grassTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture rockTexture;

sampler2D rockSampler = sampler_state
{
    Texture = (rockTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture snowTexture;

sampler2D snowSampler = sampler_state
{
    Texture = (snowTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
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
    
    //h = (h * 2) -1;
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
    

    float3 n = texCUBE(normalMapSampler, input.normal);
    
    float3 lightVector = normalize(-lightDirection);    
    float NdL = saturate(dot(input.normal, -lightVector)) + .125f;

    
    float4 splat = texCUBE(splatMapSampler, input.normal);
    
    float4 sand = tex2D(sandSampler, input.texCoords * 3);
    float4 grass = tex2D(grassSampler, input.texCoords * 3);
    float4 rock = tex2D(rockSampler, input.texCoords * 3);
    float4 snow = tex2D(snowSampler, input.texCoords * 3);
    
    float4x4 col = 0;
    col[0] = sand;
    col[1] = grass;
    col[2] = rock;
    col[3] = snow;
    
    output.Color = mul(splat, col) * NdL;
    
    output.Color = float4(n, 1);
    
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