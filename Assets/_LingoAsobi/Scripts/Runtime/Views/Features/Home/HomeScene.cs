using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Utilities.Helpers;
using Scripts.Runtime.Views.Features.Footer;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Repositories;

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

      // ユーザーデータを取得（DataManager経由）
      currentUser = await DataManager.Instance.GetCurrentUserAsync();

      // お気に入りキャラクターも取得（DataManager経由）
      CharacterData favoriteCharacter = null;
      if (!string.IsNullOrEmpty(currentUser.favoriteCharacterId))
      {
        favoriteCharacter = await DataManager.Instance.GetCharacterByIdAsync(currentUser.favoriteCharacterId);
      }

      // HomeViewを初期化（ユーザーデータとキャラクターデータの両方を設定）
      homeView.SetUserData(currentUser);
      homeView.SetCharacterData(favoriteCharacter);
    }


    #endregion

    #region Scene Lifecycle

    protected override async Task OnAfterActivate()
    {
      await base.OnAfterActivate();

      // 既に表示されている場合は何もしない
      if (homeView.isVisible && navigationFooterView.isVisible)
      {
        return;
      }

      // HomeViewを表示
      if (homeView != null)
      {
        await ShowViewAsync<HomeView>();
      }

      // NavigationFooterViewも表示
      if (navigationFooterView != null)
      {
        await ShowViewAsync<NavigationFooterView>();
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


      if (homeView?.profileHeaderView != null)
      {
        // レベルアップイベントを購読
        EventBus.Instance.Subscribe<LevelUpEvent>(homeView.profileHeaderView.OnLevelUp);

        // スタミナ変更イベントを購読
        EventBus.Instance.Subscribe<StaminaChangedEvent>(homeView.profileHeaderView.OnStaminaChanged);
      }

      // 通貨変更イベントを購読
      EventBus.Instance.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
    }

    protected override void UnsubscribeFromEvents()
    {
      base.UnsubscribeFromEvents();

      // EventBus.Instance.Unsubscribe<LevelUpEvent>(homeView.profileHeaderView.OnLevelUp);
      EventBus.Instance.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
      EventBus.Instance.Unsubscribe<StaminaChangedEvent>(homeView.profileHeaderView.OnStaminaChanged);
    }

    private void OnCurrencyChanged(CurrencyChangedEvent e)
    {

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