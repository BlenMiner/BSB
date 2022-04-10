using RewindSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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


public class BSB_Dataset : Dataset
{
    string inCaseOfEmptyString(string a)
    {
        if (string.IsNullOrEmpty(a))
        {
            return "0";
        }
        return a;
    }

    Dictionary<string, Rewind<ValeurContenuDansBytes_Idf_2017>> m_timemachine = new Dictionary<string, Rewind<ValeurContenuDansBytes_Idf_2017>>();
    List<ValeurContenuDansBytes_Idf_2017> readBinFile_Idf_2017(string readPath)
    {
        BinaryReader br = new BinaryReader(new FileStream(readPath, FileMode.Open));
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

            if (!m_timemachine.ContainsKey(ligne.commune))
            {
                m_timemachine.Add(ligne.commune, new Rewind<ValeurContenuDansBytes_Idf_2017>());
            }        }
        br.Close();
        return ligneContenuDansBytes;
    }

    float m_timemachineLength = 0;
    List<ValeurContenuDansBytes_Idf_2017> valeursContenusDansDataset = new List<ValeurContenuDansBytes_Idf_2017>();

    private void Awake()
    {
        valeursContenusDansDataset = readBinFile_Idf_2017("BSB_Dataset.bytes");
    }

    readonly DatasetProp[] m_properties = new DatasetProp[] {
        new DatasetProp {Value = "ninsee" },
        new DatasetProp {Value = "no2" },
        new DatasetProp {Value = "o3" },
        new DatasetProp {Value = "pm10" },
        new DatasetProp {Value = "commune" },
        new DatasetProp {Value = "code postal" },
        new DatasetProp {Value = "latitude" },
        new DatasetProp {Value = "longitude" },
    };

    public override DatasetProp[] GetDataProperties()
    {
        return m_properties;
    }

    public bool GetPollution(string commune, float timep100, out ValeurContenuDansBytes_Idf_2017 value)
    {
        value = default;
        if (!m_timemachine.ContainsKey(commune)) return false;
        value = m_timemachine[commune].GetFrame(timep100);
        return true;
    }
    public override bool GetData(string ninsee, string property, float time, out float value)
    {
        value = 0f;

        if (!GetPollution(ninsee, time, out var valeurContenuDansBytes_Idf_2017)) return false;

        switch (property)
        {
            case "ninsee": value = valeurContenuDansBytes_Idf_2017.ninsee; return true;
            case "no2": value = valeurContenuDansBytes_Idf_2017.no2; return true;
            case "o3": value = valeurContenuDansBytes_Idf_2017.o3; return true;
            case "pm10": value = valeurContenuDansBytes_Idf_2017.pm10; return true;
            case "commune": value = float.Parse(valeurContenuDansBytes_Idf_2017.commune); return true;
            case "latitude": value = valeurContenuDansBytes_Idf_2017.latitude; return true;
            case "longitude": value = valeurContenuDansBytes_Idf_2017.longitude; return true;
        }

        return false;
    }

    public override float GetMaxPossibleValue(string property) //une des valeurs de datasetProp[]
    {
        throw new System.NotImplementedException();
    }

    public override float GetMinPossibleValue(string property)
    {
        throw new System.NotImplementedException();
    } 
}
