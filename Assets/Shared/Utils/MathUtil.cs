using System;
using System.Numerics;
using Client.Utils;

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
            var quat = UnityEngine.Quaternion.Euler(euler.ToUnity());
            var tmp = (quat * UnityEngine.Vector3.forward).normalized;
            var dir = new Vector3(tmp.x, tmp.y, tmp.z);
            
            Vector3 pointIntersect = point + dir * Vector3.Dot(center - point, dir);
            float intersectDistance = Vector3.Distance(pointIntersect, center);
            return intersectDistance < radius;
        }

        public static bool RaySphereIntersection(Vector3 rayPos, Vector3 rayDir, Vector3 spherePos, float radius)
        {
            var l = spherePos - rayPos;
            var tc = Vector3.Dot(l, rayDir);

            if (tc < 0) return false;

            var d2 = tc * tc - Vector3.Dot(l, l);

            float radius2 = radius * radius;

            if (d2 > radius2) return false;

            return true;
        }
    }
}