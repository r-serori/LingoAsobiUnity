using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Grammar;
using System.Collections.Generic;
using Scripts.Runtime.Views.Features.Footer;
using Scripts.Runtime.Data.Models.Character;
using System.Linq;
using Scripts.Runtime.Views.ViewData.Grammar;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Views.Features.Shared.Modal;

namespace Scripts.Runtime.Views.Features.Grammar.GrammarTower
{
  /// <summary>
  /// GrammarTowerのシーン（文法カテゴリ選択後の画面）
  /// GrammarViewから渡された選択GrammarIDを受け取って利用する
  /// </summary>
  public class GrammarTowerScene : BaseScene
  {
    // 遷移間で値を受け渡すための一時ストレージ
    public static int SelectedGrammarId { get; set; }

    [Header("GrammarTower Scene References")]
    [SerializeField] private GrammarTowerView grammarTowerView;
    [SerializeField] private BackFooterView backFooterView;
    [SerializeField] private StandbyTowerModal standbyTowerModal;

    private UserProfile currentUser;
    private GrammarData grammarData;

    protected override async Task OnInitializeAsync()
    {
      await base.OnInitializeAsync();

      currentUser = await DataManager.Instance.GetCurrentUserAsync();
      grammarData = await DataManager.Instance.GetGrammarDataByIdAsync(SelectedGrammarId);

      SetDataToView();
    }

    protected override async Task OnAfterActivate()
    {
      await base.OnAfterActivate();

      // Viewの表示状態を確認
      if (grammarTowerView.isVisible && backFooterView.isVisible)
      {
        // データを設定
        // SetDataToView();
        return;
      }

      // GrammarViewを表示
      if (grammarTowerView != null)
      {
        await ShowViewAsync<GrammarTowerView>();

        // View表示後にデータを設定
        // SetDataToView();
      }

      // BackFooterViewも表示
      if (backFooterView != null)
      {
        await ShowViewAsync<BackFooterView>();
      }

      if (standbyTowerModal != null)
      {
        standbyTowerModal.gameObject.SetActive(false);
      }

      // データを更新
      await RefreshUserData();
    }

    private void SetDataToView()
    {
      if (grammarTowerView != null && currentUser != null && grammarData != null)
      {
        grammarTowerView.SetViewData(new GrammarTowerViewData(currentUser, grammarData, currentUser.userIconUrl));
      }
      else
      {
        Debug.LogError($"GrammarTowerScene: Cannot set data - View: {grammarTowerView != null}, User: {currentUser != null}, Data: {grammarData != null}");
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

    // Modalの表示メソッド
    public void ShowStandbyModal(GrammarFloorData floorItem)
    {
      if (standbyTowerModal != null)
      {
        standbyTowerModal.SetData(floorItem);
        standbyTowerModal.gameObject.SetActive(true);
      }
    }

    public void HideStandbyModal()
    {
      if (standbyTowerModal != null)
      {
        standbyTowerModal.gameObject.SetActive(false);
      }
    }
  }
}

