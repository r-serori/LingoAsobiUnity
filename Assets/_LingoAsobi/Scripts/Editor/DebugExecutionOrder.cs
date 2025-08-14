using Scripts.Runtime.Core;
using Scripts.Runtime.Utilities.Helpers;
using UnityEngine;

namespace Scripts.Runtime.Editor
{
    /// <summary>
    /// 実行順序をランタイムで確認するためのデバッグクラス
    /// </summary>
    public class DebugExecutionOrder : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LogInitializationOrder()
        {
            Debug.Log("===== Initialization Order Check =====");
            Debug.Log("[RuntimeInitialize] BeforeSceneLoad called");
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void LogAfterSceneLoad()
        {
            Debug.Log("[RuntimeInitialize] AfterSceneLoad called");
            
            // 各マネージャーの存在確認
            CheckManagerStatus();
        }
        
        private static void CheckManagerStatus()
        {
            Debug.Log("===== Manager Status Check =====");
            
            var eventBus = FindObjectOfType<EventBus>();
            var sceneHelper = FindObjectOfType<SceneHelper>();
            var eventRegistry = FindObjectOfType<GameEventHandlerRegistry>();
            var gameBootstrap = FindObjectOfType<GameBootstrap>();
            
            Debug.Log($"EventBus: {(eventBus != null ? "Found" : "Not Found")}");
            Debug.Log($"SceneHelper: {(sceneHelper != null ? "Found" : "Not Found")}");
            Debug.Log($"GameEventHandlerRegistry: {(eventRegistry != null ? "Found" : "Not Found")}");
            Debug.Log($"GameBootstrap: {(gameBootstrap != null ? "Found" : "Not Found")}");
            
            // Singletonインスタンスの確認
            Debug.Log($"EventBus.Instance: {EventBus.Instance != null}");
            Debug.Log($"SceneHelper.Instance: {SceneHelper.Instance != null}");
            Debug.Log($"GameEventHandlerRegistry.Instance: {GameEventHandlerRegistry.Instance != null}");
        }
        
        void Awake()
        {
            Debug.Log($"[{GetType().Name}] Awake called at {Time.time}");
        }
        
        void Start()
        {
            Debug.Log($"[{GetType().Name}] Start called at {Time.time}");
        }
    }
}