// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using DG.Tweening;
// using UnityEngine.Events;
//
// public class CheckpointMgr : MonoSingleton<CheckpointMgr>
// {
//     // 检查点数据
//     public Checkpoint lastCheckpoint;
//     public Checkpoint currentCheckpoint;
//     public Dictionary<string, Checkpoint> allCheckpoints = new Dictionary<string, Checkpoint>();
//     public Dictionary<int, Checkpoint> idToCheckpoints = new Dictionary<int, Checkpoint>();
//
//
//     protected override void Awake()
//     {
//         base.Awake();
//
//         
//     }
//     
//     
//     /// <summary>
//     /// 注册检查点到管理器
//     /// </summary>
//     public void Register(Checkpoint checkpoint)
//     {
//         if (checkpoint == null) return;
//
//         // 防止重复注册
//         if (!allCheckpoints.ContainsKey(checkpoint.Name))
//         {
//             allCheckpoints.Add(checkpoint.Name, checkpoint);
//             idToCheckpoints.Add(checkpoint.ID, checkpoint);
//             Debug.Log($"Registered checkpoint: {checkpoint.Name} (ID: {checkpoint.ID})");
//         }
//     }
//
//     /// <summary>
//     /// 通过名称获取检查点（泛型）
//     /// </summary>
//     public T GetCheckpoint<T>(string checkpointName) where T : Checkpoint
//     {
//         if (allCheckpoints.TryGetValue(checkpointName, out Checkpoint checkpoint))
//         {
//             return checkpoint as T;
//         }
//         Debug.LogError($"Checkpoint not found: {checkpointName}");
//         return null;
//     }
//
//     /// <summary>
//     /// 通过ID获取检查点（泛型）
//     /// </summary>
//     public T GetCheckpoint<T>(int id) where T : Checkpoint
//     {
//         if (idToCheckpoints.TryGetValue(id, out Checkpoint checkpoint))
//         {
//             return checkpoint as T;
//         }
//         Debug.LogError($"Checkpoint ID not found: {id}");
//         return null;
//     }
//
//     /// <summary>
//     /// 通过名称获取检查点
//     /// </summary>
//     public Checkpoint GetCheckpoint(string checkpointName)
//     {
//         if (checkpointName == null) MenuMgr.Instance.BackToMenu();
//
//         if (allCheckpoints.TryGetValue(checkpointName, out Checkpoint checkpoint))
//         {
//             return checkpoint;
//         }
//         Debug.LogError($"Checkpoint not found: {checkpointName}");
//         return null;
//     }
//
//     /// <summary>
//     /// 通过ID获取检查点
//     /// </summary>
//     public Checkpoint GetCheckpoint(int id)
//     {
//         if (idToCheckpoints.TryGetValue(id, out Checkpoint checkpoint))
//         {
//             return checkpoint;
//         }
//         Debug.LogError($"Checkpoint ID not found: {id}");
//         return null;
//     }
//
//     /// <summary>
//     /// 更新当前检查点
//     /// </summary>
//     public void SetCurrentCheckpoint(Checkpoint checkpoint)
//     {
//         if (checkpoint == null) return;
//
//         lastCheckpoint = currentCheckpoint;
//         currentCheckpoint = checkpoint;
//         Debug.Log($"Current checkpoint updated to: {checkpoint.Name}");
//     }
//
//     /// <summary>
//     /// 启动检查点（优化逻辑）
//     /// </summary>
//     public void StartCheckpoint(Checkpoint checkpoint)
//     {
//         if (checkpoint == null)
//         {
//             Debug.LogError("Cannot start null checkpoint!");
//             return;
//         }
//
//         SetCurrentCheckpoint(checkpoint);
//         checkpoint.Begin();
//     }
//
//     /// <summary>
//     /// 启动检查点（通过名称）
//     /// </summary>
//     public void StartCheckpoint(string checkpointName)
//     {
//         //if (allCheckpoints.ContainsKey(checkpointName))
//         //{
//         //    StartCheckpoint(allCheckpoints[checkpointName]);
//         //}
//         Checkpoint checkpoint = GetCheckpoint(checkpointName);
//         StartCheckpoint(checkpoint);
//         Debug.Log($"Checkpoint not found: {checkpointName}");
//     }
//
// }