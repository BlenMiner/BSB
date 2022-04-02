using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatasetAutocompletion : MonoBehaviour
{
    [SerializeField, Provides] DatasetAutocompletion m_provider;

    Dataset[] m_datasets;

    Dictionary<string, Dataset> m_datasetsByName;

    List<DatasetProp> m_autocompletion;

    private void Awake()
    {
        m_datasets = GetComponentsInChildren<Dataset>();
        m_autocompletion = new List<DatasetProp>();

        m_datasetsByName = new Dictionary<string, Dataset>();

        foreach(var d in m_datasets) m_datasetsByName.Add(d.DatasetName, d);
    }

    public DatasetProp[] GetAutocompletion(TMPro.TMP_WordInfo word)
    {
        m_autocompletion.Clear();

        if (word.characterCount > 1)
        {
            foreach(var dataset in m_datasets)
                if (dataset.DatasetName.ToLower().StartsWith(word.GetWord().ToLower()) && dataset.DatasetName != word.GetWord())
                    m_autocompletion.Add(new DatasetProp { Value = dataset.DatasetName });
        }
        else
        {
            foreach(var dataset in m_datasets)
                m_autocompletion.Add(new DatasetProp { Value = dataset.DatasetName });
        }

        return m_autocompletion.ToArray();
    }

    public DatasetProp[] GetAutocompletion(string parent, TMPro.TMP_WordInfo word)
    {
        m_autocompletion.Clear();

        Dataset parentData = null;

        foreach(var dataset in m_datasets)
        {
            if (dataset.DatasetName == parent)
            {
                parentData = dataset;
                break;
            }
        }
        
        if (parentData == null) return System.Array.Empty<DatasetProp>();

        var props = parentData.GetDataProperties();

        if (word.characterCount > 1)
        {
            foreach(var prop in props)
            {
                if (prop.Value.ToLower().Contains(word.GetWord().ToLower()) && prop.Value != word.GetWord() ||
                    (prop.Description != null && prop.Description.ToLower().Contains(word.GetWord().ToLower()) && prop.Description != word.GetWord()))
                {
                    m_autocompletion.Add(prop);
                }
            }
        }
        else m_autocompletion.AddRange(parentData.GetDataProperties());

        return m_autocompletion.ToArray();
    }

    internal Dataset GetDataset(string v)
    {
        if (m_datasetsByName.TryGetValue(v, out var data))
            return data;
        return null;
    }
}
