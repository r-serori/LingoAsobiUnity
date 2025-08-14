using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scripts.Runtime.Core;

namespace Scripts.Runtime.Utilities.Helpers
{
    /// <summary>
    /// 改善版シーンヘルパー
    /// </summary>
    public static class SceneHelper
    {
        private static bool isTransitioning = false;
        
        /// <summary>
        /// 非同期でシーンをロード（改善版）
        /// </summary>
        public static async Task<bool> LoadSceneAsync(string sceneName, bool additive = false)
        {
            if (isTransitioning)
            {
                Debug.LogWarning($"[SceneHelper] Scene transition already in progress");
                return false;
            }
            
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneHelper] Scene name is null or empty");
                return false;
            }
            
            try
            {
                isTransitioning = true;
                
                // GameEventManagerの確認と初期化
                EnsureEventManagerExists();
                
                // 遷移開始イベントを発行
                EventBus.Publish(new SceneTransitionEvent(sceneName, 
                    SceneTransitionEvent.TransitionPhase.Started));
                
                // シーンのロード開始
                var loadOperation = SceneManager.LoadSceneAsync(sceneName, 
                    additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                
                if (loadOperation == null)
                {
                    throw new Exception($"Failed to start loading scene: {sceneName}");
                }
                
                loadOperation.allowSceneActivation = false;
                
                // ロード進捗の監視
                while (!loadOperation.isDone)
                {
                    float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
                    
                    // 進捗イベントを発行
                    EventBus.Publish(new SceneTransitionEvent(sceneName, 
                        SceneTransitionEvent.TransitionPhase.Loading)
                    {
                        Progress = progress
                    });
                    
                    // 90%まで読み込まれたらシーンをアクティベート
                    if (loadOperation.progress >= 0.9f)
                    {
                        await Task.Delay(100); // 少し待機（フェード演出などのため）
                        loadOperation.allowSceneActivation = true;
                    }
                    
                    await Task.Yield();
                }
                
                // 完了イベントを発行
                EventBus.Publish(new SceneTransitionEvent(sceneName, 
                    SceneTransitionEvent.TransitionPhase.Completed));
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneHelper] Error loading scene {sceneName}: {e.Message}");
                
                // 失敗イベントを発行
                EventBus.Publish(new SceneTransitionEvent(sceneName, 
                    SceneTransitionEvent.TransitionPhase.Failed));
                
                return false;
            }
            finally
            {
                isTransitioning = false;
            }
        }
        
        /// <summary>
        /// GameEventManagerが存在することを保証
        /// </summary>
        private static void EnsureEventManagerExists()
        {
            if (GameEventManager.Instance == null)
            {
                Debug.LogWarning("[SceneHelper] GameEventManager not found, creating...");
            }
            
            // Instanceプロパティアクセスで自動的に作成される
            var manager = GameEventManager.Instance;
            Debug.Log($"[SceneHelper] GameEventManager is ready: {manager != null}");
        }
        
        /// <summary>
        /// 現在のシーン名を取得
        /// </summary>
        public static string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }
        
        /// <summary>
        /// シーンが存在するか確認
        /// </summary>
        public static bool SceneExists(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}