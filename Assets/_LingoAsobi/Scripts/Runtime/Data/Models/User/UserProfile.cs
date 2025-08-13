using System;
using UnityEngine;

namespace Scripts.Runtime.Data.Models.User
{
  [System.Serializable]
  public class UserProfile
  {
    // 基本情報
    [Header("基本情報")]
    public string userId;
    public string userName;
    public string email;
    public string userIconUrl;

    // プレイヤーステータス
    [Header("プレイヤーステータス")]
    public int level;
    public int exp;
    public int nextLevelExp;

    // ゲーム内通貨
    [Header("ゲーム内通貨")]
    public int gold;
    public int gem;

    // スタミナシステム
    [Header("スタミナ")]
    public int stamina;
    public int maxStamina;
    public DateTime lastStaminaUpdateTime;
    public int staminaRecoverySeconds; // スタミナ回復間隔（秒）

    // お気に入りキャラクター
    [Header("キャラクター設定")]
    public string favoriteCharacterId;

    // ユーザー設定
    [Header("ユーザー設定")]
    public bool enableCharacterAnimation;
    public ImageQuality preferredImageQuality;
    public bool enableImagePreloading;
    public bool enableSoundEffects;
    public bool enableBGM;
    public float soundVolume;
    public float bgmVolume;

    // タイムスタンプ
    [Header("タイムスタンプ")]
    public DateTime createdAt;
    public DateTime lastLoginAt;
    public DateTime updatedAt;

    /// <summary>
    /// デフォルトコンストラクタ
    /// </summary>
    public UserProfile()
    {
      // デフォルト値の設定
      userId = Guid.NewGuid().ToString();
      userName = "Guest";
      level = 1;
      exp = 0;
      nextLevelExp = 100;
      gold = 0;
      gem = 0;
      stamina = 100;
      maxStamina = 100;
      staminaRecoverySeconds = 180; // 3分
      lastStaminaUpdateTime = DateTime.Now;

      // 設定のデフォルト値
      enableCharacterAnimation = true;
      preferredImageQuality = ImageQuality.High;
      enableImagePreloading = true;
      enableSoundEffects = true;
      enableBGM = true;
      soundVolume = 1.0f;
      bgmVolume = 0.7f;

      // タイムスタンプ
      createdAt = DateTime.Now;
      lastLoginAt = DateTime.Now;
      updatedAt = DateTime.Now;
    }

    /// <summary>
    /// 現在のスタミナを計算（時間回復を考慮）
    /// </summary>
    public int CalculateCurrentStamina()
    {
      if (stamina >= maxStamina)
      {
        return stamina;
      }

      var timePassed = DateTime.Now - lastStaminaUpdateTime;
      var recoveredStamina = (int)(timePassed.TotalSeconds / staminaRecoverySeconds);

      return Math.Min(stamina + recoveredStamina, maxStamina);
    }

    /// <summary>
    /// スタミナを消費
    /// </summary>
    /// <param name="amount">消費量</param>
    /// <returns>消費可能な場合true</returns>
    public bool ConsumeStamina(int amount)
    {
      int currentStamina = CalculateCurrentStamina();

      if (currentStamina < amount)
      {
        return false;
      }

      stamina = currentStamina - amount;
      lastStaminaUpdateTime = DateTime.Now;
      updatedAt = DateTime.Now;
      return true;
    }

    /// <summary>
    /// 経験値を追加
    /// </summary>
    /// <param name="amount">追加する経験値</param>
    /// <returns>レベルアップした場合true</returns>
    public bool AddExperience(int amount)
    {
      exp += amount;
      bool leveledUp = false;

      while (exp >= nextLevelExp)
      {
        exp -= nextLevelExp;
        level++;
        nextLevelExp = CalculateNextLevelExp(level);
        leveledUp = true;

        Debug.Log($"Level Up! New level: {level}");
      }

      updatedAt = DateTime.Now;
      return leveledUp;
    }

    /// <summary>
    /// 次のレベルに必要な経験値を計算
    /// </summary>
    private int CalculateNextLevelExp(int currentLevel)
    {
      // レベルが上がるごとに必要経験値が増加
      return 100 + (currentLevel * 50);
    }

    /// <summary>
    /// ゴールドを追加
    /// </summary>
    public void AddGold(int amount)
    {
      gold = Math.Max(0, gold + amount);
      updatedAt = DateTime.Now;
    }

    /// <summary>
    /// ジェムを追加
    /// </summary>
    public void AddGem(int amount)
    {
      gem = Math.Max(0, gem + amount);
      updatedAt = DateTime.Now;
    }
  }

  /// <summary>
  /// 画像品質の設定
  /// </summary>
  public enum ImageQuality
  {
    Low,
    Medium,
    High,
    Ultra
  }
}