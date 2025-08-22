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
using Scripts.Runtime.Data.Models.Training;
using Scripts.Runtime.Views.ViewData.Training;

namespace Scripts.Runtime.Views.Features.Training
{
  /// <summary>
  /// トレーニング画面のシーン管理クラス
  /// </summary>
  public class TrainingScene : BaseScene
  {
    [Header("Training Scene References")]
    [SerializeField] private TrainingView trainingView;
    [SerializeField] private NavigationFooterView navigationFooterView;

    private UserProfile currentUser;
    private TrainingData trainingData;

    #region Initialization

    protected override async Task OnInitializeAsync()
    {
      await base.OnInitializeAsync();

      // ユーザーデータを取得（DataManager経由）
      currentUser = await DataManager.Instance.GetCurrentUserAsync();
      trainingData = await DataManager.Instance.GetTrainingDataAsync();

      trainingView.SetViewData(new TrainingViewData(currentUser, trainingData));
    }

    #endregion

    #region Scene Lifecycle

    protected override async Task OnAfterActivate()
    {
      await base.OnAfterActivate();

      // 既に表示されている場合は何もしない
      if (trainingView.isVisible && navigationFooterView.isVisible)
      {
        return;
      }

      // TrainingViewを表示
      if (trainingView != null)
      {
        await ShowViewAsync<TrainingView>();
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

    private async Task RefreshUserData()
    {
      currentUser = await DataManager.Instance.GetCurrentUserAsync();
    }

    #endregion

    protected override void SubscribeToEvents()
    {
      base.SubscribeToEvents();


      if (trainingView?.profileHeaderView != null)
      {
        // レベルアップイベントを購読
        EventBus.Instance.Subscribe<LevelUpEvent>(trainingView.profileHeaderView.OnLevelUp);

        // スタミナ変更イベントを購読
        EventBus.Instance.Subscribe<StaminaChangedEvent>(trainingView.profileHeaderView.OnStaminaChanged);
      }

      // 通貨変更イベントを購読
      EventBus.Instance.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
    }

    protected override void UnsubscribeFromEvents()
    {
      base.UnsubscribeFromEvents();

      EventBus.Instance.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
    }

    private void OnCurrencyChanged(CurrencyChangedEvent e)
    {
      // 通貨表示を更新
      if (trainingView != null)
      {
        _ = trainingView.RefreshAsync();
      }
    }
  }
}