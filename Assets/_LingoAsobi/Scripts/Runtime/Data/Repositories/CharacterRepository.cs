using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// キャラクター関連データの管理リポジトリ
/// </summary>
public class CharacterRepository : BaseRepository
{
  // キャッシュキー定数
  private const string USER_CHARACTERS_KEY = "user_characters";
  private const string CHARACTER_DETAILS_KEY = "character_details";
  private const string AVAILABLE_CHARACTERS_KEY = "available_characters";

  // イベント
  public static event Action<CharacterData[]> OnUserCharactersUpdated;
  public static event Action<CharacterData> OnCharacterUpdated;
  public static event Action<int> OnCharacterLevelUp;

  public CharacterRepository(APIClient api, DataCache cache, LocalDataStorage localStorage)
      : base(api, cache, localStorage, "Character")
  {
  }

  #region Public Methods

  /// <summary>
  /// ユーザーが所持しているキャラクター一覧を取得
  /// </summary>
  public async Task<CharacterData[]> GetUserCharactersAsync()
  {
    return await GetDataAsync(
        USER_CHARACTERS_KEY,
        apiFetcher: () => api.GetAsync<CharacterData[]>("/api/characters/user"),
        localFetcher: () => localStorage.Load<CharacterData[]>(GetLocalKey(USER_CHARACTERS_KEY))
    ) ?? new CharacterData[0];
  }

  /// <summary>
  /// 特定キャラクターの詳細情報を取得
  /// </summary>
  public async Task<CharacterData> GetCharacterDetailsAsync(int characterId)
  {
    var detailsKey = $"{CHARACTER_DETAILS_KEY}_{characterId}";

    return await GetDataAsync(
        detailsKey,
        apiFetcher: () => api.GetAsync<CharacterData>($"/api/characters/{characterId}"),
        localFetcher: () => localStorage.Load<CharacterData>(GetLocalKey(detailsKey))
    );
  }

  /// <summary>
  /// 入手可能なキャラクター一覧を取得
  /// </summary>
  public async Task<CharacterData[]> GetAvailableCharactersAsync()
  {
    return await GetDataAsync(
        AVAILABLE_CHARACTERS_KEY,
        apiFetcher: () => api.GetAsync<CharacterData[]>("/api/characters/available"),
        localFetcher: () => localStorage.Load<CharacterData[]>(GetLocalKey(AVAILABLE_CHARACTERS_KEY))
    ) ?? new CharacterData[0];
  }

  /// <summary>
  /// ユーザーキャラクターデータの初回読み込み
  /// </summary>
  public async Task LoadUserCharactersAsync()
  {
    try
    {
      var characters = await GetUserCharactersAsync();
      OnUserCharactersUpdated?.Invoke(characters);
      Debug.Log($"[CharacterRepository] Loaded {characters.Length} user characters");
    }
    catch (Exception ex)
    {
      HandleError("LoadUserCharacters", ex);
    }
  }

  /// <summary>
  /// キャラクターのレベルアップ
  /// </summary>
  public async Task<bool> LevelUpCharacterAsync(int characterId)
  {
    try
    {
      // APIにレベルアップリクエスト送信
      var request = new LevelUpRequest { CharacterId = characterId };
      var response = await api.PostAsync<LevelUpResponse>("/api/characters/level-up", request);

      if (response.Success)
      {
        // ローカルキャッシュを更新
        await UpdateCharacterInCache(response.UpdatedCharacter);

        OnCharacterLevelUp?.Invoke(characterId);
        Debug.Log($"[CharacterRepository] Character {characterId} leveled up to {response.UpdatedCharacter.Level}");
        return true;
      }
      else
      {
        Debug.LogWarning($"[CharacterRepository] Level up failed: {response.ErrorMessage}");
        return false;
      }
    }
    catch (Exception ex)
    {
      HandleError($"LevelUpCharacter_{characterId}", ex);
      return false;
    }
  }

  /// <summary>
  /// キャラクター装備変更
  /// </summary>
  public async Task<bool> EquipItemAsync(int characterId, int itemId, EquipmentSlot slot)
  {
    try
    {
      var request = new EquipItemRequest
      {
        CharacterId = characterId,
        ItemId = itemId,
        Slot = slot
      };

      var response = await api.PostAsync<EquipItemResponse>("/api/characters/equip", request);

      if (response.Success)
      {
        await UpdateCharacterInCache(response.UpdatedCharacter);
        OnCharacterUpdated?.Invoke(response.UpdatedCharacter);
        return true;
      }

      return false;
    }
    catch (Exception ex)
    {
      HandleError($"EquipItem_{characterId}_{itemId}", ex);
      return false;
    }
  }

  /// <summary>
  /// キャラクター検索（ローカルキャッシュから）
  /// </summary>
  public async Task<CharacterData[]> SearchCharactersAsync(string query, CharacterRarity? rarity = null)
  {
    var allCharacters = await GetUserCharactersAsync();

    var filtered = allCharacters.Where(c =>
    {
      var matchesName = string.IsNullOrEmpty(query) ||
                          c.Name.ToLower().Contains(query.ToLower());
      var matchesRarity = rarity == null || c.Rarity == rarity;

      return matchesName && matchesRarity;
    });

    return filtered.ToArray();
  }

  #endregion

  #region Private Methods

  /// <summary>
  /// キャッシュ内のキャラクター情報を更新
  /// </summary>
  private async Task UpdateCharacterInCache(CharacterData updatedCharacter)
  {
    // ユーザーキャラクター一覧のキャッシュを更新
    var userCharacters = GetCache<CharacterData[]>(USER_CHARACTERS_KEY);
    if (userCharacters != null)
    {
      var index = Array.FindIndex(userCharacters, c => c.Id == updatedCharacter.Id);
      if (index >= 0)
      {
        userCharacters[index] = updatedCharacter;
        SetCache(USER_CHARACTERS_KEY, userCharacters, TimeSpan.FromMinutes(30));
      }
    }

    // 個別キャラクター詳細のキャッシュを更新
    var detailsKey = $"{CHARACTER_DETAILS_KEY}_{updatedCharacter.Id}";
    SetCache(detailsKey, updatedCharacter, TimeSpan.FromMinutes(30));

    OnCharacterUpdated?.Invoke(updatedCharacter);
  }

  #endregion

  #region BaseRepository Implementation

  public override async Task RefreshAllAsync()
  {
    try
    {
      // 全キャッシュを無効化
      InvalidateCache(USER_CHARACTERS_KEY);
      InvalidateCache(AVAILABLE_CHARACTERS_KEY);

      // 再読み込み
      var userCharacters = await GetUserCharactersAsync();
      var availableCharacters = await GetAvailableCharactersAsync();

      OnUserCharactersUpdated?.Invoke(userCharacters);

      Debug.Log($"[CharacterRepository] Refreshed all data - User: {userCharacters.Length}, Available: {availableCharacters.Length}");
    }
    catch (Exception ex)
    {
      HandleError("RefreshAll", ex);
    }
  }

  public override void SaveToLocal()
  {
    try
    {
      // ユーザーキャラクターをローカル保存
      var userCharacters = GetCache<CharacterData[]>(USER_CHARACTERS_KEY);
      if (userCharacters != null)
      {
        localStorage.Save(GetLocalKey(USER_CHARACTERS_KEY), userCharacters);
      }

      // 入手可能キャラクターをローカル保存
      var availableCharacters = GetCache<CharacterData[]>(AVAILABLE_CHARACTERS_KEY);
      if (availableCharacters != null)
      {
        localStorage.Save(GetLocalKey(AVAILABLE_CHARACTERS_KEY), availableCharacters);
      }

      Debug.Log("[CharacterRepository] Data saved to local storage");
    }
    catch (Exception ex)
    {
      HandleError("SaveToLocal", ex);
    }
  }

  public override void LoadFromLocal()
  {
    try
    {
      // ローカルからユーザーキャラクターを読み込み
      var userCharacters = localStorage.Load<CharacterData[]>(GetLocalKey(USER_CHARACTERS_KEY));
      if (userCharacters != null)
      {
        SetCache(USER_CHARACTERS_KEY, userCharacters, TimeSpan.FromHours(1));
      }

      // ローカルから入手可能キャラクターを読み込み
      var availableCharacters = localStorage.Load<CharacterData[]>(GetLocalKey(AVAILABLE_CHARACTERS_KEY));
      if (availableCharacters != null)
      {
        SetCache(AVAILABLE_CHARACTERS_KEY, availableCharacters, TimeSpan.FromHours(1));
      }

      Debug.Log("[CharacterRepository] Data loaded from local storage");
    }
    catch (Exception ex)
    {
      HandleError("LoadFromLocal", ex);
    }
  }

  #endregion
}

#region Request/Response Models

[System.Serializable]
public class LevelUpRequest
{
  public int CharacterId;
}

[System.Serializable]
public class LevelUpResponse
{
  public bool Success;
  public string ErrorMessage;
  public CharacterData UpdatedCharacter;
}

[System.Serializable]
public class EquipItemRequest
{
  public int CharacterId;
  public int ItemId;
  public EquipmentSlot Slot;
}

[System.Serializable]
public class EquipItemResponse
{
  public bool Success;
  public string ErrorMessage;
  public CharacterData UpdatedCharacter;
}

public enum EquipmentSlot
{
  Weapon,
  Armor,
  Accessory
}

#endregion