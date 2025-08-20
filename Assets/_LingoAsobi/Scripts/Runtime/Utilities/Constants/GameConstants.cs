using UnityEngine;

namespace Scripts.Runtime.Utilities.Constants
{
  /// <summary>
  /// ゲーム全体で使用する定数定義
  /// </summary>
  public static class GameConstants
  {
    #region Scene Names

    /// <summary>
    /// シーン名の定義
    /// </summary>
    public static class Scenes
    {
      public const string Splash = "SplashScene";
      public const string Title = "TitleScene";
      public const string Home = "HomeScene";
      public const string Character = "CharacterScene";
      public const string Shop = "ShopScene";
      public const string Gacha = "GachaScene";
      public const string Training = "TrainingScene";
      public const string Grammar = "GrammarScene";
      public const string Pronunciation = "PronunciationScene";
      public const string Vocabulary = "VocabularyScene";
      public const string Conversation = "ConversationScene";
      public const string Speaking = "SpeakingScene";
      public const string Login = "LoginScene";
      public const string Register = "RegisterScene";
      public const string Battle = "BattleScene";
      public const string Inventory = "InventoryScene";
      public const string Settings = "SettingsScene";
      public const string Loading = "LoadingScene";
      public const string Result = "ResultScene";
    }

    #endregion

    #region UI Layers

    /// <summary>
    /// UIレイヤーの定義
    /// </summary>
    public static class UILayers
    {
      public const string Default = "Default";
      public const string UI = "UI";
      public const string Overlay = "Overlay";
      public const string Popup = "Popup";
      public const string Tutorial = "Tutorial";
      public const string Debug = "Debug";
    }

    /// <summary>
    /// ソートオーダー
    /// </summary>
    public static class SortOrders
    {
      public const int Background = -1000;
      public const int Default = 0;
      public const int Character = 100;
      public const int UI = 1000;
      public const int Popup = 2000;
      public const int Overlay = 3000;
      public const int Tutorial = 4000;
      public const int Debug = 9999;
    }

    #endregion

    #region Animation

    /// <summary>
    /// アニメーション関連の定数
    /// </summary>
    public static class Animation
    {
      public const float DefaultFadeDuration = 0.3f;
      public const float QuickFadeDuration = 0.15f;
      public const float SlowFadeDuration = 0.5f;

      public const float PopupShowDuration = 0.25f;
      public const float PopupHideDuration = 0.2f;

      public const float SceneTransitionDuration = 0.5f;

      // Ease Types (for custom animations)
      public const string EaseInOut = "easeInOut";
      public const string EaseIn = "easeIn";
      public const string EaseOut = "easeOut";
      public const string Linear = "linear";
      public const string Bounce = "bounce";
      public const string Elastic = "elastic";
    }

    #endregion

    #region Tags

    /// <summary>
    /// GameObjectのタグ定義
    /// </summary>
    public static class Tags
    {
      public const string Player = "Player";
      public const string Enemy = "Enemy";
      public const string UICamera = "UICamera";
      public const string MainCamera = "MainCamera";
      public const string GameController = "GameController";
      public const string Canvas = "Canvas";
    }

    #endregion

    #region PlayerPrefs Keys

    /// <summary>
    /// PlayerPrefsのキー定義
    /// </summary>
    public static class PlayerPrefsKeys
    {
      // User
      public const string CurrentUserId = "CurrentUserId";
      public const string LastLoginDate = "LastLoginDate";
      public const string IsFirstLaunch = "IsFirstLaunch";

      // Settings
      public const string MasterVolume = "MasterVolume";
      public const string BGMVolume = "BGMVolume";
      public const string SEVolume = "SEVolume";
      public const string Language = "Language";
      public const string GraphicsQuality = "GraphicsQuality";

      // Tutorial
      public const string TutorialCompleted = "TutorialCompleted";
      public const string TutorialStep = "TutorialStep";

      // Cache
      public const string CacheVersion = "CacheVersion";
      public const string LastCacheClear = "LastCacheClear";
    }

    #endregion

    #region Resource Paths

    /// <summary>
    /// リソースパスの定義
    /// </summary>
    public static class ResourcePaths
    {
      public const string CharacterSprites = "Sprites/Characters/";
      public const string ItemIcons = "Sprites/Items/";
      public const string UIElements = "UI/Elements/";
      public const string Prefabs = "Prefabs/";
      public const string Materials = "Materials/";
      public const string AudioClips = "Audio/";
      public const string Fonts = "Fonts/";
    }

    #endregion

    #region Game Settings

    /// <summary>
    /// ゲーム設定の定数
    /// </summary>
    public static class GameSettings
    {
      // Stamina
      public const int DefaultMaxStamina = 100;
      public const int StaminaRecoverySeconds = 180; // 3分

      // Level
      public const int MaxPlayerLevel = 999;
      public const int MaxCharacterLevel = 100;

      // Currency
      public const int MaxGold = 999999999;
      public const int MaxGem = 999999;

      // Inventory
      public const int DefaultInventorySize = 100;
      public const int MaxInventorySize = 500;

      // Battle
      public const float BattleTimeLimit = 180f; // 3分
      public const int MaxPartySize = 5;
    }

    #endregion

    #region Network

    /// <summary>
    /// ネットワーク関連の定数
    /// </summary>
    public static class Network
    {
      public const float DefaultTimeout = 30f;
      public const int MaxRetryCount = 3;
      public const float RetryDelay = 1f;

      // API Endpoints
      public const string ApiBaseUrl = "https://api.example.com";
      public const string ApiVersion = "v1";
    }

    #endregion

    #region Colors

    /// <summary>
    /// ゲーム内で使用する色定義
    /// </summary>
    public static class Colors
    {
      // UI Colors
      public static readonly Color Primary = new Color(0.2f, 0.6f, 1f);
      public static readonly Color Secondary = new Color(1f, 0.6f, 0.2f);
      public static readonly Color Success = new Color(0.2f, 0.8f, 0.2f);
      public static readonly Color Warning = new Color(1f, 0.8f, 0.2f);
      public static readonly Color Danger = new Color(1f, 0.2f, 0.2f);
      public static readonly Color Info = new Color(0.2f, 0.8f, 1f);

      // Rarity Colors
      public static readonly Color Common = new Color(0.7f, 0.7f, 0.7f);
      public static readonly Color Uncommon = new Color(0.2f, 0.8f, 0.2f);
      public static readonly Color Rare = new Color(0.2f, 0.6f, 1f);
      public static readonly Color Epic = new Color(0.8f, 0.2f, 1f);
      public static readonly Color Legendary = new Color(1f, 0.8f, 0.2f);

      // 黄色
      public static readonly Color ExpBar = new Color(1f, 0.8f, 0.2f);
    }

    #endregion
  }
}