using Mapbox.Utils;
using ThisOtherThing.UI.Shapes;
using UnityEngine;
using UnityEngine.UI;

public class MapMarker : MonoBehaviour
{
    [SerializeField] Graphic m_shape;
    
    [Inject] TimeMachine m_timeMachine;

    MapLayer m_layerData;

    Vector2d m_pos;

    int m_postalCode;

    public Vector2d LongLat => m_pos;

    MapCanvasLayers m_parent;

    MaterialPropertyBlock m_material;

    int m_layerId = -1;

    public void Setup(int layerId, MapCanvasLayers parent, MapLayer layer, Vector2d longLat, int postalCode)
    {
        m_layerId = layerId;

        if (layerId > 0) MapboxPolygonDrawer.GetMaterialProperties(layerId, Dataset.GetDepartmentID(postalCode), out m_material);

        m_parent = parent;
        m_layerData = layer;
        m_pos = longLat;
        m_postalCode = postalCode;

        TimeUpdated(m_timeMachine.CurrentPercentage);

        if (layer.Type == MapType.Area)
            m_shape.enabled = false;
        
        if (layer.Type == MapType.Heatmap)
            m_shape.color = default;
    }

    private void OnEnable()
    {
        m_timeMachine.OnTimeMachineUpdate += TimeUpdated;
    }

    public void TimeUpdated(float time)
    {
        if (m_layerData.Formula.Compute(m_postalCode, time, out var value))
        {
            var minv = m_parent.MinVal;
            var maxv = m_parent.MaxVal;

            float normalizedValue = (value - minv) / (maxv - minv);
            var color = Color.Lerp(m_layerData.MinColor, m_layerData.MaxColor, normalizedValue);

            if (m_layerData.Type == MapType.Heatmap)
            {
                Ellipse knob = (Ellipse)m_shape;
                knob.ShadowProperties.Shadows[0].Color = new Color(color.r, color.g, color.b, 0.5f);
                knob.SetAllDirty();
            }
            else m_shape.color = color;

            if (m_layerData.SizeFormula.Compute(m_postalCode, time, out var scale))
                m_shape.transform.localScale = Vector3.one * scale;

            if (m_layerData.Type == MapType.Text)
            {
                TMPro.TMP_Text text = (TMPro.TMP_Text)m_shape;
                text.text = AddNewLayerWindow.ToKMB(value);
            }

            if (m_material != null)
                m_material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.5f));

            if (m_layerData.Type != MapType.Area && !m_shape.enabled)
                m_shape.enabled = true;
        }
        else if (m_shape.enabled)
        {
            m_shape.enabled = false;

            if (m_material != null)
                m_material.SetColor("_Color", default);
        }

        if (m_material != null)
            MapboxPolygonDrawer.SendUpdate(m_layerId, Dataset.GetDepartmentID(m_postalCode));
    }

    private void OnDisable()
    {
        m_timeMachine.OnTimeMachineUpdate -= TimeUpdated;
    }
}
