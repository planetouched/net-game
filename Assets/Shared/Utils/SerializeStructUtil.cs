using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Shared.Utils
{
    public static class SerializeStructUtil
    {
        #region serialize

        public static void SetVector2(Vector2 value, ref int offset, byte[] dest)
        {
            SerializeStruct(value, ref offset, dest);
        }

        public static void SetVector3(Vector3 value, ref int offset, byte[] dest)
        {
            SerializeStruct(value, ref offset, dest);
        }

        public static void BlockCopy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count, ref int offset)
        {
            Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
            offset += count;
        }

        private static void SerializeStruct<T>(T value, ref int offset, byte[] buffer) where T : struct
        {
            int rawSize = Marshal.SizeOf(value);
            IntPtr bytesPtr = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(value, bytesPtr, false);
            Marshal.Copy(bytesPtr, buffer, offset, rawSize);
            offset += rawSize;
            Marshal.FreeHGlobal(bytesPtr);
        }

        #endregion

        #region deserialize

        public static Vector2 GetVector2(ref int offset, byte[] buffer, bool shiftOffset = true)
        {
            return DeserializeStruct<Vector2>(buffer, ref offset, shiftOffset);
        }

        public static Vector3 GetVector3(ref int offset, byte[] buffer, bool shiftOffset = true)
        {
            return DeserializeStruct<Vector3>(buffer, ref offset, shiftOffset);
        }

        private static T DeserializeStruct<T>(byte[] rawData, ref int offset, bool shiftOffset) where T : struct
        {
            int rawSize = Marshal.SizeOf(typeof(T));
            if (rawSize > rawData.Length - offset)
                throw new ArgumentException("Not enough data to fill struct. Array length from position: " + (rawData.Length - offset) + ", Struct length: " + rawSize);
            var buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.Copy(rawData, offset, buffer, rawSize);
            var obj = (T) Marshal.PtrToStructure(buffer, typeof(T));
            Marshal.FreeHGlobal(buffer);
            if (shiftOffset)
                offset += rawSize;
            return obj;
        }

        #endregion
    }
}