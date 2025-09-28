using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CheckpointFactory
{
    public static EventCenter.IEvent GetLoadEventType(string checkpointName)
    {
        switch (checkpointName)
        {
            case "Checkpoint1_1":
                return new Checkpoint1_1.LoadCheckpoint1_1Event();
            default:
                return null;
        }
    }
}
