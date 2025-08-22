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
using System.Linq;
using Scripts.Runtime.Views.ViewData.Grammar;
using Scripts.Runtime.Views.Features.Grammar.GrammarFloor;
using Scripts.Runtime.Views.Features.Grammar.GrammarTower;

namespace Scripts.Runtime.Views.Features.Grammar
{
  /// <summary>
  /// トレーニング画面のView
  /// トレーニング内容を管理
  /// </summary>
  public class GrammarTowerView : BaseView
  {
    private GrammarTowerScene parentScene;

    private void Start()
    {
      parentScene = FindFirstObjectByType<GrammarTowerScene>(FindObjectsInactive.Include);
    }

    [Header("Header")]
    [SerializeField] private ProfileHeaderView profileHeaderView;

    [Header("Grammar Sections")]
    [SerializeField] private Transform sectionFloorContainer; // セクションを配置するコンテナ
    [SerializeField] private GameObject floorSectionButtonPrefab; // sectionFloor Prefab
    [SerializeField] private Image characterImage;

    private UserProfile currentUser;
    private GrammarData grammarData;
    private string characterImagePath;

    protected override void GetUIReferences()
    {
      base.GetUIReferences();

      if (profileHeaderView == null)
      {
        profileHeaderView = GetComponentInChildren<ProfileHeaderView>();
      }

      if (sectionFloorContainer == null)
      {
        sectionFloorContainer = transform.Find("SectionFloorContainer");
      }

      if (characterImage == null)
      {
        characterImage = transform.Find("CharacterImage").GetComponent<Image>();
      }
    }

    protected override void UpdateDisplay()
    {

      if (currentUser == null)
      {
        return;
      }

      if (characterImage != null)
      {
        characterImage.sprite = Resources.Load<Sprite>(characterImagePath);
      }

      // 文法セクションの生成
      GenerateGrammarFloorSections();

    }

    private void GenerateGrammarFloorSections()
    {

      if (grammarData == null)
      {
        return;
      }

      if (sectionFloorContainer == null)
      {
        return;
      }

      if (floorSectionButtonPrefab == null)
      {
        return;
      }

      // 既存のセクションをクリア
      ClearExistingSections();

      // 各文法データに対してセクションを生成
      foreach (var floorItem in grammarData.floors)
      {
        var sectionButton = Instantiate(floorSectionButtonPrefab, sectionFloorContainer);
        var floorSectionBox = sectionButton.GetComponentInChildren<FloorSectionBox>();

        if (floorSectionBox != null)
        {
          // データを設定
          floorSectionBox.SetData(floorItem.title, floorItem.description);

          // デフォルト画像を設定
          floorSectionBox.SetDefaultImage();

          // クリックイベントを設定
          SetupSectionClickEvent(sectionButton, floorItem);
        }
      }
    }

    private void ClearExistingSections()
    {
      if (sectionFloorContainer == null) return;

      // 既存の子要素を削除
      for (int i = sectionFloorContainer.childCount - 1; i >= 0; i--)
      {
        DestroyImmediate(sectionFloorContainer.GetChild(i).gameObject);
      }
    }

    private void SetupSectionClickEvent(GameObject sectionBox, GrammarFloorData floorItem)
    {
      var button = sectionBox.GetComponent<Button>();
      button?.onClick.AddListener(() => OnSectionClicked(floorItem));
    }

    /// <summary>
    /// セクションがクリックされた時の処理
    /// </summary>
    private async void OnSectionClicked(GrammarFloorData floorItem)
    {
      // SectionContainerの底を0として考える。
      // order = 1の時は、高さ0に移動させる。
      // order = 2の時は、高さ400に移動させる
      // order = 3の時は、高さ800に移動させる
      // order = 4の時は、高さ1200に移動させる
      int order = floorItem.order;
      float targetY = (order - 1) * 410; // order=1の時は0、order=2の時は410...

      // characterImageの底を基準にして移動
      RectTransform rectTransform = characterImage.GetComponent<RectTransform>();
      float characterHeight = rectTransform.rect.height;
      float bottomOffset = characterHeight; // 底からのオフセット（画像の高さ分）

      characterImage.transform.position = new Vector3(
          characterImage.transform.position.x,
          targetY + bottomOffset,
          characterImage.transform.position.z
      );

      if (parentScene == null)
      {
        Debug.LogError("parentScene is null");
      }

      // Scene経由でModalを表示
      parentScene?.ShowStandbyModal(
            floorItem
        );

      // 選択IDを次シーンへ渡す
      // GrammarFloorScene.SelectedGrammarFloorId = floorItem.id;
      // シーン遷移
      // await SceneHelper.Instance.LoadSceneAsync(GameConstants.Scenes.GrammarFloor);
    }

    public void SetViewData(GrammarTowerViewData data)
    {
      SetUserData(data.CurrentUser);
      SetGrammarData(data.GrammarData);
      SetCharacterImage(data.CharacterImageUrl);

      UpdateDisplay();
    }

    public void SetUserData(UserProfile user)
    {
      currentUser = user;
      profileHeaderView.SetUserData(currentUser);

      if (characterImage != null)
      {
        characterImage.sprite = Resources.Load<Sprite>(currentUser.userIconUrl);
      }
    }

    public void SetGrammarData(GrammarData grammar)
    {
      grammarData = grammar;

      if (grammarData != null)
      {
        UpdateDisplay();
      }
    }

    public void SetCharacterImage(string characterImagePath)
    {
      this.characterImagePath = characterImagePath;
    }
  }
}