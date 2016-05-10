using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    public static class Packet
    {
        public static ushort ReadUint8(byte[] array, ref int offset)
        {
            offset++;
            return array[offset - 1];
        }

        public static ushort ReadUint16(byte[] array, ref int offset)
        {
            byte[] a = new byte[2];
            for (int t = 0; t < 2; t++)
            {
                a[t] = array[offset];
                offset++;
            }
            return BitConverter.ToUInt16(a, 0);
        }

        public static uint ReadUint32(byte[] array, ref int offset)
        {
            byte[] a = new byte[4];
            for (int t = 0; t < 4; t++)
            {
                a[t] = array[offset];
                offset++;
            }
            return BitConverter.ToUInt32(a, 0);
        }

        public static ulong ReadUint64(byte[] array, ref int offset)
        {
            byte[] a = new byte[8];
            for (int t = 0; t < 8; t++)
            {
                a[t] = array[offset];
                offset++;
            }
            return BitConverter.ToUInt64(a, 0);
        }
        public static short Readint16(byte[] array, ref int offset)
        {
            byte[] a = new byte[2];
            for (int t = 0; t < 2; t++)
            {
                a[t] = array[offset];
                offset++;
            }
            return BitConverter.ToInt16(a, 0);
        }

        public static int Readint32(byte[] array, ref int offset)
        {
            byte[] a = new byte[4];
            for (int t = 0; t < 4; t++)
            {
                a[t] = array[offset];
                offset++;
            }
            return BitConverter.ToInt32(a, 0);
        }

        public static long Readint64(byte[] array, ref int offset)
        {
            byte[] a = new byte[8];
            for (int t = 0; t < 8; t++)
            {
                a[t] = array[offset];
                offset++;
            }
            return BitConverter.ToInt64(a, 0);
        }

        public static float ReadFloat(byte[] array, ref int offset)
        {
            byte[] a = new byte[4];
            for (int t = 0; t < 4; t++)
            {
                a[t] = array[offset];
                offset++;
            }
            return BitConverter.ToSingle(a, 0);
        }

        public static double ReadDouble(byte[] array, ref int offset)
        {
            byte[] a = new byte[8];
            for (int t = 0; t < 8; t++)
            {
                a[t] = array[offset];
                offset++;
            }
            return BitConverter.ToDouble(a, 0);
        }
    }
}
