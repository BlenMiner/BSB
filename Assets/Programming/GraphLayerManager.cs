using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphLayerManager : MonoBehaviour
{
    [Header("UI stuff")]

    [SerializeField] TMP_Dropdown m_prefabList;

    [SerializeField] RectTransform m_contentParent;

    [SerializeField] GameObject m_entryPrefab;

    [Header("Prefabs")]

    [SerializeField] GraphLayerRaw[] m_rawPrefabs;

    [Inject] DatasetAutocompletion m_autoCompletionProvider;

    List<GraphLayer> m_layers = new List<GraphLayer>();

    private void Start()
    {
        LoadLayers();

        m_prefabList.ClearOptions();

        var opts = new List<TMP_Dropdown.OptionData>();

        opts.Add(new TMP_Dropdown.OptionData("Prefabs"));

        foreach (var p in m_rawPrefabs)
            opts.Add(new TMP_Dropdown.OptionData(p.Name));

        m_prefabList.AddOptions(opts);
        m_prefabList.onValueChanged.AddListener(OnAddPrefab);
    }

    private void OnAddPrefab(int id)
    {

    }

    void LoadLayers()
    {
        try
        {
            var str = PlayerPrefs.GetString("SAVE_GRAPH", "");

            if (!string.IsNullOrWhiteSpace(str))
            {
                GraphLayerRaw[] rawLayers = Newtonsoft.Json.JsonConvert.DeserializeObject<GraphLayerRaw[]>(str);

                foreach (var layer in rawLayers)
                {
                    AddLayer(layer.ToLayer(m_autoCompletionProvider));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to load, " + ex.Message);
            PlayerPrefs.SetString("SAVE_GRAPH", "");
        }
    }

    public void SaveLayers()
    {
        try
        {
            string save = Newtonsoft.Json.JsonConvert.SerializeObject(m_layers.ToArray());
            PlayerPrefs.SetString("SAVE_GRAPH", save);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save, " + ex.Message);
            PlayerPrefs.SetString("SAVE_GRAPH", "");
        }
    }

    public void AddNewLayer()
    {

    }

    public void AddLayer(GraphLayer layer)
    {
        LayersUpdated();
    }

    void LayersUpdated()
    {
        SaveLayers();
    }
}

public class GraphLayer
{
    public string Name;

    public Formula Formula;

    public GraphLayerRaw ToRawLayer()
    {
        return new GraphLayerRaw()
        {
            Name = Name,
            Formula = Formula.RawText
        };
    }
}

[System.Serializable]
public class GraphLayerRaw
{
    public string Name;

    public string Formula;

    public GraphLayer ToLayer(DatasetAutocompletion datasetInfo)
    {
        return new GraphLayer()
        {
            Name = Name,
            Formula = new Formula(datasetInfo, Formula),
        };
    }

    public GraphLayerRaw ToRawRaw()
    {
        return new GraphLayerRaw()
        {
            Name = Name,
            Formula = Formula,
        };
    }
}