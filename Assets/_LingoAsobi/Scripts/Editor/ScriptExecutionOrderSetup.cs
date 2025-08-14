#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Scripts.Editor
{
    /// <summary>
    /// Script Execution Orderを自動的に設定するエディタスクリプト
    /// </summary>
    public static class ScriptExecutionOrderSetup
    {
        /// <summary>
        /// メニューから手動で実行
        /// </summary>
        [MenuItem("Tools/Setup Script Execution Order")]
        public static void SetupExecutionOrder()
        {
            Debug.Log("Setting up Script Execution Order...");
            
            // 設定したいスクリプトと実行順序のペア
            var scriptOrders = new Dictionary<string, int>
            {
                { "Scripts.Runtime.Core.GameEventHandlerRegistry", -300 },
                { "Scripts.Runtime.Core.EventBus", -250 },
                { "Scripts.Runtime.Utilities.Helpers.SceneHelper", -200 },
                { "Scripts.Runtime.Core.GameBootstrap", -100 },
                { "Scripts.Runtime.Core.GameEventManager", -150 }
            };
            
            int successCount = 0;
            int failCount = 0;
            
            foreach (var kvp in scriptOrders)
            {
                if (SetScriptExecutionOrder(kvp.Key, kvp.Value))
                {
                    Debug.Log($"✓ Set {kvp.Key} to {kvp.Value}");
                    successCount++;
                }
                else
                {
                    Debug.LogWarning($"✗ Failed to set {kvp.Key}");
                    failCount++;
                }
            }
            
            Debug.Log($"Script Execution Order Setup Complete: {successCount} success, {failCount} failed");
            
            if (successCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Changes saved successfully!");
            }
        }
        
        /// <summary>
        /// 特定のスクリプトの実行順序を設定
        /// </summary>
        private static bool SetScriptExecutionOrder(string className, int order)
        {
            // クラス名でMonoScriptを検索
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                
                if (script != null && script.GetClass() != null)
                {
                    string fullClassName = script.GetClass().FullName;
                    
                    if (fullClassName == className)
                    {
                        // 現在の実行順序を取得
                        int currentOrder = MonoImporter.GetExecutionOrder(script);
                        
                        if (currentOrder != order)
                        {
                            MonoImporter.SetExecutionOrder(script, order);
                            Debug.Log($"Changed execution order for {className}: {currentOrder} → {order}");
                        }
                        else
                        {
                            Debug.Log($"Execution order for {className} is already {order}");
                        }
                        
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// プロジェクトロード時に自動実行（オプション）
        /// </summary>
        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            // 自動設定を有効にする場合はコメントを解除
            // EditorApplication.delayCall += () =>
            // {
            //     Debug.Log("Auto-setting Script Execution Order...");
            //     SetupExecutionOrder();
            // };
        }
        
        /// <summary>
        /// 現在のScript Execution Order設定を表示
        /// </summary>
        [MenuItem("Tools/Show Current Script Execution Order")]
        public static void ShowCurrentExecutionOrder()
        {
            Debug.Log("===== Current Script Execution Order =====");
            
            var targetScripts = new List<string>
            {
                "Scripts.Runtime.Core.GameEventHandlerRegistry",
                "Scripts.Runtime.Core.EventBus",
                "Scripts.Runtime.Utilities.Helpers.SceneHelper",
                "Scripts.Runtime.Core.GameBootstrap",
                "Scripts.Runtime.Core.GameEventManager"
            };
            
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                
                if (script != null && script.GetClass() != null)
                {
                    string fullClassName = script.GetClass().FullName;
                    
                    if (targetScripts.Contains(fullClassName))
                    {
                        int order = MonoImporter.GetExecutionOrder(script);
                        if (order != 0)
                        {
                            Debug.Log($"{fullClassName}: {order}");
                        }
                        else
                        {
                            Debug.Log($"{fullClassName}: Default (0)");
                        }
                    }
                }
            }
            
            Debug.Log("==========================================");
        }
    }
}
#endif