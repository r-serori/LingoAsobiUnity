using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Scripts.Runtime.DataModels;

namespace Scripts.Runtime.Managers
{
  /// <summary>
  /// キャラクターデータの管理とアクセスを提供
  /// MonoBehaviour Singleton パターンで全シーンで利用可能
  /// </summary>
  public class CharacterManager : MonoBehaviour
  {
    #region Singleton Pattern

    private static CharacterManager _instance;
    public static CharacterManager Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = FindObjectOfType<CharacterManager>();

          if (_instance == null)
          {
            // 自動生成
            GameObject go = new GameObject("CharacterManager");
            _instance = go.AddComponent<CharacterManager>();
            DontDestroyOnLoad(go);
          }
        }
        return _instance;
      }
    }

    #endregion

    [Header("キャラクターデータベース")]
    [SerializeField] private List<Character> characters = new List<Character>();

    [Header("デフォルト設定")]
    [SerializeField] private string defaultCharacterId = "char_001";

    void Awake()
    {
      // Singleton 重複チェック
      if (_instance != null && _instance != this)
      {
        Debug.LogWarning("CharacterManager の重複インスタンスを破棄します。");
        Destroy(gameObject);
        return;
      }

      _instance = this;
      DontDestroyOnLoad(gameObject);

      Debug.Log("CharacterManager Singleton 初期化完了");
    }

    /// <summary>
    /// キャラクターIDからデータを取得
    /// </summary>
    public Character GetCharacterById(string characterId)
    {
      var character = characters.FirstOrDefault(c => c.characterId == characterId);

      if (character == null)
      {
        Debug.LogWarning($"キャラクター '{characterId}' が見つかりません。デフォルトキャラクターを返します。");
        return GetDefaultCharacter();
      }

      return character;
    }

    /// <summary>
    /// デフォルトキャラクターを取得
    /// </summary>
    public Character GetDefaultCharacter()
    {
      var defaultChar = GetCharacterById(defaultCharacterId);

      if (defaultChar == null && characters.Count > 0)
      {
        Debug.LogWarning("デフォルトキャラクターが見つかりません。最初のキャラクターを返します。");
        return characters[0];
      }

      return defaultChar ?? CreateFallbackCharacter();
    }

    /// <summary>
    /// アンロック済みキャラクターのリストを取得
    /// </summary>
    public List<Character> GetUnlockedCharacters()
    {
      return characters.Where(c => c.isUnlocked).ToList();
    }

    /// <summary>
    /// レアリティ別キャラクターを取得
    /// </summary>
    public List<Character> GetCharactersByRarity(CharacterRarity rarity)
    {
      return characters.Where(c => c.rarity == rarity && c.isUnlocked).ToList();
    }

    /// <summary>
    /// キャラクターの所持状況を更新
    /// </summary>
    public void UnlockCharacter(string characterId)
    {
      var character = GetCharacterById(characterId);
      if (character != null)
      {
        character.isUnlocked = true;
        Debug.Log($"キャラクター '{character.characterName}' をアンロックしました！");
      }
    }

    /// <summary>
    /// お気に入り設定を切り替え
    /// </summary>
    public void ToggleFavorite(string characterId)
    {
      var character = GetCharacterById(characterId);
      if (character != null)
      {
        character.isFavorite = !character.isFavorite;
        Debug.Log($"キャラクター '{character.characterName}' のお気に入り: {character.isFavorite}");
      }
    }

    /// <summary>
    /// 緊急時のフォールバックキャラクター作成
    /// </summary>
    private Character CreateFallbackCharacter()
    {
      return new Character
      {
        characterId = "fallback",
        characterName = "デフォルトキャラクター",
        description = "システムデフォルト",
        portraitImagePath = "Characters/default_character",
        fullBodyImagePath = "Characters/default_character",
        iconImagePath = "Characters/default_character",
        isUnlocked = true
      };
    }

    /// <summary>
    /// デバッグ用: 全キャラクター情報を表示
    /// </summary>
    [ContextMenu("Show All Characters")]
    public void ShowAllCharacters()
    {
      Debug.Log($"=== Character Database ({characters.Count} characters) ===");
      foreach (var character in characters)
      {
        Debug.Log($"ID: {character.characterId}, Name: {character.characterName}, " +
                 $"Unlocked: {character.isUnlocked}, Favorite: {character.isFavorite}");
      }
    }

    void OnDestroy()
    {
      if (_instance == this)
      {
        _instance = null;
      }
    }
  }
}