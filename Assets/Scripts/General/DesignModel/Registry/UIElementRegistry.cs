using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 使用：Gameplay 相关类直接调用获取 UI 元素的方法；Gameplay 中的变量使用属性来延迟获取引用
//     例：private GameObject FrontHalf => UIElementRegistry.GetUIElement("FrontHalf");
// 功能说明：UI 元素仓库，
public static class UIElementRegistry
{
    private static Dictionary<string, GameObject> _uiElements = new Dictionary<string, GameObject>();
    private static Dictionary<string, Image> _images = new Dictionary<string, Image>();
    private static Dictionary<string, Button> _buttons = new Dictionary<string, Button>();

    public static void RegisterUIElement(string key, GameObject element)
    {
        _uiElements[key] = element;
    }

    public static void RegisterImage(string key, Image image)
    {
        _images[key] = image;
    }

    public static void RegisterButton(string key, Button button)
    {
        _buttons[key] = button;
    }

    public static void CancelUIElement(string key)
    {
        _uiElements.Remove(key);
    }

    public static void CancelImage(string key)
    {
        _images.Remove(key);
    }

    public static void CancelButton(string key)
    {
        _buttons.Remove(key);
    }

    // 判断类型，注销订阅
    public static void CancelRegister(string key, UIElementType elementType)
    {
        switch (elementType)
        {
            case UIElementType.Image:
                _images.Remove(key);
                break;
            case UIElementType.Button:
                _buttons.Remove(key);
                break;
            default:
                _uiElements.Remove(key);
                break;
        }
    }

    /* 供 Gameplay 相关类调用，通过名称直接获取已经注册了的 UI 物体的引用 */
    
    public static GameObject GetUIElement(string key)
    {
        return _uiElements.TryGetValue(key, out var element) ? element : null;
    }

    public static Image GetImage(string key)
    {
        return _images.TryGetValue(key, out var image) ? image : null;
    }

    public static Button GetButton(string key)
    {
        return _buttons.TryGetValue(key, out var button) ? button : null;
    }

    // 清理方法，在关卡结束时调用
    public static void ClearAll()
    {
        _uiElements.Clear();
        _images.Clear();
        _buttons.Clear();
    }
}


public enum UIElementType
{
    GameObject = 0,
    Image = 1,
    Button = 2
}
