using System;
using UnityEngine;

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
            if (value.x > 180)
                value.x -= 360;

            if (value.x < -180)
                value.x += 360;

            if (value.y > 180)
                value.y -= 360;

            if (value.y < -180)
                value.y += 360;

            if (value.z > 180)
                value.z -= 360;

            if (value.z < -180)
                value.z += 360;

            return value;
        }
    }
}