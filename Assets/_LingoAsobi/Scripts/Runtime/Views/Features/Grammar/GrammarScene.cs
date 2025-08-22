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
using Scripts.Runtime.Views.ViewData.Grammar;

namespace Scripts.Runtime.Views.Features.Grammar
{
  /// <summary>
  /// タイトルシーンの管理クラス
  /// BaseSceneを継承して共通機能を活用
  /// </summary>
  public class GrammarScene : BaseScene
  {
    [Header("Grammar Scene References")]
    [SerializeField] private GrammarView grammarView;
    [SerializeField] private BackFooterView backFooterView;

    private UserProfile currentUser;
    private List<GrammarData> grammarDataList;

    #region Initialization

    protected override async Task OnInitializeAsync()
    {
      await base.OnInitializeAsync();

      currentUser = await DataManager.Instance.GetCurrentUserAsync();
      grammarDataList = await DataManager.Instance.GetGrammarAllDataAsync();

      grammarView.SetViewData(new GrammarViewData(currentUser, grammarDataList));
    }
    #endregion

    #region Scene Lifecycle

    protected override async Task OnAfterActivate()
    {
      await base.OnAfterActivate();

      // Viewの表示状態を確認
      if (grammarView.isVisible && backFooterView.isVisible)
      {
        // データを設定
        // SetDataToView();
        return;
      }

      // GrammarViewを表示
      if (grammarView != null)
      {
        await ShowViewAsync<GrammarView>();

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

    private async Task RefreshUserData()
    {
      currentUser = await DataManager.Instance.GetCurrentUserAsync();
    }

    protected override async Task OnBeforeDeactivate()
    {
      await base.OnBeforeDeactivate();
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