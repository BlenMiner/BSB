using UnityEngine;
using UnityEngine.EventSystems;

public class UIProxy : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IScrollHandler
{
    [SerializeField, Provides] UIProxy provider;

    public bool Dragging {get; private set;} = false;

    public event System.Action<Vector2> OnScrollEvent;

    public void OnPointerDown(PointerEventData eventData)
    {
        Dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Dragging = false;
    }

    public void OnScroll(PointerEventData eventData)
    {
        OnScrollEvent?.Invoke(eventData.scrollDelta);
    }
}
