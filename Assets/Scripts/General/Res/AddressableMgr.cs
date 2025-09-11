using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableMgr : MonoSingleton<AddressableMgr>
{
    private static Dictionary<string, IEnumerator> resDic = new Dictionary<string, IEnumerator>();

    // 直接通过资源的名称（ key ）进行加载
    public static void LoadAssetAsync<T>(string name, Action<AsyncOperationHandle<T>> callback)
    {
        string resName = name + "_" + typeof(T).Name;
        AsyncOperationHandle<T> handle;
        if (resDic.ContainsKey(resName))
        {
            handle = (AsyncOperationHandle<T>)resDic[resName];

            if (handle.IsDone)
            {
                callback(handle);
            }
            else
            {
                handle.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                        callback(obj);
                };
            }

            return;
        }
        
        handle = Addressables.LoadAssetAsync<T>(resName);
        handle.Completed += (obj) =>
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
                callback(obj);
            else
            {
                Debug.LogWarning(resName + " 资源加载失败");
                if(resDic.ContainsKey(resName))
                    resDic.Remove(resName);
            }
        };
        resDic[resName] = handle;
    }

    public static void ReleaseAsset<T>(string name)
    {
        string resName = name + "_" + typeof(T).Name;
        if (resDic.ContainsKey(resName))
        {
            AsyncOperationHandle<T> handle = (AsyncOperationHandle<T>)resDic[resName];
            Addressables.Release(handle);
            resDic.Remove(resName);
        }
    }

    public static void Clear()
    {
        resDic.Clear();
        AssetBundle.UnloadAllAssetBundles(true);
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
    
}
