using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DatasetProp
{
    public string Value;

    public string Description;
}

public abstract class Dataset : MonoBehaviour
{
    [SerializeField] string m_datasetName;

    public static string GetDepartmentID(int insee)
    {
        if (INSEEDataset.me.GetINSEE(insee, out var v))
            return v.DepId;
        return null;
    }
    
    public string DatasetName => m_datasetName;
    
    public abstract DatasetProp[] GetDataProperties();

    public abstract bool GetData(int insee, string property, float time, out float value);

    public abstract float GetMinPossibleValue(string property);

    public abstract float GetMaxPossibleValue(string property);
}
