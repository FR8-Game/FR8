float4 _FogColor;

// X: Fog Density
// Y: Height Falloff Lower
// Z: Height Falloff Upper
float4 _FogAttenuation;

float heightCurve(float v)
{
    return v * v;
}

float fogDensity(float height)
{
    float lower = _FogAttenuation.y;
    float upper = _FogAttenuation.z;
    float c = _FogAttenuation.x;
    
    if (height < lower) return c;
    if (height < upper) return c * heightCurve((height - upper) / (lower - upper));
    return 0.0;
}

float linearFogAttenuation(float3 start, float3 end)
{
    float3 vec = end - start;
    float l = length(vec);
    const float step = 1.0 / 100.0;
    
    float res = 0.0;
    for (float t = 0.0; t <= 1.0; t += step)
    {
        res += fogDensity(start.y + vec.y * t) * l * step;
    }
    return res;
    
}

float4 getFogColorWS(float3 position)
{
    float attenuation = linearFogAttenuation(_WorldSpaceCameraPos, position);
    float fog = pow(2.71, -(attenuation * attenuation));
    return float4(_FogColor.rgb, clamp(1.0f - fog, 0.0, 1.0));
}

float4 getFogColorSS(float2 screenUV)
{
    float depth = SampleSceneDepth(screenUV);
    float3 pos = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);
    return getFogColorWS(pos);
}