using System.Collections;
using System.Collections.Generic;
using ThisOtherThing.UI.Shapes;
using TMPro;
using UnityEngine;

public class FormulaUI : MonoBehaviour
{
    [SerializeField] TMP_Text m_errorText;

    [SerializeField] TMP_Text m_minText;

    [SerializeField] TMP_Text m_maxText;

    [SerializeField] TMP_InputField m_formulaInput;

    [SerializeField] GameObject m_minmaxContainer;

    [SerializeField] Rectangle m_forumlaBox;

    [SerializeField] Color m_validForumlaColor = Color.green, m_invalidForumlaColor = Color.red;

    [Inject] DatasetAutocompletion m_autoCompletionProvider;

    public Formula Formula { get; private set; }

    void OnEnable()
    {
        if (Formula == null)
            Formula = new Formula(m_autoCompletionProvider, "", "1");
        m_formulaInput.onValueChanged
            .AddListener(OnFormulaUpdated);

        m_errorText.gameObject.SetActive(false);
        m_minmaxContainer.SetActive(true);
    }

    void OnDisable()
    {
        m_formulaInput.onValueChanged
            .RemoveListener(OnFormulaUpdated);
    }

    void OnFormulaUpdated(string value)
    {
        UpdateFormula(value);
    }

    public void SetText(string value)
    {
        m_formulaInput.text = value;
        UpdateFormula(value);
    }

    bool UpdateFormula(string value)
    {
        Formula = new Formula(m_autoCompletionProvider, value, "1");
        bool validSyntax = Formula.IsValid;

        m_forumlaBox.ShapeProperties.OutlineColor =
            validSyntax ? m_validForumlaColor : m_invalidForumlaColor;

        if (!validSyntax)
        {
            m_errorText.text = Formula.Error;
            m_errorText.gameObject.SetActive(true);
            m_minmaxContainer.SetActive(false);
        }
        else
        {
            m_errorText.text = string.Empty;
            m_errorText.gameObject.SetActive(false);
            m_minmaxContainer.SetActive(true);
        }

        m_forumlaBox.SetAllDirty();

        UpdateMinMaxValue();

        return validSyntax;
    }

    private void UpdateMinMaxValue()
    {
        if (Formula != null && Formula.IsValid)
        {
            if (Formula.ComputeMin(out var min))
                m_minText.text = $"<color=grey>Min Value</color> {AddNewLayerWindow.ToKMB(min)}";
            else m_minText.text = "<color=grey>Min Value</color> -";

            if (Formula.ComputeMax(out var max))
                m_maxText.text = $"{AddNewLayerWindow.ToKMB(max)} <color=grey>Max Value</color>";
            else m_maxText.text = "- <color=grey>Max Value</color>";
        }
    }
}
