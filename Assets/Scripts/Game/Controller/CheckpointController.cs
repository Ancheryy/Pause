using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 在每个关卡场景中存在，控制该场景对应关卡的 开始 / 结束
// 封装了 关卡 和 对关卡的相关操作
public class CheckpointController : MonoBehaviour
{
    [SerializeField] private string checkpointName;
    
    // 场景中的当前关卡
    public Checkpoint SceneCheckpoint;

    void Awake()
    {
        GameMgr.Instance.SubscribeEndLoadSceneEvent();
    }

    // 动态创建关卡对象
    public void CreateCheckpoint()
    {
        SceneCheckpoint = CheckpointFactory.CreateCheckpoint(checkpointName);
    }
    
    public void StartCheckpoint()
    {
        if (SceneCheckpoint == null)
        {
            Debug.LogWarning("先创建关卡");
        }
    }
}
