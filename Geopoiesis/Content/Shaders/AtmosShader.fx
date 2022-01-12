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

half _Exposure;
half3 _GroundColor;
half _SunSize;
half4 _SkyTint;
half _AtmosphereThickness;

// RGB wavelengths
half3 ScatteringWavelength = half3(.65, .57, .475);
half3 RangeForScatteringWavelength = half3(.15, .15, .15);

half OUTER_RADIUS = 1.025; // The outer (atmosphere) radius
half INNER_RADUIS = 1;
static const half kOuterRadius = OUTER_RADIUS;
static const half kOuterRadius2 = OUTER_RADIUS*OUTER_RADIUS;
static const half kInnerRadius = INNER_RADUIS;
static const half kInnerRadius2 = INNER_RADUIS*INNER_RADUIS;

half kCameraHeight = 0.0001;

half kMIE = 0.0010;      		// Mie constant
half lightBrightnes = 20.0; 	// Sun brightness

static const half kSunScale = 400.0 * lightBrightnes;
static const half kKmESun = kMIE * lightBrightnes;
static const half kKm4PI = kMIE * 4.0 * 3.14159265;
static const half kScale = 1.0 / (OUTER_RADIUS - 1.0);
static const half kScaleDepth = 0.25;
static const half kScaleOverScaleDepth = (1.0 / (OUTER_RADIUS - 1.0)) / 0.25;
static const half kSamples = 2.0; 

half3 lightDirection;
half3 lightColor;
half3 EyePosition;


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

    return float4(col, n);
}



technique ProceduralSkyBox
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}