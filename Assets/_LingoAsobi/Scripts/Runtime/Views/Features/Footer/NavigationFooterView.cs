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
using Scripts.Runtime.Utilities.Constants;
using UnityEngine.PlayerLoop;

namespace Scripts.Runtime.Views.Features.Footer
{
  /// <summary>
  /// ナビゲーションフッターのView
  /// ホーム、キャラクター、ガチャ、ショップ、トレーニングボタンを管理
  /// </summary>
  public class NavigationFooterView : BaseView
  {
    [SerializeField] private Button homeButton;
    [SerializeField] private Button characterButton;
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button trainingButton;

    protected override void SetupEventListeners()
    {
      base.SetupEventListeners();

      // ボタンのイベントリスナーを設定
      homeButton.onClick?.AddListener(OnHomeButtonClicked);

      characterButton.onClick?.AddListener(OnCharacterButtonClicked);

      gachaButton.onClick?.AddListener(OnGachaButtonClicked);

      shopButton.onClick?.AddListener(OnShopButtonClicked);

      trainingButton.onClick?.AddListener(OnTrainingButtonClicked);
    }

    /// <summary>
    /// 他のシーンへ遷移
    /// </summary>
    protected virtual async Task NavigateToSceneAsync(string targetSceneName)
    {
      await SceneHelper.Instance.LoadSceneAsync(targetSceneName);
    }


    #region Button Handlers

    private async void OnHomeButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Home);
    }

    private async void OnCharacterButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Character);
    }

    private async void OnShopButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Shop);
    }

    private async void OnGachaButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Gacha);
    }

    private async void OnTrainingButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Training);
    }

    #endregion
  }
}