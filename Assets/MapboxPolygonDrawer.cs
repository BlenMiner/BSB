using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Filters;
using Mapbox.Unity.MeshGeneration.Modifiers;
using UnityEngine;
using UnityFx.Outline;

public class MapboxPolygonDrawer : MonoBehaviour
{
    [SerializeField, Provides] MapboxPolygonDrawer m_provider;

    [Inject] DepartmentDataset m_departmentDataset;

    [Inject] INSEEDataset m_inseeDataset;

    [Inject] BSB_Dataset m_polution;

    [Inject] OutlineLayerCollection m_outline;

    static MapboxPolygonDrawer m_ref;

    [Inject] AbstractMap m_map;

    [SerializeField] Material m_mat;

    private void Awake()
    {
        m_ref = m_provider;
    }

    static int m_ID = 1;

    static Dictionary<int, Dictionary<int, MaterialPropertyBlock>> m_attributes 
        = new Dictionary<int, Dictionary<int, MaterialPropertyBlock>>();
    
        static Dictionary<int, Dictionary<int, HashSet<MeshRenderer>>> m_attributesListeners 
        = new Dictionary<int, Dictionary<int, HashSet<MeshRenderer>>>();

    static Dictionary<MeshRenderer, int> RendererToINSEE = new Dictionary<MeshRenderer, int>();

    public static bool GetMaterialProperties(int id, int insee, out MaterialPropertyBlock value)
    {
        value = null;

        return m_attributes.TryGetValue(id, out var local) && local.TryGetValue(insee, out value);
    }

    public static bool GetINSEE(MeshRenderer renderer, out int insee)
    {
        return RendererToINSEE.TryGetValue(renderer, out insee);
    }

    public static void SetMaterialPropertiesListener(int id, int insee, MeshRenderer renderer)
    {
        if (ISEEMapSelector.SelectedISEE == insee)
            m_ref.m_outline.GetOrAddLayer(0).Add(renderer.gameObject);

        if (RendererToINSEE.ContainsKey(renderer)) RendererToINSEE[renderer] = insee;
        else RendererToINSEE.Add(renderer, insee);

        if (!m_attributesListeners.ContainsKey(id)) m_attributesListeners.Add(id, new Dictionary<int, HashSet<MeshRenderer>>());

        var local = m_attributesListeners[id];

        if (!local.ContainsKey(insee)) local.Add(insee, new HashSet<MeshRenderer>());

        local[insee].Add(renderer);
    }

    public static void SendUpdate(int id, int insee)
    {
        if (m_attributesListeners.TryGetValue(id, out var local) && 
            local.TryGetValue(insee, out var value) && 
            GetMaterialProperties(id, insee, out var props))
        {
            foreach(var v in value)
            {
                if (v != null)
                {
                    v.SetPropertyBlock(props);
                }
            }
        }
    }

    public static void RemoveMaterialPropertiesListener(MeshRenderer listener)
    {
        m_ref.m_outline.GetOrAddLayer(0).Remove(listener.gameObject);

        RendererToINSEE.Remove(listener);
        foreach (var a in m_attributesListeners)
        {
            foreach(var l in a.Value)
            {
                if (l.Value.Remove(listener))
                    return;
            }
        }
    }

    public static VectorSubLayerProperties AddDepartmentPolygon(out int propsId)
    {
        var geoMaterials = new GeometryMaterialOptions();

        geoMaterials.SOME_ID = m_ID++;
        geoMaterials.style = StyleTypes.Custom;
        geoMaterials.customStyleOptions = new CustomStyleBundle();
        geoMaterials.customStyleOptions.texturingType = UvMapType.Tiled;
        geoMaterials.customStyleOptions.materials[0].Materials[0] = m_ref.m_mat;
        geoMaterials.customStyleOptions.materials[1].Materials[0] = m_ref.m_mat;
        geoMaterials.HasChanged = true;

        if (!m_attributes.ContainsKey(geoMaterials.SOME_ID)) m_attributes.Add(geoMaterials.SOME_ID, new Dictionary<int, MaterialPropertyBlock>());

        var localAtrb = m_attributes[geoMaterials.SOME_ID];

        propsId = geoMaterials.SOME_ID;

        foreach (var m in m_ref.m_polution.INSEECodes)
        {
            if (m_ref.m_inseeDataset.GetINSEE(m, out var v))
            {
                var props = new MaterialPropertyBlock();
                props.SetColor("_Color", default);

                var insee = v.ID;

                if (!localAtrb.ContainsKey(insee))
                    localAtrb.Add(insee, props);
                else localAtrb[insee] = props;
            }
        }

        var layer = new VectorSubLayerProperties()
        {
            coreOptions = new CoreVectorLayerProperties() {
                geometryType = VectorPrimitiveType.Polygon,
                isActive = true,
                layerName = "communes-ile-de-france-8l28yb",
                snapToTerrain = true,
                combineMeshes = false,
                sublayerName = "SUB_POLYGON_FEATURE"
            },
            extrusionOptions = new GeometryExtrusionOptions() {
                extrusionGeometryType = ExtrusionGeometryType.RoofOnly,
                extrusionType = ExtrusionType.AbsoluteHeight,
                maximumHeight = 1000f,
            },
            performanceOptions = new LayerPerformanceOptions() {
                entityPerCoroutine = 20,
                isEnabled = true
            },
            materialOptions = geoMaterials,
            colliderOptions = new ColliderOptions() {
                colliderType = ColliderType.MeshCollider
            },
            lineGeometryOptions =  new LineGeometryOptions()
            {
                CapType = JoinType.Bevel
            }
        };

        m_ref.m_map.VectorData.AddFeatureSubLayer(layer);

        return layer;
    }

    public static void RemovePolygon(VectorSubLayerProperties layer)
    {
        m_ref.m_map.VectorData.RemoveFeatureSubLayer(layer);
        m_attributes.Remove(layer.materialOptions.SOME_ID);

        if (m_attributesListeners.ContainsKey(layer.materialOptions.SOME_ID))
            m_attributesListeners.Remove(layer.materialOptions.SOME_ID);
    }
}
