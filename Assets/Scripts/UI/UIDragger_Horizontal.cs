using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragger_Horizontal : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    // 点击时当前UI的位置
    private Vector2 originalPos;
    // 点击位置偏移量（从 UI中心位置 指向 鼠标箭头(或点击位置) 的向量）
    private Vector2 originalOffset;
    // 固定Y轴位置
    private float originalY;            
    private bool canDrag = true;
    private float longPressTime = 0f;
    private bool isPressed = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalY = rectTransform.anchoredPosition.y; // 记录初始Y位置
    }

    void Update()
    {
        if (isPressed)
        {
            longPressTime += Time.deltaTime;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag)
            return;
        // 计算点击位置与物体中心的偏移
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out originalOffset
        );
        originalPos = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        // 将屏幕坐标转换为UI局部坐标
        Vector2 localPointerPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPos
        ))
        {
            // 应用初始偏移量
            rectTransform.anchoredPosition = originalPos + (localPointerPos - originalOffset);
        }

        // 计算边界限制（考虑元素实际宽度）
        float canvasWidth = canvas.pixelRect.width;
        float elementWidth = rectTransform.rect.width * rectTransform.lossyScale.x;

        float minX = canvas.pixelRect.width / 2 - rectTransform.rect.width / 2;
        float maxX = rectTransform.rect.width / 2 - canvas.pixelRect.width / 2;

        // 限制位置
        Vector2 clampedPos = rectTransform.anchoredPosition;
        clampedPos.y = originalY; // 固定Y轴
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        rectTransform.anchoredPosition = clampedPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrag) return;
        EventCenter.Publish(new EndDragEvent(gameObject));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        longPressTime = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        EventCenter.Publish(new PointUpEvent(gameObject, longPressTime));
    }

    public class EndDragEvent : EventCenter.IEvent
    {
        public GameObject go;
        public EndDragEvent(GameObject go) => this.go = go;
    }

    public class PointUpEvent : EventCenter.IEvent
    {
        public GameObject go;
        public float pressTime;
        public PointUpEvent(GameObject go, float time)
        {
            this.go = go;
            this.pressTime = time;
        }
    }
}