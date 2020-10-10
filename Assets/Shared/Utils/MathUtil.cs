using System;
using System.Numerics;

namespace Shared.Utils
{
    public static class MathUtil
    {
        public static float ToRadians(float value)
        {
            return value * (float)Math.PI / 180;
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
    }
}