using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scripts.Runtime.Data.Repositories;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Utilities.Helpers;

namespace Scripts.Runtime.Core
{
  /// <summary>
  /// ゲーム起動時の初期化処理を管理するBootstrapクラス
  /// 最初のシーン（SplashScreenやInitシーン）に配置して使用
  /// </summary>
  [DefaultExecutionOrder(-1000)] // 他のスクリプトより先に実行
  public class GameBootstrap : MonoBehaviour
  {
    [Header("Bootstrap Settings")]
    [SerializeField] private float splashScreenDuration = 2.0f;
    [SerializeField] private string initialSceneName = GameConstants.Scenes.Title;

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool useMockData = true;
    [SerializeField] private bool clearCacheOnStart = false;

    [Header("Performance Settings")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool enableVSync = false;

    [Header("Splash Screen Settings")]
    [SerializeField] private CanvasGroup splashScreenCanvasGroup;

    // 初期化状態
    private static bool isInitialized = false;
    private bool isInitializing = false;

    // シングルトンインスタンス
    private static GameBootstrap _instance;
    public static GameBootstrap Instance => _instance;

    #region Unity Lifecycle

    private void Awake()
    {
      // シングルトン処理
      if (_instance != null && _instance != this)
      {
        Debug.LogWarning("[GameBootstrap] Another instance already exists. Destroying this one.");
        Destroy(gameObject);
        return;
      }

      _instance = this;
      DontDestroyOnLoad(gameObject);

      // 基本設定
      SetupApplicationSettings();

      // 初期化開始
      Debug.Log("[GameBootstrap] Starting initialization");
      StartInitialization();
    }

    private async void Start()
    {
        // GameEventManagerの初期化を確実に行う
        var eventManager = GameEventManager.Instance;
        Debug.Log("[GameBootstrap] Ensuring GameEventManager is initialized");
        
        // 既存の初期化処理
        await StartInitialization();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
      if (pauseStatus)
      {
        OnApplicationGoToBackground();
      }
      else
      {
        OnApplicationComeToForeground();
      }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
      if (!hasFocus)
      {
        SaveGameState();
      }
    }

    private void OnApplicationQuit()
    {
      Debug.Log("[GameBootstrap] Application is quitting...");
      SaveGameState();
      Cleanup();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 初期化を開始
    /// </summary>
    private async void StartInitialization()
    {
      if (isInitializing || isInitialized)
      {
        Debug.Log("[GameBootstrap] Initialization already in progress or completed");
        return;
      }

      isInitializing = true;

      Debug.Log("[GameBootstrap] ========== Starting Game Initialization ==========");

      try
      {
        // スプラッシュスクリーンを表示
        await ShowSplashScreen();

        // コアシステムの初期化
        await InitializeCoreSystemsAsync();

        // データの初期化
        await InitializeDataAsync();

        // ゲーム設定の読み込み
        await LoadGameSettingsAsync();

        // 認証処理
        await AuthenticateAsync();

        isInitialized = true;
        Debug.Log("[GameBootstrap] ========== Initialization Complete ==========");

        // 初期シーンへ遷移
        await TransitionToInitialScene();
      }
      catch (Exception e)
      {
        Debug.LogError($"[GameBootstrap] Critical initialization error: {e.Message}");
        Debug.LogError(e.StackTrace);
        HandleInitializationError(e);
      }
      finally
      {
        isInitializing = false;
      }
    }

    /// <summary>
    /// アプリケーション設定
    /// </summary>
    private void SetupApplicationSettings()
    {
      Debug.Log("[GameBootstrap] Setting up application settings...");

      // フレームレート設定
      Application.targetFrameRate = targetFrameRate;
      QualitySettings.vSyncCount = enableVSync ? 1 : 0;

      // スリープ設定
      Screen.sleepTimeout = SleepTimeout.NeverSleep;

      // デバッグ設定
      if (debugMode)
      {
        Debug.unityLogger.logEnabled = true;
        Debug.developerConsoleVisible = true;
      }

      // キャッシュクリア
      if (clearCacheOnStart)
      {
        Caching.ClearCache();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[GameBootstrap] Cache and PlayerPrefs cleared");
      }
    }

    /// <summary>
    /// コアシステムの初期化
    /// </summary>
    private async Task InitializeCoreSystemsAsync()
    {
      Debug.Log("[GameBootstrap] Initializing core systems...");

      // 各マネージャーの初期化
      InitializeManagers();

      // ヘルパーの初期化
      InitializeHelpers();

      // イベントシステムの初期化
      EventBus.Instance.ClearAll();

      await Task.Delay(100); // 初期化の安定化のための待機
    }

    /// <summary>
    /// マネージャーの初期化
    /// </summary>
    private void InitializeManagers()
    {
      Debug.Log("[GameBootstrap] Initializing managers...");

      // DataManagerの初期化
      if (DataManager.Instance == null)
      {
        Debug.LogError("[GameBootstrap] DataManager.Instance is null after initialization");
      }
      else
      {
        Debug.Log("[GameBootstrap] DataManager initialized successfully");
      }

      // APIClientの初期化
      if (APIClient.Instance == null)
      {
        Debug.LogError("[GameBootstrap] APIClient.Instance is null after initialization");
      }
      else
      {
        Debug.Log("[GameBootstrap] APIClient initialized successfully");
      }

      Debug.Log("[GameBootstrap] Managers initialization complete");

      // // DataManagerの初期化
      // if (DataManager.Instance == null)
      // {
      //   GameObject dataManagerObj = new GameObject("DataManager");
      //   dataManagerObj.AddComponent<DataManager>();
      //   dataManagerObj.transform.SetParent(transform);
      // }

      // // APIClientの初期化
      // if (APIClient.Instance == null)
      // {
      //   GameObject apiClientObj = new GameObject("APIClient");
      //   apiClientObj.AddComponent<APIClient>();
      //   apiClientObj.transform.SetParent(transform);
      // }

      // Debug.Log("[GameBootstrap] Managers initialized");
    }

    /// <summary>
    /// ヘルパーの初期化
    /// </summary>
    private void InitializeHelpers()
    {
      // SceneHelperの初期化
      if (SceneHelper.Instance == null)
      {
        GameObject sceneHelperObj = new GameObject("SceneHelper");
        sceneHelperObj.AddComponent<SceneHelper>();
        sceneHelperObj.transform.SetParent(transform);
      }

      // フェード用Canvasを作成
      SceneHelper.Instance.CreateFadeCanvas();

      Debug.Log("[GameBootstrap] Helpers initialized");
    }

    /// <summary>
    /// データの初期化
    /// </summary>
    private async Task InitializeDataAsync()
    {
      Debug.Log("[GameBootstrap] Initializing data...");

      try
      {
        // DataManagerの初期化
        if (DataManager.Instance != null)
        {
          Debug.Log("[GameBootstrap] DataManager.Instance is not null");
          await DataManager.Instance.InitializeAsync();
        }
        else
        {
          Debug.LogError("[GameBootstrap] DataManager.Instance is null");
          throw new InvalidOperationException("DataManager not initialized");
        }

        // 初回起動チェック
        bool isFirstLaunch = !PlayerPrefs.HasKey(GameConstants.PlayerPrefsKeys.IsFirstLaunch);
        if (isFirstLaunch)
        {
          await HandleFirstLaunch();
          PlayerPrefs.SetInt(GameConstants.PlayerPrefsKeys.IsFirstLaunch, 0);
          PlayerPrefs.Save();
        }

        Debug.Log("[GameBootstrap] Data initialization complete");
      }
      catch (Exception e)
      {
        Debug.LogError($"[GameBootstrap] Data initialization failed: {e.Message}");
        throw;
      }
    }

    /// <summary>
    /// ゲーム設定の読み込み
    /// </summary>
    private async Task LoadGameSettingsAsync()
    {
      Debug.Log("[GameBootstrap] Loading game settings...");

      // 音量設定
      float masterVolume = PlayerPrefs.GetFloat(GameConstants.PlayerPrefsKeys.MasterVolume, 1.0f);
      float bgmVolume = PlayerPrefs.GetFloat(GameConstants.PlayerPrefsKeys.BGMVolume, 0.7f);
      float seVolume = PlayerPrefs.GetFloat(GameConstants.PlayerPrefsKeys.SEVolume, 1.0f);

      AudioListener.volume = masterVolume;
      // TODO: AudioManagerに設定を適用

      // 言語設定
      string language = PlayerPrefs.GetString(GameConstants.PlayerPrefsKeys.Language, "ja");
      // TODO: LocalizationManagerに設定を適用

      // グラフィック設定
      int graphicsQuality = PlayerPrefs.GetInt(GameConstants.PlayerPrefsKeys.GraphicsQuality, 2);
      QualitySettings.SetQualityLevel(graphicsQuality);

      await Task.CompletedTask;
      Debug.Log("[GameBootstrap] Game settings loaded");
    }

    /// <summary>
    /// 認証処理
    /// </summary>
    private async Task AuthenticateAsync()
    {
      Debug.Log("[GameBootstrap] Authenticating...");
      try
      {
        if (useMockData)
        {
          // MockDataでの自動ログイン
          var user = await UserRepository.Instance.LoginAsync("test@example.com", "password");
          if (user != null)
          {
            Debug.Log($"[GameBootstrap] Mock authentication successful: {user.userName}");
            PlayerPrefs.SetString(GameConstants.PlayerPrefsKeys.LastLoginDate, DateTime.Now.ToString());
            PlayerPrefs.Save();
          }
          else
          {
            Debug.LogWarning("[GameBootstrap] Mock authentication failed");
          }
        }
        else
        {
          // 本番環境での認証
          // TODO: 実際の認証処理を実装
          await Task.Delay(500);
        }
      }
      catch (Exception e)
      {
        Debug.LogError($"[GameBootstrap] Authentication failed: {e.Message}");
        // 認証失敗でも処理を続行
      }
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// スプラッシュスクリーンを表示
    /// </summary>
    private async Task ShowSplashScreen()
    {
      Debug.Log("[GameBootstrap] Showing splash screen...");

      if (splashScreenCanvasGroup == null)
      {
        Debug.LogWarning("Splash Screen Canvas Group is not set.");
        await Task.Delay((int)(splashScreenDuration * 1000));
        return;
      }

      // ① UIHelperでフェードインさせる
      await UIHelper.FadeInAsync(splashScreenCanvasGroup, 0.5f); // 0.5秒かけてフェードイン

      // ② ロゴの表示時間（元々の待機時間からフェードイン・アウトの時間を引く）
      float waitTime = splashScreenDuration - 1.0f; // フェードイン0.5秒 + フェードアウト0.5秒
      if (waitTime > 0)
      {
        await Task.Delay((int)(waitTime * 1000));
      }

      // ③ UIHelperでフェードアウトさせる
      await UIHelper.FadeOutAsync(splashScreenCanvasGroup, 0.5f); // 0.5秒かけてフェードアウト
    }

    /// <summary>
    /// 初期シーンへ遷移
    /// </summary>
   private async Task TransitionToInitialScene()
   {
      string targetScene = initialSceneName;
      
      if (SceneHelper.SceneExists(targetScene))
      {
          bool success = await SceneHelper.LoadSceneAsync(targetScene, false);
          if (!success)
          {
              Debug.LogError($"[GameBootstrap] Failed to load initial scene: {targetScene}");
              // フォールバックシーンへの遷移など
          }
      }
      else
      {
          Debug.LogError($"[GameBootstrap] Scene does not exist: {targetScene}");
      }
   }

    #endregion

    #region First Launch

    /// <summary>
    /// 初回起動時の処理
    /// </summary>
    private async Task HandleFirstLaunch()
    {
      Debug.Log("[GameBootstrap] Handling first launch...");

      // デフォルトデータの作成
      CreateDefaultData();

      // チュートリアルフラグの設定
      PlayerPrefs.SetInt(GameConstants.PlayerPrefsKeys.TutorialCompleted, 0);

      // デフォルト設定の保存
      SaveDefaultSettings();

      await Task.CompletedTask;
    }

    /// <summary>
    /// デフォルトデータの作成
    /// </summary>
    private void CreateDefaultData()
    {
      // TODO: 初期データの作成
      Debug.Log("[GameBootstrap] Creating default data...");
    }

    /// <summary>
    /// デフォルト設定の保存
    /// </summary>
    private void SaveDefaultSettings()
    {
      PlayerPrefs.SetFloat(GameConstants.PlayerPrefsKeys.MasterVolume, 1.0f);
      PlayerPrefs.SetFloat(GameConstants.PlayerPrefsKeys.BGMVolume, 0.7f);
      PlayerPrefs.SetFloat(GameConstants.PlayerPrefsKeys.SEVolume, 1.0f);
      PlayerPrefs.SetString(GameConstants.PlayerPrefsKeys.Language, "ja");
      PlayerPrefs.SetInt(GameConstants.PlayerPrefsKeys.GraphicsQuality, 2);
      PlayerPrefs.Save();

      Debug.Log("[GameBootstrap] Default settings saved");
    }

    #endregion

    #region Application Lifecycle

    /// <summary>
    /// アプリがバックグラウンドに移行
    /// </summary>
    private void OnApplicationGoToBackground()
    {
      Debug.Log("[GameBootstrap] Application went to background");
      SaveGameState();
    }

    /// <summary>
    /// アプリがフォアグラウンドに復帰
    /// </summary>
    private void OnApplicationComeToForeground()
    {
      Debug.Log("[GameBootstrap] Application came to foreground");

      // データの同期
      _ = DataManager.Instance.SyncDataAsync();
    }

    /// <summary>
    /// ゲーム状態を保存
    /// </summary>
    private void SaveGameState()
    {
      Debug.Log("[GameBootstrap] Saving game state...");

      // PlayerPrefsの保存
      PlayerPrefs.Save();

      // その他の永続化処理
      // TODO: セーブデータの保存
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// 初期化エラーの処理
    /// </summary>
    private void HandleInitializationError(Exception error)
    {
      Debug.LogError($"[GameBootstrap] Fatal error during initialization: {error.Message}");

      // エラーダイアログを表示
      ShowErrorDialog("初期化エラー", "ゲームの起動に失敗しました。\nアプリを再起動してください。");

      // TODO: エラーレポートの送信
    }

    /// <summary>
    /// エラーダイアログを表示
    /// </summary>
    private void ShowErrorDialog(string title, string message)
    {
      // TODO: エラーダイアログUIの実装
      Debug.LogError($"[ERROR DIALOG] {title}: {message}");
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// クリーンアップ処理
    /// </summary>
    private void Cleanup()
    {
      Debug.Log("[GameBootstrap] Cleaning up...");

      // イベントの購読解除
      EventBus.Instance.ClearAll();

      // リソースの解放
      Resources.UnloadUnusedAssets();
      System.GC.Collect();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ゲームをリスタート
    /// </summary>
    public async Task RestartGame()
    {
      Debug.Log("[GameBootstrap] Restarting game...");

      // データをクリア
      DataManager.Instance.Logout();

      // 初期シーンに戻る
      await SceneHelper.Instance.LoadSceneAsync(GameConstants.Scenes.Title);
    }

    /// <summary>
    /// デバッグモードの切り替え
    /// </summary>
    public void SetDebugMode(bool enabled)
    {
      debugMode = enabled;
      Debug.unityLogger.logEnabled = enabled;
      Debug.developerConsoleVisible = enabled;
    }

    #endregion
  }
}