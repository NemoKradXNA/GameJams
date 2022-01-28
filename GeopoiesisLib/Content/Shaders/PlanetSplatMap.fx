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

float displacemntMag = 1;
float3 lightDirection;

float vocalnism = 0;

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
float4 _Ice = float4(.8, .8, 1, 1);
float temp = 0;

float normalMag = .05;
float2 res;

float4 specularColor : Specular = float4(1, 1, 1, 1);
float specularIntensity : Scalar = .5;
float3 EyePosition : CameraPosition;

texture heightTexture;

samplerCUBE heightMapSampler = sampler_state
{
    Texture = (heightTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
    AddressW = Wrap;
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



vOut VertexShaderFunction(vIn input)
{
    vOut output = (vOut)0;

    float h = getHeight(heightMapSampler,input.normal).r;
    
    float3 rn = normalize(input.pos.xyz);
    input.pos.xyz += normalize(input.pos.xyz) * h * displacemntMag;

    output.pos = mul(input.pos, wvp);
    output.normal = input.normal;
    
    float3 n = normalize(mul(input.pos.xyz, (float3x3) world));
        
    output.texCoords = input.texCoords;  
    
    output.color = input.color;

    output.CamView = EyePosition - mul(input.pos.xyz, (float3x3)world);
    
    return output;
}


PixelShaderOutput PixelShaderFunction(vOut input) : Color
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    

    float3 n = 2.0 * (getNormalMap(heightMapSampler, input.normal, res, normalMag).rgb * .5 + .5) - 1.0;
    float3x3 tangentMat = input.tangent;

    tangentMat = getTangentMat(input.normal, res, world);

    n = mul(n, tangentMat);
    
    float3 lightVector = normalize(-lightDirection);    
    float NdL = saturate(dot(-n, lightVector)) + ambientPower;

    
    float4 splat = 0;
    

    float r = texCUBE(heightMapSampler, input.normal).r;
    
    float ice = temp / 100.0;

    float4 sand = tex2D(sandSampler, input.texCoords );
    float4 grass = tex2D(grassSampler, input.texCoords);
    float4 rock = tex2D(rockSampler, input.texCoords);
    float4 snow = tex2D(snowSampler, input.texCoords );

    if (r <= _MinSeaDepth)
        splat.r = .5f;
    else if (r <= _MinShoreDepth)
        splat.r = 1;
    else if (r < _MinLand)
        splat.g = 1;
    else if (r < _MinHill)
        splat.b = 1;
    else
        splat.a = 1;
    
    float3 Half = normalize(-lightVector + normalize(input.CamView));
    float3 norm = normalize(mul(input.normal, (float3x3)world));
    float specular = pow(saturate(dot(norm, Half)), 25);
    
    float4x4 col = 0;
    col[0] = sand;
    col[1] = grass;
    col[2] = rock;
    col[3] = snow;

    float4x4 spec = 0;
    spec[0] = (specularColor * specularIntensity) * specular;
    spec[1] = 0;
    spec[2] = 0;
    spec[3] = 0;
    
    float4 volc = float4(1, .3f, 0, 1) * pow(r * vocalnism, 20);
    
    output.Color = (lerp(mul(splat, col), _Ice * r, (1 - ice) * r) * NdL) + mul(splat,spec);
    
    output.Color += volc;
    
    //output.Color = float4(input.noff, 1);
    
    
    
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