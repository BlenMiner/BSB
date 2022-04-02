using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mapbox.Utils;
using UnityEngine;

public class DepartmentDataset : Dataset
{
    [SerializeField, Provides] DepartmentDataset me;

    [SerializeField] TextAsset m_dataset;

    [SerializeField] TextAsset m_datasetPopulation;

    Dictionary<string, Vector2d> m_departmentCoords = new Dictionary<string, Vector2d>();

    Dictionary<string, int> m_depPopCount = new Dictionary<string, int>();

    int m_totalPop = 0;

    int m_minPop = int.MaxValue;

    int m_maxPop = int.MinValue;

    private void Awake()
    {
        Load();
    }

    public void Unload()
    {
        m_departmentCoords.Clear();
    }

    public void Load()
    {
        MemoryStream memoryStream = new MemoryStream(m_dataset.bytes);
        BinaryReader br = new BinaryReader(memoryStream);

        int count = br.ReadInt32();

        for (int i = 0; i < count; ++i)
        {
            string key = br.ReadString().TrimStart('0');
            double x = br.ReadDouble();
            double y = br.ReadDouble();

            m_departmentCoords.Add(key, new Vector2d(x, y));
        }

        memoryStream.Close();

        string[] popRows = m_datasetPopulation.text.Split('\n');
        for (int i = 1; i < popRows.Length; ++i)
        {
            string[] col = popRows[i].Split(',');

            if (col.Length != 2) break;

            string depId = col[0].TrimStart('0');
            int pop = int.Parse(col[1]);

            m_minPop = Mathf.Min(m_minPop, pop);
            m_maxPop = Mathf.Max(m_maxPop, pop);

            m_depPopCount.Add(depId, pop);
            m_totalPop += pop;
        }
    }

    private void OnDestroy()
    {
        Unload();
    }

    public string GetClosestDepartment(Vector2d position)
    {
        string value = null;
        double distance = double.PositiveInfinity;

        foreach(var v in m_departmentCoords)
        {
            var dist = Vector2d.Distance(position, v.Value);
            if (dist < distance)
            {
                distance = dist;
                value = v.Key;
            }
        }

        return value;
    }

    public void MapDepCoords(System.Action<string, Vector2d> cb)
    {
        foreach(var v in m_departmentCoords)
            cb(v.Key, v.Value);
    }

    public Vector2d GetDepCoords(string depId)
    {
        return m_departmentCoords[depId];
    }

    readonly DatasetProp[] m_properties = new DatasetProp[] {
        new DatasetProp {Value = "DepPopulation" },
        new DatasetProp {Value = "TotalPopulation" },
    };

    public override DatasetProp[] GetDataProperties()
    {
        return m_properties;
    }

    public override bool GetData(string departmentId, string property, float time, out float value)
    {
        value = 0f;

        switch (property)
        {
            case "DepPopulation":
            {
                if (m_depPopCount.TryGetValue(departmentId, out var v))
                {
                    value = v;
                    return true;
                }
                else return false;
            }
            case "TotalPopulation": value = m_totalPop; return true;
        }
        
        return false;
    }

    public override float GetMinPossibleValue(string property)
    {
        float value = 0f;

        switch (property)
        {
            case "DepPopulation": value = m_minPop; break;
            case "TotalPopulation": value = m_totalPop; break;
        }

        return value;
    }

    public override float GetMaxPossibleValue(string property)
    {
        float value = 0f;

        switch (property)
        {
            case "DepPopulation": value = m_maxPop; break;
            case "TotalPopulation": value = m_totalPop; break;
        }

        return value;
    }
}
