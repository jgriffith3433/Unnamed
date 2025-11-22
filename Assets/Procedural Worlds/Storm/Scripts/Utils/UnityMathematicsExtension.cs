using Unity.Mathematics;

public static class QuaternionExtensions
{
    public static float ToEulerX(this quaternion q)
    {
        return math.atan2(2 * (q.value.w * q.value.x + q.value.y * q.value.z), 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y));
    }

    public static float ToEulerY(this quaternion q)
    {
        float s = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
        if (math.abs(s) >= 1)
            return math.PI / 2 * math.sign(s);
        else
            return math.asin(s);
    }

    public static float ToEulerZ(this quaternion q)
    {
        return math.atan2(2 * (q.value.w * q.value.z + q.value.x * q.value.y), 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z));
    }

    public static float3 ToEuler(this quaternion q)
    {
        return new float3(ToEulerX(q), ToEulerY(q), ToEulerZ(q));
    }
}
