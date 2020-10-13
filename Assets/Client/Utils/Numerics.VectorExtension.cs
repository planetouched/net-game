using System.Numerics;

namespace Client.Utils
{
    public static class VectorExtension
    {
        public static UnityEngine.Vector2 ToUnity(this Vector2 vector2)
        {
            return new UnityEngine.Vector2(vector2.X, vector2.Y);
        }
        
        public static UnityEngine.Vector3 ToUnity(this Vector3 vector3)
        {
            return new UnityEngine.Vector3(vector3.X, vector3.Y, vector3.Z);
        }
    }
}