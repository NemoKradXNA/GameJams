float ambientPower = .05;

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
    float3x3 tangent : TANGENT0;
    float4 color : COLOR0;
    float3 CamView : NORMAL1;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};


float getHeight(samplerCUBE heightMapSampler, float3 normal)
{
    float h = 0;
    
    #if !OPENGL
        h = texCUBElod(heightMapSampler, float4(normal, 1)).r;
    #endif

    
    return h;
}

float getHeightPS(samplerCUBE heightMapSampler, float3 normal)
{
    return texCUBE(heightMapSampler, normal);
}

float4 getNormalMap(samplerCUBE heightMapSampler, float3 normal, float2 res, float scale)
{

    float2 step = float2(1.0 / res.x, 1.0 / res.y);
    
    float h = getHeightPS(heightMapSampler,normal);

    float2 n = h - float2(
        getHeight(heightMapSampler, normal + float3(step.x, 0, 0)),
        getHeight(heightMapSampler, normal + float3(0, 0, step.y)));
    
    return float4(float3(n * scale / step, 1.0), h);

}

float3 getNormal(samplerCUBE heightMapSampler,float3 normal, float2 res, float scale)
{
    float2 step = 1.0 / res;
    float h = getHeight(heightMapSampler,normal);

    float2 n = h - float2(
        getHeight(heightMapSampler,normal + float3(step.x, 0, 0)),
        getHeight(heightMapSampler,normal + float3(0, 0, step.y))) * scale / step;
    
    return cross(normal + float3(step.x, n.x, 0), normal + float3(0, n.y, step.y));

}

float3x3 getTangentMat(float3 normal, float2 res, float3x3 world) 
{
    float3x3 t = 0;
    float2 step = float2(1.0 / res.x, 1.0 / res.y);

    float3 n1 = cross(normal, normal + float3(step.x, 0, 0));
    float3 n2 = cross(normal, normal + float3(0, step.y, 0));

    float3 tangent = 0;
    float3 biNormal = 0;

    tangent = lerp(n1, n2, (length(n1) - length(n2)));

    biNormal = cross(tangent, normal);

    t[0] = normalize(mul(tangent, world));
    t[1] = normalize(mul(biNormal, world));
    t[2] = normalize(mul(normal, world));
    
    return t;
}