using System;
using UnityEngine;

namespace Scripts.Runtime.Data.Models.Character
{
  /// <summary>
  /// キャラクターのステータス
  /// </summary>
  [Serializable]
  public class CharacterStats
  {
    [Header("基本ステータス")]
    public int hp = 100;
    public int attack = 10;
    public int defense = 10;
    public int speed = 10;

    [Header("特殊ステータス")]
    public float criticalRate = 0.05f;
    public float criticalDamage = 1.5f;
    public float evasionRate = 0.05f;

    /// <summary>
    /// 総合力を計算
    /// </summary>
    public int CalculatePower()
    {
      return hp + (attack * 2) + (defense * 2) + speed;
    }
  }

  /// <summary>
  /// キャラクターのレアリティ
  /// </summary>
  public enum Rarity
  {
    Common,     // ★
    Uncommon,   // ★★
    Rare,       // ★★★
    Epic,       // ★★★★
    Legendary   // ★★★★★
  }

  /// <summary>
  /// アニメーションタイプ
  /// </summary>
  public enum AnimationType
  {
    None,
    Idle,
    Walk,
    Run,
    Jump,
    Attack,
    Victory,
    Defeat,
    Special
  }

  /// <summary>
  /// キャラクター画像タイプ
  /// </summary>
  public enum CharacterImageType
  {
    Portrait,   // 立ち絵
    FullBody,   // 全身
    Avatar,     // アバター
    Icon        // アイコン
  }
}