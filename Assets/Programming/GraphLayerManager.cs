using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WindinatorTools;
using XCharts.Runtime;

public class GraphLayerManager : MonoBehaviour
{
    [Header("UI stuff")]

    [SerializeField] TMP_Dropdown m_prefabList;

    [SerializeField] RectTransform m_contentParent;

    [SerializeField] GameObject m_entryPrefab;

    [Header("Prefabs")]

    [SerializeField] GraphLayerRaw[] m_rawPrefabs;

    [Inject] TimeMachine m_timeMachine;

    [Inject] DatasetAutocompletion m_autoCompletionProvider;

    UIPool m_pool;

    List<GraphLayer> m_layers = new List<GraphLayer>();

    DateTime m_timeA, m_timeB;

    private void OnEnable()
    {
        m_timeMachine.OnTimeMachineUpdate += TimeDirty;
        TimeDirty(0f, 0f);
    }

    private void OnDisable()
    {
        m_timeMachine.OnTimeMachineUpdate -= TimeDirty;
    }

    private void TimeDirty(float aTime, float bTime)
    {
        m_timeA = m_timeMachine.GetTime(aTime);
        m_timeB = m_timeMachine.GetTime(bTime);
    }

    private void Start()
    {
        m_pool = new UIPool(m_entryPrefab, m_contentParent);
        m_prefabList.ClearOptions();

        var opts = new List<TMP_Dropdown.OptionData>();

        opts.Add(new TMP_Dropdown.OptionData("Prefabs"));

        foreach (var p in m_rawPrefabs)
            opts.Add(new TMP_Dropdown.OptionData(p.Name));

        m_prefabList.AddOptions(opts);
        m_prefabList.onValueChanged.AddListener(OnAddPrefab);

        LoadLayers();
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

    bool m_dirty = false;

    public void AddNewLayer()
    {
        WindowManager.LastInstance.Push<CreateGraphWindow>().Setup(name =>
        {
            AddLayer(new GraphLayer
            {
                Name = name,
                Formulas = new List<NamedFormula>()
            });
        });
    }

    public void AddLayer(GraphLayer layer)
    {
        m_layers.Add(layer);
        m_dirty = true;
    }

    void LayersUpdated()
    {
        foreach (var layer in m_layers)
        {
            var chart = m_pool.GetInstance<LineChartScript>();

            chart.UpdateTitle(layer.Name);
            chart.UpdateGraph(new DateTime(2017, 1, 1), new DateTime(2017, 12, 31),
                m_timeA, m_timeB, layer.CalculateData(), layer.GetLabels());
        }

        m_pool.DiscardRest();

        SaveLayers();
    }

    private void Update()
    {
        if (m_dirty)
        {
            LayersUpdated();
            m_dirty = false;
        }
    }
}

public class GraphLayer
{
    public string Name;

    public List<NamedFormula> Formulas;

    public GraphLayerRaw ToRawLayer()
    {
        RawNamedFormula[] rawFormulas = new RawNamedFormula[Formulas.Count];

        for (int i = 0; i < rawFormulas.Length; ++i)
            rawFormulas[i] = new RawNamedFormula
            {
                Name = Formulas[i].Name,
                Formula = Formulas[i].Formula.RawText
            };

        return new GraphLayerRaw()
        {
            Name = Name,
            Formulas = rawFormulas
        };
    }

    public string[] GetLabels()
    {
        string[] labels = new string[Formulas.Count];

        for (int i = 0; i < labels.Length; ++i)
            labels[i] = Formulas[i].Name;

        return labels;
    }

    public float[][] CalculateData()
    {
        float[][] data = new float[Formulas.Count][];

        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = new float[365];
            for (int j = 0; j < data[i].Length; ++j)
            {
                data[i][j] = Mathf.PerlinNoise(j * 0.5f, 0);
            }
        }

        return data;
    }
}

public struct NamedFormula
{
    public string Name;
    public Formula Formula;
}

public struct RawNamedFormula
{
    public string Name;
    public string Formula;
}

[System.Serializable]
public class GraphLayerRaw
{
    public string Name;

    public RawNamedFormula[] Formulas;

    public GraphLayer ToLayer(DatasetAutocompletion datasetInfo)
    {
        List<NamedFormula> formulas = new List<NamedFormula>(Formulas.Length);

        for (int i = 0; i < Formulas.Length; ++i)
            formulas.Add(new NamedFormula
            {
                Name = Formulas[i].Name,
                Formula = new Formula(datasetInfo, Formulas[i].Formula)
            });

        return new GraphLayer()
        {
            Name = Name,
            Formulas = formulas,
        };
    }

    public GraphLayerRaw ToRawRaw()
    {
        return new GraphLayerRaw()
        {
            Name = Name,
            Formulas = Formulas,
        };
    }
}