using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Runtime.Core
{
    /// <summary>
    /// イベントハンドラーを自動的に登録・管理するレジストリ
    /// </summary>
    public class GameEventHandlerRegistry : MonoBehaviour
    {
        private static GameEventHandlerRegistry instance;
        private List<Action> unsubscribeActions = new List<Action>();
        
        // 各イベントに対するデフォルトハンドラーの有効/無効設定
        [Header("Event Handler Settings")]
        [SerializeField] private bool enableItemEvents = true;
        [SerializeField] private bool enableLevelEvents = true;
        [SerializeField] private bool enableCurrencyEvents = true;
        [SerializeField] private bool enableStaminaEvents = true;
        [SerializeField] private bool enableSceneEvents = true;
        [SerializeField] private bool enableQuestEvents = true;
        [SerializeField] private bool enableAchievementEvents = true;
        [SerializeField] private bool enableBattleEvents = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool verboseLogging = true;
        
        public static GameEventHandlerRegistry Instance
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
            
            GameObject go = new GameObject("[GameEventHandlerRegistry]");
            instance = go.AddComponent<GameEventHandlerRegistry>();
            DontDestroyOnLoad(go);
            
            Debug.Log("[GameEventHandlerRegistry] Instance created");
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            RegisterAllHandlers();
        }
        
        /// <summary>
        /// 全てのイベントハンドラーを登録
        /// </summary>
        private void RegisterAllHandlers()
        {
            var eventBus = EventBus.Instance;
            
            // ItemObtainedEvent
            if (enableItemEvents)
            {
                eventBus.Subscribe<ItemObtainedEvent>(OnItemObtained);
                unsubscribeActions.Add(() => eventBus.Unsubscribe<ItemObtainedEvent>(OnItemObtained));
            }
            
            // LevelUpEvent
            if (enableLevelEvents)
            {
                eventBus.Subscribe<LevelUpEvent>(OnLevelUp);
                unsubscribeActions.Add(() => eventBus.Unsubscribe<LevelUpEvent>(OnLevelUp));
            }
            
            // CurrencyChangedEvent
            if (enableCurrencyEvents)
            {
                eventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
                unsubscribeActions.Add(() => eventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged));
            }
            
            // StaminaChangedEvent
            if (enableStaminaEvents)
            {
                eventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
                unsubscribeActions.Add(() => eventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged));
            }
            
            // SceneTransitionEvent
            if (enableSceneEvents)
            {
                eventBus.Subscribe<SceneTransitionEvent>(OnSceneTransition);
                unsubscribeActions.Add(() => eventBus.Unsubscribe<SceneTransitionEvent>(OnSceneTransition));
            }
            
            // QuestCompletedEvent
            if (enableQuestEvents)
            {
                eventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
                unsubscribeActions.Add(() => eventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted));
            }
            
            // AchievementUnlockedEvent
            if (enableAchievementEvents)
            {
                eventBus.Subscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
                unsubscribeActions.Add(() => eventBus.Unsubscribe<AchievementUnlockedEvent>(OnAchievementUnlocked));
            }
            
            // BattleEndedEvent
            if (enableBattleEvents)
            {
                eventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
                unsubscribeActions.Add(() => eventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded));
            }
            
            LogRegistrationStatus();
        }
        
        private void LogRegistrationStatus()
        {
            if (!verboseLogging) return;
            
            var eventBus = EventBus.Instance;
            Debug.Log("[GameEventHandlerRegistry] Event Handler Registration Status:");
            Debug.Log($"  - ItemObtainedEvent: {eventBus.GetHandlerCount<ItemObtainedEvent>()} handlers");
            Debug.Log($"  - LevelUpEvent: {eventBus.GetHandlerCount<LevelUpEvent>()} handlers");
            Debug.Log($"  - CurrencyChangedEvent: {eventBus.GetHandlerCount<CurrencyChangedEvent>()} handlers");
            Debug.Log($"  - StaminaChangedEvent: {eventBus.GetHandlerCount<StaminaChangedEvent>()} handlers");
            Debug.Log($"  - SceneTransitionEvent: {eventBus.GetHandlerCount<SceneTransitionEvent>()} handlers");
            Debug.Log($"  - QuestCompletedEvent: {eventBus.GetHandlerCount<QuestCompletedEvent>()} handlers");
            Debug.Log($"  - AchievementUnlockedEvent: {eventBus.GetHandlerCount<AchievementUnlockedEvent>()} handlers");
            Debug.Log($"  - BattleEndedEvent: {eventBus.GetHandlerCount<BattleEndedEvent>()} handlers");
        }
        
        // ===== イベントハンドラー実装 =====
        
        private void OnItemObtained(ItemObtainedEvent evt)
        {
            if (verboseLogging)
                Debug.Log($"[Event] Item Obtained: {evt.ItemName} x{evt.Quantity} (Rarity: {evt.Rarity})");
            
            // UI更新の通知、サウンド再生など
            // UIManager.Instance?.ShowItemNotification(evt);
            // AudioManager.Instance?.PlayItemSound(evt.Rarity);
            // InventoryManager.Instance?.AddItem(evt.ItemId, evt.Quantity);
        }
        
        private void OnLevelUp(LevelUpEvent evt)
        {
            if (verboseLogging)
                Debug.Log($"[Event] Level Up! {evt.OldLevel} -> {evt.NewLevel} (+{evt.ExperienceGained} XP)");
            
            // レベルアップ演出、ステータス更新など
            // UIManager.Instance?.ShowLevelUpEffect(evt);
            // AudioManager.Instance?.PlayLevelUpSound();
            // PlayerStats.Instance?.UpdateLevel(evt.NewLevel);
        }
        
        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            if (verboseLogging)
            {
                string changeText = evt.ChangeAmount >= 0 ? $"+{evt.ChangeAmount}" : evt.ChangeAmount.ToString();
                Debug.Log($"[Event] {evt.CurrencyType} Changed: {evt.OldAmount} -> {evt.NewAmount} ({changeText}) Reason: {evt.Reason}");
            }
            
            // UI更新、エフェクト表示など
            // UIManager.Instance?.UpdateCurrencyDisplay(evt.CurrencyType, evt.NewAmount);
            // if (evt.ChangeAmount > 0) 
            //     UIManager.Instance?.ShowCurrencyGainEffect(evt);
        }
        
        private void OnStaminaChanged(StaminaChangedEvent evt)
        {
            if (verboseLogging)
                Debug.Log($"[Event] Stamina: {evt.NewStamina}/{evt.MaxStamina} ({evt.Percentage:P}) Reason: {evt.Reason}");
            
            // スタミナバー更新、警告表示など
            // UIManager.Instance?.UpdateStaminaBar(evt.Percentage);
            // if (evt.NewStamina == 0)
            //     UIManager.Instance?.ShowStaminaDepletedWarning();
        }
        
        private void OnSceneTransition(SceneTransitionEvent evt)
        {
            if (verboseLogging)
                Debug.Log($"[Event] Scene Transition: {evt.SceneName} - {evt.Phase} ({evt.Progress:P})");
            
            // シーン遷移処理
            switch (evt.Phase)
            {
                case SceneTransitionEvent.TransitionPhase.Started:
                    // FadeManager.Instance?.FadeOut();
                    break;
                case SceneTransitionEvent.TransitionPhase.Loading:
                    // LoadingScreen.Instance?.UpdateProgress(evt.Progress);
                    break;
                case SceneTransitionEvent.TransitionPhase.Completed:
                    // FadeManager.Instance?.FadeIn();
                    break;
            }
        }
        
        private void OnQuestCompleted(QuestCompletedEvent evt)
        {
            if (verboseLogging)
                Debug.Log($"[Event] Quest Completed: {evt.QuestName} (Rewards: {evt.RewardExperience} XP, {evt.RewardGold} Gold)");
            
            // クエスト完了通知、報酬付与など
            // UIManager.Instance?.ShowQuestCompleteNotification(evt);
            // QuestManager.Instance?.MarkQuestComplete(evt.QuestId);
        }
        
        private void OnAchievementUnlocked(AchievementUnlockedEvent evt)
        {
            if (verboseLogging)
                Debug.Log($"[Event] Achievement Unlocked: {evt.AchievementName} (+{evt.Points} points)");
            
            // 実績解除演出
            // UIManager.Instance?.ShowAchievementPopup(evt);
            // AudioManager.Instance?.PlayAchievementSound();
        }
        
        private void OnBattleEnded(BattleEndedEvent evt)
        {
            if (verboseLogging)
            {
                string result = evt.IsVictory ? "Victory" : "Defeat";
                Debug.Log($"[Event] Battle Ended: {result} (Duration: {evt.BattleDuration:F2}s, Damage Dealt: {evt.TotalDamageDealt})");
            }
            
            // バトル終了処理
            // BattleResultScreen.Instance?.Show(evt);
            // if (evt.IsVictory)
            //     AudioManager.Instance?.PlayVictoryMusic();
        }
        
        private void OnDestroy()
        {
            // 全てのイベントハンドラーを解除
            foreach (var unsubscribe in unsubscribeActions)
            {
                unsubscribe?.Invoke();
            }
            unsubscribeActions.Clear();
            
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}