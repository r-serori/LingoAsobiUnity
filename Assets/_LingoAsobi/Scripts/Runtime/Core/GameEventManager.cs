using UnityEngine;
using System.Collections.Generic;

namespace Scripts.Runtime.Core
{
    /// <summary>
    /// ゲーム全体のイベントを管理する永続的なマネージャー
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
            // SceneTransitionEventのハンドラーを登録
            EventBus.Subscribe<SceneTransitionEvent>(OnSceneTransition);
            cleanupActions.Add(() => EventBus.Unsubscribe<SceneTransitionEvent>(OnSceneTransition));
            
            Debug.Log("[GameEventManager] Event handlers initialized");
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
            Debug.Log($"Starting transition to: {evt.SceneName}");
        }
        
        private void HandleSceneLoading(SceneTransitionEvent evt)
        {
            // ローディング画面の更新
            Debug.Log($"Loading progress: {evt.Progress:P}");
        }
        
        private void HandleSceneTransitionComplete(SceneTransitionEvent evt)
        {
            // フェードイン、新しいシーンのUI表示など
            Debug.Log($"Transition completed: {evt.SceneName}");
        }
        
        private void HandleSceneTransitionFailed(SceneTransitionEvent evt)
        {
            // エラー処理
            Debug.LogError($"Failed to load scene: {evt.SceneName}");
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