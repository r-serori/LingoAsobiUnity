using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Runtime.Core
{
  /// <summary>
  /// イベントバスシステム
  /// 疎結合なコンポーネント間通信を実現
  /// </summary>
  public class EventBus
  {
    // シングルトンインスタンス
    private static EventBus _instance;
    public static EventBus Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = new EventBus();
        }
        return _instance;
      }
    }

    // イベントハンドラーのディクショナリ
    private readonly Dictionary<Type, List<Delegate>> _eventHandlers;

    // イベント履歴（デバッグ用）
    private readonly Queue<EventInfo> _eventHistory;
    private const int MaxHistorySize = 50;

    /// <summary>
    /// プライベートコンストラクタ
    /// </summary>
    private EventBus()
    {
      _eventHandlers = new Dictionary<Type, List<Delegate>>();
      _eventHistory = new Queue<EventInfo>();
    }

    /// <summary>
    /// イベントハンドラーを登録
    /// </summary>
    /// <typeparam name="T">イベントの型</typeparam>
    /// <param name="handler">ハンドラー</param>
    public void Subscribe<T>(Action<T> handler) where T : IGameEvent
    {
      Type eventType = typeof(T);

      if (!_eventHandlers.ContainsKey(eventType))
      {
        _eventHandlers[eventType] = new List<Delegate>();
      }

      _eventHandlers[eventType].Add(handler);
      Debug.Log($"[EventBus] Subscribed to {eventType.Name}");
    }

    /// <summary>
    /// イベントハンドラーを解除
    /// </summary>
    /// <typeparam name="T">イベントの型</typeparam>
    /// <param name="handler">ハンドラー</param>
    public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
    {
      Type eventType = typeof(T);

      if (_eventHandlers.ContainsKey(eventType))
      {
        _eventHandlers[eventType].Remove(handler);

        if (_eventHandlers[eventType].Count == 0)
        {
          _eventHandlers.Remove(eventType);
        }

        Debug.Log($"[EventBus] Unsubscribed from {eventType.Name}");
      }
    }

    /// <summary>
    /// イベントを発行
    /// </summary>
    /// <typeparam name="T">イベントの型</typeparam>
    /// <param name="gameEvent">イベントデータ</param>
    public void Publish<T>(T gameEvent) where T : IGameEvent
    {
      Type eventType = typeof(T);

      // イベント履歴に追加
      AddToHistory(eventType, gameEvent);

      if (_eventHandlers.ContainsKey(eventType))
      {
        var handlers = new List<Delegate>(_eventHandlers[eventType]);

        foreach (var handler in handlers)
        {
          try
          {
            (handler as Action<T>)?.Invoke(gameEvent);
          }
          catch (Exception e)
          {
            Debug.LogError($"[EventBus] Error handling event {eventType.Name}: {e.Message}");
          }
        }

        Debug.Log($"[EventBus] Published {eventType.Name} to {handlers.Count} handlers");
      }
      else
      {
        Debug.LogWarning($"[EventBus] No handlers for event {eventType.Name}");
      }
    }

    /// <summary>
    /// すべてのイベントハンドラーをクリア
    /// </summary>
    public void ClearAll()
    {
      _eventHandlers.Clear();
      _eventHistory.Clear();
      Debug.Log("[EventBus] All event handlers cleared");
    }

    /// <summary>
    /// 特定のイベントタイプのハンドラーをクリア
    /// </summary>
    public void Clear<T>() where T : IGameEvent
    {
      Type eventType = typeof(T);

      if (_eventHandlers.ContainsKey(eventType))
      {
        _eventHandlers.Remove(eventType);
        Debug.Log($"[EventBus] Cleared handlers for {eventType.Name}");
      }
    }

    /// <summary>
    /// イベント履歴に追加
    /// </summary>
    private void AddToHistory(Type eventType, IGameEvent gameEvent)
    {
      if (_eventHistory.Count >= MaxHistorySize)
      {
        _eventHistory.Dequeue();
      }

      _eventHistory.Enqueue(new EventInfo
      {
        EventType = eventType,
        Event = gameEvent,
        Timestamp = DateTime.Now
      });
    }

    /// <summary>
    /// イベント履歴を取得
    /// </summary>
    public EventInfo[] GetEventHistory()
    {
      return _eventHistory.ToArray();
    }

    /// <summary>
    /// デバッグ情報を出力
    /// </summary>
    public void PrintDebugInfo()
    {
      Debug.Log("=== EventBus Debug Info ===");
      Debug.Log($"Total event types: {_eventHandlers.Count}");

      foreach (var kvp in _eventHandlers)
      {
        Debug.Log($"  {kvp.Key.Name}: {kvp.Value.Count} handlers");
      }

      Debug.Log($"Event history: {_eventHistory.Count} events");
    }
  }

  /// <summary>
  /// ゲームイベントの基底インターフェース
  /// </summary>
  public interface IGameEvent
  {
  }

  /// <summary>
  /// イベント情報（履歴用）
  /// </summary>
  public struct EventInfo
  {
    public Type EventType;
    public IGameEvent Event;
    public DateTime Timestamp;
  }

  #region Common Game Events

  /// <summary>
  /// レベルアップイベント
  /// </summary>
  public class LevelUpEvent : IGameEvent
  {
    public string UserId { get; set; }
    public int OldLevel { get; set; }
    public int NewLevel { get; set; }
  }

  /// <summary>
  /// スタミナ変更イベント
  /// </summary>
  public class StaminaChangedEvent : IGameEvent
  {
    public int OldStamina { get; set; }
    public int NewStamina { get; set; }
  }

  /// <summary>
  /// 通貨変更イベント
  /// </summary>
  public class CurrencyChangedEvent : IGameEvent
  {
    public enum CurrencyType { Gold, Gem }

    public CurrencyType Type { get; set; }
    public int OldAmount { get; set; }
    public int NewAmount { get; set; }
  }

  /// <summary>
  /// アイテム取得イベント
  /// </summary>
  public class ItemObtainedEvent : IGameEvent
  {
    public string ItemId { get; set; }
    public int Quantity { get; set; }
  }

  /// <summary>
  /// クエスト完了イベント
  /// </summary>
  public class QuestCompletedEvent : IGameEvent
  {
    public string QuestId { get; set; }
    public List<string> RewardItemIds { get; set; }
    public int ExpReward { get; set; }
    public int GoldReward { get; set; }
  }

  /// <summary>
  /// シーン遷移イベント
  /// </summary>
  public class SceneTransitionEvent : IGameEvent
  {
    public string FromScene { get; set; }
    public string ToScene { get; set; }
  }

  #endregion
}