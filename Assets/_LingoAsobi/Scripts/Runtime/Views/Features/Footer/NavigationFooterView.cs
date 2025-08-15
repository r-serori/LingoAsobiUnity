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
        if (homeButton != null)
            homeButton.onClick.AddListener(OnHomeButtonClicked);
        
        if (characterButton != null)
            characterButton.onClick.AddListener(OnCharacterButtonClicked);
        
        if (gachaButton != null)
            gachaButton.onClick.AddListener(OnGachaButtonClicked);
        
        if (shopButton != null)
            shopButton.onClick.AddListener(OnShopButtonClicked);
        
        if (trainingButton != null)
            trainingButton.onClick.AddListener(OnTrainingButtonClicked);
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
      Debug.Log("[NavigationFooterView] Home button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Home);
    }

    private async void OnCharacterButtonClicked()
    {
      Debug.Log("[NavigationFooterView] Character button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Character);
    }

    private async void OnShopButtonClicked()
    {
      Debug.Log("[NavigationFooterView] Shop button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Shop);
    }
  
    private async void OnGachaButtonClicked()
    {
      Debug.Log("[NavigationFooterView] Gacha button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Gacha);
    }

    private async void OnTrainingButtonClicked()
    {
      Debug.Log("[NavigationFooterView] Training button clicked");
      await NavigateToSceneAsync(GameConstants.Scenes.Training);
    }

    #endregion
  }
}