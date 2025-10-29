using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CheckpointFactory
{
    /// <summary>
    /// 获取 关卡加载事件 类（如：Checkpoint1_1.LoadCheckpoint1_1Event）
    /// </summary>
    /// <param name="checkpointName">关卡名称</param>
    /// <returns></returns>
    public static EventCenter.IEvent GetLoadEventType(string checkpointName)
    {
        switch (checkpointName)
        {
            case "Checkpoint1_1":
                return new Checkpoint1_1.LoadCheckpoint1_1Event();
            case "Checkpoint1_2":
                return new Checkpoint1_2.LoadCheckpoint1_2Event();
            case "Checkpoint1_3":
                return new Checkpoint1_3.LoadCheckpoint1_3Event();
            default:
                return null;
        }
    }
    
    public static Checkpoint CreateCheckpoint(string checkpointName)
    {
        switch (checkpointName)
        {
            case "Checkpoint1_1":
                return new Checkpoint1_1();
            case "Checkpoint1_2":
                return new Checkpoint1_2();
            case "Checkpoint1_3":
                return new Checkpoint1_3();
            case "Checkpoint1_4":
                return new Checkpoint1_4();
            case "Checkpoint1_5":
                return new Checkpoint1_5();
            case "Checkpoint1_6":
                return new Checkpoint1_6();
            case "Checkpoint1_7":
                return new Checkpoint1_7();
            default:
                Debug.Log($"没有找到关卡名为：{checkpointName} 的关卡");
                return null;
        }
    }
    
}
