﻿using CTRFramework.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace CTRFramework.Big
{
    public class BigFileReader : BinaryReader
    {
        public int FileCursor = -1;

        private int totalFiles = 0;
        public int TotalFiles
        {
            get { return totalFiles; }
        }

        public int FileSize
        {
            get
            {
                BaseStream.Position = FileCursor * 8 + 4;
                return ReadInt32();
            }
        }

        Dictionary<int, string> names = new Dictionary<int, string>();

        public string GetFilename()
        {
            if (FileCursor != -1)
                if (names.ContainsKey(FileCursor))
                    return names[FileCursor];

            return $"file_{FileCursor.ToString("0000")}.bin";
        }

        private int fileDefPtr
        {
            get { return 8 + FileCursor * 8; }
        }

        public BigFileReader(Stream stream) : base(stream)
        {
            SanityCheck();
            KnownFileCheck();
        }

        /// <summary>
        /// Sanity check for CTR BIG format.
        /// </summary>
        private void SanityCheck()
        {
            BaseStream.Position = 0;

            if (ReadInt32() != 0)
                throw new NotSupportedException($"{this.GetType().Name}: unlikely a CTR BIG file.");

            totalFiles = ReadInt32();

            if (totalFiles > 2048)
                throw new NotSupportedException($"{this.GetType().Name}: unlikely a CTR BIG file, more than 2048 files.");

            for (int i = 0, ptr, size; i < totalFiles; i++)
            {
                ptr = ReadInt32();
                size = ReadInt32();

                if (ptr > BaseStream.Length || ptr + size > BaseStream.Length)
                    throw new NotSupportedException($"{this.GetType().Name}: unlikely a CTR BIG file.");
            }
        }

        /// <summary>
        /// Reads the external list of file names if it's a known BIG file.
        /// </summary>
        private void KnownFileCheck()
        {
            BaseStream.Position = 0;

            if (File.Exists(Meta.XmlPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(File.ReadAllText(Meta.XmlPath));

                var hash = MD5.Create().ComputeHash(BaseStream);
                string md5 = BitConverter.ToString(hash).Replace("-", "");

                foreach (XmlElement el in doc.SelectNodes("/data/versions/entry"))
                {
                    if (md5.ToLower() == el["md5"].InnerText.ToLower())
                    {
                        Console.WriteLine($"{md5}\r\n{el["name"].InnerText} [{el["region"].InnerText}] detected.\r\nUsing {el["list"].InnerText}");
                        names = Meta.GetBigList(el["list"].InnerText);
                        return;
                    }
                }

                foreach (XmlElement el in doc.SelectNodes("/data/filenums/entry"))
                {
                    if (TotalFiles == Int32.Parse(el["num"].InnerText))
                    {
                        Console.WriteLine($"Using {el["list"].InnerText}");
                        names = Meta.GetBigList(el["list"].InnerText);
                        return;
                    }
                }
            }

            Console.WriteLine("Unknown BIG file.");
        }

        /// <summary>
        /// Reads file entry.
        /// </summary>
        /// <returns>File data as array of byte.</returns>
        public byte[] ReadFile()
        {
            if (FileCursor == -1)
                throw new ArgumentOutOfRangeException($"{this.GetType().Name}: Must use NextFile() first!");

            if (fileDefPtr > BaseStream.Length)
                throw new OverflowException($"{this.GetType().Name}: out of bounds.");

            BaseStream.Position = fileDefPtr;

            int _ptr = ReadInt32() * 0x800;
            int _size = ReadInt32();

            if (_ptr + _size > BaseStream.Length)
                throw new OverflowException($"{this.GetType().Name}: out of bounds.");

            BaseStream.Position = _ptr;

            return ReadBytes(_size);
        }

        /// <summary>
        /// Moves cursor to next file.
        /// </summary>
        /// <returns>True if next file exists, false otherwise.</returns>
        public bool NextFile()
        {
            FileCursor++;
            return FileCursor < TotalFiles;
        }
    }
}