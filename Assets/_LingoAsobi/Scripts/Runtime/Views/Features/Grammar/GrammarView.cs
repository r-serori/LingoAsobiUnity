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

namespace Scripts.Runtime.Views.Features.Grammar
{
  /// <summary>
  /// トレーニング画面のView
  /// トレーニング内容を管理
  /// </summary>
  public class GrammarView : BaseView
  {
    [Header("Header")]
    [SerializeField] public ProfileHeaderView profileHeaderView;

    [Header("Grammar Sections")]
    [SerializeField] private Transform sectionContainer; // セクションを配置するコンテナ
    [SerializeField] private Button questSectionBoxButton; // QuestSectionBox Prefab

    private UserProfile currentUser;
    private List<GrammarData> grammarData;

    protected override void GetUIReferences()
    {
      base.GetUIReferences();
      Debug.Log("GrammarView: GetUIReferences called");

      if (profileHeaderView == null)
      {
        profileHeaderView = GetComponentInChildren<ProfileHeaderView>();
        Debug.Log($"GrammarView: ProfileHeaderView found: {profileHeaderView != null}");
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
            Debug.Log($"GrammarView: Found section container: {container.name}");
            break;
          }
        }

        if (sectionContainer == null)
        {
          Debug.LogError("GrammarView: No suitable section container found");
        }
      }

      if (questSectionBoxButton == null)
      {
        Debug.LogError("GrammarView: questSectionBoxPrefab is not set in Inspector");
      }
      Debug.Log("GrammarView: GetUIReferences completed");
    }

    private void UpdateGrammarDataDisplay()
    {
      if (grammarData == null) return;
    }

    protected override void UpdateDisplay()
    {
      Debug.Log("GrammarView: UpdateDisplay called");

      if (currentUser == null)
      {
        Debug.LogWarning("GrammarView: currentUser is null");
        return;
      }

      // UpdateGrammarDataDisplay();

      // 文法セクションの生成
      GenerateGrammarSections();
    }
    private void GenerateGrammarSections()
    {
      Debug.Log("GrammarView: GenerateGrammarSections called");

      if (grammarData == null)
      {
        Debug.LogWarning("GrammarView: grammarData is null in GenerateGrammarSections");
        return;
      }

      if (sectionContainer == null)
      {
        Debug.LogError("GrammarView: sectionContainer is null");
        return;
      }

      if (questSectionBoxButton == null)
      {
        Debug.LogError("GrammarView: questSectionBoxPrefab is null");
        return;
      }

      Debug.Log($"GrammarView: Generating {grammarData.Count} sections");

      // 既存のセクションをクリア
      ClearExistingSections();

      // 各文法データに対してセクションを生成
      foreach (var grammarDataItem in grammarData)
      {
        var sectionButton = Instantiate(questSectionBoxButton, sectionContainer);
        var questSectionBox = sectionButton.GetComponentInChildren<QuestSectionBox>();

        if (questSectionBox != null)
        {
          // データを設定
          questSectionBox.SetData(grammarDataItem.title, grammarDataItem.description);
          Debug.Log($"GrammarView: Created section: {grammarDataItem.title}");

          // デフォルト画像を設定
          questSectionBox.SetDefaultImage();

          // クリックイベントを設定
          SetupSectionClickEvent(sectionButton, grammarDataItem);
        }
        else
        {
          Debug.LogError($"GrammarView: QuestSectionBox component not found on instantiated prefab");
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

    private void SetupSectionClickEvent(Button sectionButton, GrammarData grammarData)
    {
      var button = sectionButton.GetComponent<Button>();
      if (button != null)
      {
        button.onClick.AddListener(() => OnSectionClicked(grammarData));
        Debug.Log($"GrammarView: Click event set for section: {grammarData.title}");
      }
      else
      {
        Debug.LogError($"GrammarView: Button component not found on section button");
      }
    }
    /// <summary>
    /// セクションがクリックされた時の処理
    /// </summary>
    private void OnSectionClicked(GrammarData grammarData)
    {

      // TODO: セクション選択時の処理を実装
      // 例：詳細画面への遷移、選択状態の更新など
    }

    public void SetUserData(UserProfile user)
    {
      currentUser = user;
      profileHeaderView.SetUserData(currentUser);
    }

    public void SetGrammarData(List<GrammarData> grammarData)
    {
      Debug.Log("GrammarView: SetGrammarData called");

      this.grammarData = grammarData;
      Debug.Log($"GrammarView: GrammarData count: {this.grammarData?.Count ?? 0}");

      // データが設定されたら表示を更新
      if (this.grammarData != null)
      {
        Debug.Log("GrammarView: Calling UpdateDisplay");
        UpdateDisplay();
      }
      else
      {
        Debug.LogWarning("GrammarView: grammarData is null");
      }
    }
    protected override async Task LoadDataAsync()
    {
      // 必要に応じてデータの非同期読み込みを実装
    }
  }
}