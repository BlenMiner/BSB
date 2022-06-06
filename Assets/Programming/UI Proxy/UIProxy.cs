using UnityEngine;
using UnityEngine.EventSystems;

public class UIProxy : MonoBehaviour, IPointerClickHandler, IScrollHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler
{
    [SerializeField, Provides] UIProxy provider;

    public bool Dragging {get; private set;} = false;

    bool skipClick = false;

    public event System.Action<Vector2> OnScrollEvent;

    public event System.Action<Vector2> OnClickedScreen;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Dragging = true;
        skipClick = true;
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        Dragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!skipClick) OnClickedScreen?.Invoke(Input.mousePosition);
    }

    public void OnScroll(PointerEventData eventData)
    {
        OnScrollEvent?.Invoke(eventData.scrollDelta);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        skipClick = false;
    }
}
