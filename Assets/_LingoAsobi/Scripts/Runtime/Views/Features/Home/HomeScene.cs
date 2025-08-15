using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Utilities.Helpers;
using Scripts.Runtime.Views.Features.Footer;

namespace Scripts.Runtime.Views.Features.Home
{
  /// <summary>
  /// ホーム画面のシーン管理クラス
  /// </summary>
  public class HomeScene : BaseScene
  {
    [Header("Home Scene References")]
    [SerializeField] private HomeView homeView;
    [SerializeField] private NavigationFooterView navigationFooterView;

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
      homeView?.SetUserData(currentUser);
    }

    protected override void InitializeViews()
    {
      base.InitializeViews();

      navigationFooterView?.Initialize();
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
      EventBus.Instance.Subscribe<LevelUpEvent>(homeView.profileHeaderView.OnLevelUp);

      // 通貨変更イベントを購読
      EventBus.Instance.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);

      // スタミナ変更イベントを購読
      EventBus.Instance.Subscribe<StaminaChangedEvent>(homeView.profileHeaderView.OnStaminaChanged);
    }

    protected override void UnsubscribeFromEvents()
    {
      base.UnsubscribeFromEvents();

      EventBus.Instance.Unsubscribe<LevelUpEvent>(homeView.profileHeaderView.OnLevelUp);
      EventBus.Instance.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
      EventBus.Instance.Unsubscribe<StaminaChangedEvent>(homeView.profileHeaderView.OnStaminaChanged);
    }

    private void OnCurrencyChanged(CurrencyChangedEvent e)
    {
      Debug.Log($"[HomeScene] {e.CurrencyType} changed: {e.OldAmount} -> {e.NewAmount}");

      // 通貨表示を更新
      if (homeView != null)
      {
        _ = homeView.RefreshAsync();
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