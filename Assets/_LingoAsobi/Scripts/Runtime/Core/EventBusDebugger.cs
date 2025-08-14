using UnityEngine;
using System.Collections.Generic;

namespace Scripts.Runtime.Core
{
    /// <summary>
    /// EventBusのデバッグ用コンポーネント
    /// </summary>
    public class EventBusDebugger : MonoBehaviour
    {
        [Header("Test Event Publishing")]
        [SerializeField] private bool testItemEvent;
        [SerializeField] private bool testLevelUpEvent;
        [SerializeField] private bool testCurrencyEvent;
        [SerializeField] private bool testStaminaEvent;
        
        [Header("Event Statistics")]
        [SerializeField] private List<string> registeredEventTypes = new List<string>();
        [SerializeField] private int totalEventsPublished = 0;
        
        private void Update()
        {
            // テスト用: Inspectorでフラグを立てるとイベントを発行
            if (testItemEvent)
            {
                testItemEvent = false;
                PublishTestItemEvent();
            }
            
            if (testLevelUpEvent)
            {
                testLevelUpEvent = false;
                PublishTestLevelUpEvent();
            }
            
            if (testCurrencyEvent)
            {
                testCurrencyEvent = false;
                PublishTestCurrencyEvent();
            }
            
            if (testStaminaEvent)
            {
                testStaminaEvent = false;
                PublishTestStaminaEvent();
            }
        }
        
        private void PublishTestItemEvent()
        {
            var evt = new ItemObtainedEvent
            {
                ItemId = "sword_01",
                ItemName = "Iron Sword",
                Quantity = 1,
                Rarity = ItemObtainedEvent.ItemRarity.Common
            };
            
            EventBus.Instance.Publish(evt);
            totalEventsPublished++;
            Debug.Log("[EventBusDebugger] Published test ItemObtainedEvent");
        }
        
        private void PublishTestLevelUpEvent()
        {
            var evt = new LevelUpEvent
            {
                OldLevel = 5,
                NewLevel = 6,
                ExperienceGained = 1000,
                CharacterId = "player"
            };
            
            EventBus.Instance.Publish(evt);
            totalEventsPublished++;
            Debug.Log("[EventBusDebugger] Published test LevelUpEvent");
        }
        
        private void PublishTestCurrencyEvent()
        {
            var evt = new CurrencyChangedEvent
            {
                CurrencyType = "Gold",
                OldAmount = 100,
                NewAmount = 150,
                Reason = "Quest Reward"
            };
            
            EventBus.Instance.Publish(evt);
            totalEventsPublished++;
            Debug.Log("[EventBusDebugger] Published test CurrencyChangedEvent");
        }
        
        private void PublishTestStaminaEvent()
        {
            var evt = new StaminaChangedEvent
            {
                OldStamina = 80,
                NewStamina = 60,
                MaxStamina = 100,
                Reason = "Sprint"
            };
            
            EventBus.Instance.Publish(evt);
            totalEventsPublished++;
            Debug.Log("[EventBusDebugger] Published test StaminaChangedEvent");
        }
        
        [ContextMenu("Update Event Statistics")]
        private void UpdateEventStatistics()
        {
            var eventBus = EventBus.Instance;
            registeredEventTypes.Clear();
            
            registeredEventTypes.Add($"ItemObtainedEvent: {eventBus.GetHandlerCount<ItemObtainedEvent>()} handlers");
            registeredEventTypes.Add($"LevelUpEvent: {eventBus.GetHandlerCount<LevelUpEvent>()} handlers");
            registeredEventTypes.Add($"CurrencyChangedEvent: {eventBus.GetHandlerCount<CurrencyChangedEvent>()} handlers");
            registeredEventTypes.Add($"StaminaChangedEvent: {eventBus.GetHandlerCount<StaminaChangedEvent>()} handlers");
            registeredEventTypes.Add($"SceneTransitionEvent: {eventBus.GetHandlerCount<SceneTransitionEvent>()} handlers");
            
            Debug.Log("[EventBusDebugger] Statistics updated");
        }
    }
}
