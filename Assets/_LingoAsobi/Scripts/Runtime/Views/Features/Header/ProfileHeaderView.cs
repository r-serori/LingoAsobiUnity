using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Repositories;
using Scripts.Runtime.Utilities.Helpers;
using Scripts.Runtime.Utilities.Constants;
using System;

namespace Scripts.Runtime.Views.Features.Header
{
  public class ProfileHeaderView : BaseView
  {
    [Header("User Info UI")]
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI userLevelText;
    [SerializeField] private Image profileImage;

    [Header("Exp Bar UI")]
    [SerializeField] private Image expBar;
    [SerializeField] private Image expBarMax;

    [Header("Assets UI")]
    [SerializeField] private TextMeshProUGUI goldAmountText;
    [SerializeField] private TextMeshProUGUI gemAmountText;
    [SerializeField] private Button goldButton;
    [SerializeField] private Button gemButton;
    [SerializeField] private Button staminaButton;

    [Header("Stamina UI")]
    [SerializeField] private TextMeshProUGUI staminaAmountText;
    [SerializeField] private Image staminaBar;
    [SerializeField] private Image staminaBarMax;

    [Header("Effects")]
    [SerializeField] private GameObject levelUpEffectPrefab;
    [SerializeField] private Transform effectContainer;

    private UserProfile currentUser;
    private CharacterData currentCharacter;
    private float staminaUpdateTimer = 0f;

    protected override void UpdateDisplay()
    {
      if (currentUser == null) return;

      // ユーザー情報を表示
      if (userNameText != null)
        userNameText.text = currentUser.userName;

      if (userLevelText != null)
        userLevelText.text = $"Lv.{currentUser.level}";

      // 経験値バー
      if (expBar != null)
      {
        float expProgress = (float)currentUser.exp / currentUser.nextLevelExp;
        expBar.fillAmount = expProgress;
        expBar.color = GameConstants.Colors.ExpBar;
      }

      if (expBarMax != null)
      {
        expBarMax.fillAmount = 1f;
        expBarMax.color = Color.clear;
      }

      // 通貨
      UpdateCurrencyDisplay();

      // スタミナ
      UpdateStaminaDisplay(currentUser.CalculateCurrentStamina());
    }


    /// <summary>
    /// 通貨表示を更新
    /// </summary>
    private void UpdateCurrencyDisplay()
    {
      if (currentUser == null) return;

      if (goldAmountText != null)
        goldAmountText.text = currentUser.FormatCurrency(currentUser.gold);

      if (gemAmountText != null)
        gemAmountText.text = currentUser.FormatCurrency(currentUser.gem);
    }

    public void ShowLevelUpEffect(int newLevel)
    {
      if (levelUpEffectPrefab != null && effectContainer != null)
      {
        var effect = Instantiate(levelUpEffectPrefab, effectContainer);
        effect.GetComponent<TextMeshProUGUI>().text = $"+{newLevel}";
        Destroy(effect, 1f);
      }
    }
  
    public void OnLevelUp(LevelUpEvent e)
    {
      // レベルアップ演出を表示
      ShowLevelUpEffect(e.NewLevel);
    }

    public void OnStaminaChanged(StaminaChangedEvent e)
    {
      // スタミナ表示を更新
      UpdateStaminaDisplay(e.NewStamina);
    }

    /// <summary>
    /// スタミナ表示を更新
    /// </summary>
    public void UpdateStaminaDisplay(int currentStamina)
    {
      if (currentUser == null) return;

      if (staminaAmountText != null)
        staminaAmountText.text = $"{currentStamina}/{currentUser.maxStamina}";

      if (staminaBar != null)
      {
        float staminaProgress = (float)currentStamina / currentUser.maxStamina;
        staminaBar.fillAmount = staminaProgress;
      }

      // スタミナ回復タイマーを更新
      UpdateStaminaTimer(currentStamina);
    }

    /// <summary>
    /// スタミナタイマーを更新
    /// </summary>
    private void UpdateStaminaTimer(int currentStamina)
    {
      if (staminaAmountText == null) return;

      if (currentStamina >= currentUser.maxStamina)
      {
        staminaAmountText.text = "MAX";
        staminaAmountText.color = GameConstants.Colors.Success;
      }
      else
      {
        // 次のスタミナ回復までの時間を計算
        var timeSinceLastUpdate = DateTime.Now - currentUser.lastStaminaUpdateTime;
        var secondsUntilNext = currentUser.staminaRecoverySeconds - (timeSinceLastUpdate.TotalSeconds % currentUser.staminaRecoverySeconds);

        int minutes = (int)(secondsUntilNext / 60);
        int seconds = (int)(secondsUntilNext % 60);

        staminaAmountText.text = $"{minutes:00}:{seconds:00}";
        staminaAmountText.color = Color.white;
      }
    }

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

    public void SetUserData(UserProfile currentUser)
    {
      this.currentUser = currentUser;
      UpdateDisplay();
    }

    #region Helper Methods

  
    #endregion
    }
}

