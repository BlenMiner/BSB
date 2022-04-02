using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Canvas))]
public abstract class WindowBehaviour : MonoBehaviour
{
    [SerializeField] bool m_autoAssignBackground = true;

    [SerializeField] bool m_backgroundClosesWindow = true;

    public event System.Action onWindowClose;

    public Canvas Canvas { get; private set; }

    public CanvasGroup CanvasGroup { get; private set; }

    protected bool CanExitWindow = true;

    protected WindowManager WindowManager;

    public void PreAwake(WindowManager wm)
    {
        Canvas = GetComponent<Canvas>();
        CanvasGroup = GetComponent<CanvasGroup>();

        if (CanvasGroup == null) CanvasGroup = gameObject.AddComponent<CanvasGroup>();

        CanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.InOutSine);
        WindowManager = wm;

        if (m_autoAssignBackground)
        {
            GameObject background = new GameObject("Background",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement)
            );

            background.transform.SetParent(transform, false);
            background.transform.SetAsFirstSibling();

            var rect = background.GetComponent<RectTransform>();
            
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            var img = background.GetComponent<Image>();

            img.color = new Color(0, 0, 0, 0.5f);

            var btn = background.GetComponent<Button>();

            if (m_backgroundClosesWindow)
                btn.onClick.AddListener(() => { PopWindow(); });

            var layout = background.GetComponent<LayoutElement>();

            layout.ignoreLayout = true;
        }
    }

    public void SetCanExit(bool canExit)
    {
        CanExitWindow = canExit;
    }

    public bool PopWindow()
    {
        if (CanExitWindow)
        {
            ForcePopWindow();
            return true;
        }
        else return false;
    }

    public void ForcePopWindow()
    {
        if (CanvasGroup != null)
        {
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InOutSine).onComplete = () => {
                Destroy(gameObject);
            };
        }
        else
        {
            Destroy(gameObject);
        }

        WindowManager.Pop(this);
        onWindowClose?.Invoke();
    }
}
