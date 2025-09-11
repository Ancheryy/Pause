using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UIFade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f; // 默认1秒
    [SerializeField] private Ease fadeEase = Ease.Linear; // 动画曲线

    private CanvasGroup _canvasGroup;
    private Tween _currentTween;

    void OnEnable()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        // 初始化DOTween（如果尚未初始化）
        DOTween.Init();
    }

    void OnDestroy()
    {
        // 清理未完成的动画
        _currentTween?.Kill();
    }

    // 淡入（使用默认时间）
    public void FadeIn() => Fade(0f, 1f, fadeDuration);

    // 淡入（自定义时间）
    public void FadeIn(float customDuration) => Fade(0f, 1f, customDuration);

    // 淡出（使用默认时间）
    public void FadeOut() => Fade(1f, 0f, fadeDuration);

    // 淡出（自定义时间）
    public void FadeOut(float customDuration) => Fade(1f, 0f, customDuration);

    // 淡出到（自定义时间）
    public void FadeOutTo(float targetAlpha, float customDuration) => Fade(1f, targetAlpha, customDuration);

    // 淡入到（自定义时间）
    public void FadeInTo(float targetAlpha, float customDuration) => Fade(0f, targetAlpha, customDuration);

    // 渐变（自定义时间）
    public void FadeFromTo(float originalAlpha, float targetAlpha, float customDuration) => Fade(originalAlpha, targetAlpha, customDuration);


    private void Fade(float startAlpha, float targetAlpha, float duration)
    {
        // 停止当前正在进行的动画
        _currentTween?.Kill();

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        // 设置初始状态
        _canvasGroup.alpha = startAlpha;

        // 创建并存储新的动画
        _currentTween = _canvasGroup.DOFade(targetAlpha, duration)
            .SetEase(fadeEase)
            .OnComplete(() => {
                _canvasGroup.alpha = targetAlpha;
                _currentTween = null;
            });
    }

    // 立即完成当前动画
    public void CompleteCurrentFade()
    {
        _currentTween?.Complete();
    }
}