using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerWindow : WindowBehaviour
{
    [SerializeField] ColorPicker m_colorPicker;

    [SerializeField] Graphic m_originalColor;

    [SerializeField] Graphic m_currentColor;

    [SerializeField] Slider m_alpha;

    private void OnEnable()
    {
        onWindowClose += OnWindowClose;
        m_colorPicker.onColorChanged += ColorChanged;
    }

    private void ColorChanged(Color obj)
    {
        m_currentColor.color = new Color(obj.r, obj.g, obj.b);
    }

    private void OnDisable()
    {
        onWindowClose -= OnWindowClose;
        m_colorPicker.onColorChanged -= ColorChanged;
    }

    private void OnWindowClose()
    {
        var c = m_colorPicker.color;
        c.a = m_alpha.value;
        m_onColorUpdated?.Invoke(c);
    }

    Action<Color> m_onColorUpdated;

    public void Setup(Color start, Action<Color> onColorUpdated)
    {
        m_alpha.value = start.a;
        start.a = 1f;
        m_onColorUpdated = onColorUpdated;

        m_currentColor.color = start;
        m_originalColor.color = start;
        m_colorPicker.color = start;

    }
}
