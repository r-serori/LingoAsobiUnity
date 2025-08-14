using System;
using UnityEngine;

namespace Scripts.Runtime.Core.Attributes
{
    /// <summary>
    /// スクリプトの実行順序を指定する属性
    /// エディタ拡張と組み合わせて使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExecutionOrderAttribute : Attribute
    {
        public int Order { get; }
        
        public ExecutionOrderAttribute(int order)
        {
            Order = order;
        }
    }
}