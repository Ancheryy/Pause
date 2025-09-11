using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UILongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // 长按判定时间
    [SerializeField] private float _longPressDuration = 3f;
    private bool _isPointerDown;
    private bool _isEndPressed;
    private float _pressTime;

    void Update()
    {
        if (_isPointerDown && Time.time - _pressTime >= _longPressDuration)
        {
            EventCenter.Publish(new EndLongPressEvent(this.gameObject));
            // ResetPress();
        }
        if (_isEndPressed)
        {
            Debug.Log(this.gameObject);
            _isEndPressed = false;
            // 执行松开后的逻辑
            EventCenter.Publish(new EndPressEvent(this.gameObject));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        _pressTime = Time.time;
        EventCenter.Publish(new StartLongPressEvent(this.gameObject));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetPress();
    }

    private void ResetPress()
    {
        _isPointerDown = false;
        _isEndPressed = true;
    }


    /// <summary>
    /// 长按开始事件
    /// </summary>
    public class StartLongPressEvent : EventCenter.IEvent
    {
        public GameObject go;

        public StartLongPressEvent(GameObject go)
        {
            this.go = go;
        }
    }

    /// <summary>
    /// 长按结束事件
    /// </summary>
    public class EndLongPressEvent : EventCenter.IEvent
    {
        public GameObject go;

        public EndLongPressEvent(GameObject go)
        {
            this.go = go;
        }
    }

    /// <summary>
    /// 结束按下事件
    /// </summary>
    public class EndPressEvent : EventCenter.IEvent
    {
        public GameObject go;

        public EndPressEvent(GameObject go)
        {
            this.go = go;
        }
    }
}