using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetZone1_1 : MonoBehaviour
{
    [SerializeField] public int ID;   
    [SerializeField] public Sprite goldenSeason;   
    // 是否可以被占用
    public bool isAvailable = true;

    void Start()
    {
        isAvailable = true;
    }

    public bool IsMatch()
    {
        int objectId = this.GetComponent<AttachableZone1_1>().attachedObject.ID;
        if (this.GetComponent<AttachableZone1_1>().attachedObject != null && objectId == this.ID)
        {
            return true;
        }
        return false;
    }

    // 显示金光特效
    public void ShowGoldenLight()
    {
        if (!isAvailable)
            return;   // 代表已经匹配，不再显示金光特效

        MonoMgr.StartGlobalCoroutine(DoReplaceSprite());
    }
    
    // 变成金光
    IEnumerator DoReplaceSprite()
    {
        GetComponent<AttachableZone1_1>().attachedObject.GetComponent<SpriteFade>().SetAlphaImmediate(0f);
        GetComponent<AttachableZone1_1>().collider2D.GetComponent<SpriteRenderer>().sprite = goldenSeason;
        GetComponent<AttachableZone1_1>().collider2D.GetComponent<SpriteFade>().FadeIn(0.4f);
        isAvailable = false;
        // 短暂闪过金光
        yield return new WaitForSeconds(0.2f);
        GetComponent<AttachableZone1_1>().collider2D.GetComponent<SpriteFade>().FadeOut(0.4f);
        GetComponent<AttachableZone1_1>().attachedObject.GetComponent<SpriteFade>().FadeIn(0.4f);
        GetComponent<AttachableZone1_1>().attachedObject.GetComponent<Dragger>().enableDrag = false;
        // GetComponent<AttachableZone1_1>().collider2D.gameObject.SetActive(false);
    }

    public bool IsAvailable()
    {
        return isAvailable;
    }
    
}
