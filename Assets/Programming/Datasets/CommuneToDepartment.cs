using RewindSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CommuneToDepartment : Dataset
{
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

    public struct CommuneToDep
    {
        public byte ninsee;
        public int code_postal;
    }

    List<ValeurContenuDansBytes_Idf_2017> readBinFile_Idf_2017(string readPath)
    {
        BinaryReader br = new BinaryReader(new FileStream(readPath, FileMode.Open));
        List<ValeurContenuDansBytes_Idf_2017> ligneContenuDansBytes = new List<ValeurContenuDansBytes_Idf_2017>();
        List<CommuneToDep> ligneCommuneDep = new List<CommuneToDep>();
        int compteur = 1;
        while (br.BaseStream.Position < br.BaseStream.Length && compteur < 1310) //Il y a 1310 commune en ile de france
        {
            ValeurContenuDansBytes_Idf_2017 ligne = new ValeurContenuDansBytes_Idf_2017();
            CommuneToDep lignes_dep = new CommuneToDep();
            ligne.date = DateTime.Parse(br.ReadString());
            ligne.ninsee = br.ReadByte();
            ligne.no2 = br.ReadByte();
            ligne.o3 = br.ReadByte();
            ligne.pm10 = br.ReadByte();
            ligne.code_postal = br.ReadInt32();
            ligne.latitude = br.ReadSingle();
            ligne.longitude = br.ReadSingle();

            lignes_dep.ninsee = ligne.ninsee;
            lignes_dep.code_postal = ligne.code_postal;

            ligneContenuDansBytes.Add(ligne);
            ligneCommuneDep.Add(lignes_dep);
        }
        br.Close();
        return ligneContenuDansBytes;
    }
    string inCaseOfEmptyString(string a)
    {
        if (string.IsNullOrEmpty(a))
        {
            return "0";
        }
        return a;
    }

    public override bool GetData(string departmentId, string property, float time, out float value)
    {
        throw new System.NotImplementedException();
    }

    public override DatasetProp[] GetDataProperties()
    {
        throw new System.NotImplementedException();
    }

    public override float GetMaxPossibleValue(string property)
    {
        throw new System.NotImplementedException();
    }

    public override float GetMinPossibleValue(string property)
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
