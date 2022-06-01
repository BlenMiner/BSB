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

        for (int i = 0; i < enumValues.Length; i++)
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

        m_valueFormulaUI.SetText(input.Formula.RawText);
        m_sizeFormulaUI.SetText(input.SizeFormula.RawText);

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

    [SerializeField] FormulaUI m_valueFormulaUI;

    [SerializeField] FormulaUI m_sizeFormulaUI;

    [Header("Formula Colors")]

    [SerializeField] Graphic m_startColor;

    [SerializeField] Graphic m_endColor;

    public static string ToKMB(float num)
    {
        if (num > 999999999 || num < -999999999)
        {
            return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
        }
        else
        if (num > 999999 || num < -999999)
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

    public void Submit()
    {
        Type enumType = typeof(MapType);
        Type enumUnderlyingType = Enum.GetUnderlyingType(enumType);
        Array enumValues = Enum.GetValues(enumType);

        MapType type = (MapType)enumValues.GetValue(m_type.value);

        m_onSubmit?.Invoke(new MapLayer
        {
            Name = m_name.text,
            Formula = m_valueFormulaUI.Formula,
            SizeFormula = m_sizeFormulaUI.Formula,
            MaxColor = m_endColor.color,
            MinColor = m_startColor.color,
            Type = type
        });

        ForcePopWindow();
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
