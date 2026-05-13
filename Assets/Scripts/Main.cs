using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public RawImage image;
    private void Start()
    {
        Tast().Forget();
    }

    async UniTask Tast()
    {
        var handle1 = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Capsule.prefab");
        await handle1.Task;
        GameObject prefab = handle1.Result;
        GameObject cube = Instantiate(prefab);

        var handle2 = Addressables.LoadAssetAsync<Texture2D>("Assets/Textures/P1.jpg");
        await handle2.Task;
        Texture2D texture2D = handle2.Result;
        image.texture = texture2D;
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(texture2D.width, texture2D.height);
    }
}
