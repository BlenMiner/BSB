using System;
using System.Collections.Generic;
using System.IO;

namespace BSB
{
    public struct ValeurContenuDansBytes
    {
        public byte no_2;
        public byte o3;
        public byte pm10;
        public byte pm25;
        public byte code_equal;
        public byte code_so2;
        public int code_zone;
        public DateTime date_ech;
        public string lib_qual;
        public string lib_zone;
        public float x_wgs84;
        public float y_wgs84;
    }

    public struct ValeurContenuDansBytes_Idf_2017
    {
        public DateTime date;
        public byte ninsee;
        public byte no2;
        public byte o3;
        public byte pm10;
        public string commune;
        public int code_postal;
        public float latitude;
        public float longitude;
    }

    class Program
    {
        static string inCaseOfEmptyString(string a)
        {
            if (string.IsNullOrEmpty(a))
            {
                return "0";
            }
            return a;
        }

        static void saveCsvToBytes(string readPath, string savePath)
        {
            Console.WriteLine(readPath);
            string csv = File.ReadAllText(readPath);
            var lignes = csv.Split('\n');

            BinaryWriter bw = new BinaryWriter(new FileStream(savePath, FileMode.Create));

            for (int i = 1; i < lignes.Length; i++)
            {
                var cols = lignes[i].Split(';');
                byte no_2 = byte.Parse(inCaseOfEmptyString(cols[0]));
                byte o3 = byte.Parse(inCaseOfEmptyString(cols[1]));
                byte pm10 = byte.Parse(inCaseOfEmptyString(cols[2]));
                byte pm25 = byte.Parse(inCaseOfEmptyString(cols[3]));
                byte code_equal = byte.Parse(inCaseOfEmptyString(cols[4]));
                byte code_so2 = byte.Parse(inCaseOfEmptyString(cols[5]));
                int code_zone = int.Parse(cols[6]);
                String date_ech = cols[7];
                //int epsg_reg = int.Parse(cols[8]);
                string lib_qual = cols[9];
                string lib_zone = cols[10];
                //float x_reg = float.Parse(cols[11]);
                float x_wgs84 = float.Parse(cols[12].Replace('.', ','));
                //float y_reg = float.Parse(cols[13]);
                float y_wgs84 = float.Parse(cols[14].Replace('.', ','));


                bw.Write(no_2);
                bw.Write(o3);
                bw.Write(pm10);
                bw.Write(pm25);
                bw.Write(code_equal);
                bw.Write(code_so2);
                bw.Write(code_zone);
                bw.Write(date_ech);
                bw.Write(lib_qual);
                bw.Write(lib_zone);
                //bw.Write(x_reg);
                bw.Write(x_wgs84);
                //bw.Write(y_reg);
                bw.Write(y_wgs84);
            }

            bw.Close();
        }

        static void saveCsvToBytes_Idf_2017(string readPath, string savePath)
        {
            Console.WriteLine(readPath);
            string csv = File.ReadAllText(readPath);
            var lignes = csv.Split('\n');

            BinaryWriter bw = new BinaryWriter(new FileStream(savePath, FileMode.Create));

            for (int i = 1; i < lignes.Length; i++)
            {
                var cols = lignes[i].Split(';');
                String date = cols[0];
                int ninsee = int.Parse(cols[1]);
                byte no2 = byte.Parse(inCaseOfEmptyString(cols[2]));
                byte o3 = byte.Parse(inCaseOfEmptyString(cols[3]));
                byte pm10 = byte.Parse(inCaseOfEmptyString(cols[4]));
                string commune = inCaseOfEmptyString(cols[5]);
                int code_postal = int.Parse(inCaseOfEmptyString(cols[6]));
                float latitute = float.Parse(inCaseOfEmptyString(cols[7]).Replace('.', ','));
                float longitude = float.Parse(inCaseOfEmptyString(cols[8]).Replace('.', ','));


                bw.Write(date);
                bw.Write(ninsee);
                bw.Write(no2);
                bw.Write(o3);
                bw.Write(pm10);
                bw.Write(commune);
                bw.Write(code_postal);
                bw.Write(latitute);
                bw.Write(longitude);
            }

            bw.Close();
        }

        static List<ValeurContenuDansBytes> readBinFile(string readPath)
        {
            BinaryReader br = new BinaryReader(new FileStream(readPath, FileMode.Open));
            List<ValeurContenuDansBytes> ligneContenuDansBytes = new List<ValeurContenuDansBytes>();
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                ValeurContenuDansBytes ligne = new ValeurContenuDansBytes();
                ligne.no_2 = br.ReadByte();
                ligne.o3 = br.ReadByte();
                ligne.pm10 = br.ReadByte();
                ligne.pm25 = br.ReadByte();
                ligne.code_equal = br.ReadByte();
                ligne.code_so2 = br.ReadByte();
                ligne.code_zone = br.ReadInt32();
                ligne.date_ech = DateTime.Parse(br.ReadString());
                ligne.lib_qual = br.ReadString();
                ligne.lib_zone = br.ReadString();
                ligne.x_wgs84 = br.ReadSingle();
                ligne.y_wgs84 = br.ReadSingle();

                ligneContenuDansBytes.Add(ligne);
            }
            br.Close();
            return ligneContenuDansBytes;
        }

        static List<ValeurContenuDansBytes_Idf_2017> readBinFile_Idf_2017(string readPath)
        {
            BinaryReader br = new BinaryReader(new FileStream(@"C: \Users\Singh\Downloads\test", FileMode.Open));
            List<ValeurContenuDansBytes_Idf_2017> ligneContenuDansBytes = new List<ValeurContenuDansBytes_Idf_2017>();
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                ValeurContenuDansBytes_Idf_2017 ligne = new ValeurContenuDansBytes_Idf_2017();
                ligne.date = DateTime.Parse(br.ReadString());
                ligne.ninsee = br.ReadByte();
                ligne.no2 = br.ReadByte();
                ligne.o3 = br.ReadByte();
                ligne.pm10 = br.ReadByte();
                ligne.code_postal = br.ReadInt32();
                ligne.latitude = br.ReadSingle();
                ligne.longitude = br.ReadSingle();

                ligneContenuDansBytes.Add(ligne);
            }
            br.Close();
            return ligneContenuDansBytes;
        }


        static void Main(string[] args)
        {
            
        }
    }
}
