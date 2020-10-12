using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Shared.Utils
{
    public static class SerializeUtil
    {
        #region serialize

        public static void SetByte(byte value, ref int offset, byte[] dest)
        {
            dest[offset] = value;
            offset++;
        }

        public static void SetBool(bool value, ref int offset, byte[] dest)
        {
            dest[offset] = BitConverter.GetBytes(value)[0];
            offset++;
        }

        public static void SetFloat(float value, ref int offset, byte[] dest)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, dest, offset, 4);
            offset += 4;
        }

        public static void SetInt(int value, ref int offset, byte[] dest)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, dest, offset, 4);
            offset += 4;
        }

        public static void SetUInt(uint value, ref int offset, byte[] dest)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, dest, offset, 4);
            offset += 4;
        }

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

        public static byte GetByte(ref int offset, byte[] buffer, bool shiftOffset = true)
        {
            var value = buffer[offset];
            if (shiftOffset)
                offset++;
            return value;
        }

        public static bool GetBool(ref int offset, byte[] buffer, bool shiftOffset = true)
        {
            var value = BitConverter.ToBoolean(buffer, offset);
            if (shiftOffset)
                offset++;
            return value;
        }

        public static float GetFloat(ref int offset, byte[] buffer, bool shiftOffset = true)
        {
            var value = BitConverter.ToSingle(buffer, offset);
            if (shiftOffset)
                offset += 4;
            return value;
        }

        public static int GetInt(ref int offset, byte[] buffer, bool shiftOffset = true)
        {
            var value = BitConverter.ToInt32(buffer, offset);
            if (shiftOffset)
                offset += 4;
            return value;
        }

        public static uint GetUInt(ref int offset, byte[] buffer, bool shiftOffset = true)
        {
            var value = BitConverter.ToUInt32(buffer, offset);
            if (shiftOffset)
                offset += 4;
            return value;
        }

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