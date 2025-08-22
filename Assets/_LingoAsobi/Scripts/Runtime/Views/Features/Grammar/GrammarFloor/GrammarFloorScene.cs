
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
using Scripts.Runtime.Views.Features.Shared.Modal;
using TMPro;

namespace Scripts.Runtime.Views.Features.Grammar.GrammarFloor
{
  /// <summary>
  /// GrammarFloorのシーン（文法カテゴリ選択後の画面）
  /// GrammarViewから渡された選択GrammarIDを受け取って利用する
  /// </summary>
  public class GrammarFloorScene : BaseScene
  {
    // 遷移間で値を受け渡すための一時ストレージ
    public static int SelectedFloorId { get; set; }

    [Header("GrammarFloor Scene References")]
    [SerializeField] private GrammarFloorView grammarFloorView;
    [SerializeField] private BackFooterView backFooterView;
    [SerializeField] private StandbyFloorModal standbyFloorModal;

    private UserProfile currentUser;
    private List<CharacterData> characterDataList;
    private List<CharacterFormationData> characterFormations;
    private GrammarFloorData floorItem;

    protected override async Task OnInitializeAsync()
    {
      await base.OnInitializeAsync();
      currentUser = await DataManager.Instance.GetCurrentUserAsync();
      characterFormations = await DataManager.Instance.GetAllCharacterFormationsAsync();
      List<string> characterIds = characterFormations.Select(x => x.characterId).Distinct().ToList();
      characterDataList = await DataManager.Instance.GetCharacterByIdsAsync(characterIds);
      floorItem = await DataManager.Instance.GetGrammarFloorDataByIdAsync(SelectedFloorId);

      SetDataToView();
    }

    protected override async Task OnAfterActivate()
    {
      await base.OnAfterActivate();

      if (grammarFloorView.isVisible && backFooterView.isVisible)
      {
        return;
      }

      if (grammarFloorView != null)
      {
        await ShowViewAsync<GrammarFloorView>();
      }

      if (backFooterView != null)
      {
        await ShowViewAsync<BackFooterView>();
      }

      if (standbyFloorModal != null)
      {
        standbyFloorModal.gameObject.SetActive(false);
      }

      await RefreshUserData();
    }

    private async Task RefreshUserData()
    {
      currentUser = await DataManager.Instance.GetCurrentUserAsync();
    }

    private void SetDataToView()
    {
      if (grammarFloorView != null && currentUser != null && floorItem != null)
      {
        grammarFloorView.SetViewData(new GrammarFloorViewData(currentUser, floorItem, currentUser.userIconUrl));
      }
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
    public void ShowStandbyModal(GrammarLessonData lesson)
    {
      if (standbyFloorModal != null)
      {
        standbyFloorModal.SetData(characterDataList, lesson);
        standbyFloorModal.gameObject.SetActive(true);
      }
    }

    public void HideStandbyModal()
    {
      if (standbyFloorModal != null)
      {
        standbyFloorModal.gameObject.SetActive(false);
      }
    }
  }
}