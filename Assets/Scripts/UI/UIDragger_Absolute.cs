using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragger_Absolute : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    // 点击时当前UI的位置
    // private Vector2 originalPos;
    // 点击位置偏移量（从 UI中心位置 指向 鼠标箭头(或点击位置) 的向量）
    private Vector2 originalOffset;
    private bool canDrag = true;
    private float longPressTime = 0f;
    private bool isPressed = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (isPressed)
        {
            longPressTime += Time.deltaTime;
        }
    }

    public void SetCanDrag(bool canDrag)
    {
        this.canDrag = canDrag;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag)
            return;
        // 转换屏幕坐标到 UI 世界坐标
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform,                  
            eventData.position,             
            eventData.pressEventCamera,     
            out Vector3 worldPos
        );
        // originalPos = rectTransform.anchoredPosition;
        originalOffset = (Vector2)rectTransform.position - (Vector2)worldPos;

        transform.SetAsLastSibling();
        GetComponent<CanvasGroup>().alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag)
            return;
        // 将屏幕坐标转换为UI局部坐标
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPos
        ))
        {
            // 应用初始偏移量
            rectTransform.position = (Vector2)worldPos - originalOffset;
        }

        // 定义边界（基于父Canvas的尺寸）
        float minX = -canvas.pixelRect.width / 2;
        float maxX = canvas.pixelRect.width / 2;
        float minY = -canvas.pixelRect.height / 2;
        float maxY = canvas.pixelRect.height / 2;

        // 限制位置
        Vector2 clampedPos = rectTransform.anchoredPosition;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minY, maxY);
        // rectTransform.anchoredPosition = clampedPos;
        // transform.position = clampedPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrag)
            return;
        GetComponent<CanvasGroup>().alpha = 1f;

        // 发布一个新的 EndDragEvent 事件
        EventCenter.Publish(new EndDragEvent(this.gameObject));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        longPressTime = 0f;

        EventCenter.Publish(new PointDownEvent(this.gameObject));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        EventCenter.Publish(new PointUpEvent(this.gameObject, longPressTime));
    }



    /// <summary>
    /// 鼠标按下事件
    /// </summary>
    public class PointDownEvent : EventCenter.IEvent
    {
        public GameObject go;

        public PointDownEvent(GameObject go)
        {
            this.go = go;
        }
    }

    /// <summary>
    /// 拖拽结束事件
    /// </summary>
    public class EndDragEvent : EventCenter.IEvent
    {
        public GameObject go;

        public EndDragEvent(GameObject go)
        {
            this.go = go;
        }
    }

    /// <summary>
    /// 鼠标抬起事件
    /// </summary>
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
