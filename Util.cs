using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Reflection;

namespace x86CS
{
    public static class Util
    {
        public static int CountSet(this BitArray bits)
        {
            int count = 0;

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    count++;
            }

            return count;
        }

        public static byte GetLow(this byte b)
        {
            return (byte)(b & 0x0f);
        }

        public static byte SetLow(this byte b, byte value)
        {
            return (byte)((b & 0xf0) + (value & 0x0f));
        }

        public static byte SetHigh(this byte b, byte value)
        {
            return (byte)((value.GetLow() << 4) + b.GetLow());
        }

        public static byte GetHigh(this byte b)
        {
            return (byte)((b >> 4) & 0x0f);
        }

        public static ushort GetLow(this ushort b)
        {
            return (ushort)(b & 0x00ff);
        }

        public static ushort SetLow(this ushort b, ushort value)
        {
            return (byte)((b & 0xff00) + (value & 0x00ff));
        }

        public static ushort SetHigh(this ushort b, ushort value)
        {
            return (ushort)((value.GetLow() << 8) + b.GetLow());
        }

        public static ushort GetHigh(this ushort b)
        {
            return (ushort)((b >> 8) & 0x00ff);
        }

        public static byte ToBCD(int value)
        {
            int tens = value / 10;
            int ones = value % 10;

            var ret = (byte)(((byte)tens << 4) + (byte)ones);

            return ret;
        }

        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return structure;
        }

        public static T ByteArrayToStructureBigEndian<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            System.Type t = structure.GetType();
            FieldInfo[] fieldInfo = t.GetFields();
            foreach (FieldInfo fi in fieldInfo)
            {
                if (fi.FieldType == typeof(System.Int16))
                {
                    // TODO
                }
                else if (fi.FieldType == typeof(System.Int32))
                {
                    // TODO
                }
                else if (fi.FieldType == typeof(System.Int64))
                {
                    // TODO
                }
                else if (fi.FieldType == typeof(System.UInt16))
                {
                    UInt16 num = (UInt16)fi.GetValue(structure);
                    byte[] tmp = BitConverter.GetBytes(num);
                    Array.Reverse(tmp);
                    fi.SetValueDirect(__makeref(structure), BitConverter.ToUInt16(tmp, 0));
                }
                else if (fi.FieldType == typeof(System.UInt32))
                {
                    UInt32 num = (UInt32)fi.GetValue(structure);
                    byte[] tmp = BitConverter.GetBytes(num);
                    Array.Reverse(tmp);
                    fi.SetValueDirect(__makeref(structure), BitConverter.ToUInt32(tmp, 0));
                }
                else if (fi.FieldType == typeof(System.UInt64))
                {
                    UInt64 num = (UInt64)fi.GetValue(structure);
                    byte[] tmp = BitConverter.GetBytes(num);
                    Array.Reverse(tmp);
                    fi.SetValueDirect(__makeref(structure), BitConverter.ToUInt64(tmp, 0));
                }
            }
            return structure;
        }
    }
}
