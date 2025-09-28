using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class SpriteButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("按钮事件")]
    public UnityEvent onClick;           // 点击事件
    public UnityEvent onPointerEnter;    // 鼠标进入
    public UnityEvent onPointerExit;     // 鼠标离开
    
    [Header("按钮设置")]
    [SerializeField] private bool isInteractable = true;
    
    [Header("视觉反馈")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    // [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color disabledColor = Color.white;

    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) return;
        if (!IsOverUI()) return;
        
        onClick?.Invoke();  // 只触发这个按钮的点击事件
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        _spriteRenderer.color = hoverColor;
        onPointerEnter?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable) return;
        
        _spriteRenderer.color = normalColor;
        onPointerExit?.Invoke();
    }

    private bool IsOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        _spriteRenderer.color = interactable ? normalColor : disabledColor;
    }
}