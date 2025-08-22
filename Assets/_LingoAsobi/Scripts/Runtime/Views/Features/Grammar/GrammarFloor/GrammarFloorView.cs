
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

namespace Scripts.Runtime.Views.Features.Grammar
{
  /// <summary>
  /// トレーニング画面のView
  /// トレーニング内容を管理
  /// </summary>
  public class GrammarFloorView : BaseView
  {
    private GrammarFloorScene parentScene;

    private void Start()
    {
      parentScene = FindFirstObjectByType<GrammarFloorScene>(FindObjectsInactive.Include);

      // ScrollRectの初期位置を一番下に設定
      StartCoroutine(SetInitialScrollPosition());
    }

    [Header("Header")]
    [SerializeField] private ProfileHeaderView profileHeaderView;
    [SerializeField] private TextMeshProUGUI floorTitleText;

    [Header("Grammar Sections")]
    [SerializeField] private Transform sectionFloorContainer; // セクションを配置するコンテナ
    [SerializeField] private Transform sectionFloorContent; // セクションのコンテンツを配置するコンテナ
    [SerializeField] private GameObject floorLessonLayout;

    [SerializeField] private Image characterImage;

    private UserProfile currentUser;
    private GrammarFloorData floorItem;
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

      if (sectionFloorContent == null)
      {
        sectionFloorContent = transform.Find("SectionFloorContent");
      }

      // ScrollRectの設定はUnity Editor上で行ってください
      // 以下の設定が必要です：
      // - ScrollRectコンポーネントを追加
      // - horizontal = false, vertical = true
      // - content = sectionFloorContainer
      // - viewport = sectionFloorContainer

      if (floorLessonLayout == null)
      {
        floorLessonLayout = Resources.Load<GameObject>("Prefabs/Views/Grammar/GrammarFloor/LessonLayout");
      }

      if (floorTitleText == null)
      {
        floorTitleText = transform.Find("FloorTitleText").GetComponent<TextMeshProUGUI>();
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

      floorTitleText.text = floorItem.title;
      GenerateGrammarFloorSections();

      StartCoroutine(MoveCharacterAfterLayout(floorItem.lastCorrectedLessonOrder));
    }

    private void GenerateGrammarFloorSections()
    {
      if (floorItem == null)
      {
        return;
      }

      if (sectionFloorContainer == null)
      {
        return;
      }

      if (sectionFloorContent == null)
      {
        return;
      }

      if (floorLessonLayout == null)
      {
        return;
      }

      // 既存のセクションをクリア
      ClearExistingSections();

      for (int i = 0; i < floorItem.lessons.Count; i++)
      {
        var lesson = floorItem.lessons[i];

        // Prefabをインスタンス化
        var lessonLayoutInstance = Instantiate(floorLessonLayout, sectionFloorContent);

        // インスタンス化されたオブジェクトからLessonLayoutコンポーネントを取得
        var lessonLayout = lessonLayoutInstance.GetComponent<LessonLayout>();

        if (lessonLayout != null)
        {
          // データを設定
          lessonLayout.SetData(lesson.order, lesson.title);

          // デフォルト画像を設定
          lessonLayout.SetDefaultImage();

          // クリックイベントを設定（LessonLayoutのlessonButtonに対して）
          SetupSectionClickEvent(lessonLayout, lesson);

          // LessonLayoutのChild Alignmentを変更していく。
          SetLessonLayoutAlignment(lessonLayout, i);
        }
        else
        {
        }
      }


    }

    /// <summary>
    /// LessonLayoutのChildAlignmentを設定
    /// </summary>
    private void SetLessonLayoutAlignment(LessonLayout lessonLayout, int index)
    {
      if (lessonLayout == null) return;
      var alignment = index switch
      {
        // 1つ目
        0 => TextAnchor.MiddleCenter,
        // 2つ目
        1 => TextAnchor.MiddleRight,
        // 3つ目以降
        _ => (index % 2 == 0) ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight,// 偶数番号はMiddleRight、奇数番号はMiddleLeft
      };
      lessonLayout.SetChildAlignment(alignment);
    }

    /// <summary>
    /// ScrollRectの初期位置を一番下に設定
    /// </summary>
    private System.Collections.IEnumerator SetInitialScrollPosition()
    {
      // レイアウトが完了するまで待機
      yield return new WaitForEndOfFrame();

      if (sectionFloorContainer == null) yield break;

      var scrollRect = sectionFloorContainer.GetComponent<ScrollRect>();
      if (scrollRect != null)
      {
        // 一番下にスクロール（verticalNormalizedPosition = 0）
        scrollRect.verticalNormalizedPosition = 0f;
      }
    }

    private void ClearExistingSections()
    {
      if (sectionFloorContent == null) return;

      // 既存の子要素を削除
      for (int i = sectionFloorContent.childCount - 1; i >= 0; i--)
      {
        DestroyImmediate(sectionFloorContent.GetChild(i).gameObject);
      }
    }

    private void SetupSectionClickEvent(LessonLayout lessonLayout, GrammarLessonData lesson)
    {
      var button = lessonLayout.LessonButton;
      button?.onClick.AddListener(() => OnSectionClicked(lesson));
    }

    /// <summary>
    /// セクションがクリックされた時の処理
    /// </summary>
    private async void OnSectionClicked(GrammarLessonData lesson)
    {
      // 各floorSectionButtonPrefabに対して移動していく動き
      // クリックしたfloorSectionButtonPrefabにcharacterImageを移動させる
      int order = lesson.order;

      // クリックされたセクションの位置を取得してcharacterImageを移動
      MoveCharacterToSection(order);

      if (parentScene == null)
      {
        Debug.LogError("parentScene is null");
      }

      // Scene経由でModalを表示
      parentScene?.ShowStandbyModal(
            lesson
        );

      // 選択IDを次シーンへ渡す
      // GrammarFloorScene.SelectedGrammarFloorId = floorItem.id;
      // シーン遷移
      // await SceneHelper.Instance.LoadSceneAsync(GameConstants.Scenes.GrammarFloor);
    }

    /// <summary>
    /// 指定されたorderのセクションの位置にcharacterImageを移動
    /// </summary>
    private void MoveCharacterToSection(int targetOrder)
    {
      if (characterImage == null || sectionFloorContent == null)
      {
        return;
      }

      // 指定されたorderのセクションを探す
      for (int i = 0; i < sectionFloorContent.childCount; i++)
      {
        var child = sectionFloorContent.GetChild(i);
        var lessonLayout = child.GetComponent<LessonLayout>();

        if (lessonLayout != null)
        {
          int lessonOrder = lessonLayout.GetOrder();

          if (lessonOrder == targetOrder)
          {
            // セクションの位置を取得
            RectTransform lessonButtonRect = lessonLayout.LessonButton.GetComponent<RectTransform>();
            if (lessonButtonRect != null)
            {
              // LessonButtonのワールド座標を取得
              Vector3 worldPosition = lessonButtonRect.position;

              // characterImageをLessonButtonの位置に移動
              if (characterImage != null)
              {
                characterImage.transform.position = worldPosition;
              }
            }
            break;
          }
        }
      }
    }

    /// <summary>
    /// レイアウト完了後にキャラクター移動を行う
    /// </summary>
    private System.Collections.IEnumerator MoveCharacterAfterLayout(int targetOrder)
    {
      // レイアウトの完了を待機
      yield return new WaitForEndOfFrame();

      // さらに1フレーム待機してレイアウト計算を確実に完了させる
      yield return null;

      // キャラクター移動を実行
      MoveCharacterToSection(targetOrder);

    }


    public void SetViewData(GrammarFloorViewData data)
    {
      SetUserData(data.CurrentUser);
      SetCharacterImage(data.CharacterImageUrl);

      // floorItemを設定
      if (data != null)
      {
        floorItem = data.FloorItem;
      }

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

    public void SetCharacterImage(string characterImagePath)
    {
      this.characterImagePath = characterImagePath;
    }

  }
}
