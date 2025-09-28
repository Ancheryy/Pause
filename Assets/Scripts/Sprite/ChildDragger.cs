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
    
    void Start()
    {
        _camera = Camera.main;
        _col = GetComponent<Collider2D>();
        _parentTransform = transform.parent;
        _localStartPosition = transform.localPosition;
        
        if (_col == null)
        {
            _col = gameObject.AddComponent<BoxCollider2D>();
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
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }
    
    private void StartDragging()
    {
        _isDragging = true;
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
        return false; // 默认返回false，需要你根据实际情况实现
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
    
    // 当父物体移动时，更新本地起始位置（如果需要）
    public void UpdateLocalPosition(Vector3 newLocalPosition)
    {
        _localStartPosition = newLocalPosition;
    }
}