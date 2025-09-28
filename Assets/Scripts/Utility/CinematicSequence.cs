using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 播片流程核心控制器（废弃）
/// </summary>
public static class CinematicSequence
{
    public static AnimSequence CreateAnimSequence()
    {
        return new AnimSequence();
    }
    
    

    public class AnimSequence
    {
        private int _currentIndex = 0;
        private SequenceStatus _status = SequenceStatus.None;
        
        private readonly List<AnimNode> _animNodes = new List<AnimNode>();

        public AnimSequence()
        {
            
        }

        // 从头开始
        public void Start()
        {
            _currentIndex = 0;
            StartFrom(0);
        }

        // 暂停（当前节点播放结束）
        public void Pause()
        {
            if (_status != SequenceStatus.Playing)
            {
                Debug.LogWarning("AnimSequence 尚未开始执行，无法 Pause");
                return;
            }
            
            _status = SequenceStatus.Pause;
        }

        // 继续
        public void Continue()
        {
            if (_status != SequenceStatus.Pause)
            {
                Debug.LogWarning("AnimSequence 尚未进入暂停状态，无法 Continue");
                return;
            }
            
            _status =  SequenceStatus.Playing;
            StartFrom(_currentIndex);
        }

        // 终止
        public void End()
        {
            if (_status != SequenceStatus.Playing && _status != SequenceStatus.Pause)
            {
                Debug.LogWarning("AnimSequence 尚未开始执行 或 尚未进入暂停状态，无法 End");
                return;
            }

            _status = SequenceStatus.ForcedEnd;
        }
        
        // 添加播放节点
        public void AddAnimNode(IEnumerator anim, string name)
        {
            AnimNode animNode = new AnimNode(this);
            animNode.AddAnim(anim, name);
            
            _animNodes.Add(animNode);
        }
        
        // 只允许 AnimNode 调用
        // 检查播放
        private void CheckPlay(int index)
        {
            int lastIndex = index;
            if (_status == SequenceStatus.Pause)
            {
                Debug.Log($"播放已暂停，下一个播放的节点为 {_animNodes[lastIndex + 1].Name} ，处在播放队列下标 {lastIndex + 1} 处");
                _currentIndex = lastIndex + 1;
                return;
            }
            else if (_status == SequenceStatus.ForcedEnd)
            {
                Debug.Log("播放已被强制暂停，下一个播放的节点回归起点 0");
                _currentIndex = 0;
                return;
            }
            
            _currentIndex = lastIndex + 1;
            if (_currentIndex < _animNodes.Count)
            {
                _status =  SequenceStatus.Playing;
                Play(_currentIndex);
            }
            else
            {
                _status = SequenceStatus.Complete;
                Debug.LogWarning("当前 AnimSequence 播放结束");
            }
            
        }

        
        // 从 index 开始
        private void StartFrom(int index)
        {
            Play(index);
        }
        
        // 播放
        private void Play(int index)
        {
            _animNodes[index].Play(index);
        }
        
            
        private class AnimNode
        {
            private int _indexInSequence;
            private string _name;
            public string Name => _name;
        
            private IEnumerator _anim;
            private Coroutine _coroutine;
            private AnimSequence _animSequence;

            public AnimNode(AnimSequence animSequence)
            {
                _animSequence = animSequence;
            }
        
            // 播放动画（协程）节点，不允许中途中断
            public void Play(int index)
            {
                _indexInSequence = index;
                _coroutine = MonoMgr.StartGlobalCoroutine(DoPlay(_indexInSequence));
            }

            // 动画节点结束
            public void End()
            {
                MonoMgr.StopGlobalCoroutine(_coroutine);
            }

            public void AddAnim(IEnumerator anim, string name)
            {
                _name = name;
                _anim = anim;
            }


            private IEnumerator DoPlay(int index)
            {
                yield return MonoMgr.StartGlobalCoroutine(_anim);
                _animSequence.CheckPlay(index);
            }
        }
        
    }



    //
    // public class AnimNodeEndEvent : EventCenter.IEvent
    // {
    //     public int Index { get; }
    //     public AnimSequence FatherSequence { get; }
    //
    //     public AnimNodeEndEvent(int index, AnimSequence fatherSequence)
    //     {
    //         Index = index;
    //         FatherSequence = fatherSequence;
    //     }
    // }
    
    
    private enum SequenceStatus
    {
        None = 0,
        
        Playing = 1,
        Pause = 2,
        Complete = 3,
        ForcedEnd = 4
    }
}