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
using Scripts.Runtime.Views.ViewData.Training;

namespace Scripts.Runtime.Views.Features.Training
{
  /// <summary>
  /// トレーニング画面のView
  /// トレーニング内容を管理
  /// </summary>
  public class TrainingView : BaseView
  {
    [Header("Header")]
    [SerializeField] public ProfileHeaderView profileHeaderView;
    [SerializeField] public Image GrammarStatusBadge;
    [SerializeField] public Image PronunciationStatusBadge;
    [SerializeField] public Image VocabularyStatusBadge;
    [SerializeField] public Image ConversationStatusBadge;
    [SerializeField] public Image SpeakingStatusBadge;

    [SerializeField] private Button grammarButton;
    [SerializeField] private Button pronunciationButton;
    [SerializeField] private Button vocabularyButton;
    [SerializeField] private Button conversationButton;
    [SerializeField] private Button speakingButton;

    private UserProfile currentUser;
    private TrainingData trainingData;

    protected override void SetupEventListeners()
    {
      base.SetupEventListeners();

      // ボタンのイベントリスナーを設定
      grammarButton.onClick?.AddListener(OnGrammarButtonClicked);

      pronunciationButton.onClick?.AddListener(OnPronunciationButtonClicked);

      vocabularyButton.onClick?.AddListener(OnVocabularyButtonClicked);

      conversationButton.onClick?.AddListener(OnConversationButtonClicked);

      speakingButton.onClick?.AddListener(OnSpeakingButtonClicked);
    }

    protected override void GetUIReferences()
    {
      base.GetUIReferences();

      if (profileHeaderView == null)
      {
        profileHeaderView = GetComponentInChildren<ProfileHeaderView>();
      }

      if (GrammarStatusBadge == null)
      {
        GrammarStatusBadge = GetComponentInChildren<Image>();
      }

      if (PronunciationStatusBadge == null)
      {
        PronunciationStatusBadge = GetComponentInChildren<Image>();
      }

      if (VocabularyStatusBadge == null)
      {
        VocabularyStatusBadge = GetComponentInChildren<Image>();
      }

      if (ConversationStatusBadge == null)
      {
        ConversationStatusBadge = GetComponentInChildren<Image>();
      }

      if (SpeakingStatusBadge == null)
      {
        SpeakingStatusBadge = GetComponentInChildren<Image>();
      }
    }

    /// <summary>
    /// ユーザーデータを設定
    /// </summary>
    public void SetUserData(UserProfile user)
    {
      currentUser = user;
      profileHeaderView.SetUserData(currentUser);
    }

    /// <summary>
    /// トレーニングデータを設定
    /// </summary>
    public void SetTrainingData(TrainingData trainingData)
    {
      this.trainingData = trainingData;
    }

    protected override void UpdateDisplay()
    {
      if (currentUser == null) return;
      // トレーニングデータ
      UpdateTrainingDataDisplay();
    }

    public void SetViewData(TrainingViewData data)
    {
      SetUserData(data.CurrentUser);
      SetTrainingData(data.TrainingData);

      UpdateDisplay();
    }

    protected override async Task LoadDataAsync()
    {
    }

    private void UpdateTrainingDataDisplay()
    {
      if (trainingData == null) return;

      // TODO: 本番用、現状は固定画像を表示
      // // グラムマスター
      // if (GrammarStatusBadge != null)
      // {
      //   GrammarStatusBadge.sprite = Resources.Load<Sprite>(TrainingData.GetStatusImageName(trainingData.grammarStatus));
      // }

      // // 発音
      // if (PronunciationStatusBadge != null)
      // {
      //   PronunciationStatusBadge.sprite = Resources.Load<Sprite>(TrainingData.GetStatusImageName(trainingData.pronunciationStatus));
      // }

      // // 単語
      // if (VocabularyStatusBadge != null)
      // {
      //   VocabularyStatusBadge.sprite = Resources.Load<Sprite>(TrainingData.GetStatusImageName(trainingData.vocabularyStatus));
      // }

      // // 会話
      // if (ConversationStatusBadge != null)
      // {
      //   ConversationStatusBadge.sprite = Resources.Load<Sprite>(TrainingData.GetStatusImageName(trainingData.conversationStatus));
      // }

      // // 発話
      // if (SpeakingStatusBadge != null)
      // {
      //   SpeakingStatusBadge.sprite = Resources.Load<Sprite>(TrainingData.GetStatusImageName(trainingData.speakingStatus));
      // }
    }

    /// <summary>
    /// 他のシーンへ遷移
    /// </summary>
    protected virtual async Task NavigateToSceneAsync(string targetSceneName)
    {
      await SceneHelper.Instance.LoadSceneAsync(targetSceneName);
    }

    #region Button Handlers

    private async void OnGrammarButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Grammar);
    }

    private async void OnPronunciationButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Pronunciation);
    }

    private async void OnVocabularyButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Vocabulary);
    }

    private async void OnConversationButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Conversation);
    }

    private async void OnSpeakingButtonClicked()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Speaking);
    }

    #endregion

  }
}