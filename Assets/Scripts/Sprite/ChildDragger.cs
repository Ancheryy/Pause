using System.Collections;
using UnityEngine;

public class ChildDragger : MonoBehaviour
{
    [Header("拖拽设置")]
    public bool enableDrag = true;
    public float dragSpeed = 20f;
    public bool returnToParent = true;
    
    private Vector3 _localStartPosition; // 相对于父物体的初始位置
    private Vector3 _offset;
    private bool _isDragging = false;
    private Camera _camera;
    private Collider2D _col;
    private Transform _parentTransform;
    // 拖拽的目标区域
    private Collider2D _attachableZoneCol;
    private bool _canAttach = false;
    
    void Start()
    {
        _camera = Camera.main;
        _col = GetComponent<Collider2D>();
        this._parentTransform = transform.parent;
        _localStartPosition = transform.localPosition;
        
        if (_col == null)
        {
            _col = gameObject.AddComponent<BoxCollider2D>();
        }
        // 将子物体设置为更上层的渲染顺序
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // renderer.sortingOrder = 1;  // 子物体在上层
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
        if (!_isDragging || !enableDrag) return;
        Dragging();
    }
    
    void OnMouseUp()
    {
        if (_isDragging) StopDragging();
    }
    
    private bool IsClickOnObject()
    {
        Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
    
        // 按渲染顺序或特定条件排序，确保最上层的对象优先
        System.Array.Sort(hits, (a, b) => 
            b.collider.bounds.size.magnitude.CompareTo(a.collider.bounds.size.magnitude));
    
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == gameObject)
            {
                return true;
            }
        }
        return false;
    }
    
    private void StartDragging()
    {
        _isDragging = true;
        EventCenter.Publish(new StartDraggingEvent(this.gameObject));
        Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        _offset = transform.position - mouseWorldPos;
        _offset.z = 0;
    }
    
    private void Dragging()
    {
        Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        Vector3 targetPosition = mouseWorldPos + _offset;
        
        // 移动子物体
        if (dragSpeed > 0)
            transform.position = Vector3.Lerp(transform.position, targetPosition, dragSpeed * Time.deltaTime);
        else
            transform.position = targetPosition;
    }
    
    private void StopDragging()
    {
        _isDragging = false;
        EventCenter.Publish(new StopDraggingEvent(this.gameObject));
        
        // 检查是否在AttachableZone上
        bool isOnAttachableZone = CheckAttachableZone();
        
        // 如果不在吸附区域且需要返回父物体
        if (!isOnAttachableZone && returnToParent)
        {
            StartCoroutine(ReturnToParentPosition());
        }
    }
    
    private bool CheckAttachableZone()
    {
        // 这里可以添加更复杂的检测逻辑
        // 例如使用物理检测或者事件系统
        if(_canAttach && _attachableZoneCol != null)
        {
            return _attachableZoneCol.OverlapPoint(transform.position);
        }
        
        // 默认返回false，需要根据实际情况实现
        return false;
    }
    
    private IEnumerator ReturnToParentPosition()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        
        // 计算目标位置（父物体上的相对位置）
        Vector3 targetPosition = _parentTransform.TransformPoint(_localStartPosition);
        
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        transform.localPosition = _localStartPosition; // 确保精确回到本地位置
    }
    
    /// <summary>
    /// 更新本地起始位置
    /// </summary>
    /// <param name="newLocalPosition"></param>
    public void UpdateLocalPosition(Vector3 newLocalPosition)
    {
        _localStartPosition = newLocalPosition;
    }
    
    /// <summary>
    /// 设置是否返回父物体位置
    /// </summary>
    /// <param name="reset"></param>
    public void SetReturnToParent(bool reset)
    {
        returnToParent = reset;
    }

    /// <summary>
    /// 设置可吸附区域（不固定，优点在于外部可以视情况而修改 AttachableZone）
    /// </summary>
    /// <param name="attachableZone"></param>
    public void SetAttachableZone(Collider2D attachableZone)
    {
        this._attachableZoneCol = attachableZone;
        this._canAttach = true;
    }
    
    

    // 停止拖拽事件
    public class StartDraggingEvent : EventCenter.IEvent
    {
        public GameObject DraggedObject;

        public StartDraggingEvent(GameObject draggedObject)
        {
            DraggedObject = draggedObject;
        }
    }
    
    // 停止拖拽事件
    public class StopDraggingEvent : EventCenter.IEvent
    {
        public GameObject DraggedObject;

        public StopDraggingEvent(GameObject draggedObject)
        {
            DraggedObject = draggedObject;
        }
    }
}