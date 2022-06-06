using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NamedFormulaEntry : MonoBehaviour
{
    [SerializeField] TMP_InputField m_name;
    [SerializeField] TMP_InputField m_formula;

    GraphEditorWindow m_parent;
    int m_formulaID;

    private void OnEnable()
    {
        m_name.onValueChanged.AddListener(NameDirty);
        m_formula.onValueChanged.AddListener(FormulaDirty);
    }

    private void OnDisable()
    {
        m_name.onValueChanged.RemoveListener(NameDirty);
        m_formula.onValueChanged.RemoveListener(FormulaDirty);
    }

    void NameDirty(string name)
    {
        m_parent.Layer.Formulas[m_formulaID].Name = name;
    }

    void FormulaDirty(string formula)
    {
        m_parent.Layer.Formulas[m_formulaID].Formula = new Formula(m_parent.Autocompletion, formula);
    }

    public void Setup(GraphEditorWindow parent, int formulaID)
    {
        m_parent = parent;
        m_formulaID = formulaID;

        var f = m_parent.Layer.Formulas[m_formulaID];

        m_name.SetTextWithoutNotify(f.Name);
        m_formula.SetTextWithoutNotify(f.Formula.RawText);
    }

    public void Delete()
    {
        m_parent.Layer.Formulas.RemoveAt(m_formulaID);
        m_parent.Refresh();
    }
}
