using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoicingObject1_6 : MonoBehaviour
{
    [SerializeField] public int voicingObjectId;
    
    private Vector2 originalPosition;
    private bool isAvaliable = true;


    void Awake()
    {
        originalPosition = transform.position;
    }
    
    
    // 检测物体是否要附着在目标碰撞体上
    public bool IsAttachable(Collider2D targetCollider)
    {
        if (targetCollider.OverlapPoint(transform.position))
        {
            return true;
        }
        return false;
    }
    
    // 吸附到目标碰撞体上
    public void SnapToTarget(Collider2D targetCollider)
    {
        transform.position = targetCollider.bounds.center;
        isAvaliable = false;
        this.GetComponent<Dragger>().enableDrag = false;
    }
    
    // 重置物体位置
    public void ResetPosition()
    {
        transform.position = originalPosition;
    }
}
