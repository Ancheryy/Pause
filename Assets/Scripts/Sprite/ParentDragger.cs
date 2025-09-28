using UnityEngine;

public class ParentDragger : MonoBehaviour
{
    [Header("拖拽设置")]
    public bool enableDrag = true;
    public float dragSpeed = 20f;
    public bool limitX = true;
    public float minX = -5f;
    public float maxX = 5f;
    
    private Vector3 _startPosition;
    private Vector3 _offset;
    private bool _isDragging = false;
    private Camera _camera;
    private Collider2D _col;
    
    void Start()
    {
        _camera = Camera.main;
        _col = GetComponent<Collider2D>();
        _startPosition = transform.position;
        
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
        
        // 父物体只能横向移动
        targetPosition.y = transform.position.y;
        
        // 应用位置限制
        if (limitX)
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        
        // 移动父物体（所有子物体会自动跟随）
        if (dragSpeed > 0)
            transform.position = Vector3.Lerp(transform.position, targetPosition, dragSpeed * Time.deltaTime);
        else
            transform.position = targetPosition;
    }
    
    private void StopDragging()
    {
        _isDragging = false;
    }
}