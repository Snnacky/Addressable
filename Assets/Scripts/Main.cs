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
    public RawImage image;
    private void Start()
    {
        Initialize().Forget();
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
        var handle1 = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Capsule.prefab");
        await handle1.Task;
        GameObject prefab = handle1.Result;
        GameObject cube = Instantiate(prefab);
        
    }
}

/*
public class AddressableUpdateManager : MonoBehaviour
{
    async void Start()
    {
        // 1. 初始化 Addressables
        await Addressables.InitializeAsync().Task;

        // 2. 检查目录更新
        // 检查远程服务器上的 catalog 是否比本地新
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        await checkHandle.Task;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
        {
            Debug.Log("检测到资源更新，正在更新目录...");
            // 3. 更新目录
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            await updateHandle.Task;
            
            // 更新完成后释放 handle
            Addressables.Release(updateHandle);
        }
        Addressables.Release(checkHandle);

        // 4. 下载所有受更新影响或缺失的资源
        await DownloadAllAssets();
    }

    async System.Threading.Tasks.Task DownloadAllAssets()
    {
        // 获取所有资源定位符中的所有 Keys
        List<object> allKeys = new List<object>();
        foreach (var locator in Addressables.ResourceLocators)
        {
            allKeys.AddRange(locator.Keys);
        }

        // 5. 获取下载大小
        var sizeHandle = Addressables.GetDownloadSizeAsync(allKeys);
        await sizeHandle.Task;
        long totalSize = sizeHandle.Result;

        if (totalSize > 0)
        {
            Debug.Log($"需要下载的资源大小: {totalSize / 1024f / 1024f:F2} MB");

            // 6. 开始下载
            var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union);
            
            while (!downloadHandle.IsDone)
            {
                float progress = downloadHandle.PercentComplete;
                Debug.Log($"下载进度: {progress * 100}%");
                yield return null; // 如果在协程中，这里可以使用 yield
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("所有资源下载更新完成！");
            }
            Addressables.Release(downloadHandle);
        }
        else
        {
            Debug.Log("没有需要更新的资源内容。");
        }
        
        Addressables.Release(sizeHandle);
    }
}
 */