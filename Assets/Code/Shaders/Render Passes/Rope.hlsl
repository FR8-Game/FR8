float4x4 _RopeData[3];

void GetRopePositionOS_float(float3 inPositionOS, out float3 outPositionOS)
{
    //float p = inPositionOS.z + 0.5;
    float p = 0.5;
    
    float4x4 a = _RopeData[0];
    float4x4 b = _RopeData[1];
    float4x4 c = _RopeData[2];

    float4x4 ab = lerp(a, b, p);
    float4x4 bc = lerp(b, c, p);
    float4x4 abc = lerp(ab, bc, p);

    outPositionOS = mul(abc, float4(inPositionOS.xyz, 1.0));
}

void GetRopePositionOS_half(half3 inPositionOS, out half3 outPositionOS)
{
    GetRopePositionOS_float(inPositionOS, outPositionOS);
}