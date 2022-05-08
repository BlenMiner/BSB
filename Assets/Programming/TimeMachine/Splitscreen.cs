using UnityEngine;
using UnityEngine.EventSystems;

public class Splitscreen : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Provides] Splitscreen m_me;

    [SerializeField] RectTransform m_parent;

    [SerializeField, Range(0, 1)] float m_position;

    public float Value => m_position;

    private void OnValidate()
    {
        Update();
    }

    private void Start()
    {
        m_position = (m_parent.rect.width - 50) / m_parent.rect.width;
    }

    float GetPercentage(PointerEventData data) => Mathf.Min(m_parent.rect.width - 50, Mathf.Max(50, data.position.x)) / m_parent.rect.width;

    public void OnDrag(PointerEventData eventData) { m_position = GetPercentage(eventData); }

    public void OnPointerDown(PointerEventData eventData) { m_position = GetPercentage(eventData); }

    public void OnPointerUp(PointerEventData eventData) { m_position = GetPercentage(eventData); }

    public void Update()
    {
        RectTransform me = transform as RectTransform;
        me.anchoredPosition = new Vector2(m_position * m_parent.rect.width, 0);

        Shader.SetGlobalFloat("_Split", m_position);
    }
}
