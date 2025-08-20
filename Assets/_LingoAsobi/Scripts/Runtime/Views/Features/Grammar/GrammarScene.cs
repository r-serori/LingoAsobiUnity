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
using System.Collections.Generic;
using Scripts.Runtime.Data.Models.Grammar;

namespace Scripts.Runtime.Views.Features.Grammar
{
  /// <summary>
  /// タイトルシーンの管理クラス
  /// BaseSceneを継承して共通機能を活用
  /// </summary>
  public class GrammarScene : BaseScene
  {
    [Header("Training Scene References")]
    [SerializeField] private GrammarView grammarView;
    [SerializeField] private BackFooterView backFooterView;

    private UserProfile currentUser;
    private List<GrammarData> grammarData;

    #region Initialization

    private async void Start()
    {
      Debug.Log("GrammarScene: Start called");

      try
      {
        // まずViewの初期化完了を待つ
        await WaitForViewInitialization();

        // データ取得を個別に実行し、エラーハンドリングを追加
        try
        {
          currentUser = await DataManager.Instance.GetCurrentUserAsync();
          Debug.Log($"GrammarScene: User data retrieved: {currentUser != null}");
        }
        catch (System.Exception ex)
        {
          Debug.LogError($"GrammarScene: Error getting user data: {ex.Message}");
          currentUser = null;
        }

        try
        {
          grammarData = await DataManager.Instance.GetGrammarAllDataAsync();
          Debug.Log($"GrammarScene: Grammar data retrieved: {grammarData?.Count ?? 0}");
        }
        catch (System.Exception ex)
        {
          Debug.LogError($"GrammarScene: Error getting grammar data: {ex.Message}");
          // エラーが発生した場合は空のリストを使用
          grammarData = new List<GrammarData>();
        }

        Debug.Log("GrammarScene: Start completed successfully");

        // データを設定
        SetDataToView();
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"GrammarScene: Error in Start: {ex.Message}");
      }
    }

    protected override async Task OnInitializeAsync()
    {
      // await base.OnInitializeAsync();

      currentUser = await DataManager.Instance.GetCurrentUserAsync();
      grammarData = await DataManager.Instance.GetGrammarAllDataAsync();
    }
    #endregion

    #region Scene Lifecycle

    protected override async Task OnAfterActivate()
    {
      // await base.OnAfterActivate();
      Debug.Log("GrammarScene: OnAfterActivate started");

      // Viewの表示状態を確認
      if (grammarView.isVisible && backFooterView.isVisible)
      {
        Debug.Log("GrammarScene: Views already visible, setting data");
        // データを設定
        // SetDataToView();
        return;
      }

      // GrammarViewを表示
      if (grammarView != null)
      {
        await ShowViewAsync<GrammarView>();
        Debug.Log("GrammarScene: GrammarView shown");

        // View表示後にデータを設定
        // SetDataToView();
      }

      // BackFooterViewも表示
      if (backFooterView != null)
      {
        await ShowViewAsync<BackFooterView>();
      }

      // データを更新
      await RefreshUserData();
    }

    private void SetDataToView()
    {
      Debug.Log("GrammarScene: Setting data to view");

      if (grammarView != null && currentUser != null && grammarData != null)
      {
        grammarView.SetUserData(currentUser);
        grammarView.SetGrammarData(grammarData);
        Debug.Log("GrammarScene: Data set to view successfully");
      }
      else
      {
        Debug.LogError($"GrammarScene: Cannot set data - View: {grammarView != null}, User: {currentUser != null}, Data: {grammarData != null}");
      }
    }

    private async Task RefreshUserData()
    {
      currentUser = await DataManager.Instance.GetCurrentUserAsync();
    }

    protected override async Task OnBeforeDeactivate()
    {
      await base.OnBeforeDeactivate();
    }

    private async Task WaitForViewInitialization()
    {
      Debug.Log("GrammarScene: Waiting for view initialization");

      // Viewの初期化完了を待つ
      int maxWaitTime = 100; // 最大1秒待機
      int waitCount = 0;

      while (grammarView == null && waitCount < maxWaitTime)
      {
        await Task.Delay(10);
        waitCount++;
      }

      if (grammarView != null)
      {
        Debug.Log("GrammarScene: View initialization completed");
      }
      else
      {
        Debug.LogError("GrammarScene: View initialization timeout");
      }
    }

    protected override void SubscribeToEvents()
    {
      base.SubscribeToEvents();
    }

    protected override void UnsubscribeFromEvents()
    {
      base.UnsubscribeFromEvents();
    }

    private void OnCurrencyChanged(CurrencyChangedEvent e)
    {
    }

    #endregion
  }
}