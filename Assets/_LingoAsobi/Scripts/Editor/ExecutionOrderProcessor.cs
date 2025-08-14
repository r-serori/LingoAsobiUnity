#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using Scripts.Runtime.Core.Attributes;

namespace Scripts.Editor
{
    /// <summary>
    /// ExecutionOrderAttribute を自動的に処理
    /// </summary>
    [InitializeOnLoad]
    public static class ExecutionOrderProcessor
    {
        static ExecutionOrderProcessor()
        {
            EditorApplication.delayCall += ProcessAllScripts;
        }
        
        private static void ProcessAllScripts()
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            int updatedCount = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                
                if (script != null && script.GetClass() != null)
                {
                    var type = script.GetClass();
                    var attribute = type.GetCustomAttribute<ExecutionOrderAttribute>();
                    
                    if (attribute != null)
                    {
                        int currentOrder = MonoImporter.GetExecutionOrder(script);
                        if (currentOrder != attribute.Order)
                        {
                            MonoImporter.SetExecutionOrder(script, attribute.Order);
                            Debug.Log($"[ExecutionOrderProcessor] Set {type.Name} execution order to {attribute.Order}");
                            updatedCount++;
                        }
                    }
                }
            }
            
            if (updatedCount > 0)
            {
                Debug.Log($"[ExecutionOrderProcessor] Updated {updatedCount} script execution orders");
                AssetDatabase.SaveAssets();
            }
        }
        
        [MenuItem("Tools/Process Execution Order Attributes")]
        public static void ManualProcess()
        {
            ProcessAllScripts();
        }
    }
}
#endif
