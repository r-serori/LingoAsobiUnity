using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Scripts.Runtime.Managers
{
    /// <summary>
    /// Singleton ImagePreloader - 全シーンで利用可能
    /// </summary>
    public class ImagePreloader : MonoBehaviour
    {
        #region Singleton Pattern

        private static ImagePreloader _instance;

        public static ImagePreloader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ImagePreloader>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ImagePreloader");
                        _instance = go.AddComponent<ImagePreloader>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        [SerializeField] private string[] criticalImages;
        [SerializeField] private SceneImageBatch[] sceneImageBatches;

        [System.Serializable]
        public class SceneImageBatch
        {
            public string sceneName;
            public string[] imageKeys;
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            _ = PreloadCriticalImagesAsync();
        }

        private async Task PreloadCriticalImagesAsync()
        {
            var resourceManager = PerformanceResourceManager.Instance; // ✅ Singleton使用

            if (criticalImages == null || criticalImages.Length == 0) return;

            try
            {
                var tasks = criticalImages.Select(imageKey =>
                    resourceManager.LoadSpriteAsync(imageKey));

                var spriteTasks = tasks.Cast<Task>().ToArray();
                await Task.WhenAll(spriteTasks);

                Debug.Log($"Preloaded {criticalImages.Length} critical images");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Critical images の読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        public async Task PreloadSceneBatch(string sceneName)
        {
            var resourceManager = PerformanceResourceManager.Instance; // ✅ Singleton使用
            var sceneImages = GetSceneSpecificImages(sceneName);

            if (sceneImages == null || sceneImages.Length == 0) return;

            try
            {
                var tasks = sceneImages.Select(imageKey =>
                    resourceManager.LoadSpriteAsync(imageKey));

                var spriteTasks = tasks.Cast<Task>().ToArray();
                await Task.WhenAll(spriteTasks);

                Debug.Log($"Preloaded {sceneImages.Length} images for scene: {sceneName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"シーン '{sceneName}' の画像読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        private string[] GetSceneSpecificImages(string sceneName)
        {
            if (sceneImageBatches == null) return new string[0];

            var batch = sceneImageBatches.FirstOrDefault(batch =>
                string.Equals(batch.sceneName, sceneName, System.StringComparison.OrdinalIgnoreCase));

            return batch?.imageKeys ?? new string[0];
        }
    }
}