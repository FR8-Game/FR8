float3 rotate(float3 v, float a)
{
    float c = cos(a);
    float s = sin(a);
    return float3
    (
        v.x * c + v.y * s,
        v.y * c - v.x * s,
        v.z
    );
}

void GetRopePositionOS_float(float3 inPositionOS, float3 inNormalOS, out float3 outPositionOS, out float3 outNormalOS)
{
    float p = inPositionOS.z + 0.5;

    float3 ropePos = inPositionOS;
    float angle = p * _Twist;
    ropePos = rotate(ropePos, angle);
    ropePos.z = 0.0f;

    float3 normal = rotate(inNormalOS, angle);
    
    float3 a = mul(_RopeStart, float4(ropePos, 1.0)).xyz;
    float3 b = mul(_RopeMid, float4(ropePos, 1.0)).xyz;
    float3 c = mul(_RopeEnd, float4(ropePos, 1.0)).xyz;

    float3 ab = lerp(a, b, p);
    float3 bc = lerp(b, c, p);

    outPositionOS = lerp(inPositionOS, lerp(ab, bc, p), _IsRope);
    outNormalOS = normalize(lerp(inNormalOS, normal, _IsRope));
}

void GetRopePositionOS_half(half3 inPositionOS, half3 inNormalOS, out half3 outPositionOS, out half3 outNormalOS)
{
    GetRopePositionOS_float(inPositionOS, inNormalOS, outPositionOS, outNormalOS);
}