using UnityEngine;
using System.Collections.Generic;

namespace Scripts.Runtime.Core
{
    /// <summary>
    /// ゲーム全体のイベントを管理する永続的なマネージャー
    /// SceneTransitionEventの処理を保証
    /// </summary>
    public class GameEventManager : MonoBehaviour
    {
        private static GameEventManager instance;
        private List<System.Action> cleanupActions = new();
        
        public static GameEventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    CreateInstance();
                }
                return instance;
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            CreateInstance();
        }
        
        private static void CreateInstance()
        {
            if (instance != null) return;
            
            GameObject go = new GameObject("[GameEventManager]");
            instance = go.AddComponent<GameEventManager>();
            DontDestroyOnLoad(go);
            
            Debug.Log("[GameEventManager] Instance created and set to DontDestroyOnLoad");
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            InitializeEventHandlers();
        }
        
        /// <summary>
        /// 全ての必須イベントハンドラーを初期化
        /// </summary>
        private void InitializeEventHandlers()
        {
            // EventBusのインスタンスを確実に初期化
            var eventBus = EventBus.Instance;
            
            // SceneTransitionEventのハンドラーを登録
            eventBus.Subscribe<SceneTransitionEvent>(OnSceneTransition);
            cleanupActions.Add(() => eventBus.Unsubscribe<SceneTransitionEvent>(OnSceneTransition));
            
            Debug.Log("[GameEventManager] Event handlers initialized");
            Debug.Log($"[GameEventManager] SceneTransitionEvent handlers: {eventBus.GetHandlerCount<SceneTransitionEvent>()}");
        }
        
        private void OnSceneTransition(SceneTransitionEvent evt)
        {
            Debug.Log($"[GameEventManager] Scene transition: {evt.SceneName} - Phase: {evt.Phase}");
            
            switch (evt.Phase)
            {
                case SceneTransitionEvent.TransitionPhase.Started:
                    HandleSceneTransitionStart(evt);
                    break;
                case SceneTransitionEvent.TransitionPhase.Loading:
                    HandleSceneLoading(evt);
                    break;
                case SceneTransitionEvent.TransitionPhase.Completed:
                    HandleSceneTransitionComplete(evt);
                    break;
                case SceneTransitionEvent.TransitionPhase.Failed:
                    HandleSceneTransitionFailed(evt);
                    break;
            }
        }
        
        private void HandleSceneTransitionStart(SceneTransitionEvent evt)
        {
            // フェードアウト、UIの非表示など
            Debug.Log($"[GameEventManager] Starting transition to: {evt.SceneName}");
            
            // 他のサブシステムへ通知（例：UIManager、AudioManager等）
            // UIManager.Instance?.StartSceneTransition();
            // AudioManager.Instance?.FadeOutBGM();
        }
        
        private void HandleSceneLoading(SceneTransitionEvent evt)
        {
            // ローディング画面の更新
            Debug.Log($"[GameEventManager] Loading progress: {evt.Progress:P}");
            
            // LoadingUI.Instance?.UpdateProgress(evt.Progress);
        }
        
        private void HandleSceneTransitionComplete(SceneTransitionEvent evt)
        {
            // フェードイン、新しいシーンのUI表示など
            Debug.Log($"[GameEventManager] Transition completed: {evt.SceneName}");
            
            // UIManager.Instance?.EndSceneTransition();
            // AudioManager.Instance?.FadeInBGM();
        }
        
        private void HandleSceneTransitionFailed(SceneTransitionEvent evt)
        {
            // エラー処理
            Debug.LogError($"[GameEventManager] Failed to load scene: {evt.SceneName}");
            
            // エラーダイアログの表示など
            // UIManager.Instance?.ShowErrorDialog($"Failed to load scene: {evt.SceneName}");
        }
        
        private void OnDestroy()
        {
            // クリーンアップ
            foreach (var cleanup in cleanupActions)
            {
                cleanup?.Invoke();
            }
            cleanupActions.Clear();
            
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}