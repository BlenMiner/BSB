using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRotateImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] RectTransform m_target;

    [SerializeField] float m_angle = 90f;
    
    Quaternion m_startRotation;

    bool m_hovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_hovering = false;
    }

    void Start()
    {
        m_startRotation = m_target.localRotation;
    }

    private void Update()
    {
        Quaternion rotation = m_target.localRotation;
        Quaternion target;

        if (m_hovering)
             target = m_startRotation * Quaternion.Euler(0, 0, m_angle);
        else target = m_startRotation;

        rotation = Quaternion.Lerp(rotation, target, Time.deltaTime * 10f);

        if (rotation != transform.localRotation)
            m_target.localRotation = rotation;
    }
}
