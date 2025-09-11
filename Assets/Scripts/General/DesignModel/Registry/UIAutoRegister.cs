using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 使用：为要注册的游戏内的 UI 物体挂载该脚本
// 功能说明：自动为要注册的 UI 物体注册到仓库（主动为上层逻辑层提供引用获取方式）
public class UIAutoRegister : MonoBehaviour
{
    // 在 Inspector 中设置唯一标识
    [SerializeField] private string registerKey; 
    // UI 元素类型
    [SerializeField] private UIElementType uiElementType;

    private void Awake()
    {
        switch (uiElementType)
        {
            case UIElementType.Image when TryGetComponent<Image>(out var image):
                UIElementRegistry.RegisterImage(registerKey, image);
                break;
            case UIElementType.Button when TryGetComponent<Button>(out var button):
                UIElementRegistry.RegisterButton(registerKey, button);
                break;
            default:
                UIElementRegistry.RegisterUIElement(registerKey, gameObject);
                break;
        }
    }

    private void OnDestroy()
    {
        // 可选：在对象销毁时从注册表中移除
        UIElementRegistry.CancelRegister(registerKey, uiElementType);
    }
}
