using System;
using UnityEngine;

namespace Scripts.Runtime.Core
{
    /// <summary>
    /// イベントの基底クラス
    /// </summary>
    public abstract class BaseEvent : IEvent
    {
        public DateTime Timestamp { get; }
        
        protected BaseEvent()
        {
            Timestamp = DateTime.Now;
        }
    }

    // ===== ゲームプレイ関連イベント =====
    
    /// <summary>
    /// アイテム取得イベント
    /// </summary>
    public class ItemObtainedEvent : BaseEvent
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public ItemRarity Rarity { get; set; }
        
        public enum ItemRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }
    }

    /// <summary>
    /// レベルアップイベント
    /// </summary>
    public class LevelUpEvent : BaseEvent
    {
        public int OldLevel { get; set; }
        public int NewLevel { get; set; }
        public int ExperienceGained { get; set; }
        public string CharacterId { get; set; }
    }

    /// <summary>
    /// 通貨変更イベント
    /// </summary>
    public class CurrencyChangedEvent : BaseEvent
    {
        public string CurrencyType { get; set; } // "Gold", "Gems", etc.
        public int OldAmount { get; set; }
        public int NewAmount { get; set; }
        public int ChangeAmount => NewAmount - OldAmount;
        public string Reason { get; set; } // "Purchase", "Reward", "Quest", etc.
    }

    /// <summary>
    /// スタミナ変更イベント
    /// </summary>
    public class StaminaChangedEvent : BaseEvent
    {
        public int OldStamina { get; set; }
        public int NewStamina { get; set; }
        public int MaxStamina { get; set; }
        public float Percentage => MaxStamina > 0 ? (float)NewStamina / MaxStamina : 0f;
        public string Reason { get; set; } // "Used", "Recovered", "Item", etc.
    }

    /// <summary>
    /// シーン遷移イベント
    /// </summary>
    public class SceneTransitionEvent : BaseEvent
    {
        public string SceneName { get; set; }
        public bool IsAdditive { get; set; }
        public float Progress { get; set; }
        public TransitionPhase Phase { get; set; }

        public enum TransitionPhase
        {
            Started,
            Loading,
            Completed,
            Failed
        }
        
        public SceneTransitionEvent(string sceneName, TransitionPhase phase = TransitionPhase.Started)
        {
            SceneName = sceneName;
            Phase = phase;
        }
    }

    // ===== 追加の一般的なゲームイベント =====
    
    /// <summary>
    /// クエスト完了イベント
    /// </summary>
    public class QuestCompletedEvent : BaseEvent
    {
        public string QuestId { get; set; }
        public string QuestName { get; set; }
        public int RewardExperience { get; set; }
        public int RewardGold { get; set; }
    }

    /// <summary>
    /// 実績解除イベント
    /// </summary>
    public class AchievementUnlockedEvent : BaseEvent
    {
        public string AchievementId { get; set; }
        public string AchievementName { get; set; }
        public int Points { get; set; }
    }

    /// <summary>
    /// バトル終了イベント
    /// </summary>
    public class BattleEndedEvent : BaseEvent
    {
        public bool IsVictory { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public float BattleDuration { get; set; }
    }
}
