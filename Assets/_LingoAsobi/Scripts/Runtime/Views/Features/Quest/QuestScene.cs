using UnityEngine;
using System;
using System.Threading.Tasks;
using Scripts.Runtime.Core;
using Scripts.Runtime.DataModels;
using Scripts.Runtime.Data.Repositories;
using Scripts.Runtime.Services;

namespace Scripts.Runtime.Views.Features.Quest
{
  public class QuestScene : MonoBehaviour
  {
    [SerializeField] private QuestListView questListView;
    [SerializeField] private QuestDetailView questDetailView;
    [SerializeField] private RewardDisplayDialog rewardDialog;

    private QuestRepository questRepo;
    private QuestData[] activeQuests;

    private async void Start()
    {
      questRepo = DataManager.Instance.Quests;
      await LoadQuestData();
      SetupEventListeners();
    }

    private async Task LoadQuestData()
    {
      try
      {
        // アクティブクエストと完了可能クエストを取得
        var activeQuestsTask = questRepo.GetActiveQuestsAsync();
        var completableQuestsTask = questRepo.GetCompletableQuestsAsync();

        await Task.WhenAll(activeQuestsTask, completableQuestsTask);

        var active = await activeQuestsTask;
        var completable = await completableQuestsTask;

        // UI表示（完了可能なクエストを優先表示）
        activeQuests = completable.Concat(active).ToArray();
        questListView.DisplayQuests(activeQuests);
      }
      catch (Exception ex)
      {
        Debug.LogError($"Failed to load quest data: {ex.Message}");
      }
    }

    private void SetupEventListeners()
    {
      questListView.OnQuestSelected += OnQuestSelected;
      questListView.OnQuestCompleteRequested += OnQuestCompleteRequested;

      QuestRepository.OnQuestProgressUpdated += OnQuestProgressUpdated;
      QuestRepository.OnQuestCompleted += OnQuestCompleted;
    }

    private async void OnQuestSelected(QuestData quest)
    {
      try
      {
        // クエスト詳細データを取得
        var questDetail = await questRepo.GetQuestDetailAsync(quest.Id);
        questDetailView.ShowQuestDetail(questDetail);
      }
      catch (Exception ex)
      {
        Debug.LogError($"Failed to load quest detail: {ex.Message}");
      }
    }

    private async void OnQuestCompleteRequested(QuestData quest)
    {
      try
      {
        var result = await questRepo.CompleteQuestAsync(quest.Id);

        if (result.Success)
        {
          // 報酬表示
          rewardDialog.ShowRewards(result.Rewards);

          // クエストリスト更新
          await LoadQuestData();
        }
        else
        {
          ShowErrorDialog("クエスト完了処理に失敗しました");
        }
      }
      catch (Exception ex)
      {
        Debug.LogError($"Quest completion failed: {ex.Message}");
        ShowErrorDialog("クエスト完了処理でエラーが発生しました");
      }
    }

    private void OnQuestProgressUpdated(QuestProgressData progress)
    {
      questListView.UpdateQuestProgress(progress);
    }

    private void OnQuestCompleted(int questId, RewardData[] rewards)
    {
      Debug.Log($"Quest {questId} completed with {rewards.Length} rewards");

      // 完了エフェクト表示
      questListView.ShowQuestCompletionEffect(questId);
    }
  }
}