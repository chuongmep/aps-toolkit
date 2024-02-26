// Copyright (c) Alexandre Piro - Piro CIE. All rights reserved
// This code is made from the package Forge-Convert-Utils by Petr Broz (Autodesk)
// Most parts of this code are Typescript -> C# adaptation

using System.Numerics;
using APSToolkit.Schema;
using ICSharpCode.SharpZipLib.GZip;

namespace APSToolkit
{
    public class PackFileReader : InputStream
    {
        public string Type { get; set; }
        protected int Version { get; set; }
        private int[] Entries { get; set; }
        private ISvfManifestType[] Types { get; set; }

        public static byte[] DecompressBuffer(byte[] inputBuffer)
        {
            if (inputBuffer[0] == 31 && inputBuffer[1] == 139)
            {
                using (var compressedStream = new MemoryStream(inputBuffer))
                using (var resultStream = new MemoryStream())
                {
                    GZip.Decompress(compressedStream, resultStream, true);
                    return resultStream.ToArray();
                }
            }

            return inputBuffer;
        }


        public PackFileReader(byte[] buffer) : base(DecompressBuffer(buffer))
        {
            this.Type = this.GetString(this.GetVarint());
            this.Version = this.GetInt32();
            this.ParseContents();
        }

        protected void ParseContents()
        {
            // Get offsets to TOC and type sets from the end of the file
            int originalOffset = this.Offset;
            this.Seek(this.Length - 8);
            uint entriesOffset = this.GetUint32();
            uint typesOffset = this.GetUint32();

            // Populate entries
            this.Seek((int)entriesOffset);
            int entriesCount = this.GetVarint();
            this.Entries = new int[entriesCount];
            for (int i = 0; i < entriesCount; i++)
            {
                this.Entries[i] = (int)this.GetUint32();
            }

            // Populate type sets
            this.Seek((int)typesOffset);
            int typesCount = this.GetVarint();
            this.Types = new ISvfManifestType[typesCount];
            for (int i = 0; i < typesCount; i++)
            {
                string @class = this.GetString(this.GetVarint());
                string type = this.GetString(this.GetVarint());

                this.Types[i] = new ISvfManifestType()
                {
                    typeClass = @class,
                    type = type,
                    version = this.GetVarint()
                };
            }

            // Restore offset
            this.Seek(originalOffset);
        }

        public int NumEntries()
        {
            return this.Entries.Length;
        }

        public ISvfManifestType? SeekEntry(int i)
        {
            if (i >= this.NumEntries())
            {
                return null;
            }

            // Read the type index and populate the entry data
            int offset = this.Entries[i];
            this.Seek(offset);
            uint type = this.GetUint32();
            if (type >= this.Types.Length)
            {
                return null;
            }

            return this.Types[type];
        }

        // explicit cast of double into float to fit the unity format
        protected System.Numerics.Vector3 GetVector3D()
        {
            return new System.Numerics.Vector3(
                (float)this.GetFloat64(),
                (float)this.GetFloat64(),
                (float)this.GetFloat64());
        }

        protected Quaternion GetQuaternion()
        {
            return new Quaternion(
                this.GetFloat32(),
                this.GetFloat32(),
                this.GetFloat32(),
                this.GetFloat32());
        }

        protected double[] GetMatrix3X3()
        {
            double[] elements = new double[9];
            int count = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    elements[count] = this.GetFloat32();
                    count += 1;
                }
            }

            return elements;
        }

        public ISvfTransform? GetTransform()
        {
            byte xformType = this.GetUint8();
            Quaternion q;
            System.Numerics.Vector3 t;
            System.Numerics.Vector3 s;
            double[] matrix;

            switch (xformType)
            {
                case 0: // translation
                    return new ISvfTransform() { t = this.GetVector3D() };
                case 1: // rotation & translation
                    q = this.GetQuaternion();
                    t = this.GetVector3D();
                    s = System.Numerics.Vector3.One; //{ x: 1, y: 1, z: 1 };
                    return new ISvfTransform() { t = t, q = q, s = s };
                case 2: // uniform scale & rotation & translation
                    float scale = this.GetFloat32();
                    q = this.GetQuaternion();
                    t = this.GetVector3D();
                    s = new System.Numerics.Vector3(scale, scale, scale); //{ x: scale, y: scale, z: scale };
                    return new ISvfTransform() { t = t, q = q, s = s };
                case 3: // affine matrix
                    matrix = this.GetMatrix3X3();
                    t = this.GetVector3D();
                    return new ISvfTransform() { t = t, matrix = matrix };
            }

            return null;
        }
    }
}