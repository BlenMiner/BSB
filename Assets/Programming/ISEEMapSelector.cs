using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class ISEEMapSelector : MonoBehaviour
{
    public static int SelectedISEE;

    public static event Action SelectedISEEChanged;

    [Inject] Camera m_camera;

    [Inject] UIProxy m_proxy;

    [Inject] OutlineLayerCollection m_outline;

    private void OnEnable()
    {
        m_proxy.OnClickedScreen += ScreenClick;
        SelectedISEE = -1;
    }

    private void ScreenClick(Vector2 pos)
    {
        var ray = m_camera.ScreenPointToRay(new Vector3(pos.x, pos.y, 1f));
        if (Physics.Raycast(ray, out var hit))
        {
            MeshRenderer mr = hit.transform.GetComponentInChildren<MeshRenderer>();
            if (MapboxPolygonDrawer.GetINSEE(mr, out var insee) && INSEEDataset.me.GetINSEE(insee, out var inseeData))
            {
                m_outline.GetOrAddLayer(0).Clear();
                m_outline.GetOrAddLayer(0).Add(mr.gameObject);

                SelectedISEE = insee;
                SelectedISEEChanged?.Invoke();
            }
        }
    }

    private void Update()
    {
        var pos = Input.mousePosition;
        var ray = m_camera.ScreenPointToRay(new Vector3(pos.x, pos.y, 1f));

        if (Physics.Raycast(ray, out var hit))
        {
            MeshRenderer mr = hit.transform.GetComponentInChildren<MeshRenderer>();
            if (MapboxPolygonDrawer.GetINSEE(mr, out var insee) && INSEEDataset.me.GetINSEE(insee, out var inseeData))
            {
                m_outline.GetOrAddLayer(1).Clear();
                m_outline.GetOrAddLayer(1).Add(mr.gameObject);
            }
        }
    }

    private void OnDisable()
    {
        m_proxy.OnClickedScreen -= ScreenClick;
    }
}
