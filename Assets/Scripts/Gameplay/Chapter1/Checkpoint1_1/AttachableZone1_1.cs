using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AttachableZone1_1 : MonoBehaviour
{
    [SerializeField] private bool smoothAttach = false;
    [SerializeField] public new Collider2D collider2D;
    // 吸附的物体
    [SerializeField] public Season1_1 attachedObject;
    
    

    void Start()
    {
        // 将 z 位置设置为 1
        var vector3 = this.transform.position;
        vector3.z = 1.0f;
        this.transform.position = vector3;
    }

    void OnDestroy()
    {
        
    }


    public void CheckSnap(GameObject go, out bool needReset)
    {
        if (!GetComponent<TargetZone1_1>().isAvailable)
        {
            needReset = true;
            return;
        }
        
        needReset = false;
        // 符合吸附条件
        if (collider2D.OverlapPoint(go.transform.position))
        {
            // 1.交换前留存一个当前 attachedObject 引用（交换后变成另一个 Zone 的 attachedObject，当前 attachedObject 变为 evt.GameObject.GetComponent<AttachableObject>()）
            AttachableZone1_1 tmpZone1_1 = go.GetComponent<Season1_1>().attachedZone;
            // 2.交换（检查是否需要）
            if (go.GetComponent<Season1_1>().attachedZone != this)
            {
                SwitchAttachedObject(go.GetComponent<Season1_1>());
                go.GetComponent<Season1_1>().ResetPosition();
            }
            // 3.吸附
            Attach();
            tmpZone1_1.Attach();
        }
        // 不符合条件
        else
        {
            // 标记归位（并非立刻归位）
            // evt.GameObject.GetComponent<AttachableObject>().ResetPosition();
            needReset = true;
        }
    }

    private void SwitchAttachedObject(Season1_1 another)
    {
        // 交换相互引用
        Season1_1 tmp1 = attachedObject;
        attachedObject = another;
        another.attachedZone.attachedObject = tmp1;
        AttachableZone1_1 tmp2 = tmp1.attachedZone;
        tmp1.attachedZone = another.attachedZone;
        tmp2.attachedObject.attachedZone = tmp2;
        
        // 交换初始位置（析构交换方式）
        (tmp1.OriginalPosition, tmp2.attachedObject.OriginalPosition) = (tmp2.attachedObject.OriginalPosition, tmp1.OriginalPosition);
        
        // Vector2 tmp = tmp1.OriginalPosition;
        // tmp1.OriginalPosition = tmp2.attachedObject.OriginalPosition;
        // tmp2.attachedObject.OriginalPosition = tmp;
    }

    private void Attach()
    {
        MonoMgr.StartGlobalCoroutine(SetDraggerPosition(this.attachedObject.gameObject));
    }
    
    private IEnumerator SetDraggerPosition(GameObject go)
    {
        if (smoothAttach)
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 startPos = go.transform.position;
        
            while (elapsed < duration)
            {
                go.transform.position = Vector3.Lerp(startPos, transform.position, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        SetAttachableObject(go);
    }

    private void SetAttachableObject(GameObject go)
    {
        var vector3 = transform.position;
        vector3.z = 0f;
        go.transform.position = vector3;
        
        attachedObject = go.GetComponent<Season1_1>();
    }

    
    
    
    
    

    public class AfterAttachEvent : EventCenter.IEvent
    {
        public GameObject GameObject;
        public Season1_1 AttachedObject11;

        public AfterAttachEvent(GameObject go)
        {
            GameObject = go;
        }
    }
    
}
