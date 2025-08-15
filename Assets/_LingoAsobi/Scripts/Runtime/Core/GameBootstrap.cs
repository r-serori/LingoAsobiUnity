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

    private async Task Awake()
    {
      // シングルトン処理
      if (_instance != null && _instance != this)
      {
        Destroy(gameObject);
        return;
      }

      _instance = this;
      DontDestroyOnLoad(gameObject);

      // 基本設定
      SetupApplicationSettings();

      // 初期化開始
      await StartInitialization();
    }

    private async void Start()
    {
        // 全ての必須マネージャーを初期化
        InitializeManagers();
        
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
      SaveGameState();
      Cleanup();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 初期化を開始
    /// </summary>
    private async Task StartInitialization()
    {
      if (isInitializing || isInitialized)
      {
        return;
      }

      isInitializing = true;

      try
      {
        // スプラッシュスクリーンを表示
        await ShowSplashScreen();

        // コアシステムの初期化
        InitializeCoreSystems();

        // データの初期化
        await InitializeDataAsync();

        // ゲーム設定の読み込み
        await LoadGameSettingsAsync();

        // 認証処理
        await AuthenticateAsync();

        isInitialized = true;

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
      }
    }

    /// <summary>
    /// コアシステムの初期化
    /// </summary>
    private void InitializeCoreSystems()
    {
      // 各マネージャーの初期化
      InitializeManagers();

      // ヘルパーの初期化
      InitializeHelpers();
    }

    /// <summary>
    /// マネージャーの初期化
    /// </summary>
    private void InitializeManagers()
    {
        // コアマネージャーの初期化と確認
        var eventBus = EventBus.Instance;
        var eventRegistry = GameEventHandlerRegistry.Instance;
        var sceneHelper = SceneHelper.Instance;
        var dataManager = DataManager.Instance;
        
        // デバッグモードの場合、デバッガーを追加
      #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (GameObject.Find("EventBusDebugger") == null)
        {
          GameObject debuggerGo = new GameObject("EventBusDebugger");
          debuggerGo.AddComponent<EventBusDebugger>();
          DontDestroyOnLoad(debuggerGo);
        }
      #endif
    }

    /// <summary>
    /// ヘルパーの初期化
    /// </summary>
    private void InitializeHelpers()
    {
      // SceneHelperの初期化（Instanceアクセスで自動作成される）
      var sceneHelper = SceneHelper.Instance;
      sceneHelper.CreateFadeCanvas();
    }

    /// <summary>
    /// データの初期化
    /// </summary>
    private async Task InitializeDataAsync()
    {
      try
      {
        // DataManagerの初期化
        if (DataManager.Instance != null)
        {
          await DataManager.Instance.InitializeAsync();
        }

        // 初回ログインチェック
        bool isFirstLaunch = !PlayerPrefs.HasKey(GameConstants.PlayerPrefsKeys.IsFirstLaunch);
        if (isFirstLaunch)
        {
          await HandleFirstLaunch(); // 初回起動時の特別な処理を実行
          PlayerPrefs.SetInt(GameConstants.PlayerPrefsKeys.IsFirstLaunch, 0); // 初回起動フラグを0に設定
          PlayerPrefs.Save(); // 設定を保存
        }
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
      // 音量設定
      float masterVolume = PlayerPrefs.GetFloat(GameConstants.PlayerPrefsKeys.MasterVolume, 1.0f);
      float bgmVolume = PlayerPrefs.GetFloat(GameConstants.PlayerPrefsKeys.BGMVolume, 0.7f);
      float seVolume = PlayerPrefs.GetFloat(GameConstants.PlayerPrefsKeys.SEVolume, 1.0f);

      AudioListener.volume = masterVolume;
      // AudioManager.Instance.SetVolume(masterVolume, bgmVolume, seVolume);
      // TODO: AudioManagerに設定を適用

      // 言語設定
      string language = PlayerPrefs.GetString(GameConstants.PlayerPrefsKeys.Language, "ja");
      // TODO: LocalizationManagerに設定を適用

      // グラフィック設定
      int graphicsQuality = PlayerPrefs.GetInt(GameConstants.PlayerPrefsKeys.GraphicsQuality, 2);
      QualitySettings.SetQualityLevel(graphicsQuality);

      await Task.CompletedTask;
    }

    /// <summary>
    /// 認証処理
    /// </summary>
    private async Task AuthenticateAsync()
    {
      try
      {
        if (useMockData)
        {
          // MockDataでの自動ログイン
          var user = await UserRepository.Instance.LoginAsync("test@example.com", "password");
          if (user != null)
          {
            PlayerPrefs.SetString(GameConstants.PlayerPrefsKeys.LastLoginDate, DateTime.Now.ToString());
            PlayerPrefs.Save();
          }
          else
          {
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
      if (splashScreenCanvasGroup == null)
      {
        await Task.Delay((int)(splashScreenDuration * 1000));
        return;
      }

      // ① UIHelperでフェードインさせる
      await UIHelper.FadeInAsync(splashScreenCanvasGroup, 0.5f); // 0.5秒かけてフェードイン

      // ③ UIHelperでフェードアウトさせる
      await UIHelper.FadeOutAsync(splashScreenCanvasGroup, 0.5f); // 0.5秒かけてフェードアウト
    }

    private async Task TransitionToInitialScene()
    {
      bool success = await SceneHelper.Instance.LoadSceneAsync(initialSceneName, false);
      if (!success)
      {
          // フォールバックシーンへの遷移
          await HandleSceneLoadFailure(initialSceneName);
      }
    }

    private async Task HandleSceneLoadFailure(string failedScene)
    {
        // エラー画面への遷移など
        string errorScene = "ErrorScene";
        if (SceneHelper.Instance.SceneExists(errorScene))
        {
            await SceneHelper.Instance.LoadSceneAsync(errorScene, false);
        }
    }

    #endregion

    #region First Launch

    /// <summary>
    /// 初回起動時の処理
    /// </summary>
    private async Task HandleFirstLaunch()
    {
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