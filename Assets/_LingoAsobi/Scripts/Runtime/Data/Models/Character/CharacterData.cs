using System;
using UnityEngine;

namespace Scripts.Runtime.Data.Models.Character
{

  public enum CharacterAttribute
  {
    Fire,
    Water,
    Wood,
    Light,
    Dark,
    Neutral,
  }

  /// <summary>
  /// キャラクターデータモデル
  /// ゲーム内のキャラクター情報を管理
  /// </summary>
  [Serializable]
  public class CharacterData
  {
    [Header("基本情報")]
    public string characterId;
    public string characterName;
    public string description;
    public CharacterAttribute attribute;

    [Header("画像リソース")]
    public string portraitImagePath;      // 立ち絵
    public string fullBodyImagePath;      // 全身画像
    public string avatarImagePath;        // アバター画像
    public string iconImagePath;          // アイコン画像

    [Header("アニメーション設定")]
    public AnimationType defaultAnimation;
    public float animationSpeed = 1.0f;
    public bool enableRandomAnimation = true;

    [Header("ゲームデータ")]
    public Rarity rarity;
    public int level = 1;
    public CharacterStats stats;

    [Header("ステータス")]
    public bool isUnlocked;
    public bool isFavorite;
    public DateTime unlockedDate;

    /// <summary>
    /// デフォルトコンストラクタ
    /// </summary>
    public CharacterData()
    {
      characterId = Guid.NewGuid().ToString();
      stats = new CharacterStats();
      unlockedDate = DateTime.MinValue;
    }

    /// <summary>
    /// キャラクターが利用可能かチェック
    /// </summary>
    public bool IsAvailable()
    {
      return isUnlocked;
    }

    /// <summary>
    /// お気に入り状態を切り替え
    /// </summary>
    public void ToggleFavorite()
    {
      isFavorite = !isFavorite;
    }
  }
}