using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.DataModels;
using Scripts.Runtime.MockData;
using Scripts.Runtime.Services;

namespace Scripts.Runtime.Views.Shared.Header
{
  /// <summary>
  /// ProfileHeaderプレハブ全体を管理するコントローラー
  /// 軽量なImage-based Progress Bar実装
  /// </summary>
  public class ProfileHeaderController : MonoBehaviour
  {
    [Header("UI References")]
    [SerializeField] private ProfileImageView profileImageView;
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemText;

    [Header("経験値バー（Image実装）")]
    [SerializeField] private Image expMaxImage;       // 背景画像（最大値）
    [SerializeField] private Image expProgressImage;  // 進行画像（現在値）
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private GameObject expBarContainer;

    [Header("スタミナバー（Image実装）")]
    [SerializeField] private Image staminaMaxImage;       // 背景画像（最大値）
    [SerializeField] private Image staminaProgressImage;  // 進行画像（現在値）
    [SerializeField] private TextMeshProUGUI staminaValueText;
    [SerializeField] private GameObject staminaBarContainer;

    [Header("Game Balance")]
    [SerializeField] private GameBalanceManager gameBalance; // Inspector で設定

    [Header("Progress Bar 設定")]
    [SerializeField] private bool useAnimatedProgress = true;
    [SerializeField] private float progressAnimationSpeed = 2.0f;

    [Header("レイアウト設定")]
    [Range(0.08f, 0.15f)]
    [SerializeField] private float headerHeightRatio = 0.1f;
    [SerializeField] private bool enableResponsiveLayout = true;
    [SerializeField] private bool autoSetupOnStart = true;

    [Header("Service Dependencies")]
    [SerializeField] private UserManager userService;  // 🆕 Service層統一

    [Header("Test Data")]
    [SerializeField] private MockUser testMockUser;
    [SerializeField] private bool useTestDataOnStart = true;

    private User currentUser;
    private RectTransform headerRectTransform;
    private Vector2 originalSizeDelta;
    private bool isLayoutSetup = false;

    void Awake()
    {
      headerRectTransform = GetComponent<RectTransform>();
      if (headerRectTransform != null)
      {
        originalSizeDelta = headerRectTransform.sizeDelta;
      }

      // Progress Image の初期設定
      SetupProgressImages();
    }

    void Start()
    {
      if (autoSetupOnStart)
      {
        SetupHeaderLayout();
      }

      // Service 層からデータ取得（統一アーキテクチャ）
      if (userService != null)
      {
        userService.OnUserDataUpdated += OnUserDataUpdated;
        LoadUserDataFromService();
      }
      else if (useTestDataOnStart && testMockUser != null)
      {
        // フォールバック: MockUser 直接使用
        SetUser(testMockUser.user);
      }
    }

    /// <summary>
    /// Service層からユーザーデータを読み込み
    /// </summary>
    private async void LoadUserDataFromService()
    {
      try
      {
        var userData = await userService.GetCurrentUserAsync();
        SetUser(userData);
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"ProfileHeaderController: ユーザーデータ読み込みエラー - {ex.Message}");

        // エラー時のフォールバック
        if (testMockUser?.user != null)
        {
          SetUser(testMockUser.user);
        }
      }
    }

    /// <summary>
    /// ユーザーデータ更新イベントハンドラー
    /// </summary>
    private void OnUserDataUpdated(User updatedUser)
    {
      if (updatedUser != null)
      {
        SetUser(updatedUser);
        Debug.Log($"ProfileHeaderController: ユーザーデータ更新 - {updatedUser.userName}");
      }
    }

    /// <summary>
    /// Progress Bar用のImageを初期設定
    /// </summary>
    private void SetupProgressImages()
    {
      // 経験値バーの設定
      if (expProgressImage != null)
      {
        expProgressImage.type = Image.Type.Filled;
        expProgressImage.fillMethod = Image.FillMethod.Horizontal;
        expProgressImage.fillOrigin = 0; // 左から右へ
      }

      // スタミナバーの設定
      if (staminaProgressImage != null)
      {
        staminaProgressImage.type = Image.Type.Filled;
        staminaProgressImage.fillMethod = Image.FillMethod.Horizontal;
        staminaProgressImage.fillOrigin = 0; // 左から右へ
      }
    }

    /// <summary>
    /// ヘッダーのレイアウトを画面サイズに合わせて設定
    /// </summary>
    public void SetupHeaderLayout()
    {
      if (headerRectTransform == null) return;

      float headerHeight = Screen.height * headerHeightRatio;

      Canvas parentCanvas = GetComponentInParent<Canvas>();
      if (parentCanvas != null)
      {
        CanvasScaler canvasScaler = parentCanvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
          float scaleFactor = canvasScaler.referenceResolution.y / Screen.height;
          headerHeight *= scaleFactor;
        }
      }

      headerRectTransform.sizeDelta = new Vector2(headerRectTransform.sizeDelta.x, headerHeight);
      headerRectTransform.anchorMin = new Vector2(0, 1);
      headerRectTransform.anchorMax = new Vector2(1, 1);
      headerRectTransform.pivot = new Vector2(0.5f, 1);
      headerRectTransform.anchoredPosition = Vector2.zero;

      isLayoutSetup = true;

      Debug.Log($"ProfileHeader レイアウト設定完了: Height={headerHeight}, Ratio={headerHeightRatio}");
    }

    /// <summary>
    /// ユーザー情報を設定して全UIを更新
    /// </summary>
    public void SetUser(User user)
    {
      if (user == null)
      {
        Debug.LogWarning("ProfileHeaderController: User が null です");
        return;
      }

      currentUser = user;

      if (profileImageView != null)
      {
        profileImageView.SetUser(user);
      }

      UpdateUserTexts(user);
      UpdateExperienceBar(user);
      UpdateStaminaBar(user);

      Debug.Log($"ProfileHeader更新完了: {user.userName}");
    }

    /// <summary>
    /// ユーザー情報のテキストを更新
    /// </summary>
    private void UpdateUserTexts(User user)
    {
      if (userNameText != null)
        userNameText.text = user.userName ?? "Unknown";

      if (levelText != null)
        levelText.text = $"Lv.{user.level}";

      if (goldText != null)
        goldText.text = user.gold.ToString("N0");

      if (gemText != null)
        gemText.text = user.gem.ToString();

      if (staminaValueText != null)
        staminaValueText.text = $"{user.stamina} / {user.GetMaxStamina(gameBalance)}";
    }

    /// <summary>
    /// 経験値バーを更新（Image.fillAmount使用）
    /// </summary>
    private void UpdateExperienceBar(User user)
    {
      if (expProgressImage == null || gameBalance == null) return;

      // GameBalanceConfigから進行度を取得
      float expRatio = user.GetExpProgressRatio(gameBalance);

      // アニメーション付きで更新
      if (useAnimatedProgress)
      {
        UpdateProgressImageAnimated(expProgressImage, expRatio);
      }
      else
      {
        expProgressImage.fillAmount = expRatio;
      }

      // テキスト更新
      if (expText != null)
      {
        int currentLevelTotalExp = gameBalance.GetTotalExpRequiredForLevel(user.level);
        int nextLevelTotalExp = gameBalance.GetTotalExpRequiredForLevel(user.level + 1);
        int currentLevelExp = user.exp - currentLevelTotalExp;
        int requiredForLevel = nextLevelTotalExp - currentLevelTotalExp;

        expText.text = $"{currentLevelExp:N0}/{requiredForLevel:N0}";
      }

      if (expBarContainer != null)
      {
        expBarContainer.SetActive(true);
      }

      Debug.Log($"経験値バー更新: Progress={expRatio:P1}");
    }

    /// <summary>
    /// スタミナバーを更新（GameBalanceConfig使用）
    /// </summary>
    private void UpdateStaminaBar(User user)
    {
      if (staminaProgressImage == null || gameBalance == null) return;

      // GameBalanceConfigから進行度を取得
      float staminaRatio = user.GetStaminaProgressRatio(gameBalance);
      int maxStamina = user.GetMaxStamina(gameBalance);

      // アニメーション付きで更新
      if (useAnimatedProgress)
      {
        UpdateProgressImageAnimated(staminaProgressImage, staminaRatio);
      }
      else
      {
        staminaProgressImage.fillAmount = staminaRatio;
      }

      // テキスト更新
      if (staminaValueText != null)
      {
        staminaValueText.text = $"{user.stamina} / {maxStamina}";
      }

      if (staminaBarContainer != null)
      {
        staminaBarContainer.SetActive(true);
      }

      Debug.Log($"スタミナバー更新: {user.stamina} / {maxStamina} ({staminaRatio:P1})");
    }

    /// <summary>
    /// Progress Imageをアニメーション付きで更新
    /// </summary>
    private void UpdateProgressImageAnimated(Image progressImage, float targetFillAmount)
    {
      if (progressImage == null) return;

      // DOTweenが利用可能な場合はそちらを使用、そうでなければコルーチン
#if DOTWEEN_ENABLED
            progressImage.DOFillAmount(targetFillAmount, 1f / progressAnimationSpeed);
#else
      StartCoroutine(AnimateProgressBar(progressImage, targetFillAmount));
#endif
    }

    /// <summary>
    /// Progress Barアニメーション（コルーチン版）
    /// </summary>
    private System.Collections.IEnumerator AnimateProgressBar(Image progressImage, float targetFillAmount)
    {
      float startFillAmount = progressImage.fillAmount;
      float elapsedTime = 0f;
      float duration = 1f / progressAnimationSpeed;

      while (elapsedTime < duration)
      {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;

        // Ease Out Cubic カーブ
        t = 1f - Mathf.Pow(1f - t, 3f);

        progressImage.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, t);
        yield return null;
      }

      progressImage.fillAmount = targetFillAmount;
    }

    /// <summary>
    /// レベルに基づいて必要経験値を計算
    /// </summary>
    private int CalculateMaxExpForLevel(int level)
    {
      return level * 100;
    }

    /// <summary>
    /// スタミナ消費（最大値チェック付き）
    /// </summary>
    public void ConsumeStamina(int amount)
    {
      if (currentUser == null || gameBalance == null) return;

      currentUser.stamina = Mathf.Max(0, currentUser.stamina - amount);
      UpdateStaminaBar(currentUser);

      Debug.Log($"スタミナ消費: -{amount} (残り: {currentUser.stamina}/{currentUser.GetMaxStamina(gameBalance)})");
    }

    /// <summary>
    /// スタミナ回復（最大値チェック付き）
    /// </summary>
    public void RecoverStamina(int amount)
    {
      if (currentUser == null || gameBalance == null) return;

      int maxStamina = currentUser.GetMaxStamina(gameBalance);
      currentUser.stamina = Mathf.Min(maxStamina, currentUser.stamina + amount);
      UpdateStaminaBar(currentUser);

      Debug.Log($"スタミナ回復: +{amount} (現在: {currentUser.stamina}/{maxStamina})");
    }

    /// <summary>
    /// 経験値を追加
    /// </summary>
    public void AddExperience(int addExp)
    {
      if (currentUser == null) return;

      int oldLevel = currentUser.level;
      currentUser.exp += addExp;

      CheckLevelUp(oldLevel);
      UpdateExperienceBar(currentUser);

      if (oldLevel != currentUser.level)
      {
        UpdateUserTexts(currentUser);
      }

      Debug.Log($"経験値追加: +{addExp} (総経験値: {currentUser.exp})");
    }

    /// <summary>
    /// レベルアップチェック（GameBalanceConfig使用）
    /// </summary>
    private void CheckLevelUp(int oldLevel)
    {
      if (gameBalance == null) return;

      while (true)
      {
        int requiredExp = gameBalance.GetTotalExpRequiredForLevel(currentUser.level + 1);

        if (currentUser.exp >= requiredExp)
        {
          currentUser.level++;

          // レベルアップ時のスタミナ回復
          int staminaRecovery = gameBalance.GetMaxStaminaForLevel(currentUser.level);
          int maxStamina = currentUser.GetMaxStamina(gameBalance);
          currentUser.stamina = Mathf.Min(maxStamina, currentUser.stamina + staminaRecovery);

          Debug.Log($"レベルアップ！ Lv.{oldLevel} → Lv.{currentUser.level} (スタミナ+{staminaRecovery})");
          OnLevelUp();
        }
        else
        {
          break;
        }
      }
    }

    /// <summary>
    /// 特定レベルまでの総必要経験値を計算
    /// </summary>
    private int CalculateTotalExpForLevel(int targetLevel)
    {
      int totalExp = 0;
      for (int i = 1; i < targetLevel; i++)
      {
        totalExp += CalculateMaxExpForLevel(i);
      }
      return totalExp;
    }



    /// <summary>
    /// レベルアップ時の処理
    /// </summary>
    private void OnLevelUp()
    {
      Debug.Log("🎉 Level Up!");
      // レベルアップエフェクト等
    }

    /// <summary>
    /// ヘッダーサイズを動的に変更
    /// </summary>
    public void SetHeaderHeightRatio(float newRatio)
    {
      headerHeightRatio = Mathf.Clamp(newRatio, 0.05f, 0.2f);
      SetupHeaderLayout();
    }

    /// <summary>
    /// Progress Bar の色を設定
    /// </summary>
    public void SetProgressBarColors(Color expColor, Color staminaColor)
    {
      if (expProgressImage != null)
        expProgressImage.color = expColor;

      if (staminaProgressImage != null)
        staminaProgressImage.color = staminaColor;
    }

    /// <summary>
    /// デバッグ用テスト関数
    /// </summary>
    [ContextMenu("Test Add Experience")]
    private void TestAddExperience()
    {
      AddExperience(50);
    }

    [ContextMenu("Test Consume Stamina")]
    private void TestConsumeStamina()
    {
      ConsumeStamina(10);
    }

    [ContextMenu("Test Recover Stamina")]
    private void TestRecoverStamina()
    {
      RecoverStamina(20);
    }

    /// <summary>
    /// イベント購読解除
    /// </summary>
    void OnDestroy()
    {
      if (userService != null)
      {
        userService.OnUserDataUpdated -= OnUserDataUpdated;
      }
    }
  }
}