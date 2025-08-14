using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Repositories;
using Scripts.Runtime.Utilities.Helpers;

namespace Scripts.Runtime.Views.Features.Home
{
  /// <summary>
  /// ホーム画面のView
  /// ユーザー情報、キャラクター表示、メニューボタンを管理
  /// </summary>
  public class HomeView : BaseView
  {
    [Header("User Info UI")]
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI userLevelText;
    [SerializeField] private Image userIconImage;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("Currency UI")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemText;

    [Header("Stamina UI")]
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TextMeshProUGUI staminaTimerText;

    [Header("Character Display")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Button characterChangeButton;

    [Header("Effects")]
    [SerializeField] private GameObject levelUpEffectPrefab;
    [SerializeField] private Transform effectContainer;

    private UserProfile currentUser;
    private CharacterData currentCharacter;
    private float staminaUpdateTimer = 0f;

    #region Initialization

    protected override void GetUIReferences()
    {
      base.GetUIReferences();

      // UI要素の参照を追加で取得（必要に応じて）
    }

    protected override void SetupEventListeners()
    {
      base.SetupEventListeners();

      if (characterChangeButton != null)
      {
        characterChangeButton.onClick.AddListener(OnCharacterChangeClicked);
      }

      // 通貨ボタンにショップへの遷移を追加
      if (goldText != null)
      {
        var goldButton = goldText.GetComponentInParent<Button>();
        if (goldButton != null)
        {
          goldButton.onClick.AddListener(() => OnCurrencyClicked("gold"));
        }
      }

      if (gemText != null)
      {
        var gemButton = gemText.GetComponentInParent<Button>();
        if (gemButton != null)
        {
          gemButton.onClick.AddListener(() => OnCurrencyClicked("gem"));
        }
      }
    }

    #endregion

    #region Data Management

    /// <summary>
    /// ユーザーデータを設定
    /// </summary>
    public void SetUserData(UserProfile user)
    {
      currentUser = user;
      UpdateDisplay();
    }

    protected override async Task LoadDataAsync()
    {
      try
      {
        // ユーザーデータを取得
        currentUser = await DataManager.Instance.GetCurrentUserAsync();

        // お気に入りキャラクターを取得
        if (!string.IsNullOrEmpty(currentUser?.favoriteCharacterId))
        {
          currentCharacter = await DataManager.Instance.GetCharacterAsync(currentUser.favoriteCharacterId);
        }
      }
      catch (Exception e)
      {
        Debug.LogError($"[HomeView] Failed to load data: {e.Message}");
        ShowError("データの読み込みに失敗しました");
      }
    }

    protected override void UpdateDisplay()
    {
      if (currentUser == null) return;

      // ユーザー情報を表示
      if (userNameText != null)
        userNameText.text = currentUser.userName;

      if (userLevelText != null)
        userLevelText.text = $"Lv.{currentUser.level}";

      // 経験値バー
      if (expSlider != null)
      {
        float expProgress = (float)currentUser.exp / currentUser.nextLevelExp;
        expSlider.value = expProgress;
      }

      if (expText != null)
        expText.text = $"{currentUser.exp}/{currentUser.nextLevelExp}";

      // 通貨
      UpdateCurrencyDisplay();

      // スタミナ
      UpdateStaminaDisplay(currentUser.CalculateCurrentStamina());

      // キャラクター
      UpdateCharacterDisplay();
    }

    /// <summary>
    /// 通貨表示を更新
    /// </summary>
    private void UpdateCurrencyDisplay()
    {
      if (currentUser == null) return;

      if (goldText != null)
        goldText.text = FormatCurrency(currentUser.gold);

      if (gemText != null)
        gemText.text = FormatCurrency(currentUser.gem);
    }

    /// <summary>
    /// スタミナ表示を更新
    /// </summary>
    public void UpdateStaminaDisplay(int currentStamina)
    {
      if (currentUser == null) return;

      if (staminaText != null)
        staminaText.text = $"{currentStamina}/{currentUser.maxStamina}";

      if (staminaSlider != null)
      {
        float staminaProgress = (float)currentStamina / currentUser.maxStamina;
        staminaSlider.value = staminaProgress;
      }

      // スタミナ回復タイマーを更新
      UpdateStaminaTimer(currentStamina);
    }

    /// <summary>
    /// スタミナタイマーを更新
    /// </summary>
    private void UpdateStaminaTimer(int currentStamina)
    {
      if (staminaTimerText == null) return;

      if (currentStamina >= currentUser.maxStamina)
      {
        staminaTimerText.text = "MAX";
        staminaTimerText.color = Color.green;
      }
      else
      {
        // 次のスタミナ回復までの時間を計算
        var timeSinceLastUpdate = DateTime.Now - currentUser.lastStaminaUpdateTime;
        var secondsUntilNext = currentUser.staminaRecoverySeconds - (timeSinceLastUpdate.TotalSeconds % currentUser.staminaRecoverySeconds);

        int minutes = (int)(secondsUntilNext / 60);
        int seconds = (int)(secondsUntilNext % 60);

        staminaTimerText.text = $"{minutes:00}:{seconds:00}";
        staminaTimerText.color = Color.white;
      }
    }

    /// <summary>
    /// キャラクター表示を更新
    /// </summary>
    private void UpdateCharacterDisplay()
    {
      if (currentCharacter == null) return;

      if (characterNameText != null)
        characterNameText.text = currentCharacter.characterName;

      // キャラクター画像を読み込む
      if (characterImage != null && !string.IsNullOrEmpty(currentCharacter.fullBodyImagePath))
      {
        // TODO: Addressables or Resources.Loadで画像を読み込む
        // Sprite sprite = Resources.Load<Sprite>(currentCharacter.fullBodyImagePath);
        // if (sprite != null) characterImage.sprite = sprite;
      }
    }

    #endregion

    #region Update Loop

    private void Update()
    {
      if (!isVisible || currentUser == null) return;

      // スタミナタイマーを定期的に更新
      staminaUpdateTimer += Time.deltaTime;
      if (staminaUpdateTimer >= 1f) // 1秒ごとに更新
      {
        staminaUpdateTimer = 0f;
        int currentStamina = currentUser.CalculateCurrentStamina();
        UpdateStaminaDisplay(currentStamina);
      }
    }

    #endregion

    #region Effects

    /// <summary>
    /// レベルアップエフェクトを表示
    /// </summary>
    public void ShowLevelUpEffect(int newLevel)
    {
      if (levelUpEffectPrefab != null && effectContainer != null)
      {
        GameObject effect = Instantiate(levelUpEffectPrefab, effectContainer);

        // エフェクトにレベルを設定
        var levelText = effect.GetComponentInChildren<TextMeshProUGUI>();
        if (levelText != null)
        {
          levelText.text = $"LEVEL UP!\nLv.{newLevel}";
        }

        // 3秒後に削除
        Destroy(effect, 3f);
      }

      // UIも更新
      if (userLevelText != null)
        userLevelText.text = $"Lv.{newLevel}";

      // アニメーション
      _ = AnimateLevelUp();
    }

    /// <summary>
    /// レベルアップアニメーション
    /// </summary>
    private async Task AnimateLevelUp()
    {
      if (userLevelText != null)
      {
        // テキストを点滅させる
        Color originalColor = userLevelText.color;

        for (int i = 0; i < 3; i++)
        {
          userLevelText.color = Color.yellow;
          await Task.Delay(200);
          userLevelText.color = originalColor;
          await Task.Delay(200);
        }
      }
    }

    #endregion

    #region Button Handlers

    private void OnCharacterChangeClicked()
    {
      Debug.Log("[HomeView] Character change clicked");
      // キャラクター選択画面を開く
      // TODO: キャラクター選択ポップアップを表示
    }

    private void OnCurrencyClicked(string currencyType)
    {
      Debug.Log($"[HomeView] Currency clicked: {currencyType}");
      // ショップ画面へ遷移
      EventBus.Instance.Publish(new SceneTransitionEvent
      {
        FromScene = "Home",
        ToScene = "Shop"
      });
    }

    #endregion

    #region Utility

    /// <summary>
    /// 通貨をフォーマット
    /// </summary>
    private string FormatCurrency(int amount)
    {
      if (amount >= 1000000)
        return $"{amount / 1000000f:F1}M";
      else if (amount >= 1000)
        return $"{amount / 1000f:F1}K";
      else
        return amount.ToString();
    }

    #endregion
  }
}