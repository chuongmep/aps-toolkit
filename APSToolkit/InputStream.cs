// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved
// This code is made from the package Forge-Convert-Utils by Petr Broz (Autodesk)
// Most parts of this code are Typescript -> C# adaptation

namespace APSToolkit
{
    public class InputStream
    {
        protected byte[] Buffer;
        protected int MOffset;
        protected int MLength;

        public int Offset
        {
            get { return MOffset; }
            set { MOffset = value; }
        }

        public int Length
        {
            get { return MLength; }
            set { MLength = value; }
        }

        public InputStream(byte[] buffer)
        {
            Buffer = buffer;
            Offset = 0;
            Length = Buffer.Length;
        }

        public void Seek(int offset)
        {
            Offset = offset;
        }

        public byte GetUint8()
        {
            var val = Buffer[Offset];
            Offset += 1;
            return val;
        }

        public ushort GetUint16()
        {
            var val = BitConverter.ToUInt16(Buffer, Offset);
            Offset += 2;
            return val;
        }

        public short GetInt16()
        {
            var val = BitConverter.ToInt16(Buffer, Offset);
            Offset += 2;
            return val;
        }

        public uint GetUint32()
        {
            var val = BitConverter.ToUInt32(Buffer, Offset);
            Offset += 4;
            return val;
        }

        public int GetInt32()
        {
            var val = BitConverter.ToInt32(Buffer, Offset);
            Offset += 4;
            return val;
        }

        public float GetFloat32()
        {
            var val = BitConverter.ToSingle(Buffer, Offset);
            Offset += 4;
            return val;
        }

        public double GetFloat64()
        {
            var val = BitConverter.ToDouble(Buffer, Offset);
            Offset += 8;
            return val;
        }

        public int GetVarint()
        {
            byte @byte;
            int val = 0;
            int shift = 0;
            do
            {
                @byte = Buffer[Offset++];
                val |= (@byte & 0x7f) << shift;
                shift += 7;
            } while ((@byte & 0x80) != 0);

            return val;
        }

        public string GetString(int len)
        {
            var val = System.Text.Encoding.UTF8.GetString(Buffer, Offset, len);
            Offset += len;
            return val;
        }
    }
}