// 文件：UITransitionStrategies.cs
using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IEnterStrategy
{
    IEnumerator ExecuteEnter(Image currentLevelUI);

}

// 默认淡入
public class DefaultFadeEnter : IEnterStrategy
{
    UnityAction onComplete;

    public DefaultFadeEnter(UnityAction onComplete = null)
    {
        this.onComplete = onComplete;
    }

    public IEnumerator ExecuteEnter(Image currentLevelUI)
    {
        CanvasGroup group = currentLevelUI.GetComponent<CanvasGroup>();
        group.alpha = 1f;
        yield return group.DOFade(0f, 0.5f).SetEase(Ease.Linear).WaitForCompletion();
        this.onComplete?.Invoke();
    }
}

// 直切进入
public class StraightEnter : IEnterStrategy
{
    UnityAction onComplete;

    public StraightEnter(UnityAction onComplete = null)
    {
        this.onComplete = onComplete;
    }

    public IEnumerator ExecuteEnter(Image currentLevelUI)
    {
        yield return null;
        CanvasGroup group = currentLevelUI.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        this.onComplete?.Invoke();
    }
}


// -------------------------------------------------------------------------------------------

public interface IExitStrategy
{
    IEnumerator ExecuteExit(Image currentLevelUI);
}

// 默认淡出
public class DefaultFadeExit : IExitStrategy
{
    public IEnumerator ExecuteExit(Image currentLevelUI)
    {
        CanvasGroup group = currentLevelUI.GetComponent<CanvasGroup>();
        yield return group.DOFade(1f, 0.8f).SetEase(Ease.Linear).WaitForCompletion();
    }
}

// 直切退出
public class StraightExit : IExitStrategy
{
    public IEnumerator ExecuteExit(Image currentLevelUI)
    {
        yield return null;
        CanvasGroup group = currentLevelUI.GetComponent<CanvasGroup>();
        group.alpha = 1f;
    }
}
