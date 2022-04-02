using System;
using System.Collections.Generic;
using System.Globalization;
using ThisOtherThing.UI.Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum MapType
{
    Knob,
    Heatmap,
    Text,
    Area
}

public struct RawColor
{
    public float r, g, b, a;

    public RawColor(Color c)
    {
        r = c.r;
        g = c.g;
        b = c.b;
        a = c.a;
    }

    public Color ToColor()
    {
        return new Color(r, g, b, a);
    }
}

[System.Serializable]
public class MapLayerRawRaw
{
    public string Name;

    public string Formula;

    public string SizeFormula;

    public RawColor MinColor, MaxColor;

    public MapType Type;

    public MapLayer ToLayer(DatasetAutocompletion datasetInfo)
    {
        return new MapLayer()
        {
            Name = Name,
            Formula = new Formula(datasetInfo, Formula),
            SizeFormula = new Formula(datasetInfo, SizeFormula),
            MinColor = MinColor.ToColor(),
            MaxColor = MaxColor.ToColor(),
            Type = Type
        };
    }
}

[System.Serializable]
public class MapLayerRaw
{
    public string Name;

    public string Formula;

    public string SizeFormula;

    public Color MinColor, MaxColor;

    public MapType Type;

    public MapLayer ToLayer(DatasetAutocompletion datasetInfo)
    {
        return new MapLayer()
        {
            Name = Name,
            Formula = new Formula(datasetInfo, Formula),
            SizeFormula = new Formula(datasetInfo, SizeFormula),
            MinColor = MinColor,
            MaxColor = MaxColor,
            Type = Type
        };
    }

    public MapLayerRawRaw ToRawRaw()
    {
        return new MapLayerRawRaw()
        {
            Name = Name,
            Formula = Formula,
            SizeFormula = SizeFormula,
            MinColor = new RawColor(MinColor),
            MaxColor = new RawColor(MaxColor),
            Type = Type
        };
    }
}

public class MapLayer
{
    public string Name;

    public Formula Formula;

    public Formula SizeFormula;

    public Color MinColor, MaxColor;

    public MapType Type;
    
    public MapLayerRaw ToRawLayer()
    {
        return new MapLayerRaw()
        {
            Name = Name,
            Formula = Formula.RawText,
            SizeFormula = SizeFormula.RawText,
            MinColor = MinColor,
            MaxColor = MaxColor,
            Type = Type
        };
    }
}

public class AddNewLayerWindow : WindowBehaviour
{
    [Inject] TimeMachine m_timeMachine;

    [Inject] DatasetAutocompletion m_autoCompletionProvider;

    Action<MapLayer> m_onSubmit;

    void SetupOpts()
    {
        var opts = new List<TMP_Dropdown.OptionData>();

        Type enumType = typeof(MapType);
        Array enumValues = Enum.GetValues(enumType);

        for (int i=0; i < enumValues.Length; i++)
        {
            // Retrieve the value of the ith enum item.
            string value = enumValues.GetValue(i).ToString();
            opts.Add(new TMP_Dropdown.OptionData(value));
        }
        m_type.options = opts;
    }

    internal void Setup(MapLayer input, Action<MapLayer> layer)
    {
        SetupOpts();

        m_type.interactable = false;

        m_type.value = (int)input.Type;
        m_startColor.color = input.MinColor;
        m_endColor.color = input.MaxColor;
        m_name.text = input.Name;
        m_formulaInput.text = input.Formula.RawText;
        m_sizeFormulaInput.text = input.SizeFormula.RawText;

        m_onSubmit = layer;
    }

    internal void Setup(Action<MapLayer> layer)
    {
        SetupOpts();

        m_onSubmit = layer;
    }

    [Header("General settings")]
    [SerializeField] TMP_Dropdown m_type;

    [SerializeField] TMP_InputField m_name;

    [Header("Formula Settings")]

    [SerializeField] TMP_Text m_errorText;

    [SerializeField] TMP_Text m_minText;

    [SerializeField] TMP_Text m_maxText;

    [SerializeField] TMP_InputField m_formulaInput;

    [SerializeField] Rectangle m_forumlaBox;

    [SerializeField] Color m_validForumlaColor = Color.green, m_invalidForumlaColor = Color.red;

    [Header("Size Formula Settings")]

    [SerializeField] TMP_Text m_sizeErrorText;

    [SerializeField] TMP_Text m_sizeMinText;

    [SerializeField] TMP_Text m_sizeMaxText;

    [SerializeField] TMP_InputField m_sizeFormulaInput;

    [SerializeField] Rectangle m_sizeForumlaBox;

    [Header("Formula Colors")]

    [SerializeField] Graphic m_startColor;
    
    [SerializeField] Graphic m_endColor;
 
    public static string ToKMB(float num)
    {
        if (num > 999999999 || num < -999999999 )
        {
            return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
        }
        else
        if (num > 999999 || num < -999999 )
        {
            return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
        }
        else
        if (num > 999 || num < -999)
        {
            return num.ToString("0,.#K", CultureInfo.InvariantCulture);
        }
        else
        {
            return num.ToString(CultureInfo.InvariantCulture);
        }
    }

    private void OnEnable()
    {
        m_formulaInput.onValueChanged
            .AddListener(OnFormulaUpdated);

        m_sizeFormulaInput.onValueChanged
            .AddListener(OnSizeFormulaUpdated);
        
        //m_timeMachine.OnTimeMachineUpdate += OnTimeUpdated;
    }

    private void OnTimeUpdated(float _)
    {
        if (m_formula != null && m_formula.IsValid)
        {
            if (m_formula.ComputeMin(out var min))
                 m_minText.text = $"<color=grey>Min Value</color> {ToKMB(min)}";
            else m_minText.text = "<color=grey>Min Value</color> -";

            if (m_formula.ComputeMax(out var max))
                 m_maxText.text = $"{ToKMB(max)} <color=grey>Max Value</color>";
            else m_maxText.text = "- <color=grey>Max Value</color>";
        }

        if (m_sizeFormula != null && m_sizeFormula.IsValid)
        {
            if (m_sizeFormula.ComputeMin(out var min))
                 m_sizeMinText.text = $"<color=grey>Min Value</color> {ToKMB(min)}";
            else m_sizeMinText.text = "<color=grey>Min Value</color> -";

            if (m_sizeFormula.ComputeMax(out var max))
                 m_sizeMaxText.text = $"{ToKMB(max)} <color=grey>Max Value</color>";
            else m_sizeMaxText.text = "- <color=grey>Max Value</color>";
        }
    }

    private void Start()
    {
        OnFormulaUpdated(m_formulaInput.text);
        OnSizeFormulaUpdated(m_sizeFormulaInput.text);
    }

    private void OnDisable()
    {
        m_formulaInput.onValueChanged
            .RemoveListener(OnFormulaUpdated);

        m_sizeFormulaInput.onValueChanged
            .RemoveListener(OnSizeFormulaUpdated);
    }

    public void Submit()
    {
        Type enumType = typeof(MapType);
        Type enumUnderlyingType = Enum.GetUnderlyingType(enumType);
        Array enumValues = Enum.GetValues(enumType);

        MapType type = (MapType)enumValues.GetValue(m_type.value);

        m_onSubmit?.Invoke(new MapLayer {
            Name = m_name.text,
            Formula = m_formula,
            SizeFormula = m_sizeFormula,
            MaxColor = m_endColor.color,
            MinColor = m_startColor.color,
            Type = type
        });

        ForcePopWindow();
    }

    Formula m_formula;

    Formula m_sizeFormula;

    bool UpdateFormula(string value)
    {
        m_formula = new Formula(m_autoCompletionProvider, value);
        return m_formula.IsValid;
    }

    bool UpdateSizeFormula(string value)
    {
        m_sizeFormula = new Formula(m_autoCompletionProvider, value, "1");
        return m_sizeFormula.IsValid;
    }

    void OnFormulaUpdated(string value)
    {
        bool validSyntax = UpdateFormula(value);

        m_forumlaBox.ShapeProperties.OutlineColor =
            validSyntax ? m_validForumlaColor : m_invalidForumlaColor;

        if (!validSyntax) m_errorText.text = m_formula.Error;
        else m_errorText.text = string.Empty;
        
        m_forumlaBox.SetAllDirty();

        if (validSyntax) OnTimeUpdated(m_timeMachine.CurrentPercentage);
    }

    void OnSizeFormulaUpdated(string value)
    {
        bool validSyntax = UpdateSizeFormula(value);

        m_sizeForumlaBox.ShapeProperties.OutlineColor =
            validSyntax ? m_validForumlaColor : m_invalidForumlaColor;

        if (!validSyntax) m_sizeErrorText.text = m_formula.Error;
        else m_sizeErrorText.text = string.Empty;
        
        m_sizeForumlaBox.SetAllDirty();

        if (validSyntax) OnTimeUpdated(m_timeMachine.CurrentPercentage);
    }

    public void ChangeStartColor()
    {
        var colorPicker = WindowManager.LastInstance.Push<ColorPickerWindow>();
        colorPicker.Setup(m_startColor.color, (clr) => m_startColor.color = clr);
    }

    public void ChangeEndColor()
    {
        var colorPicker = WindowManager.LastInstance.Push<ColorPickerWindow>();
        colorPicker.Setup(m_endColor.color, (clr) => m_endColor.color = clr);
    }
}
