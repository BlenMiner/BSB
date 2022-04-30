using RewindSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct PolutionData
{
    public DateTime date;

    public int ninsee;

    public byte no2;

    public byte o3;

    public byte pm10;

    public int code_postal;

    public string name;

    public int unk;

    public string departement;

    public float latitude;

    public float longitude;
}


public class BSB_Dataset : Dataset
{
    Dictionary<int, Rewind<PolutionData>> m_data = 
        new Dictionary<int, Rewind<PolutionData>>();

    [SerializeField, Provides] BSB_Dataset m_provider;

    [SerializeField] TextAsset m_dataset;

    [Inject] INSEEDataset m_inseeDataset;

    [Inject] TimeMachine m_timeMachine;

    public HashSet<int> INSEECodes {get; private set;} = new HashSet<int>();

    int minNO2 = int.MaxValue, minO3 = int.MaxValue, minPM10 = int.MaxValue;

    int maxNO2 = int.MinValue, maxO3 = int.MinValue, maxPM10 = int.MinValue;

    private void Awake()
    {
        ImportData();
    }

    void ImportData()
    {
        // Creates and initializes a CultureInfo.
        CultureInfo myCI = new CultureInfo("fr-FR", false);
        CultureInfo.CurrentCulture = myCI;

        using BinaryReader br = new BinaryReader(new MemoryStream(m_dataset.bytes));
        int added = 0;

        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var dateStr = br.ReadString();

            if (!DateTime.TryParse(dateStr, out var date))
                Debug.LogError("Failed to parse: " + dateStr);

            PolutionData ligne = new PolutionData
            {
                date = date,
                ninsee = br.ReadInt32(),
                no2 = br.ReadByte(),
                o3 = br.ReadByte(),
                pm10 = br.ReadByte(),
                name = br.ReadString(),
                code_postal = br.ReadInt32(),
                latitude = br.ReadSingle(),
                longitude = br.ReadSingle()
            };

            INSEECodes.Add(ligne.ninsee);

            minNO2 = Mathf.Min(minNO2, ligne.no2);
            minO3 = Mathf.Min(minO3, ligne.o3);
            minPM10 = Mathf.Min(minPM10, ligne.pm10);

            maxNO2 = Mathf.Max(maxNO2, ligne.no2);
            maxO3 = Mathf.Max(maxO3, ligne.o3);
            maxPM10 = Mathf.Max(maxPM10, ligne.pm10);
            
            if (ligne.code_postal == 0)
            {
                if (m_inseeDataset.GetINSEE(ligne.ninsee, out var data))
                {
                    ligne.departement = data.DepId;
                    ligne.code_postal = data.PostalCode;
                    ligne.longitude = (float)data.LonLat.x;
                    ligne.latitude = (float)data.LonLat.y;
                }
                else continue;
            }
            else ligne.departement = ligne.code_postal.ToString().Substring(0, 2);

            if (!m_data.ContainsKey(ligne.ninsee))
                m_data.Add(ligne.ninsee, new Rewind<PolutionData>());
            
            var time = m_data[ligne.ninsee];
            float actualDate = (float)(date - WeatherDataset.START_DATE).TotalDays;

            float percentage = actualDate / (m_timeMachine.LengthDate * 0.01f);

            time.RegisterFrame(ligne, percentage);

            ++added;
        }
    }

    readonly DatasetProp[] m_properties = new DatasetProp[]
    {
        new DatasetProp {Value = "NO2" },
        new DatasetProp {Value = "O3" },
        new DatasetProp {Value = "PM10" },
    };

    public override DatasetProp[] GetDataProperties()
    {
        return m_properties;
    }

    public bool GetPollution(int codePostal, float timep100, out PolutionData value)
    {
        value = default;
        if (!m_data.ContainsKey(codePostal)) return false;
        value = m_data[codePostal].GetFrame(timep100);
        return true;
    }

    public override bool GetData(int codePostal, string property, float time, out float value)
    {
        value = 0f;

        if (!GetPollution(codePostal, time, out var polutionData))
            return false;

        switch (property)
        {
            case "NO2": value = polutionData.no2; return true;
            case "O3": value = polutionData.o3; return true;
            case "PM10": value = polutionData.pm10; return true;
        }

        return false;
    }

    public override float GetMaxPossibleValue(string property) //une des valeurs de datasetProp[]
    {
        switch (property)
        {
            case "NO2": return maxNO2;
            case "O3": return maxO3;
            case "PM10": return maxPM10;
        }

        return 0f;
    }

    public override float GetMinPossibleValue(string property)
    {
        switch (property)
        {
            case "NO2": return minNO2;
            case "O3": return minO3;
            case "PM10": return minPM10;
        }

        return 0f;
    } 
}
