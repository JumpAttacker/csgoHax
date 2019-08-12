using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable InconsistentNaming

namespace CsgoHaxOverlay
{
    public class MemUtils
    {
        #region CONSTANTS
        private const int SIZE_BYTE = sizeof(byte);
        private const int SIZE_INT16 = sizeof(short);
        private const int SIZE_INT32 = sizeof(int);
        private const int SIZE_INT64 = sizeof(long);
        private const int SIZE_UINT16 = sizeof(ushort);
        private const int SIZE_UINT32 = sizeof(uint);
        private const int SIZE_UINT64 = sizeof(ulong);
        private const int SIZE_FLOAT = sizeof(float);
        private const int SIZE_DOUBLE = sizeof(double);
        #endregion
        #region PROPERTIES
        public static IntPtr Handle { get; set; }
        #endregion
        #region METHODS
        #region PRIMITIVE WRAPPERS
        public static bool Read( IntPtr address, out byte[] data, int length)
        {
            data = new byte[length];
            var result = WinApi.ReadProcessMemory(Handle, address, data, length, out var numBytes);
            if (!result)
                return false;
            return numBytes.ToInt32() == length;
        }

        public static bool Write(IntPtr address, byte[] data)
        {
            var result = WinApi.WriteProcessMemory(Handle, address, data, data.Length, out var numBytes);
            if (!result)
                return false;
            return numBytes.ToInt32() == data.Length;
        }
        #endregion
        #region SPECIALIZED FUNCTIONS
        #region READ
        public static byte ReadByte(IntPtr address, byte defaultValue = 0)
        {
            return Read(address, out var data, SIZE_BYTE) ? data[0] : defaultValue;
        }
        public static char ReadChar(IntPtr address, char defaultValue = '\x0')
        {
            return (char)ReadByte(address, (byte)defaultValue);
        }
        public static short ReadInt16(IntPtr address, short defaultValue = 0)
        {
            return Read(address, out var data, SIZE_INT16) ? BitConverter.ToInt16(data, 0) : defaultValue;
        }
        public static ushort ReadUInt16(IntPtr address, ushort defaultValue = 0)
        {
            return Read(address, out var data, SIZE_UINT16) ? BitConverter.ToUInt16(data, 0) : defaultValue;
        }
        public static int ReadInt32(IntPtr address, int defaultValue = 0)
        {
            return Read(address, out var data, SIZE_INT32) ? BitConverter.ToInt32(data, 0) : defaultValue;
        }
        public static uint ReadUInt32(IntPtr address, uint defaultValue = 0)
        {
            return Read(address, out var data, SIZE_UINT32) ? BitConverter.ToUInt32(data, 0) : defaultValue;
        }
        public static long ReadInt64(IntPtr address, long defaultValue = 0)
        {
            return Read(address, out var data, SIZE_INT64) ? BitConverter.ToInt64(data, 0) : defaultValue;
        }
        public static ulong ReadUInt64(IntPtr address, ulong defaultValue = 0)
        {
            return Read(address, out var data, SIZE_UINT64) ? BitConverter.ToUInt64(data, 0) : defaultValue;
        }
        public static float ReadFloat(IntPtr address, float defaultValue = 0)
        {
            return Read(address, out var data, SIZE_FLOAT) ? BitConverter.ToSingle(data, 0) : defaultValue;
        }
        public static double ReadDouble(IntPtr address, double defaultValue = 0)
        {
            return Read(address, out var data, SIZE_DOUBLE) ? BitConverter.ToDouble(data, 0) : defaultValue;
        }
        public static String ReadString(IntPtr address, int length, Encoding encoding)
        {
            return Read(address, out var data, length) ? encoding.GetString(data) : null;
        }
        public static T ReadStruct<T>(IntPtr address, int structSize = 0) where T : struct
        {
            if (structSize == 0)
                structSize = Marshal.SizeOf(typeof(T));
            Read(address, out var data, structSize);
            var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var structure = (T)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            gcHandle.Free();
            return structure;
        }
        public static Vector2 ReadVector2(IntPtr address)
        {
            return ReadStruct<Vector2>(address);
        }
        public static Vector3 ReadVector3(IntPtr address)
        {
            return ReadStruct<Vector3>(address);
        }
        public static Matrix ReadMatrix(IntPtr address, int rows, int columns)
        {
            var matrix = new Matrix(rows, columns);
            if (Read(address, out var data, SIZE_FLOAT * rows * columns))
                matrix.Read(data);
            return matrix;
        }
        #endregion
        #region WRITE
        public bool WriteByte(IntPtr address, byte value)
        {
            return Write(address, new[] { value });
        }
        public bool WriteChar(IntPtr address, char value)
        {
            return Write(address, new[] { (byte)value });
        }
        public bool WriteInt16(IntPtr address, short value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public bool WriteUInt16(IntPtr address, ushort value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public static bool WriteInt32(IntPtr address, int value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public bool WriteUInt32(IntPtr address, uint value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public bool WriteInt64(IntPtr address, long value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public bool WriteUInt64(IntPtr address, ulong value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public static bool WriteFloat(IntPtr address, float value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public bool WriteDouble(IntPtr address, double value)
        {
            return Write(address, BitConverter.GetBytes(value));
        }
        public bool WriteString(IntPtr address, string text, Encoding encoding)
        {
            return Write(address, encoding.GetBytes(text));
        }
        public bool WriteVector2(IntPtr address, Vector2 vec)
        {
            var data = new byte[SIZE_FLOAT * 2];
            Array.Copy(BitConverter.GetBytes(vec.X), 0, data, 0, SIZE_FLOAT);
            Array.Copy(BitConverter.GetBytes(vec.Y), 0, data, SIZE_FLOAT, SIZE_FLOAT);
            return Write(address, data);
        }
        public static bool WriteVector3(IntPtr address, Vector3 vec)
        {
            var data = new byte[SIZE_FLOAT * 3];
            Array.Copy(BitConverter.GetBytes(vec.X), 0, data, 0, SIZE_FLOAT);
            Array.Copy(BitConverter.GetBytes(vec.Y), 0, data, SIZE_FLOAT, SIZE_FLOAT);
            Array.Copy(BitConverter.GetBytes(vec.Z), 0, data, SIZE_FLOAT * 2, SIZE_FLOAT);
            return Write(address, data);
        }
        #endregion
        #endregion
        #endregion
    }
}