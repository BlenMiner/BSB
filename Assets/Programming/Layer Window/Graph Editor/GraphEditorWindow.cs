using System;
using UnityEngine;
using UnityEngine.UI;
using WindinatorTools;

public class GraphEditorWindow : WindowBehaviour
{
    [Header("Prefab & containers")]
    [SerializeField] RectTransform m_content;

    [SerializeField] GameObject m_entryPrefab;
    [SerializeField] GameObject m_buttonPrefab;

    [Header("Properties")]
    [SerializeField] TMPro.TMP_InputField m_name;

    [Inject] DatasetAutocompletion m_autocomplete;

    GraphLayer m_layer;

    UIPool m_buttonPool;
    UIPool m_entryPool;

    Action<GraphLayer> m_onSubmit;

    public GraphLayer Layer => m_layer;

    public DatasetAutocompletion Autocompletion => m_autocomplete;

    private void Awake()
    {
        m_entryPool = new UIPool(m_entryPrefab, m_content);
        m_buttonPool = new UIPool(m_buttonPrefab, m_content);
    }

    public void Setup(GraphLayer layer, Action<GraphLayer> onSubmit)
    {
        if (layer == null)
        {
            layer = new GraphLayer()
            {
                Name = m_name.text,
                Formulas = new System.Collections.Generic.List<NamedFormula>()
            };
        }

        m_layer = layer;
        m_onSubmit = onSubmit;

        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < m_layer.Formulas.Count; ++i)
        {
            var entry = m_entryPool.GetInstance<NamedFormulaEntry>();
            entry.Setup(this, i);
        }

        var btn = m_buttonPool.GetInstance<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(AddNewEntry);
        btn.transform.SetAsLastSibling();

        m_buttonPool.DiscardRest();
        m_entryPool.DiscardRest();
    }

    public void AddNewEntry()
    {
        m_layer.Formulas.Add(new NamedFormula
        {
            Name = "New Formula " + m_layer.Formulas.Count,
            Formula = new Formula(m_autocomplete, "")
        });

        Refresh();
    }

    public void Submit()
    {
        m_layer.Name = m_name.text;
        m_onSubmit?.Invoke(m_layer);

        ForcePopWindow();
    }
}
