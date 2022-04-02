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

    private void OnEnable()
    {
        onWindowClose += OnWindowClose;
        m_colorPicker.onColorChanged += ColorChanged;
    }

    private void ColorChanged(Color obj)
    {
        m_currentColor.color = obj;
    }

    private void OnDisable()
    {
        onWindowClose -= OnWindowClose;
        m_colorPicker.onColorChanged -= ColorChanged;
    }

    private void OnWindowClose()
    {
        m_onColorUpdated?.Invoke(m_colorPicker.color);
    }

    Action<Color> m_onColorUpdated;

    public void Setup(Color start, Action<Color> onColorUpdated)
    {
        m_onColorUpdated = onColorUpdated;

        m_currentColor.color = start;
        m_originalColor.color = start;
        m_colorPicker.color = start;

    }
}
