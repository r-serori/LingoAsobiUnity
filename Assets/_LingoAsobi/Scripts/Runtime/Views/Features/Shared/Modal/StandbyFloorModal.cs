using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.Data.Models.Character;
using System.Collections.Generic;
using Scripts.Runtime.Data.Models.Grammar;
using Scripts.Runtime.Utilities.Helpers;
using Scripts.Runtime.Views.Features.Grammar.GrammarFloor;

namespace Scripts.Runtime.Views.Features.Shared.Modal
{
  public class StandbyFloorModal : MonoBehaviour
  {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private TextMeshProUGUI exampleText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backgroundButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Transform characterContainer; // キャラクターを配置するコンテナ
    [SerializeField] private GameObject characterButtonPrefab; // キャラクターを配置するボタン

    private List<CharacterData> characterDataList;
    private GrammarLessonData lessonItem;
    private void Start()
    {
      SetupButtons();
    }

    public void SetData(List<CharacterData> characterDataList, GrammarLessonData lessonItem)
    {
      titleText.text = lessonItem.title;
      descriptionText.text = lessonItem.description;
      explanationText.text = lessonItem.explanation;
      exampleText.text = lessonItem.examples;
      this.characterDataList = characterDataList;
      this.lessonItem = lessonItem;

      GenerateStandbyCharacterBoxes();
    }

    private void GenerateStandbyCharacterBoxes()
    {
      if (characterDataList == null)
      {
        return;
      }

      if (characterContainer == null)
      {
        return;
      }

      if (characterButtonPrefab == null)
      {
        return;
      }

      // 既存のセクションをクリア
      ClearExistingSections();

      // 各文法データに対してセクションを生成
      foreach (var characterData in characterDataList)
      {
        var characterButton = Instantiate(characterButtonPrefab, characterContainer);
        var standbyCharacterBox = characterButton.GetComponentInChildren<StandbyCharacterBox>();

        if (characterButton != null)
        {
          // データを設定
          standbyCharacterBox.SetCharacterImage(characterData.iconImagePath);
          standbyCharacterBox.SetFrameImage(characterData.attribute);
          // クリックイベントを設定
          SetupCharacterButtonClickEvent(characterButton, characterData);
        }
      }
    }

    private void ClearExistingSections()
    {
      if (characterContainer == null) return;

      // 既存の子要素を削除
      for (int i = characterContainer.childCount - 1; i >= 0; i--)
      {
        DestroyImmediate(characterContainer.GetChild(i).gameObject);
      }
    }

    private void SetupCharacterButtonClickEvent(GameObject characterBox, CharacterData characterData)
    {
      var button = characterBox.GetComponent<Button>();
      button?.onClick.AddListener(() => OnCharacterButtonClicked(characterData));
    }

    private void SetupButtons()
    {
      backgroundButton?.onClick.AddListener(OnBackgroundClicked);
      closeButton?.onClick.AddListener(OnCloseButtonClicked);
      startButton?.onClick.AddListener(OnStartButtonClicked);
    }

    public void OnCloseButtonClicked()
    {
      gameObject.SetActive(false);
    }

    public void OnBackgroundClicked()
    {
      gameObject.SetActive(false);
    }

    public async void OnStartButtonClicked()
    {
      // GrammarFloorScene.SelectedFloorId = lessonItem.id;

      // await SceneHelper.Instance.LoadSceneAsync(sceneName);
    }

    public void OnCharacterButtonClicked(CharacterData characterData)
    {
      // gameObject.SetActive(false);
    }
  }
}