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
using Scripts.Runtime.Core;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Views.Features.Header;

namespace Scripts.Runtime.Views.Features.Home
{
  /// <summary>
  /// ホーム画面のView
  /// ユーザー情報、キャラクター表示、メニューボタンを管理
  /// </summary>
  public class HomeView : BaseView
  {
    [Header("Header")]
    [SerializeField] public ProfileHeaderView profileHeaderView;

    [Header("Character Display")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Button characterChangeButton;

    private UserProfile currentUser;
    private CharacterData currentCharacter;

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
    }

    #endregion

    #region Data Management

    /// <summary>
    /// ユーザーデータを設定
    /// </summary>
    public void SetUserData(UserProfile user)
    {
      currentUser = user;
      profileHeaderView.SetUserData(currentUser);
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
      // キャラクター
      UpdateCharacterDisplay();
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
    }

    #endregion

    #region Effects


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
      (
        GameConstants.Scenes.Shop,
        SceneTransitionEvent.TransitionPhase.Started
      ));
    }

    #endregion

  }
}