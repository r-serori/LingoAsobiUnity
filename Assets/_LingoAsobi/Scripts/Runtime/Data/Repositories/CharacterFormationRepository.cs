using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Repositories.Base;

namespace Scripts.Runtime.Data.Repositories
{
  /// <summary>
  /// キャラクターデータのリポジトリクラス
  /// キャラクター情報のCRUD操作を管理
  /// </summary>
  public class CharacterFormationRepository : BaseRepository<CharacterFormationData>
  {
    // APIエンドポイント
    protected override string EndpointUrl => "/api/character-formations";

    // キャッシュキーのプレフィックス
    protected override string CacheKeyPrefix => "character-formation";

    // シングルトンインスタンス
    private static readonly CharacterFormationRepository _instance;
    public static CharacterFormationRepository Instance
    {
      get
      {
        return _instance ?? new CharacterFormationRepository();
      }
    }

    /// <summary>
    /// プライベートコンストラクタ（シングルトン）
    /// </summary>
    private CharacterFormationRepository() : base()
    {
    }

    /// <summary>
    /// すべてのキャラクターの編成を取得
    /// </summary>
    public async Task<List<CharacterFormationData>> GetAllByFormationIdAsync(int formationId)
    {
      // formationIdに合致するデータを全て取得
      var formations = await GetAllAsync();
      return formations.Where(f => f.id == formationId).ToList();
    }

    protected override async Task<List<CharacterFormationData>> GetAllMockDataAsync()
    {
      return GetMockCharacterFormations();
    }

    protected override async Task<CharacterFormationData> GetMockDataAsync()
    {
      return null;
    }

    protected override async Task<CharacterFormationData> GetMockDataByIdAsync(string id)
    {
      return null;
    }

    /// <summary>
    /// Mockキャラクターデータを生成
    /// </summary>
    private List<CharacterFormationData> GetMockCharacterFormations()
    {
      return new List<CharacterFormationData>
      {
        new CharacterFormationData
        {
          characterId = "char_001",
          characterOrder = 1,
          formationNumber = 1
        },
        new CharacterFormationData
        {
          characterId = "char_002",
          characterOrder = 2,
          formationNumber = 1
        },
        new CharacterFormationData
        {
          characterId = "char_003",
          characterOrder = 3,
          formationNumber = 1
        }
      };
    }

  }
}