using UnityEngine;
using DG.Tweening;

public class UIPanel : MonoBehaviour
{
    RectTransform m_panel;

    [SerializeField] float m_animationDuration = 0.2f;

    [SerializeField] Ease m_animationEase = Ease.InOutElastic;

    [SerializeField] Vector2 m_positionOpen;

    [SerializeField] Vector2 m_positionClosed;

    bool m_open = false;

    void Start()
    {
        m_panel = (RectTransform)transform;
        m_panel.anchoredPosition = m_positionClosed;
    }

    public void TogglePanel(bool show)
    {
        m_open = show;

        m_panel.DOComplete(true);
        m_panel.DOAnchorPos(m_open ? m_positionOpen : m_positionClosed, m_animationDuration).SetEase(m_animationEase);
    }

    public void TogglePanel()
    {
        TogglePanel(!m_open);
    }
}
