using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑器资源管理器
/// 只是为了方便在编辑器中开发时使用，游戏打包后不使用该代码
/// </summary>
public class EditorResMgr : Singleton<EditorResMgr>
{
    // 防止需要打包进AB包中的资源路径
    private readonly string _rootPath = "Assets/Editor/ArtRes/";
    // 防止已经加载过的图集资源
    private readonly Dictionary<string, List<Object>> _loadedSprites = new Dictionary<string, List<Object>>();

    // 1.加载单个资源
    public T LoadEditorRes<T>(string path) where T : Object
    {
        string suffixName = "";
        // 需要为加载资源的类型进行对应的后缀名确定
        // 参考资源类型：预设体，纹理（图片），材质球，音效等
        if(typeof(T) == typeof(GameObject))
            suffixName = ".prefab";
        else if(typeof(T) == typeof(Texture2D))
            suffixName = ".png";
        else if(typeof(T) == typeof(AudioClip))
            suffixName = ".mp3";
        else
        {
            Debug.LogError($"没有找到类型为 {typeof(T).Name} 的资源，无法确认后缀名并加载");
            return null;
        }
        
        T res = AssetDatabase.LoadAssetAtPath<T>(_rootPath + path + suffixName);
        return res;
    }

    // 2.加载图集相关资源
    public Sprite LoadEditorRes(string path, string spriteName)
    {
        List<Object> sprites = null;
        if (!_loadedSprites.ContainsKey(_rootPath + path))
        {
            sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(_rootPath + path).ToList();
            _loadedSprites.Add(_rootPath + path, sprites);
        }
        else
        {
            sprites = _loadedSprites[_rootPath + path];
        }

        foreach (var sprite in sprites)
        {
            if(sprite.name == spriteName)
                return sprite as Sprite;
        }
        return null;
    } 
    
}
