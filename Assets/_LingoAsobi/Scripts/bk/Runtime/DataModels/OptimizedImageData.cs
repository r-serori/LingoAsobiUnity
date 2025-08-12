using UnityEngine;
using System.Collections.Generic;


namespace Scripts.Runtime.DataModels
{
    /// <summary>
    /// 画像品質設定
    /// ユーザーの環境や設定に応じて画像の品質を制御するために使用
    /// </summary>
    public enum ImageQuality
    {
        Low,      // 低品質（512px以下、圧縮高）
        Medium,   // 中品質（1024px以下、圧縮中）
        High      // 高品質（フル解像度、圧縮低）
    }

    /// <summary>
    /// 画像カテゴリ分類
    /// リソース管理とプリロード戦略に使用
    /// </summary>
    public enum ImageCategory
    {
        Character,   // キャラクター画像
        Background,  // 背景画像
        UI,          // UI要素画像
        Item         // アイテム画像
    }

    [System.Serializable]
    public struct ImageReference
    {
        public string imageKey;      // "char_001"
        public ImageCategory category; // enum for grouping
        public ImageQuality quality;   // Low, Medium, High

        public string GetAddressableKey()
        {
            return $"{category.ToString().ToLower()}_{imageKey}_{quality.ToString().ToLower()}";
        }
    }
}