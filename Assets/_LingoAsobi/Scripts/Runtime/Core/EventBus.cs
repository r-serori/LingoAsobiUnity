using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Runtime.Core
{
    /// <summary>
    /// 改善版EventBus - シングルトンパターンを維持しつつ改善
    /// </summary>
    public class EventBus : MonoBehaviour
    {
        private static EventBus instance;
        private readonly Dictionary<Type, List<Delegate>> eventHandlers = new();
        private readonly object lockObject = new object();
        
        /// <summary>
        /// シングルトンインスタンス（既存コードとの互換性のため維持）
        /// </summary>
        public static EventBus Instance
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
            
            GameObject go = new GameObject("[EventBus]");
            instance = go.AddComponent<EventBus>();
            DontDestroyOnLoad(go);
            
            Debug.Log("[EventBus] Instance created and set to DontDestroyOnLoad");
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            Debug.Log("[EventBus] Initialized");
        }
        
        /// <summary>
        /// ハンドラーが存在するか確認
        /// </summary>
        public bool HasHandlers<T>() where T : IEvent
        {
            lock (lockObject)
            {
                return eventHandlers.TryGetValue(typeof(T), out var handlers) 
                    && handlers.Count > 0;
            }
        }
        
        /// <summary>
        /// デバッグ用: 登録されているハンドラーの数を取得
        /// </summary>
        public int GetHandlerCount<T>() where T : IEvent
        {
            lock (lockObject)
            {
                return eventHandlers.TryGetValue(typeof(T), out var handlers) 
                    ? handlers.Count : 0;
            }
        }
        
        /// <summary>
        /// イベントの購読（インスタンスメソッド版）
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler == null) return;
            
            lock (lockObject)
            {
                var type = typeof(T);
                if (!eventHandlers.ContainsKey(type))
                {
                    eventHandlers[type] = new List<Delegate>();
                }
                
                // 重複登録を防ぐ
                if (!eventHandlers[type].Contains(handler))
                {
                    eventHandlers[type].Add(handler);
                    Debug.Log($"[EventBus] Subscribed to {type.Name}. Total handlers: {eventHandlers[type].Count}");
                }
            }
        }
        
        /// <summary>
        /// イベントの購読解除（インスタンスメソッド版）
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler == null) return;
            
            lock (lockObject)
            {
                var type = typeof(T);
                if (eventHandlers.TryGetValue(type, out var handlers))
                {
                    handlers.Remove(handler);
                    Debug.Log($"[EventBus] Unsubscribed from {type.Name}. Remaining handlers: {handlers.Count}");
                    
                    // ハンドラーが0になったらリストを削除
                    if (handlers.Count == 0)
                    {
                        eventHandlers.Remove(type);
                    }
                }
            }
        }
        
        /// <summary>
        /// イベントの発行（インスタンスメソッド版）
        /// </summary>
        public void Publish<T>(T eventData) where T : IEvent
        {
            List<Delegate> handlersToInvoke = null;
            
            lock (lockObject)
            {
                var type = typeof(T);
                if (eventHandlers.TryGetValue(type, out var handlers))
                {
                    // ハンドラーリストのコピーを作成（イテレーション中の変更を防ぐ）
                    handlersToInvoke = new List<Delegate>(handlers);
                }
            }
            
            if (handlersToInvoke != null && handlersToInvoke.Count > 0)
            {
                Debug.Log($"[EventBus] Publishing {typeof(T).Name} to {handlersToInvoke.Count} handlers");
                
                foreach (var handler in handlersToInvoke)
                {
                    try
                    {
                        (handler as Action<T>)?.Invoke(eventData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventBus] Error handling event {typeof(T).Name}: {e.Message}\n{e.StackTrace}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[EventBus] No handlers for event {typeof(T).Name}");
            }
        }
        
        /// <summary>
        /// 全てのハンドラーをクリア（デバッグ/テスト用）
        /// </summary>
        public void ClearAll()
        {
            lock (lockObject)
            {
                eventHandlers.Clear();
                Debug.Log("[EventBus] All handlers cleared");
            }
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        // ===== 静的メソッド版（既存コードとの互換性のため） =====
        
        /// <summary>
        /// 静的メソッド版のSubscribe（既存コードとの互換性）
        /// </summary>
        public static void StaticSubscribe<T>(Action<T> handler) where T : IEvent
        {
            Instance.Subscribe(handler);
        }
        
        /// <summary>
        /// 静的メソッド版のUnsubscribe（既存コードとの互換性）
        /// </summary>
        public static void StaticUnsubscribe<T>(Action<T> handler) where T : IEvent
        {
            if (instance != null)
            {
                instance.Unsubscribe(handler);
            }
        }
        
        /// <summary>
        /// 静的メソッド版のPublish（既存コードとの互換性）
        /// </summary>
        public static void StaticPublish<T>(T eventData) where T : IEvent
        {
            Instance.Publish(eventData);
        }
    }
    
    /// <summary>
    /// イベントのベースインターフェース
    /// </summary>
    public interface IEvent { }
}