using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Filters;
using Mapbox.Unity.MeshGeneration.Modifiers;
using UnityEngine;

public class MapboxPolygonDrawer : MonoBehaviour
{
    [SerializeField, Provides] MapboxPolygonDrawer m_provider;

    [Inject] DepartmentDataset m_departmentDataset;

    static MapboxPolygonDrawer m_ref;

    [Inject] AbstractMap m_map;

    [SerializeField] Material m_mat;

    private void Awake()
    {
        m_ref = m_provider;
    }

    static int m_ID = 1;

    static Dictionary<int, Dictionary<string, MaterialPropertyBlock>> m_attributes 
        = new Dictionary<int, Dictionary<string, MaterialPropertyBlock>>();
    
        static Dictionary<int, Dictionary<string, HashSet<MeshRenderer>>> m_attributesListeners 
        = new Dictionary<int, Dictionary<string, HashSet<MeshRenderer>>>();

    public static bool GetMaterialProperties(int id, string department, out MaterialPropertyBlock value)
    {
        department = department.Length == 1 ? "0" + department : department;

        value = null;

        return m_attributes.TryGetValue(id, out var local) && local.TryGetValue(department, out value);
    }

    public static void SetMaterialPropertiesListener(int id, string department, MeshRenderer renderer)
    {
        department = department.Length == 1 ? "0" + department : department;

        if (!m_attributesListeners.ContainsKey(id)) m_attributesListeners.Add(id, new Dictionary<string, HashSet<MeshRenderer>>());

        var local = m_attributesListeners[id];

        if (!local.ContainsKey(department)) local.Add(department, new HashSet<MeshRenderer>());

        local[department].Add(renderer);
    }

    public static void SendUpdate(int id, string department)
    {
        if (m_attributesListeners.TryGetValue(id, out var local) && 
            local.TryGetValue(department, out var value) && 
            GetMaterialProperties(id, department, out var props))
        {
            foreach(var v in value)
            {
                if (v != null) v.SetPropertyBlock(props);
            }
        }
    }

    public static void RemoveMaterialPropertiesListener(MeshRenderer listener)
    {
        foreach(var a in m_attributesListeners)
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

        if (!m_attributes.ContainsKey(geoMaterials.SOME_ID)) m_attributes.Add(geoMaterials.SOME_ID, new Dictionary<string, MaterialPropertyBlock>());

        var localAtrb = m_attributes[geoMaterials.SOME_ID];

        propsId = geoMaterials.SOME_ID;

        m_ref.m_departmentDataset.MapDepCoords((depId, _) => {
            var props = new MaterialPropertyBlock();
            props.SetColor("_Color", default);

            var department = depId.Length == 1 ? "0" + depId : depId;
            if (!localAtrb.ContainsKey(department))
                localAtrb.Add(department, props);
            else localAtrb[department] = props;
        });

        var layer = new VectorSubLayerProperties()
        {
            coreOptions = new CoreVectorLayerProperties() {
                geometryType = VectorPrimitiveType.Polygon,
                isActive = true,
                layerName = "departements-20140306-100m_2-7t6d66",
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
                colliderType = ColliderType.None
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
