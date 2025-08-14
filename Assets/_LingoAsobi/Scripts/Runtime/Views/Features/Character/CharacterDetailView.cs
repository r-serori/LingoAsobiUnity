using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Repositories;
using Scripts.Runtime.Utilities.Helpers;

namespace Scripts.Runtime.Views.Features.Character
{
  /// <summary>
  /// キャラクター詳細表示View
  /// </summary>
  public class CharacterDetailView : BaseView
  {
    [Header("Character Info")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterDescriptionText;
    [SerializeField] private TextMeshProUGUI characterLevelText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Image rarityBadge;

    [Header("Stats Display")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI powerText;

    [Header("Action Buttons")]
    [SerializeField] private Button favoriteButton;
    [SerializeField] private Button setAsMainButton;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button backButton;

    [Header("Favorite Icon")]
    [SerializeField] private Image favoriteIcon;
    [SerializeField] private Sprite favoriteOnSprite;
    [SerializeField] private Sprite favoriteOffSprite;

    [Header("Lock Overlay")]
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private TextMeshProUGUI unlockRequirementText;

    private CharacterData currentCharacter;
    private CharacterScene parentScene;

    #region Initialization

    protected override void GetUIReferences()
    {
      base.GetUIReferences();

      parentScene = GetComponentInParent<CharacterScene>();
    }


    protected override void SetupEventListeners()
    {
      base.SetupEventListeners();

      if (favoriteButton != null)
        favoriteButton.onClick.AddListener(OnFavoriteButtonClicked);

      if (setAsMainButton != null)
        setAsMainButton.onClick.AddListener(OnSetAsMainButtonClicked);

      if (levelUpButton != null)
        levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);

      if (backButton != null)
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    #endregion

    #region Data Management

    /// <summary>
    /// キャラクターデータを設定
    /// </summary>
    public void SetCharacter(CharacterData character)
    {
      currentCharacter = character;
      UpdateDisplay();
    }

    protected override void UpdateDisplay()
    {
      if (currentCharacter == null) return;

      // 基本情報
      if (characterNameText != null)
        characterNameText.text = currentCharacter.characterName;

      if (characterDescriptionText != null)
        characterDescriptionText.text = currentCharacter.description;

      if (characterLevelText != null)
        characterLevelText.text = $"Lv.{currentCharacter.level}";

      // ステータス
      UpdateStatsDisplay();

      // レアリティ
      UpdateRarityDisplay();

      // お気に入り状態
      UpdateFavoriteDisplay();

      // ロック状態
      UpdateLockDisplay();

      // ボタンの有効/無効
      UpdateButtonStates();

      // キャラクター画像を読み込む
      LoadCharacterImage();
    }

    /// <summary>
    /// ステータス表示を更新
    /// </summary>
    private void UpdateStatsDisplay()
    {
      if (currentCharacter.stats == null) return;

      if (hpText != null)
        hpText.text = currentCharacter.stats.hp.ToString();

      if (attackText != null)
        attackText.text = currentCharacter.stats.attack.ToString();

      if (defenseText != null)
        defenseText.text = currentCharacter.stats.defense.ToString();

      if (speedText != null)
        speedText.text = currentCharacter.stats.speed.ToString();

      if (powerText != null)
        powerText.text = currentCharacter.stats.CalculatePower().ToString();
    }

    /// <summary>
    /// レアリティ表示を更新
    /// </summary>
    private void UpdateRarityDisplay()
    {
      if (rarityBadge != null)
      {
        Color rarityColor = GetRarityColor(currentCharacter.rarity);
        rarityBadge.color = rarityColor;
      }
    }

    /// <summary>
    /// お気に入り表示を更新
    /// </summary>
    private void UpdateFavoriteDisplay()
    {
      if (favoriteIcon != null)
      {
        favoriteIcon.sprite = currentCharacter.isFavorite ? favoriteOnSprite : favoriteOffSprite;
      }

      if (favoriteButton != null)
      {
        var buttonText = favoriteButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
          buttonText.text = currentCharacter.isFavorite ? "お気に入り解除" : "お気に入り登録";
        }
      }
    }

    /// <summary>
    /// ロック表示を更新
    /// </summary>
    private void UpdateLockDisplay()
    {
      bool isLocked = !currentCharacter.isUnlocked;

      if (lockOverlay != null)
      {
        lockOverlay.SetActive(isLocked);
      }

      if (isLocked && unlockRequirementText != null)
      {
        // 解放条件を表示
        unlockRequirementText.text = GetUnlockRequirement();
      }
    }

    /// <summary>
    /// ボタンの有効/無効を更新
    /// </summary>
    private void UpdateButtonStates()
    {
      bool isUnlocked = currentCharacter.isUnlocked;

      if (favoriteButton != null)
        favoriteButton.interactable = isUnlocked;

      if (setAsMainButton != null)
        setAsMainButton.interactable = isUnlocked;

      if (levelUpButton != null)
        levelUpButton.interactable = isUnlocked;
    }

    /// <summary>
    /// キャラクター画像を読み込む
    /// </summary>
    private void LoadCharacterImage()
    {
      if (characterImage != null && !string.IsNullOrEmpty(currentCharacter.fullBodyImagePath))
      {
        // TODO: Addressables or Resources.Loadで画像を読み込む
        // Sprite sprite = Resources.Load<Sprite>(currentCharacter.fullBodyImagePath);
        // if (sprite != null) characterImage.sprite = sprite;
      }
    }

    #endregion

    #region Button Handlers

    /// <summary>
    /// お気に入りボタンクリック
    /// </summary>
    private async void OnFavoriteButtonClicked()
    {
      if (currentCharacter == null) return;

      SetInteractable(false);

      try
      {
        bool success = await CharacterRepository.Instance.ToggleFavoriteAsync(currentCharacter.characterId);

        if (success)
        {
          currentCharacter.ToggleFavorite();
          UpdateFavoriteDisplay();

          // エフェクトを表示
          await ShowFavoriteToggleEffect();
        }
      }
      catch (Exception e)
      {
        Debug.LogError($"[CharacterDetailView] Failed to toggle favorite: {e.Message}");
        ShowError("お気に入りの変更に失敗しました");
      }
      finally
      {
        SetInteractable(true);
      }
    }

    /// <summary>
    /// メインキャラクター設定ボタンクリック
    /// </summary>
    private async void OnSetAsMainButtonClicked()
    {
      if (currentCharacter == null) return;

      SetInteractable(false);

      try
      {
        var user = await DataManager.Instance.GetCurrentUserAsync();
        if (user != null)
        {
          user.favoriteCharacterId = currentCharacter.characterId;
          await UserRepository.Instance.UpdateAsync(user);

          // 成功メッセージを表示
          ShowSuccessMessage($"{currentCharacter.characterName}をメインキャラクターに設定しました");
        }
      }
      catch (Exception e)
      {
        Debug.LogError($"[CharacterDetailView] Failed to set main character: {e.Message}");
        ShowError("メインキャラクターの設定に失敗しました");
      }
      finally
      {
        SetInteractable(true);
      }
    }

    /// <summary>
    /// レベルアップボタンクリック
    /// </summary>
    private void OnLevelUpButtonClicked()
    {
      if (currentCharacter == null) return;

      // TODO: レベルアップ画面を表示
      Debug.Log($"[CharacterDetailView] Level up: {currentCharacter.characterName}");
    }

    /// <summary>
    /// 戻るボタンクリック
    /// </summary>
    private async void OnBackButtonClicked()
    {
      if (parentScene != null)
      {
        await parentScene.BackToListView();
      }
    }

    #endregion

    #region Effects

    /// <summary>
    /// お気に入り切り替えエフェクト
    /// </summary>
    private async Task ShowFavoriteToggleEffect()
    {
      if (favoriteIcon != null)
      {
        await UIHelper.PopScaleAsync(favoriteIcon.transform, 0.3f);
      }
    }

    /// <summary>
    /// 成功メッセージを表示
    /// </summary>
    private void ShowSuccessMessage(string message)
    {
      // TODO: 成功メッセージのUIを実装
      Debug.Log($"[CharacterDetailView] Success: {message}");
    }

    #endregion

    #region Utility

    /// <summary>
    /// レアリティの色を取得
    /// </summary>
    private Color GetRarityColor(Rarity rarity)
    {
      return rarity switch
      {
        Rarity.Common => Utilities.Constants.GameConstants.Colors.Common,
        Rarity.Uncommon => Utilities.Constants.GameConstants.Colors.Uncommon,
        Rarity.Rare => Utilities.Constants.GameConstants.Colors.Rare,
        Rarity.Epic => Utilities.Constants.GameConstants.Colors.Epic,
        Rarity.Legendary => Utilities.Constants.GameConstants.Colors.Legendary,
        _ => Color.white
      };
    }

    /// <summary>
    /// 解放条件を取得
    /// </summary>
    private string GetUnlockRequirement()
    {
      // TODO: 実際の解放条件を返す
      return currentCharacter.rarity switch
      {
        Rarity.Legendary => "特別なイベントで入手",
        Rarity.Epic => "ガチャまたはイベントで入手",
        Rarity.Rare => "ガチャで入手",
        _ => "ストーリー進行で解放"
      };
    }

    #endregion
  }
}