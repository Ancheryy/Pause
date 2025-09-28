using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Season1_1 : MonoBehaviour
{
    [SerializeField] public int ID;
    [SerializeField] public Vector2 OriginalPosition;

    [SerializeField] public AttachableZone1_1 attachedZone;

    void Start()
    {
        OriginalPosition = transform.position;
    }

    // 归位
    public void ResetPosition()
    {
        transform.position = OriginalPosition;
    }
    


    
    
    public void SetAttachedZone(AttachableZone1_1 attachedZone11)
    {
        this.attachedZone = attachedZone11;
    }
}
