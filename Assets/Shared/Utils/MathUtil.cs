using System;
using System.Numerics;

namespace Shared.Utils
{
    public static class MathUtil
    {
        public static float ToRadians(float value)
        {
            return value * (float) Math.PI / 180;
        }

        public static Vector3 ClampTo180(Vector3 value)
        {
            if (value.X > 180)
                value.X -= 360;

            if (value.X < -180)
                value.X += 360;

            if (value.Y > 180)
                value.Y -= 360;

            if (value.Y < -180)
                value.Y += 360;

            if (value.Z > 180)
                value.Z -= 360;

            if (value.Z < -180)
                value.Z += 360;

            return value;
        }

        public static bool IntersectRaySphere(Vector3 point, Vector3 euler, Vector3 center, float radius)
        {
            var quaternion = Quaternion.CreateFromYawPitchRoll(ToRadians(euler.Y), ToRadians(euler.X), ToRadians(euler.Z));
            var direction = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, quaternion));
            
            var pointIntersect = point + direction * Vector3.Dot(center - point, direction);
            float intersectDistance = Vector3.Distance(pointIntersect, center);
            return intersectDistance < radius;
        }
    }
}