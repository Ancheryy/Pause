using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 流程控制类，高层次类，对 关卡流程 或 章节流程 进行控制
public static class FlowController
{
    private static Dictionary<int, Chapter> _chapters = new Dictionary<int, Chapter>();
    
    public static void CreateChapters()
    {
        // 创建 Chapter1
        _chapters.Add(1, new Chapter1(1, "Chapter1", 
            new Checkpoint1_1(101, "Checkpoint1_1", "Checkpoint1_2"),
            new Checkpoint1_2(102, "Checkpoint1_2", "Checkpoint1_3")
            )
        );
        
    }

    public static Checkpoint GetCheckpoint(int chapterId, int checkpointId)
    {
        foreach (var checkpoint in _chapters[chapterId].Checkpoints)
        {
            if(checkpoint.ID == checkpointId)
                return checkpoint;
        }
        
        return null;
    }
    
    public static Checkpoint GetCheckpoint(int chapterId, string checkpointName)
    {
        foreach (var checkpoint in _chapters[chapterId].Checkpoints)
        {
            if(checkpoint.Name == checkpointName)
                return checkpoint;
        }
        
        return null;
    }
    
    public static Checkpoint GetCheckpoint(string checkpointName)
    {
        foreach (var chapter in _chapters.Values)
        {
            foreach (var checkpoint in chapter.Checkpoints)
            {
                if(checkpoint.Name == checkpointName)
                    return checkpoint;
            }
        }
        
        return null;
    }
}
