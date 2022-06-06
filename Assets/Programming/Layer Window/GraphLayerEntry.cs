using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphLayerEntry : MonoBehaviour
{
    public LineChartScript Chart;

    GraphLayerManager m_manager;

    int m_layer;

    public void Setup(GraphLayerManager manager, int layer)
    {
        m_layer = layer;
        m_manager = manager;
    }

    public void Delete()
    {
        m_manager.RemoveAt(m_layer);
    }

    public void Edit()
    {
        m_manager.Edit(m_layer);
    }
}
