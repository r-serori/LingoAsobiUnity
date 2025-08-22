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
  /// 戻るフッターのView
  /// </summary>
  public class BackFooterView : BaseView
  {
    [SerializeField] private Button backButton;

    protected override void SetupEventListeners()
    {
      base.SetupEventListeners();

      if (backButton != null)
      {
        backButton.onClick.RemoveListener(OnBackButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
      }
    }

    private async void OnBackButtonClicked()
    {

      // 前のシーンに戻る
      await GoBackAsync();
    }

    /// <summary>
    /// 前の画面に戻る
    /// </summary>
    private async Task GoBackAsync()
    {
      try
      {
        // 現在のシーン名を取得
        string currentScene = SceneHelper.Instance.GetCurrentSceneName();

        // シーン名に基づいて戻り先を決定
        string previousScene = GetPreviousScene(currentScene);

        if (!string.IsNullOrEmpty(previousScene))
        {
          await SceneHelper.Instance.LoadSceneAsync(previousScene);
        }
        else
        {
          await SceneHelper.Instance.LoadSceneAsync(GameConstants.Scenes.Home);
        }
      }
      catch (Exception e)
      {
        // エラー時はホームに戻る
        await SceneHelper.Instance.LoadSceneAsync(GameConstants.Scenes.Home);
      }
    }

    /// <summary>
    /// 現在のシーンから戻り先を決定
    /// </summary>
    private string GetPreviousScene(string currentScene)
    {
      return currentScene switch
      {
        GameConstants.Scenes.Character => GameConstants.Scenes.Home,
        GameConstants.Scenes.Gacha => GameConstants.Scenes.Home,
        GameConstants.Scenes.Shop => GameConstants.Scenes.Home,
        GameConstants.Scenes.Training => GameConstants.Scenes.Home,
        GameConstants.Scenes.Grammar or GameConstants.Scenes.Pronunciation or GameConstants.Scenes.Vocabulary or GameConstants.Scenes.Conversation or GameConstants.Scenes.Speaking => GameConstants.Scenes.Training,
        GameConstants.Scenes.GrammarTower => GameConstants.Scenes.Grammar,
        GameConstants.Scenes.Result => GameConstants.Scenes.Training,
        _ => GameConstants.Scenes.Home,
      };
    }

    protected override void GetUIReferences()
    {
      base.GetUIReferences();

      if (backButton == null)
      {
        backButton = GetComponentInChildren<Button>();
      }
    }
  }
}