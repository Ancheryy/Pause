using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f; // 默认1秒
    [SerializeField] private Ease fadeEase = Ease.Linear; // 动画曲线
    [SerializeField] private bool keepActive = true; // 淡出后是否保持游戏对象激活

    private SpriteRenderer _spriteRenderer;
    private Tween _currentTween;
    private Color _originalColor;

    void OnEnable()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;

        // 初始化DOTween（如果尚未初始化）
        DOTween.Init();
    }

    void OnDestroy()
    {
        // 清理未完成的动画
        _currentTween?.Kill();
    }

    // 淡入（使用默认时间）
    public void FadeIn() => Fade(0f, _originalColor.a, fadeDuration);

    // 淡入（自定义时间）
    public void FadeIn(float customDuration) => Fade(0f, _originalColor.a, customDuration);

    // 淡入到原始透明度（使用默认时间）
    public void FadeToOriginal() => Fade(_spriteRenderer.color.a, _originalColor.a, fadeDuration);

    // 淡入到原始透明度（自定义时间）
    public void FadeToOriginal(float customDuration) => Fade(_spriteRenderer.color.a, _originalColor.a, customDuration);

    // 淡出（使用默认时间）
    public void FadeOut() => Fade(_spriteRenderer.color.a, 0f, fadeDuration);

    // 淡出（自定义时间）
    public void FadeOut(float customDuration) => Fade(_spriteRenderer.color.a, 0f, customDuration);

    // 淡出到指定透明度（自定义时间）
    public void FadeTo(float targetAlpha, float customDuration) => Fade(_spriteRenderer.color.a, targetAlpha, customDuration);

    // 从指定透明度淡入到指定透明度
    public void FadeFromTo(float startAlpha, float targetAlpha, float customDuration) => Fade(startAlpha, targetAlpha, customDuration);

    // 设置颜色并淡入（保留原始透明度）
    public void FadeInWithColor(Color newColor, float duration = -1)
    {
        _spriteRenderer.color = new Color(newColor.r, newColor.g, newColor.b, 0f);
        Fade(0f, newColor.a, duration > 0 ? duration : fadeDuration);
    }

    // 设置颜色并淡出
    public void FadeOutWithColor(Color newColor, float duration = -1)
    {
        _spriteRenderer.color = new Color(newColor.r, newColor.g, newColor.b, _spriteRenderer.color.a);
        Fade(_spriteRenderer.color.a, 0f, duration > 0 ? duration : fadeDuration);
    }

    // 立即设置透明度（无动画）
    public void SetAlphaImmediate(float alpha)
    {
        _currentTween?.Kill();
        Color newColor = _spriteRenderer.color;
        newColor.a = Mathf.Clamp01(alpha);
        _spriteRenderer.color = newColor;
    }

    // 重置到原始颜色和透明度
    public void ResetToOriginal()
    {
        _currentTween?.Kill();
        _spriteRenderer.color = _originalColor;
    }

    // 获取当前透明度
    public float GetCurrentAlpha() => _spriteRenderer.color.a;

    // 获取原始透明度
    public float GetOriginalAlpha() => _originalColor.a;

    private void Fade(float startAlpha, float targetAlpha, float duration)
    {
        // 停止当前正在进行的动画
        _currentTween?.Kill();

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;
        }

        // 设置初始状态
        Color startColor = _spriteRenderer.color;
        startColor.a = Mathf.Clamp01(startAlpha);
        _spriteRenderer.color = startColor;

        // 创建并存储新的动画
        _currentTween = _spriteRenderer.DOFade(targetAlpha, duration)
            .SetEase(fadeEase)
            .OnComplete(() => {
                Color finalColor = _spriteRenderer.color;
                finalColor.a = Mathf.Clamp01(targetAlpha);
                _spriteRenderer.color = finalColor;
                
                // 如果淡出到0且不需要保持激活，禁用游戏对象
                if (targetAlpha <= 0.01f && !keepActive)
                {
                    gameObject.SetActive(false);
                }
                
                _currentTween = null;
            });
    }

    // 立即完成当前动画
    public void CompleteCurrentFade()
    {
        _currentTween?.Complete();
    }

    // 暂停当前动画
    public void PauseFade()
    {
        _currentTween?.Pause();
    }

    // 继续当前动画
    public void ResumeFade()
    {
        _currentTween?.Play();
    }

    // 重启当前动画
    public void RestartFade()
    {
        _currentTween?.Restart();
    }
}