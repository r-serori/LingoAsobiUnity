using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Data.Repositories;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Cache;

namespace Scripts.Runtime.Core
{
  /// <summary>
  /// 中央データ管理クラス
  /// すべてのデータリポジトリへのアクセスポイント
  /// データの同期とキャッシュ管理を統括
  /// </summary>
  public class DataManager : MonoBehaviour
  {
    // シングルトンインスタンス
    private static DataManager _instance;
    public static DataManager Instance
    {
      get
      {
        if (_instance == null)
        {
          GameObject go = new GameObject("DataManager");
          _instance = go.AddComponent<DataManager>();
          DontDestroyOnLoad(go);
        }
        return _instance;
      }
    }

    // リポジトリ
    private UserRepository _userRepository;
    private CharacterRepository _characterRepository;

    // データキャッシュ
    private DataCache _cache;

    // 初期化フラグ
    private bool _isInitialized = false;

    // イベント
    public event Action OnDataInitialized;
    public event Action<string> OnDataError;

    /// <summary>
    /// 初期化
    /// </summary>
    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(gameObject);
        return;
      }

      _instance = this;
      DontDestroyOnLoad(gameObject);

      InitializeRepositories();
    }

    /// <summary>
    /// リポジトリの初期化
    /// </summary>
    private void InitializeRepositories()
    {
      _userRepository = UserRepository.Instance;
      _characterRepository = CharacterRepository.Instance;
      _cache = DataCache.Instance;

      Debug.Log("[DataManager] Repositories initialized");
    }

    /// <summary>
    /// データの初期化と読み込み
    /// </summary>
    public async Task InitializeAsync()
    {
      if (_isInitialized)
      {
        Debug.Log("[DataManager] Already initialized");
        return;
      }

      try
      {
        Debug.Log("[DataManager] Starting data initialization...");

        // ユーザーデータの読み込み
        var userProfile = await _userRepository.GetCurrentUserAsync();
        if (userProfile == null)
        {
          Debug.LogWarning("[DataManager] No user profile found, creating default");
          // デフォルトユーザーでログイン
          userProfile = await _userRepository.LoginAsync("test@example.com", "password");
        }

        // キャラクターデータの読み込み
        var characters = await _characterRepository.GetAllAsync();
        Debug.Log($"[DataManager] Loaded {characters.Count} characters");

        // その他の初期データ読み込み
        await LoadInitialDataAsync();

        _isInitialized = true;
        OnDataInitialized?.Invoke();

        Debug.Log("[DataManager] Data initialization complete");
      }
      catch (Exception e)
      {
        Debug.LogError($"[DataManager] Initialization failed: {e.Message}");
        OnDataError?.Invoke(e.Message);
      }
    }

    /// <summary>
    /// 初期データの読み込み
    /// </summary>
    private async Task LoadInitialDataAsync()
    {
      // 一括データ取得（本番環境では専用のBulk APIを使用）
      var tasks = new List<Task>
      {
        // 今後追加するリポジトリのデータ読み込み
        // _shopRepository.GetAllAsync(),
        // _questRepository.GetAllAsync(),
        // _inventoryRepository.GetAllAsync()
      };

      await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 現在のユーザープロファイルを取得
    /// </summary>
    public async Task<UserProfile> GetCurrentUserAsync()
    {
      if (!_isInitialized)
      {
        await InitializeAsync();
      }

      return await _userRepository.GetCurrentUserAsync();
    }

    /// <summary>
    /// キャラクターデータを取得
    /// </summary>
    public async Task<CharacterData> GetCharacterAsync(string characterId)
    {
      return await _characterRepository.GetByIdAsync(characterId);
    }

    /// <summary>
    /// すべてのキャラクターを取得
    /// </summary>
    public async Task<List<CharacterData>> GetAllCharactersAsync()
    {
      return await _characterRepository.GetAllAsync();
    }

    /// <summary>
    /// アンロック済みキャラクターを取得
    /// </summary>
    public async Task<List<CharacterData>> GetUnlockedCharactersAsync()
    {
      return await _characterRepository.GetUnlockedCharactersAsync();
    }

    /// <summary>
    /// データを同期
    /// </summary>
    public async Task SyncDataAsync()
    {
      try
      {
        Debug.Log("[DataManager] Starting data sync...");

        // キャッシュをクリア
        _cache.ClearAll();

        // 最新データを取得
        await InitializeAsync();

        Debug.Log("[DataManager] Data sync complete");
      }
      catch (Exception e)
      {
        Debug.LogError($"[DataManager] Sync failed: {e.Message}");
        OnDataError?.Invoke(e.Message);
      }
    }

    /// <summary>
    /// ログアウト処理
    /// </summary>
    public void Logout()
    {
      _userRepository.Logout();
      _cache.ClearAll();
      _isInitialized = false;

      Debug.Log("[DataManager] User logged out and data cleared");
    }

    /// <summary>
    /// アプリケーション終了時の処理
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
      if (pauseStatus)
      {
        // バックグラウンドに移行時
        SaveLocalData();
      }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
      if (!hasFocus)
      {
        // フォーカスを失った時
        SaveLocalData();
      }
    }

    private void OnDestroy()
    {
      SaveLocalData();
    }

    /// <summary>
    /// ローカルデータの保存
    /// </summary>
    private void SaveLocalData()
    {
      // 重要なデータをPlayerPrefsに保存
      PlayerPrefs.Save();
      Debug.Log("[DataManager] Local data saved");
    }

    /// <summary>
    /// キャッシュ統計情報を取得
    /// </summary>
    public CacheStatistics GetCacheStatistics()
    {
      return _cache.GetStatistics();
    }

    /// <summary>
    /// デバッグ情報を出力
    /// </summary>
    [ContextMenu("Print Debug Info")]
    public void PrintDebugInfo()
    {
      Debug.Log("=== DataManager Debug Info ===");
      Debug.Log($"Initialized: {_isInitialized}");

      var stats = GetCacheStatistics();
      Debug.Log($"Cache entries: {stats.TotalEntries}");
      Debug.Log($"Cache size: {stats.TotalSize / 1024f:F2} KB");
      Debug.Log($"Oldest entry age: {stats.OldestEntryAge.TotalMinutes:F1} minutes");
    }
  }
}