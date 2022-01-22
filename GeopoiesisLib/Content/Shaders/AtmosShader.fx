#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define MIE_G (-0.990)
#define MIE_G2 0.9801
#define kRAYLEIGH ((half)lerp(0, 0.0025, pow(abs(_AtmosphereThickness),2.5)))		// Rayleigh constant

#define SKY_GROUND_THRESHOLD 0.01

float4x4 world;
float4x4 wvp;

float atmos;


float3 lightDirection;
float3 lightColor;
float3 EyePosition;


struct vIn
{
	float4 pos : SV_POSITION;
	float3 normal : NORMAL0;
};

struct vOut
{
    float4 pos : SV_POSITION;
    float3 normal : NORMAL0;
};

vOut VertexShaderFunction(vIn input)
{
	vOut output;	


	output.pos = mul(input.pos, wvp);
    output.normal = normalize(float3(0,0,0) - input.pos.xyz);
	
	
	return output;
}

float4 PixelShaderFunction(vOut input) : COLOR
{
    half3 col = lightColor;

    float n = dot(input.normal, -lightDirection);
	
	col = lerp(float3(1,.5,.2), lightColor, n) * .5;
	
    return float4(col, n * atmos);
}



technique ProceduralSkyBox
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}