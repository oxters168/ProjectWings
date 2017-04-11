using UnityEngine;

public static class VectorHelpers
{
    /// <summary>
    /// Determine the signed angle between two vectors, with normal 'n'
    /// as the rotation axis.
    /// Maker:https://forum.unity3d.com/threads/need-vector3-angle-to-return-a-negtive-or-relative-value.51092/
    /// </summary>
    public static float AngleSigned(Vector3 vector1, Vector3 vector2, Vector3 normal)
    {
        return Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(vector1, vector2)), Vector3.Dot(vector1, vector2)) * Mathf.Rad2Deg;
    }
    public static Vector2 Rotate(this Vector2 original, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(cos * original.x - sin * original.y, sin * original.x + cos * original.y);
    }
    public static Vector3 Rotate(this Vector3 original, Vector3 axis, float degrees)
    {
        return Quaternion.AngleAxis(degrees, axis) * original;
    }
}
