using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scripts.Runtime.Managers
{
    /// <summary>
    /// シングルトンパターンによるリソース管理システム
    /// シーン遷移で破棄されず、全シーンで利用可能
    /// </summary>
    public class PerformanceResourceManager : MonoBehaviour
    {
        #region Singleton Pattern

        private static PerformanceResourceManager _instance;

        public static PerformanceResourceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PerformanceResourceManager>();

                    if (_instance == null)
                    {
                        // 自動生成
                        GameObject go = new GameObject("PerformanceResourceManager");
                        _instance = go.AddComponent<PerformanceResourceManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        // 既存のフィールド
        private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private Dictionary<string, Task<Sprite>> loadingTasks = new Dictionary<string, Task<Sprite>>();

        [SerializeField] private int maxCacheSize = 100;
        private Queue<string> cacheOrder = new Queue<string>();

        void Awake()
        {
            // Singleton 重複チェック
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("PerformanceResourceManager の重複インスタンスを破棄します。");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("PerformanceResourceManager Singleton 初期化完了");
        }

        /// <summary>
        /// スプライトを非同期で読み込みます
        /// Resources/フォルダ以下のパスを指定してください（例："Characters/hero_01"）
        /// </summary>
        public async Task<Sprite> LoadSpriteAsync(string resourcePath)
        {
            // 既にキャッシュにある場合は即座に返す
            if (spriteCache.TryGetValue(resourcePath, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            // 既に読み込み中の場合は待機
            if (loadingTasks.TryGetValue(resourcePath, out Task<Sprite> existingTask))
            {
                return await existingTask;
            }

            // 新規読み込みタスクを作成
            var loadTask = LoadSpriteFromResources(resourcePath);
            loadingTasks[resourcePath] = loadTask;

            try
            {
                Sprite sprite = await loadTask;

                // 読み込み成功時のみキャッシュに追加
                if (sprite != null)
                {
                    // キャッシュサイズ制限チェック
                    if (spriteCache.Count >= maxCacheSize)
                    {
                        RemoveOldestFromCache();
                    }

                    spriteCache[resourcePath] = sprite;
                    cacheOrder.Enqueue(resourcePath);
                }

                return sprite;
            }
            finally
            {
                loadingTasks.Remove(resourcePath);
            }
        }

        /// <summary>
        /// Resources APIを使用してスプライトを非同期読み込み
        /// </summary>
        private async Task<Sprite> LoadSpriteFromResources(string resourcePath)
        {
            // メインスレッドでResources.LoadAsyncを実行
            var request = Resources.LoadAsync<Sprite>(resourcePath);

            // 非同期待機
            while (!request.isDone)
            {
                await Task.Yield(); // 1フレーム待機
            }

            return request.asset as Sprite;
        }

        /// <summary>
        /// 最古のキャッシュエントリを削除
        /// </summary>
        private void RemoveOldestFromCache()
        {
            if (cacheOrder.Count > 0)
            {
                string oldestKey = cacheOrder.Dequeue();
                if (spriteCache.TryGetValue(oldestKey, out Sprite oldSprite))
                {
                    // Resourcesから読み込んだアセットは自動的にUnityが管理するため
                    // 明示的な解放は不要ですが、キャッシュから削除
                    spriteCache.Remove(oldestKey);
                }
            }
        }

        /// <summary>
        /// キャッシュをクリア（メモリ節約用）
        /// </summary>
        public void ClearCache()
        {
            spriteCache.Clear();
            cacheOrder.Clear();

            Debug.Log("PerformanceResourceManager: キャッシュをクリアしました");
        }

        /// <summary>
        /// 統計情報を取得
        /// </summary>
        public string GetCacheStats()
        {
            return $"Cache: {spriteCache.Count}/{maxCacheSize}, Loading: {loadingTasks.Count}";
        }

        void OnDestroy()
        {
            // キャッシュをクリア
            ClearCache();
            loadingTasks.Clear();

            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// デバッグ情報表示
        /// </summary>
        [ContextMenu("Show Cache Stats")]
        private void ShowCacheStats()
        {
            Debug.Log($"PerformanceResourceManager Stats: {GetCacheStats()}");
        }
    }
}