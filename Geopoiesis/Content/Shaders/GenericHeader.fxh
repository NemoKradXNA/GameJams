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
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};


float getHeight(samplerCUBE heightMapSampler, float3 normal)
{
    return texCUBElod(heightMapSampler, float4(normal, 1)).r;
}

float4 getNormalMap(samplerCUBE heightMapSampler, float3 normal, float2 res, float scale)
{
    float2 step = 1.0 / res;
    
    float h = getHeight(heightMapSampler,normal);

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