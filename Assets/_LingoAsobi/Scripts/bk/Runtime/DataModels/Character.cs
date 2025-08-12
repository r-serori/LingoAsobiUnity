using UnityEngine;

namespace Scripts.Runtime.DataModels
{
  /// <summary>
  /// キャラクター情報を管理するデータクラス
  /// </summary>
  [System.Serializable]
  public class Character
  {
    [Header("基本情報")]
    public string characterId;           // "char_001", "char_002"
    public string characterName;         // "リンゴちゃん", "アソビくん"
    public string description;           // キャラクター説明

    [Header("画像リソース")]
    public string portraitImagePath;     // "Characters/char_001_portrait"
    public string fullBodyImagePath;     // "Characters/char_001_fullbody"
    public string avatarImagePath;     // "Characters/char_001_avatar"
    public string iconImagePath;         // "Characters/char_001_icon"

    [Header("アニメーション設定")]
    public CharacterAnimationType defaultAnimation = CharacterAnimationType.Idle;
    public float animationSpeed = 1.0f;
    public bool enableRandomAnimation = true;

    [Header("ゲーム設定")]
    public CharacterRarity rarity = CharacterRarity.Common;
    public bool isUnlocked = true;       // プレイヤーが所持しているか
    public bool isFavorite = false;      // お気に入り設定

    /// <summary>
    /// ユーザーの画像品質設定に基づいて適切な画像パスを取得
    /// </summary>
    public string GetImagePath(CharacterImageType imageType, ImageQuality quality)
    {
      string basePath = imageType switch
      {
        CharacterImageType.Portrait => portraitImagePath,
        CharacterImageType.FullBody => fullBodyImagePath,
        CharacterImageType.Icon => iconImagePath,
        _ => portraitImagePath
      };

      // 品質に基づく画像パスの調整
      string qualitySuffix = quality switch
      {
        ImageQuality.Low => "_low",
        ImageQuality.Medium => "_med",
        ImageQuality.High => "",
        _ => ""
      };

      return basePath + qualitySuffix;
    }
  }

  /// <summary>
  /// キャラクター画像タイプ
  /// </summary>
  public enum CharacterImageType
  {
    Portrait,   // 肖像画（顔中心）
    FullBody,   // 全身画像
    Avatar,     // アバター
    Icon        // アイコン（小さいサイズ）
  }

  /// <summary>
  /// キャラクターアニメーションタイプ
  /// </summary>
  public enum CharacterAnimationType
  {
    Idle,       // 待機
    Happy,      // 喜び
    Thinking,   // 考え中
    Greeting,   // 挨拶
    Surprised   // 驚き
  }

  /// <summary>
  /// キャラクターレアリティ
  /// </summary>
  public enum CharacterRarity
  {
    Common,     // 一般
    Rare,       // レア
    Epic,       // エピック
    Legendary   // レジェンダリー
  }
}