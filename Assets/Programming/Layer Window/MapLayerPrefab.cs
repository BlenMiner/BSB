using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapLayerPrefab : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TMPro.TMP_Text m_label;

    MapLayer m_ref;

    MapLayerManager m_mgr;

    bool m_enabled = true;

    internal void Setup(MapLayerManager mgr, MapLayer mapLayer)
    {
        m_ref = mapLayer;
        m_mgr = mgr;

        m_label.text = mapLayer.Name;
    }

    public void ToggleVisibility()
    {
        m_enabled = !m_enabled;

        m_label.fontStyle = m_enabled ? TMPro.FontStyles.Normal : TMPro.FontStyles.Strikethrough;

        m_mgr.GetLayer(m_ref).gameObject.SetActive(m_enabled);
    }

    public void Delete()
    {
        m_mgr.RemoveLayer(m_ref);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount >= 2)
        {
            AddNewLayerWindow window = WindowManager.LastInstance.Push<AddNewLayerWindow>();

            window.Setup(m_ref, layer => {
                m_ref.Name = layer.Name;
                m_ref.Formula = layer.Formula;
                m_ref.SizeFormula = layer.SizeFormula;
                m_ref.MinColor = layer.MinColor;
                m_ref.MaxColor = layer.MaxColor;
                m_ref.Type = layer.Type;

                m_mgr.GetLayer(m_ref).SetDirty();
                Setup(m_mgr, m_ref);
                m_mgr.SaveLayers();
            });
        }
    }
}
