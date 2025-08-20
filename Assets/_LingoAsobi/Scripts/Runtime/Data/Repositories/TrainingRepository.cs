using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Repositories.Base;
using Scripts.Runtime.Data.Models.Training;

namespace Scripts.Runtime.Data.Repositories
{
  /// <summary>
  /// キャラクターデータのリポジトリクラス
  /// キャラクター情報のCRUD操作を管理
  /// </summary>
  public class TrainingRepository : BaseRepository<TrainingData>
  {
    // APIエンドポイント
    protected override string EndpointUrl => "/api/trainings";

    // キャッシュキーのプレフィックス
    protected override string CacheKeyPrefix => "training";

    // シングルトンインスタンス
    private static readonly TrainingRepository _instance;
    public static TrainingRepository Instance
    {
      get
      {
        return _instance ?? new TrainingRepository();
      }
    }

    /// <summary>
    /// プライベートコンストラクタ（シングルトン）
    /// </summary>
    private TrainingRepository() : base()
    {
    }

    /// <summary>
    /// トレーニングデータを取得
    /// </summary>
    public async Task<TrainingData> GetTrainingDataAsync()
    {
      var trainingData = await GetAllAsync();
      return trainingData.FirstOrDefault();
    }

    protected override async Task<TrainingData> GetMockDataAsync()
    {
      return new TrainingData
      {
        grammarStatus = TrainingStatus.InProgress,
        pronunciationStatus = TrainingStatus.Completed,
        vocabularyStatus = TrainingStatus.InProgress,
        conversationStatus = TrainingStatus.NotStarted,
        speakingStatus = TrainingStatus.NotStarted,
      };
    }

    protected override async Task<TrainingData> GetMockDataByIdAsync(string id)
    {
      return null;
    }

    protected override async Task<List<TrainingData>> GetAllMockDataAsync()
    {
      return null;
    }

  }
}