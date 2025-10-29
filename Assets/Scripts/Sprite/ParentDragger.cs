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
        // 将父物体设置为更下层的渲染顺序
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 0;  // 父物体在下层
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
    
    // 在Inspector中显示调试信息
    void OnDrawGizmosSelected()
    {
        // 绘制限制区域
        if (limitX)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            
            Vector3 center = new Vector3(
                limitX ? (minX + maxX) / 2f : transform.position.x,
                transform.position.y,
                transform.position.z
            );
            
            Vector3 size = new Vector3(
                limitX ? maxX - minX : 10f,
                10f,
                0.1f
            );
            
            Gizmos.DrawWireCube(center, size);
        }
    }
}