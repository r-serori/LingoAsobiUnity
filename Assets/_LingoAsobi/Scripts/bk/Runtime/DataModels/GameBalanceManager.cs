using UnityEngine;

namespace Scripts.Runtime.DataModels
{
  /// <summary>
  /// ゲームバランス設定
  /// MonoBehaviour Singleton パターンで全シーンで利用可能
  /// 命名統一: GameBalanceManager → GameBalanceManager
  /// </summary>
  public class GameBalanceManager : MonoBehaviour
  {
    #region Singleton Pattern

    private static GameBalanceManager _instance;
    public static GameBalanceManager Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = FindObjectOfType<GameBalanceManager>();

          if (_instance == null)
          {
            // 自動生成
            GameObject go = new GameObject("GameBalanceManager");
            _instance = go.AddComponent<GameBalanceManager>();
            DontDestroyOnLoad(go);
          }
        }
        return _instance;
      }
    }

    #endregion

    [Header("経験値設定")]
    [SerializeField] private int baseExpRequired = 100;          // レベル1の必要経験値
    [SerializeField] private float expGrowthRate = 1.02f;        // 成長率（2%ずつ増加）
    [SerializeField] private int maxLevel = 100;                 // 最大レベル

    [Header("スタミナ設定")]
    [SerializeField] private int baseStaminaMax = 50;            // レベル1の最大スタミナ
    [SerializeField] private float staminaGrowthRate = 1.01f;    // 成長率（5%ずつ増加）
    [SerializeField] private int staminaPerLevel = 3;            // レベル毎の固定増加量

    [Header("特別設定")]
    [SerializeField] private bool useHybridStaminaGrowth = true; // 混合成長式使用
    [SerializeField] private AnimationCurve customGrowthCurve;   // カスタム成長曲線

    void Awake()
    {
      // Singleton 重複チェック
      if (_instance != null && _instance != this)
      {
        Debug.LogWarning("GameBalanceManager の重複インスタンスを破棄します。");
        Destroy(gameObject);
        return;
      }

      _instance = this;
      DontDestroyOnLoad(gameObject);

      Debug.Log("GameBalanceManager Singleton 初期化完了");
    }

    /// <summary>
    /// 指定レベルの必要経験値を計算
    /// </summary>
    public int GetExpRequiredForLevel(int level)
    {
      if (level <= 1) return baseExpRequired;

      // 指数関数的成長: base * (growthRate ^ (level-1))
      return Mathf.RoundToInt(baseExpRequired * Mathf.Pow(expGrowthRate, level - 1));
    }

    /// <summary>
    /// 指定レベルまでの総必要経験値を計算
    /// </summary>
    public int GetTotalExpRequiredForLevel(int targetLevel)
    {
      int totalExp = 0;
      for (int i = 1; i < targetLevel; i++)
      {
        totalExp += GetExpRequiredForLevel(i);
      }
      return totalExp;
    }

    /// <summary>
    /// 指定レベルの最大スタミナを計算
    /// </summary>
    public int GetMaxStaminaForLevel(int level)
    {
      if (level <= 1) return baseStaminaMax;

      if (useHybridStaminaGrowth)
      {
        // 混合成長式: 指数関数的 + 線形的
        float exponentialGrowth = baseStaminaMax * Mathf.Pow(staminaGrowthRate, level - 1);
        int linearGrowth = staminaPerLevel * (level - 1);
        return Mathf.RoundToInt(exponentialGrowth + linearGrowth);
      }
      else
      {
        // 純粋な指数関数的成長
        return Mathf.RoundToInt(baseStaminaMax * Mathf.Pow(staminaGrowthRate, level - 1));
      }
    }

    /// <summary>
    /// カスタム成長曲線を使用したスタミナ計算
    /// </summary>
    public int GetCustomStaminaForLevel(int level)
    {
      if (customGrowthCurve == null) return GetMaxStaminaForLevel(level);

      // レベルを0-1の範囲に正規化
      float normalizedLevel = Mathf.Clamp01((float)(level - 1) / (maxLevel - 1));
      float curveValue = customGrowthCurve.Evaluate(normalizedLevel);

      // ベース値に曲線値を適用
      return Mathf.RoundToInt(baseStaminaMax * curveValue);
    }

    /// <summary>
    /// レベルアップに必要な総経験値を取得
    /// </summary>
    public int GetTotalExpForLevelUp(int currentLevel, int targetLevel)
    {
      if (currentLevel >= targetLevel) return 0;

      int totalExp = 0;
      for (int i = currentLevel; i < targetLevel; i++)
      {
        totalExp += GetExpRequiredForLevel(i);
      }
      return totalExp;
    }

    /// <summary>
    /// 現在の経験値から次のレベルまでの進行度を取得 (0.0 ~ 1.0)
    /// </summary>
    public float GetLevelProgressRatio(int currentLevel, int currentExp)
    {
      if (currentLevel >= maxLevel) return 1.0f;

      int expForCurrentLevel = GetTotalExpRequiredForLevel(currentLevel);
      int expForNextLevel = GetTotalExpRequiredForLevel(currentLevel + 1);
      int expInCurrentLevel = currentExp - expForCurrentLevel;
      int expRequiredForNextLevel = expForNextLevel - expForCurrentLevel;

      return Mathf.Clamp01((float)expInCurrentLevel / expRequiredForNextLevel);
    }

    void OnDestroy()
    {
      if (_instance == this)
      {
        _instance = null;
      }
    }
  }
}