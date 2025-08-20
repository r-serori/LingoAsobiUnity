using System;
using UnityEngine;

namespace Scripts.Runtime.Data.Models.Training
{
  public enum TrainingStatus
  {
    Locked,
    NotStarted,
    InProgress,
    Completed,
    Emergency
  }

  /// <summary>
  /// キャラクターデータモデル
  /// ゲーム内のキャラクター情報を管理
  /// </summary>
  [Serializable]
  public class TrainingData
  {
    public TrainingStatus grammarStatus = TrainingStatus.NotStarted;
    public TrainingStatus pronunciationStatus = TrainingStatus.NotStarted;
    public TrainingStatus vocabularyStatus = TrainingStatus.NotStarted;
    public TrainingStatus conversationStatus = TrainingStatus.NotStarted;
    public TrainingStatus speakingStatus = TrainingStatus.NotStarted;

    public static string GetStatusImageName(TrainingStatus status)
    {
      return status switch
      {
        TrainingStatus.Locked => "LockedBadge",
        TrainingStatus.NotStarted => "NotStartedBadge",
        TrainingStatus.InProgress => "InProgressBadge",
        TrainingStatus.Completed => "CompletedBadge",
        TrainingStatus.Emergency => "EmergencyBadge",
        _ => "NotStartedBadge",
      };
    }
  }
}