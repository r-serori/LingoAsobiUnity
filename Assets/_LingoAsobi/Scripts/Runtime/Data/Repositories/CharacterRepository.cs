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
  public class CharacterRepository : BaseRepository<CharacterData>
  {
    // APIエンドポイント
    protected override string EndpointUrl => "/api/characters";

    // キャッシュキーのプレフィックス
    protected override string CacheKeyPrefix => "character";

    // シングルトンインスタンス
    private static readonly CharacterRepository _instance;
    public static CharacterRepository Instance
    {
      get
      {
        return _instance ?? new CharacterRepository();
      }
    }

    /// <summary>
    /// プライベートコンストラクタ（シングルトン）
    /// </summary>
    private CharacterRepository() : base()
    {
    }

    /// <summary>
    /// レアリティでキャラクターを取得
    /// </summary>
    public async Task<List<CharacterData>> GetByRarityAsync(Rarity rarity)
    {
      var allCharacters = await GetAllAsync();
      return allCharacters.Where(c => c.rarity == rarity).ToList();
    }

    /// <summary>
    /// アンロック済みのキャラクターを取得
    /// </summary>
    public async Task<List<CharacterData>> GetUnlockedCharactersAsync()
    {
      var allCharacters = await GetAllAsync();
      return allCharacters.Where(c => c.isUnlocked).ToList();
    }

    /// <summary>
    /// お気に入りのキャラクターを取得
    /// </summary>
    public async Task<CharacterData> GetFavoriteCharactersAsync()
    {
      var allCharacters = await GetAllAsync();
      return allCharacters.Where(c => c.isFavorite && c.isUnlocked).FirstOrDefault();
    }

    /// <summary>
    /// キャラクターをアンロック
    /// </summary>
    public async Task<bool> UnlockCharacterAsync(string characterId)
    {
      var character = await GetByIdAsync(characterId);
      if (character == null || character.isUnlocked)
      {
        return false;
      }

      character.isUnlocked = true;
      character.unlockedDate = DateTime.Now;

      // APIで更新（Mockでは省略）
      await UpdateAsync(character);

      return true;
    }

    /// <summary>
    /// キャラクターのお気に入り状態を切り替え
    /// </summary>
    public async Task<bool> ToggleFavoriteAsync(string characterId)
    {
      var character = await GetByIdAsync(characterId);
      if (character == null || !character.isUnlocked)
      {
        return false;
      }

      character.ToggleFavorite();
      await UpdateAsync(character);

      return true;
    }

    /// <summary>
    /// キャラクターのレベルアップ
    /// </summary>
    public async Task<bool> LevelUpCharacterAsync(string characterId, int levels = 1)
    {
      var character = await GetByIdAsync(characterId);
      if (character == null || !character.isUnlocked)
      {
        return false;
      }

      character.level += levels;

      // ステータスを成長させる
      character.stats.hp += levels * 10;
      character.stats.attack += levels * 2;
      character.stats.defense += levels * 2;
      character.stats.speed += levels * 1;

      await UpdateAsync(character);

      return true;
    }

    /// <summary>
    /// キャラクターを複数取得
    /// </summary>
    public async Task<List<CharacterData>> GetCharactersByIdsAsync(List<string> characterIds)
    {
      var characters = await GetAllAsync();
      return characters.Where(c => characterIds.Contains(c.characterId)).ToList();
    }

    #region Mock Data Implementation

    /// <summary>
    /// MockDataからキャラクターを取得
    /// </summary>
    protected override async Task<CharacterData> GetMockDataByIdAsync(string id)
    {
      await Task.Delay(100); // ネットワーク遅延をシミュレート

      var characters = GetMockCharacters();
      return characters.FirstOrDefault(c => c.characterId == id);
    }

    /// <summary>
    /// MockDataから全キャラクターを取得
    /// </summary>
    protected override async Task<List<CharacterData>> GetAllMockDataAsync()
    {
      await Task.Delay(100);
      return GetMockCharacters();
    }

    /// <summary>
    /// Mockキャラクターデータを生成
    /// </summary>
    private List<CharacterData> GetMockCharacters()
    {
      return new List<CharacterData>
            {
                new CharacterData
                {
                    characterId = "char_001",
                    characterName = "キャラ1",
                    description = "キャラ1です。文法が得意。",
                    attribute = CharacterAttribute.Fire,
                    portraitImagePath = "Characters/ex_character1",
                    fullBodyImagePath = "Characters/ex_character1",
                    avatarImagePath = "Characters/ex_character1",
                    iconImagePath = "Characters/ex_character1",
                    defaultAnimation = AnimationType.Idle,
                    animationSpeed = 1.0f,
                    enableRandomAnimation = true,
                    rarity = Rarity.Rare,
                    level = 1,
                    stats = new CharacterStats
                    {
                        hp = 100,
                        attack = 15,
                        defense = 10,
                        speed = 12
                    },
                    isUnlocked = true,
                    isFavorite = true,
                    unlockedDate = DateTime.Now.AddDays(-30)
                },
                new CharacterData
                {
                    characterId = "char_002",
                    characterName = "キャラ2",
                    description = "キャラ2です。リスニングが得意。",
                    attribute = CharacterAttribute.Water,
                    portraitImagePath = "Characters/ex_character2",
                    fullBodyImagePath = "Characters/ex_character2",
                    avatarImagePath = "Characters/ex_character2",
                    iconImagePath = "Characters/ex_character2",
                    defaultAnimation = AnimationType.Walk,
                    animationSpeed = 1.2f,
                    enableRandomAnimation = true,
                    rarity = Rarity.Epic,
                    level = 5,
                    stats = new CharacterStats
                    {
                        hp = 120,
                        attack = 20,
                        defense = 15,
                        speed = 18
                    },
                    isUnlocked = true,
                    isFavorite = false,
                    unlockedDate = DateTime.Now.AddDays(-20)
                },
                new CharacterData
                {
                    characterId = "char_003",
                    characterName = "キャラ3",
                    description = "キャラ3です。スピーキングが得意。",
                    attribute = CharacterAttribute.Wood,
                    portraitImagePath = "Characters/ex_character3",
                    fullBodyImagePath = "Characters/ex_character3",
                    avatarImagePath = "Characters/ex_character3",
                    iconImagePath = "Characters/ex_character3",
                    defaultAnimation = AnimationType.Jump,
                    animationSpeed = 0.8f,
                    enableRandomAnimation = false,
                    rarity = Rarity.Legendary,
                    level = 10,
                    stats = new CharacterStats
                    {
                        hp = 200,
                        attack = 30,
                        defense = 25,
                        speed = 22
                    },
                    isUnlocked = false,
                    isFavorite = false,
                    unlockedDate = DateTime.MinValue
                },
                new CharacterData
                {
                    characterId = "char_004",
                    characterName = "キャラ4",
                    description = "キャラ4です。総合力が高い。",
                    attribute = CharacterAttribute.Light,
                    portraitImagePath = "Characters/ex_character4",
                    fullBodyImagePath = "Characters/ex_character4",
                    avatarImagePath = "Characters/ex_character4",
                    iconImagePath = "Characters/ex_character4",
                    defaultAnimation = AnimationType.Victory,
                    animationSpeed = 1.5f,
                    enableRandomAnimation = true,
                    rarity = Rarity.Legendary,
                    level = 1,
                    stats = new CharacterStats
                    {
                        hp = 80,
                        attack = 8,
                        defense = 8,
                        speed = 8
                    },
                    isUnlocked = true,
                    isFavorite = false,
                    unlockedDate = DateTime.Now.AddDays(-5)
                }
            };
    }

    protected override async Task<CharacterData> GetMockDataAsync()
    {
      return null;
    }

    #endregion
  }
}