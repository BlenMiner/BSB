using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Syrinj;
using UnityEngine;

public class MapCanvasLayers : MonoBehaviour
{
    [Inject] DepartmentDataset m_depDataset;

    [Inject] INSEEDataset m_inseeDataset;

    [Inject] BSB_Dataset m_polution;

    [Inject] AbstractMap m_map;

    [Inject] Camera m_camera;

    [SerializeField] GameObject m_knobMarker;

    [SerializeField] GameObject m_textMarker;

    MapLayer m_layerData;

    RectTransform m_canvas;

    List<MapMarker> m_markers = new List<MapMarker>();

    [Inject] TimeMachine m_timeMachine;

    public float MinVal {get; private set;}

    public float MaxVal {get; private set;}

    VectorSubLayerProperties m_layer;
    
    int m_layerId;

    private void OnEnable()
    {
        m_timeMachine.OnTimeMachineUpdate += TimeUpdated;
        m_layer?.SetActive(true);
    }

    private void TimeUpdated(float obj, float snap)
    {
        m_layerData.Formula.ComputeMin(out var min);
        m_layerData.Formula.ComputeMax(out var max);

        MinVal = min;
        MaxVal = max;
    }

    private void OnDisable()
    {
        m_timeMachine.OnTimeMachineUpdate -= TimeUpdated;
        m_layer?.SetActive(false);
    }

    public void Setup(MapLayer layer)
    {
        m_layerData = layer;

        if (layer.Type == MapType.Area)
            m_layer = MapboxPolygonDrawer.AddDepartmentPolygon(out m_layerId);

        TimeUpdated(0f, 0f);
    }
    
    private void OnDestroy()
    {
        if (m_layer != null)
            MapboxPolygonDrawer.RemovePolygon(m_layer);
    }

    private void Awake()
    {
        m_canvas = GetComponentInParent<RectTransform>();
    }

    private void Start()
    {
        GameObject prefab;

        switch (m_layerData.Type)
        {
            case MapType.Text:
            {
                prefab = m_textMarker;
                break;
            }
            default:
            {
                prefab = m_knobMarker;
                break;
            }
        }

        /*m_inseeDataset.MapDepCoords((insee, LonLat) =>
        {
            SpawnMarker(prefab, LonLat, insee);
        });*/

        foreach (var m in m_polution.INSEECodes)
        {
            if (m_inseeDataset.GetINSEE(m, out var v))
                SpawnMarker(prefab, v.LonLat, v.ID);
        }

        /*m_depDataset.MapDepCoords((depId, coord) => {
            if (m_inseeDataset.GetINSEE(depId, out var v) && !m_polution.INSEECodes.Contains(v.ID))
                SpawnMarker(prefab, coord, v.ID);
        });*/
    }

    public void SetDirty()
    {
        TimeUpdated(m_timeMachine.CurrentPercentage, m_timeMachine.SnapshotPercentage);

        int count = m_markers.Count;
        for (int i = 0; i < count; i++)
        {
            var spawnedObject = m_markers[i];
            spawnedObject.TimeUpdated(m_timeMachine.CurrentPercentage, m_timeMachine.SnapshotPercentage);
        }
    }

    public MapMarker SpawnMarker(GameObject prefab, Vector2d longLat, int postalCode)
    {
        var go = Instantiate(prefab);
        go.transform.SetParent(transform, false);

        MapMarker marker = go.GetComponent<MapMarker>();

        marker.Setup(m_layer == null ? -1 : m_layerId, this, m_layerData, longLat, postalCode);
        m_markers.Add(marker);
        return marker;
    }

    private void LateUpdate()
    {
        int count = m_markers.Count;
        for (int i = 0; i < count; i++)
        {
            var spawnedObject = m_markers[i];
            var mapPos = m_map.GeoToWorldPosition(spawnedObject.LongLat, true);

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(m_camera, mapPos);
            RectTransform point = spawnedObject.transform as RectTransform;

            point.anchoredPosition = screenPoint - m_canvas.rect.size / 2f;
        }
    }
}
