using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dragger : MonoBehaviour
{
    // 拖拽设置
    [Header("拖拽设置")]
    public bool enableDrag = true;
    public bool useRigidbody = true;            // 是否使用物理系统
    public float dragSpeed = 20f;               // 为 0 时直接跟随移动；不为零时平滑移动
    public bool returnToStartPosition = false;
    public bool snapToGrid = false;
    public float gridSize = 1f;
    
    // 限制设置
    [Header("限制设置")]
    public bool limitX = true;
    public float minX = -2.5f;
    public float maxX = 2.5f;
    
    public bool limitY = true;
    public float minY = -5f;
    public float maxY = 5f;
    
    // 事件
    [Header("事件")]
    public bool enableEvents = true;
    
    // 私有变量
    private Vector3 startPosition;
    private Vector3 offset;
    private bool isDragging = false;
    private new Camera camera;
    private Rigidbody2D rb;
    private Collider2D col;
    
    void Start()
    {
        camera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        startPosition = transform.position;
        
        // 如果没有Collider2D，自动添加一个
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            Debug.LogWarning("自动添加了BoxCollider2D组件到 " + gameObject.name);
        }
    }
    
    void OnMouseDown()
    {
        if (!enableDrag) return;
        if (!IsClickOnObject()) return;
        
        StartDragging();
    }
    
    void OnMouseDrag()
    {
        if (!isDragging || !enableDrag) return;
        
        Dragging();
    }
    
    void OnMouseUp()
    {
        if (isDragging)
        {
            StopDragging();
        }
    }
    
    private bool IsClickOnObject()
    {
        // 检测鼠标是否点击在物体的Collider上
        Vector2 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }
    
    private void StartDragging()
    {
        isDragging = true;
        transform.SetAsLastSibling();
        
        // 计算偏移量
        Vector3 mouseWorldPos = camera.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mouseWorldPos;
        offset.z = 0; // 确保z坐标为0
        
        // 如果是物理对象，暂停物理模拟
        if (useRigidbody && rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
        
        // 触发开始拖拽事件
        if (enableEvents)
            EventCenter.Publish(new OnDragStartEvent(this.gameObject));
    }
    
    private void Dragging()
    {
        // 获取鼠标世界坐标
        Vector3 mouseWorldPos = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // 确保z坐标为0
        
        // 计算目标位置
        Vector3 targetPosition = mouseWorldPos + offset;
        
        // 应用网格吸附
        if (snapToGrid)
        {
            targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
            targetPosition.y = Mathf.Round(targetPosition.y / gridSize) * gridSize;
        }
        
        // 应用位置限制
        targetPosition = ApplyPositionLimits(targetPosition);
        
        // 移动物体
        if (useRigidbody && rb != null)
        {
            // 使用物理移动
            rb.MovePosition(targetPosition);
        }
        else
        {
            // 直接移动或平滑移动
            if (dragSpeed > 0)
                transform.position = Vector3.Lerp(transform.position, targetPosition, dragSpeed * Time.deltaTime);
            else
                transform.position = targetPosition;
        }
        
        // 触发拖拽中事件
        if (enableEvents)
            EventCenter.Publish(new OnDragEvent(this.gameObject));
    }
    
    private void StopDragging()
    {
        isDragging = false;
        
        // 恢复物理模拟
        if (useRigidbody && rb != null)
        {
            rb.isKinematic = false;
        }
        
        // 如果需要返回到起始位置
        if (returnToStartPosition)
        {
            StartCoroutine(ReturnToStartPosition());
        }
        
        // 触发结束拖拽事件
        if (enableEvents)
            EventCenter.Publish(new OnDragEndEvent(this.gameObject));
    }
    
    private Vector3 ApplyPositionLimits(Vector3 position)
    {
        if (limitX)
            position.x = Mathf.Clamp(position.x, minX, maxX);
        
        if (limitY)
            position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }
    
    private IEnumerator ReturnToStartPosition()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, startPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = startPosition;
    }
    
    // 公共方法用于外部控制
    public void SetDraggingEnabled(bool enabled)
    {
        enableDrag = enabled;
        if (!enabled && isDragging)
            StopDragging();
    }
    
    public bool IsDragging()
    {
        return isDragging;
    }
    
    public void ResetToStartPosition()
    {
        transform.position = startPosition;
    }
    
    // 在Inspector中显示调试信息
    void OnDrawGizmosSelected()
    {
        // 绘制限制区域
        if (limitX || limitY)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            
            Vector3 center = new Vector3(
                limitX ? (minX + maxX) / 2f : transform.position.x,
                limitY ? (minY + maxY) / 2f : transform.position.y,
                transform.position.z
            );
            
            Vector3 size = new Vector3(
                limitX ? maxX - minX : 10f,
                limitY ? maxY - minY : 10f,
                0.1f
            );
            
            Gizmos.DrawWireCube(center, size);
        }
    }



    public class OnDragStartEvent : EventCenter.IEvent
    {
        public GameObject GameObject;

        public OnDragStartEvent(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }
    
    public class OnDragEvent : EventCenter.IEvent
    {
        public GameObject GameObject;

        public OnDragEvent(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }
    
    public class OnDragEndEvent : EventCenter.IEvent
    {
        public GameObject GameObject;

        public OnDragEndEvent(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }
    
}
