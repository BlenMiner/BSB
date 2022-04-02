using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPP : Button
{
    public ButtonClickedEvent onDoubleClick;

    public ButtonClickedEvent onRightClick;
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            eventData.Use();
            onDoubleClick?.Invoke();
            return;
        }
        else if (eventData.clickCount == 1 && eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.Use();
            onRightClick?.Invoke();
            return;
        }
        
        base.OnPointerClick(eventData);
    }
}
