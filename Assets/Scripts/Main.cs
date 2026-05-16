using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Image image;
    async void Start()
    {
        await Initialize();
        Tast().Forget();
    }

    async UniTask Initialize()
    {
        await Addressables.InitializeAsync().Task;

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        await checkHandle.Task;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
        {
            Debug.Log("检测到更新");
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            await updateHandle.Task;
            
            Addressables.Release(updateHandle);
        }
        Addressables.Release(checkHandle);

        await DownLoadAllAssets();
    }
    
    async UniTask DownLoadAllAssets()
    {
        //获取资源key(key就是资源的标识符,用来定位加载资源)
        List<object> allKeys = new List<object>();
        foreach (var locator in Addressables.ResourceLocators)
        {
            allKeys.AddRange(locator.Keys);
        }

        var sizeHandle = Addressables.GetDownloadSizeAsync(allKeys);
        await sizeHandle.Task;
        long totalSize = sizeHandle.Result;
        if (totalSize > 0)
        {
           
            Debug.Log($"需要下载的资源大小: {totalSize / 1024f / 1024f:F2} MB");
            var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union);//加载并去重

            while (!downloadHandle.IsDone)
            {
                float progress = downloadHandle.PercentComplete;
                Debug.Log($"下载进度: {progress * 100}%");
                await UniTask.NextFrame();
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("下载完毕");
            }
            Addressables.Release(downloadHandle);
        }
        else
        {
            Debug.Log("没有需要更新的资源内容。");
        }
        
        Addressables.Release(sizeHandle);
    }

    async UniTask Tast()
    {
        var handle1 = Addressables.LoadAssetAsync<GameObject>("Circle");
        await handle1.Task;
        GameObject prefab = handle1.Result;
        GameObject cube = Instantiate(prefab);

        var handle2 = Addressables.LoadAssetAsync<Sprite>("P1");
        await handle2.Task;
        Sprite sp = handle2.Result;
        image.GetComponent<SpriteRenderer>().sprite = sp;

    }
}
