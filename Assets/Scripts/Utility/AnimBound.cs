using UnityEngine;

// 动画捆绑struct
public struct AnimBound
{
    private Animator _animator;
    private AnimationClip _animationClip;

    public AnimBound(Animator animator, AnimationClip animationClip)
    {
        this._animator = animator;
        this._animationClip = animationClip;
    }

    public void PlayAnim()
    {
        _animator.gameObject.SetActive(true);
        _animator.enabled = true;
        _animator.Play(_animationClip.name);
    }

    public void StopAnim()
    {
        if (_animator == null)
        {
            Debug.LogWarning("当前animator组件不存在！");
            return;
        }

        ResetAnim();
        _animator.enabled = false;
        _animator.gameObject.SetActive(false);
    }
    
    public void SetEnable(bool isEnable)
    {
        if (_animator == null)
        {
            Debug.LogWarning("当前animator组件不存在！");
            return;
        }

        _animator.enabled = isEnable;
    }

    public void ResetAnim()
    {
        _animator.Rebind();
        // _animator.Update(0); // 立即应用重置
    }

    public float GetPlayTime()
    {
        return _animationClip.length;
    }
}
