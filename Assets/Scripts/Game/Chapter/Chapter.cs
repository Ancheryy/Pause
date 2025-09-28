using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Chapter
{
    // 章节 全局唯一标识
    public int ID { get; }
    // 章节名称
    public string Name { get; }
    // 章节中关卡数量
    public int CheckpointNum { get; }
    
    // 章节关卡集合
    private readonly List<Checkpoint> _checkpoints = new List<Checkpoint>();
    public List<Checkpoint> Checkpoints => _checkpoints;

    public Chapter(int id, string name, params Checkpoint[] checkpoints)
    {
        foreach (Checkpoint checkpoint in checkpoints)
        {
            this._checkpoints.Add(checkpoint);
        }
        
        this.ID = id;
        this.Name = name;
        this.CheckpointNum = checkpoints.Length;
    }
    
}

public class Chapter1 : Chapter
{
    public Chapter1(int id, string name, params Checkpoint[] checkpoints) : base(id, name, checkpoints)
    {
    }
}
