﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace model_reader
{
    class CTRModel
    {
        public string path;


        BinaryReader br;
        MemoryStream ms;

        CTRHeader header;
        List<CTRVertex> vertex = new List<CTRVertex>();
        List<QuadBlock> quad = new List<QuadBlock>();
        List<PickupHeader> pickups = new List<PickupHeader>();
        List<PosAng> startGrid = new List<PosAng>();


        public CTRModel(string s, string fmt, CTRVRAM vrm)
        {
            path = s;


            //rewrite this!
            ms = new MemoryStream(File.ReadAllBytes(s));
            br = new BinaryReader(ms);
            
            int size = br.ReadInt32();

            ms = new MemoryStream(br.ReadBytes((int)br.BaseStream.Length-4));
            br = new BinaryReader(ms);
            

            header = new CTRHeader(br);

            for (int i = 0; i < header.numPickupHeaders; i++)
            {
                br.BaseStream.Position = header.ptrPickupHeadersPtrArray + 4 * i;
                br.BaseStream.Position = br.ReadUInt32();

                pickups.Add(new PickupHeader(br));
            }

            br.BaseStream.Position = header.ptrInfo;

            int facesnum = br.ReadInt32();
            int vertexnum = br.ReadInt32();
            br.ReadInt32();  //???
            int ptrNgonArray = br.ReadInt32();
            uint ptrvertarray = br.ReadUInt32();
            br.ReadInt32();  //null?
            uint ptrfacearray = br.ReadUInt32();    //something else
            int facenum = br.ReadInt32();           //something else

            //read vertices
            br.BaseStream.Position = ptrvertarray;

            for (int i = 0; i < vertexnum; i++)
            {
                CTRVertex vert = new CTRVertex(br);
                vertex.Add(vert);
            }


            /*
            //starting positions
            br.BaseStream.Position = 0x6C;

            for (int i = 0; i < 8; i++)
            {
                 PosAng pa = new PosAng(new Vector3s(br.ReadBytes(6)), new Vector3s(br.ReadBytes(6)));
                    startGrid.Add(pa);
            }

            foreach (PosAng pa in startGrid)
            sb.Append(pa.Position.ToObjVertex() + "\r\n");
            */




            //read faces
            br.BaseStream.Position = ptrNgonArray;

            List<short> uniflag = new List<short>();

            for (int i = 0; i < facesnum; i++)
            {
                QuadBlock qb = new QuadBlock(br);
                quad.Add(qb);

                if (vrm != null)
                    qb.ExportTexture(vrm);

                //check unique values here
                if (!uniflag.Contains(qb.unk2[3]))
                    uniflag.Add(qb.unk2[3]); 
                 
            }

            uniflag.Sort();

            foreach (byte b in uniflag)       
                 Console.WriteLine(b);
            
        }



        public string Export(string fmt)
        {
            string fname = Path.ChangeExtension(path, fmt);
            string mtllib = Path.ChangeExtension(path, ".mtl");
            Console.WriteLine("Exporting  to: " + fname);

            StringBuilder sb = new StringBuilder();

            switch (fmt)
            {
                case "obj":

                    sb.Append("#Converted to OBJ using model_reader, CTR-Tools by DCxDemo*.\r\n");
                    sb.Append("#(C) 1999, Activision, Naughty Dog.\r\n\r\n");

                    sb.Append("mtllib " + mtllib + "\r\n\r\n");

                    foreach (QuadBlock g in quad)
                        sb.Append(g.ToObj(vertex, Detail.Low) + "\r\n");
                    break;

                case "ply":
                    Console.WriteLine("ply format is yet to be reimplemented");
                    break;

 /*                   //get back ply later
                    if (fmt == "ply")
                    {
                        sb.Append("ply\r\n");
                        sb.Append("format ascii 1.0\r\n");
                        sb.Append("comment CTR-Tools by DCxDemo*\r\n");
                        sb.Append("comment source file: " + path + "\r\n");
                        sb.Append("element vertex " + vertexnum + "\r\n");
                        sb.Append("property float x\r\n");
                        sb.Append("property float y\r\n");
                        sb.Append("property float z\r\n");
                        sb.Append("property uchar red\r\n");
                        sb.Append("property uchar green\r\n");
                        sb.Append("property uchar blue\r\n");
                        sb.Append("element face " + facesnum * 8 + "\r\n");
                        sb.Append("property list uchar int vertex_indices\r\n");
                        sb.Append("end_header\r\n");
                    }
*/

                default:
                    Console.WriteLine("Unknown export format.");
                    break;

            }

            WriteToFile(fname, sb.ToString());

            sb.Clear();

            for (int i = 0; i < 16; i++)
                for (int j = 0; j < 2; j++)
                {
                    sb.Append(String.Format("newmtl texpage_{0}_{1}\r\n", i, j));
                    sb.Append(String.Format("map_Ka texpage_{0}_{1}.png\r\n", i, j));
                    sb.Append(String.Format("map_Kd texpage_{0}_{1}.png\r\n\r\n", i, j));
                }

            WriteToFile(mtllib, sb.ToString());

            Console.WriteLine("Done!");

            return fname;
        }


        public void WriteToFile(string fname, string content)
        {
            using (FileStream fs = new FileStream(fname, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    fs.SetLength(content.Length);
                    sw.Write(content);
                }
            }
        }


        public string ASCIIFace(string label, short[] inds, int x, int y, int z)
        {
            return label + " " + inds[x] + " " + inds[y] + " " + inds[z] + "\r\n";
        }

    }
}