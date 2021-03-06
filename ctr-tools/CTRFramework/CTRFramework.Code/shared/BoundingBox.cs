﻿using System.ComponentModel;

namespace CTRFramework.Shared
{
    public class BoundingBox : IReadWrite
    {
        [CategoryAttribute("Values"), DescriptionAttribute("Mininum.")]
        public Vector3s Min
        {
            get { return min; }
            set { Min = value; }
        }

        [CategoryAttribute("Values"), DescriptionAttribute("Maximum.")]
        public Vector3s Max
        {
            get { return max; }
            set { Max = value; }
        }


        private Vector3s min;
        private Vector3s max;


        public BoundingBox()
        {
            min = new Vector3s(short.MaxValue);
            max = new Vector3s(short.MinValue);
        }

        public BoundingBox(BinaryReaderEx br)
        {
            Read(br);
        }

        public void Read(BinaryReaderEx br)
        {
            min = new Vector3s(br);
            max = new Vector3s(br);
        }

        public void Write(BinaryWriterEx bw)
        {
            min.Write(bw);
            max.Write(bw);
        }

        public override string ToString()
        {
            return "BB: min " + min.ToString() + " max " + max.ToString();
        }


        /*
        public int Max(int x, int y, int z)
        {
            int max = x;
            if (y > max) max = y;
            if (z > max) max = z;
            return max;
        }

        public int Min(int x, int y, int z)
        {
            int min = x;
            if (y < min) min = y;
            if (z < min) min = z;
            return min;
        }
        */

    }
}