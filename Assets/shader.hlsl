float4x4 World;
float4x4 View;
float4x4 Projection;

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;

float3 LightPos;
float LightPower;
float4 DeffuseColor;

float4x4 LightWorldViewProjection;

Texture ShadowMap;
sampler ShadowMapSampler = sampler_state { texture = <ShadowMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Normal: NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal: TEXCOORD1;
	float3 Position3D: TEXCOORD2;
};

struct ShadowMapVertexOutput
{
	float4 Position     : POSITION0;
    float4 Position2D    : TEXCOORD0;
};

struct ShadowSceenVertexOutput
{
	float4 Position     : POSITION0;
    float4 Pas2DAsSeenByLight    : TEXCOORD0;
	float3 Normal: TEXCOORD1;
	float4 Position3D: TEXCOORD2;
};

float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
    float3 lightDir = normalize(pos3D - lightPos);
    return dot(-lightDir, normal);    
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Normal = normalize(mul(input.Normal, (float3x3)World));
	output.Position3D = worldPosition;
	
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float diffuseLightingFactor = DotProduct(LightPos, input.Position3D, input.Normal);
	diffuseLightingFactor = saturate(diffuseLightingFactor);
	diffuseLightingFactor *= LightPower;
	return DeffuseColor * (diffuseLightingFactor + AmbientIntensity);
}

ShadowMapVertexOutput ShadowMapVertexShaderFunction(VertexShaderInput input)
{
	ShadowMapVertexOutput output;
	output.Position = mul(input.Position, LightWorldViewProjection);
	output.Position2D = output.Position;
	
	return output;
}

float4 ShadowMapPixelShaderFunction(ShadowMapVertexOutput input) : COLOR0
{
	return input.Position2D.z/input.Position2D.w;
}

ShadowSceenVertexOutput ShadowSceenVertexShaderFunction(VertexShaderInput input)
{
	ShadowSceenVertexOutput output;
	float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	
	output.Pas2DAsSeenByLight = mul(input.Position, LightWorldViewProjection);
	
	output.Position3D = worldPosition;
	output.Normal = normalize(mul(input.Normal, (float3x3)World));
	
	return output;
}

float4 ShadowSceenPixelShaderFunction(ShadowSceenVertexOutput input) : COLOR0
{
	float2 ProjectiodTexCoords;
	ProjectiodTexCoords[0] = input.Pas2DAsSeenByLight.x/input.Pas2DAsSeenByLight.w/2.0f + 0.5f;
	ProjectiodTexCoords[1] = -input.Pas2DAsSeenByLight.y/input.Pas2DAsSeenByLight.w/2.0f + 0.5f;
	
	float diffuseLightingFactor = 0;
	if((saturate(ProjectiodTexCoords).x == ProjectiodTexCoords.x) && (saturate(ProjectiodTexCoords).y == ProjectiodTexCoords.y))
	{
		float depthStoredInShadowMap = tex2D(ShadowMapSampler, ProjectiodTexCoords).r;
		float realDistance = input.Pas2DAsSeenByLight.z/input.Pas2DAsSeenByLight.w;
		if((realDistance - 1.0f/100.0f) <= depthStoredInShadowMap)
		{
			diffuseLightingFactor = DotProduct(LightPos, input.Position3D, input.Normal);
			diffuseLightingFactor = saturate(diffuseLightingFactor);
			diffuseLightingFactor *= LightPower;
		}
	}
	
	return DeffuseColor * (diffuseLightingFactor + AmbientIntensity);
}


technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

technique ShadowMap
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 ShadowMapVertexShaderFunction();
        PixelShader = compile ps_4_0 ShadowMapPixelShaderFunction();
	}
}

technique ShadowScene
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 ShadowSceenVertexShaderFunction();
        PixelShader = compile ps_4_0 ShadowSceenPixelShaderFunction();
	}
}