using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapLayerManager : MonoBehaviour
{
    [Header("UI stuff")]
    [SerializeField] TMP_Dropdown m_prefabList;

    [SerializeField] RectTransform m_contentParent;

    [SerializeField] GameObject m_entryPrefab;

    [Header("Map stuff")]
    
    [SerializeField] RectTransform m_mapParent;

    [SerializeField] GameObject m_mapPrefab;

    [Header("Prefabs")]

    [SerializeField] MapLayerRaw[] m_rawPrefabs;

    List<MapLayer> m_layers = new List<MapLayer>();

    List<MapLayerPrefab> m_prefabs = new List<MapLayerPrefab>();

    Dictionary<MapLayer, MapCanvasLayers> m_renderers = 
        new Dictionary<MapLayer, MapCanvasLayers>();

    void LoadLayers()
    {
        try
        {
            var str = PlayerPrefs.GetString("SAVE", "");

            if (!string.IsNullOrWhiteSpace(str))
            {
                MapLayerRawRaw[] rawLayers = Newtonsoft.Json.JsonConvert.DeserializeObject<MapLayerRawRaw[]>(str);

                foreach(var layer in rawLayers)
                {
                    AddLayer(layer.ToLayer(m_autoCompletionProvider));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to load, " + ex.Message);
            PlayerPrefs.SetString("SAVE", "");
        }
    }

    public void SaveLayers()
    {
        try{
            List<MapLayerRawRaw> m_rawLayers = new List<MapLayerRawRaw>();

            foreach(var l in m_layers)
                m_rawLayers.Add(l.ToRawLayer().ToRawRaw());
            
            string save = Newtonsoft.Json.JsonConvert.SerializeObject(m_rawLayers.ToArray());
            PlayerPrefs.SetString("SAVE", save);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save, " + ex.Message);
            PlayerPrefs.SetString("SAVE", "");
        }
    }

    private void Start()
    {
        LoadLayers();

        m_prefabList.ClearOptions();

        var opts = new List<TMP_Dropdown.OptionData>();

        opts.Add(new TMP_Dropdown.OptionData("Prefabs"));

        foreach(var p in m_rawPrefabs)
            opts.Add(new TMP_Dropdown.OptionData(p.Name));

        m_prefabList.AddOptions(opts);
        m_prefabList.onValueChanged.AddListener(OnPrefabSelected);
    }

    [Inject] DatasetAutocompletion m_autoCompletionProvider;

    private void OnPrefabSelected(int prefab)
    {
        if (prefab == 0) return;

        AddLayer(m_rawPrefabs[prefab - 1].ToLayer(m_autoCompletionProvider));

        m_prefabList.value = 0;
    }

    public void OpenNewLayerWindow()
    {
        AddNewLayerWindow window = WindowManager.LastInstance.Push<AddNewLayerWindow>();

        window.Setup(layer => {
            AddLayer(layer);
        });
    }

    public void AddLayer(MapLayer layer)
    {
        GameObject go = Instantiate(m_mapPrefab, m_mapParent, false);
        var layerC = go.GetComponent<MapCanvasLayers>();

        layerC.Setup(layer);

        m_renderers.Add(layer, layerC);
        m_layers.Add(layer);
        LayersUpdated();
    }

    public MapCanvasLayers GetLayer(MapLayer layer)
    {
        return m_renderers[layer];
    }

    public void RemoveLayer(MapLayer layer)
    {
        if (m_renderers.ContainsKey(layer)) Destroy(m_renderers[layer].gameObject);
    
        m_layers.Remove(layer);
        LayersUpdated();
    }

    void LayersUpdated()
    {
        while (m_prefabs.Count > m_layers.Count)
        {
            int last = m_prefabs.Count - 1;
            Destroy(m_prefabs[last].gameObject);
            m_prefabs.RemoveAt(last);
        }

        while (m_prefabs.Count < m_layers.Count)
        {
            GameObject prefab = Instantiate(m_entryPrefab, m_contentParent, false);
            m_prefabs.Add(prefab.GetComponent<MapLayerPrefab>());
        }

        for (int i = 0; i < m_layers.Count; ++i)
        {
            m_prefabs[i].Setup(this, m_layers[i]);
        }

        SaveLayers();
    }
}
