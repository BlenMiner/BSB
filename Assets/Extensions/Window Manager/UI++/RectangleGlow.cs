using UnityEngine;
using UnityEngine.EventSystems;
using ThisOtherThing.UI.Shapes;
using static ThisOtherThing.UI.GeoUtils;

public class RectangleGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [GetComponent] Rectangle m_graphic;

    [SerializeField] float m_glowSpeed = 10f;

    [SerializeField] float m_glowSize = 10f;

    [SerializeField, Range(0f, 1f)] float m_glowAlpha = 0.2f;

    float m_glow = 0f;
    
    bool m_isHovering = false;

    private void Awake()
    {
        UpdateGlow(m_glow);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_isHovering = false;
    }

    void UpdateGlow(float glow)
    {
        if (m_graphic.ShadowProperties.Shadows.Length == 0)
            m_graphic.ShadowProperties.Shadows = new ShadowProperties[1];
        
        ShadowProperties newShadow = new ShadowProperties();

        Color graphicColor = m_graphic.ShapeProperties.OutlineColor;
        graphicColor.a = glow * m_glowAlpha;

        newShadow.Color = graphicColor;
        newShadow.Size = m_glowSize;
        
        m_graphic.ShadowProperties.Shadows[0] = newShadow;
        m_graphic.SetAllDirty();
    }

    void Update()
    {
        float newGlow = Mathf.Lerp(m_glow, m_isHovering ? 1f : 0f, Time.deltaTime * m_glowSpeed);

        if      (newGlow > 0.99f) newGlow = 1f;
        else if (newGlow < 0.01f) newGlow = 0f;

        if (newGlow != m_glow)
        {
            m_glow = newGlow;
            UpdateGlow(m_glow);
        }
    }
}
