using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Utilities.Helpers;

namespace Scripts.Runtime.Views.Features.Home
{
  /// <summary>
  /// ホーム画面のシーン管理クラス
  /// </summary>
  public class HomeScene : BaseScene
  {
    [Header("Home Scene References")]
    [SerializeField] private HomeView homeView;
    [SerializeField] private Button characterButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button questButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button settingsButton;

    private UserProfile currentUser;

    #region Initialization

    protected override async Task OnInitializeAsync()
    {
      await base.OnInitializeAsync();

      // ユーザーデータを取得
      currentUser = await DataManager.Instance.GetCurrentUserAsync();

      if (currentUser == null)
      {
        Debug.LogError("[HomeScene] Failed to get user data");
        return;
      }

      // HomeViewを初期化
      if (homeView != null)
      {
        homeView.SetUserData(currentUser);
      }
    }

    protected override void InitializeViews()
    {
      base.InitializeViews();

      // ボタンのイベントを設定
      if (characterButton != null)
        characterButton.onClick.AddListener(OnCharacterButtonClicked);

      if (shopButton != null)
        shopButton.onClick.AddListener(OnShopButtonClicked);

      if (questButton != null)
        questButton.onClick.AddListener(OnQuestButtonClicked);

      if (inventoryButton != null)
        inventoryButton.onClick.AddListener(OnInventoryButtonClicked);

      if (settingsButton != null)
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }

    #endregion

    #region Scene Lifecycle

    protected override async Task OnAfterActivate()
    {
      await base.OnAfterActivate();

      // HomeViewを表示
      if (homeView != null)
      {
        await ShowViewAsync<HomeView>();
      }

      // データを更新
      await RefreshUserData();
    }

    #endregion

    #region Button Handlers

    private async void OnCharacterButtonClicked()
    {
      Debug.Log("[HomeScene] Character button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Character);
    }

    private async void OnShopButtonClicked()
    {
      Debug.Log("[HomeScene] Shop button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Shop);
    }

    private async void OnQuestButtonClicked()
    {
      Debug.Log("[HomeScene] Quest button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Quest);
    }

    private async void OnInventoryButtonClicked()
    {
      Debug.Log("[HomeScene] Inventory button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Inventory);
    }

    private async void OnSettingsButtonClicked()
    {
      Debug.Log("[HomeScene] Settings button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Settings);
    }

    #endregion

    #region Data Management

    /// <summary>
    /// ユーザーデータを更新
    /// </summary>
    private async Task RefreshUserData()
    {
      currentUser = await DataManager.Instance.GetCurrentUserAsync();

      if (homeView != null && currentUser != null)
      {
        homeView.SetUserData(currentUser);
        await homeView.RefreshAsync();
      }
    }

    #endregion

    #region Event Handling

    protected override void SubscribeToEvents()
    {
      base.SubscribeToEvents();

      // レベルアップイベントを購読
      EventBus.Instance.Subscribe<LevelUpEvent>(OnLevelUp);

      // 通貨変更イベントを購読
      EventBus.Instance.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);

      // スタミナ変更イベントを購読
      EventBus.Instance.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
    }

    protected override void UnsubscribeFromEvents()
    {
      base.UnsubscribeFromEvents();

      EventBus.Instance.Unsubscribe<LevelUpEvent>(OnLevelUp);
      EventBus.Instance.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
      EventBus.Instance.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);
    }

    private void OnLevelUp(LevelUpEvent e)
    {
      Debug.Log($"[HomeScene] Level up! {e.OldLevel} -> {e.NewLevel}");

      // レベルアップ演出を表示
      if (homeView != null)
      {
        homeView.ShowLevelUpEffect(e.NewLevel);
      }
    }

    private void OnCurrencyChanged(CurrencyChangedEvent e)
    {
      Debug.Log($"[HomeScene] {e.Type} changed: {e.OldAmount} -> {e.NewAmount}");

      // 通貨表示を更新
      if (homeView != null)
      {
        _ = homeView.RefreshAsync();
      }
    }

    private void OnStaminaChanged(StaminaChangedEvent e)
    {
      Debug.Log($"[HomeScene] Stamina changed: {e.OldStamina} -> {e.NewStamina}");

      // スタミナ表示を更新
      if (homeView != null)
      {
        homeView.UpdateStaminaDisplay(e.NewStamina);
      }
    }

    #endregion

    #region Navigation Override

    public override async Task OnBackButtonPressed()
    {
      // ホーム画面では確認ダイアログを表示
      bool shouldExit = await ShowExitConfirmationDialog();

      if (shouldExit)
      {
        Application.Quit();
      }
    }

    private async Task<bool> ShowExitConfirmationDialog()
    {
      // TODO: 確認ダイアログを実装
      await Task.CompletedTask;
      return false;
    }

    #endregion
  }
}