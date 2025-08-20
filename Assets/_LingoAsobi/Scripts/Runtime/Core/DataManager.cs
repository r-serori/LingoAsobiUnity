using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Data.Repositories;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Cache;
using Scripts.Runtime.Data.Models.Training;
using Scripts.Runtime.Data.Models.Grammar;

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
    private TrainingRepository _trainingRepository;
    private GrammarRepository _grammarRepository;
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
      try
      {
        _userRepository = UserRepository.Instance;

        _characterRepository = CharacterRepository.Instance;

        _trainingRepository = TrainingRepository.Instance;

        _grammarRepository = GrammarRepository.Instance;

        _cache = DataCache.Instance;
      }
      catch (Exception e)
      {
        Debug.LogError($"[DataManager] Repository initialization failed: {e.Message}");
        Debug.LogError(e.StackTrace);
      }
    }

    /// <summary>
    /// データの初期化と読み込み
    /// </summary>
    public async Task InitializeAsync()
    {
      if (_isInitialized)
      {
        return;
      }

      try
      {
        // リポジトリの確認
        if (_userRepository == null)
        {
          InitializeRepositories();
        }

        // ユーザーデータの読み込み（直接リポジトリから取得）
        var userProfile = await _userRepository.GetCurrentUserAsync();
        if (userProfile == null)
        {
          try
          {
            // デフォルトユーザーでログイン
            userProfile = await _userRepository.LoginAsync("test@example.com", "password");
          }
          catch (Exception e)
          {
            Debug.LogError($"[DataManager] Default login error: {e.Message}");
            Debug.LogError($"[DataManager] Stack trace: {e.StackTrace}");
            // ログイン失敗でも処理を続行
          }
        }

        // キャラクターデータの読み込み
        try
        {
          var characters = await _characterRepository.GetAllAsync();
        }
        catch (Exception e)
        {
          Debug.LogError($"[DataManager] Character loading error: {e.Message}");
          Debug.LogError($"[DataManager] Stack trace: {e.StackTrace}");
          // キャラクター読み込み失敗でも処理を続行
        }

        // その他の初期データ読み込み
        await LoadInitialDataAsync(); // BULK APIをInvoke

        _isInitialized = true;
        OnDataInitialized?.Invoke();

      }
      catch (Exception e)
      {
        Debug.LogError($"[DataManager] Initialization failed: {e.Message}");
        Debug.LogError($"[DataManager] Stack trace: {e.StackTrace}");
        OnDataError?.Invoke(e.Message);
        throw;
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
        _trainingRepository.GetTrainingDataAsync(),
        _grammarRepository.GetAllAsync(),
        // 今後追加するリポジトリのデータ読み込み
        // _shopRepository.GetAllAsync(),
        // _questRepository.GetAllAsync(),
      };

      await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 現在のユーザープロファイルを取得
    /// </summary>
    public async Task<UserProfile> GetCurrentUserAsync()
    {
      // 初期化が完了していない場合は、直接リポジトリから取得を試行
      if (!_isInitialized)
      {
        Debug.LogWarning("[DataManager] GetCurrentUserAsync called before initialization, attempting direct access");

        // リポジトリが初期化されているかチェック
        if (_userRepository == null)
        {
          Debug.LogError("[DataManager] UserRepository is null, cannot get user profile");
          return null;
        }

        try
        {
          // 直接リポジトリから取得
          return await _userRepository.GetCurrentUserAsync();
        }
        catch (Exception e)
        {
          Debug.LogError($"[DataManager] Failed to get user profile: {e.Message}");
          return null;
        }
      }

      // 初期化完了後は通常の処理
      return await _userRepository.GetCurrentUserAsync();
    }

    /// <summary>
    /// キャラクターデータを取得
    /// </summary>
    public async Task<CharacterData> GetCharacterByIdAsync(string characterId)
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
    /// トレーニングデータを取得
    /// </summary>
    public async Task<TrainingData> GetTrainingDataAsync()
    {
      return await _trainingRepository.GetTrainingDataAsync();
    }

    /// <summary>
    /// 文法データを取得
    /// </summary>
    public async Task<List<GrammarData>> GetGrammarAllDataAsync()
    {
      return await _grammarRepository.GetAllAsync();
    }

    /// <summary>
    /// データを同期
    /// </summary>
    public async Task SyncDataAsync()
    {
      try
      {

        // キャッシュをクリア
        _cache.ClearAll();

        // 最新データを取得
        await InitializeAsync();

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

      var stats = GetCacheStatistics();
    }
  }
}