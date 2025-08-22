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
using Scripts.Runtime.Core;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Views.Features.Header;
using Scripts.Runtime.Data.Models.Training;
using Scripts.Runtime.Data.Models.Grammar;
using System.Collections.Generic;
using Scripts.Runtime.Views.ViewData.Grammar;
using Scripts.Runtime.Views.Features.Grammar.GrammarTower;

namespace Scripts.Runtime.Views.Features.Grammar
{
  /// <summary>
  /// トレーニング画面のView
  /// トレーニング内容を管理
  /// </summary>
  public class GrammarView : BaseView
  {
    [Header("Header")]
    [SerializeField] private ProfileHeaderView profileHeaderView;

    [Header("Grammar Sections")]
    [SerializeField] private Transform sectionContainer; // セクションを配置するコンテナ
    [SerializeField] private GameObject questSectionBoxPrefab; // QuestSectionBox Prefab

    private UserProfile currentUser;
    private List<GrammarData> grammarDataList;

    protected override void GetUIReferences()
    {
      base.GetUIReferences();

      if (profileHeaderView == null)
      {
        profileHeaderView = GetComponentInChildren<ProfileHeaderView>();
      }

      if (sectionContainer == null)
      {
        // 子要素から適切なコンテナを探す
        var containers = GetComponentsInChildren<Transform>();
        foreach (var container in containers)
        {
          if (container.name.Contains("Container") || container.name.Contains("Content") || container.name.Contains("Panel"))
          {
            sectionContainer = container;
            break;
          }
        }
      }
    }

    protected override void UpdateDisplay()
    {

      if (currentUser == null)
      {
        return;
      }

      // 文法セクションの生成
      GenerateGrammarSections();
    }

    private void GenerateGrammarSections()
    {

      if (grammarDataList == null)
      {
        return;
      }

      if (sectionContainer == null)
      {
        return;
      }

      if (questSectionBoxPrefab == null)
      {
        return;
      }


      // 既存のセクションをクリア
      ClearExistingSections();

      // 各文法データに対してセクションを生成
      foreach (var grammarDataItem in grammarDataList)
      {
        var sectionButton = Instantiate(questSectionBoxPrefab, sectionContainer);
        var questSectionBox = sectionButton.GetComponentInChildren<QuestSectionBox>();

        if (questSectionBox != null)
        {
          // データを設定
          questSectionBox.SetData(grammarDataItem.title, grammarDataItem.description);

          // デフォルト画像を設定
          questSectionBox.SetDefaultImage();

          // クリックイベントを設定
          SetupSectionClickEvent(sectionButton, grammarDataItem);
        }
      }
    }

    /// <summary>
    /// 既存のセクションをクリア
    /// </summary>
    private void ClearExistingSections()
    {
      if (sectionContainer == null) return;

      // 既存の子要素を削除
      for (int i = sectionContainer.childCount - 1; i >= 0; i--)
      {
        DestroyImmediate(sectionContainer.GetChild(i).gameObject);
      }
    }

    /// <summary>
    /// セクションのクリックイベントを設定
    /// </summary>
    private void SetupSectionClickEvent(GameObject sectionBox, GrammarData grammarData)
    {
      var button = sectionBox.GetComponent<Button>();
      button?.onClick.AddListener(() => OnSectionClicked(grammarData));
    }

    /// <summary>
    /// セクションがクリックされた時の処理
    /// </summary>
    private async void OnSectionClicked(GrammarData grammarData)
    {
      // 選択IDを次シーンへ渡す
      GrammarTowerScene.SelectedGrammarId = grammarData.id;
      // シーン遷移
      await SceneHelper.Instance.LoadSceneAsync(GameConstants.Scenes.GrammarTower);
    }

    public void SetUserData(UserProfile user)
    {
      currentUser = user;
      profileHeaderView.SetUserData(currentUser);
    }

    public void SetGrammarData(List<GrammarData> grammarDataList)
    {
      this.grammarDataList = grammarDataList;
    }

    public void SetViewData(GrammarViewData data)
    {
      SetUserData(data.CurrentUser);
      SetGrammarData(data.GrammarDataList);

      UpdateDisplay();
    }

    protected override async Task LoadDataAsync()
    {
      // 必要に応じてデータの非同期読み込みを実装
    }
  }
}