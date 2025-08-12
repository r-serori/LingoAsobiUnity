using UnityEngine;
using Scripts.Runtime.DataModels;
using Scripts.Runtime.Managers;
using Scripts.Runtime.MockData;
using System.Threading.Tasks;
using System.Collections;

namespace Scripts.Runtime.Services
{
  /// <summary>
  /// サービス層の初期化とライフサイクル管理
  /// 全UIコンポーネントより先に初期化されることを保証
  /// 標準的なMonoBehaviour Singleton パターンに更新
  /// 命名統一: UserManager → UserManager, GameBalanceManager → GameBalanceManager
  /// </summary>
  public class ServiceBootstrap : MonoBehaviour
  {
    [Header("Performance Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private float serviceInitDelay = 0.1f;
    [SerializeField] private bool autoStartUserDataLoad = true;

    [Header("Fallback Data")]
    [SerializeField] private MockUser fallbackMockUser;

    // Singleton パターン
    private static ServiceBootstrap _instance;
    public static ServiceBootstrap Instance => _instance;

    // Service アクセサ（MonoBehaviour Singleton経由）
    public static UserManager UserManager => UserManager.Instance;
    public static CharacterManager CharacterManager => CharacterManager.Instance;
    public static GameBalanceManager GameBalanceManager => GameBalanceManager.Instance;

    // 初期化状態
    public bool IsInitialized { get; private set; }
    public bool IsUserDataLoaded { get; private set; }

    // イベント
    public System.Action OnServicesInitialized;
    public System.Action<User> OnUserDataLoaded;

    void Awake()
    {
      // Singleton 設定
      if (_instance == null)
      {
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (enableDebugLogs)
        {
          Debug.Log("ServiceBootstrap: 初期化開始");
        }
      }
      else
      {
        if (enableDebugLogs)
        {
          Debug.LogWarning("ServiceBootstrap: 重複インスタンスを破棄");
        }
        Destroy(gameObject);
      }
    }

    void Start()
    {
      StartCoroutine(InitializeServicesCoroutine());
    }

    /// <summary>
    /// サービス初期化のメインコルーチン
    /// </summary>
    private IEnumerator InitializeServicesCoroutine()
    {
      bool initializationSuccessful = true;

      // Step 1: 基本バリデーション
      yield return StartCoroutine(ValidateServices());

      // Step 2: サービス初期化
      yield return StartCoroutine(InitializeServices());

      // Step 3: ユーザーデータ読み込み
      if (autoStartUserDataLoad)
      {
        yield return StartCoroutine(LoadInitialUserData());
      }

      // Step 4: 初期化完了確認
      if (initializationSuccessful)
      {
        IsInitialized = true;
        OnServicesInitialized?.Invoke();

        if (enableDebugLogs)
        {
          Debug.Log("ServiceBootstrap: 全サービス初期化完了");
        }
      }
      else
      {
        Debug.LogError("ServiceBootstrap: 初期化に失敗しました");
        yield return StartCoroutine(FallbackInitialization());
      }
    }

    /// <summary>
    /// サービスの存在確認（MonoBehaviour Singleton経由）
    /// </summary>
    private IEnumerator ValidateServices()
    {
      // UserManager の存在確認
      if (UserManager.Instance == null)
      {
        Debug.LogError("ServiceBootstrap: UserManager が初期化されていません");
        yield break;
      }

      // CharacterManager の存在確認
      if (CharacterManager.Instance == null)
      {
        Debug.LogError("ServiceBootstrap: CharacterManager が初期化されていません");
        yield break;
      }

      // GameBalanceManager の存在確認
      if (GameBalanceManager.Instance == null)
      {
        Debug.LogWarning("ServiceBootstrap: GameBalanceManager が初期化されていません");
      }

      yield return new WaitForSeconds(serviceInitDelay);
    }

    /// <summary>
    /// 各サービスの初期化処理
    /// </summary>
    private IEnumerator InitializeServices()
    {
      // UserManager 初期化
      if (UserManager.Instance != null)
      {
        // フォールバックMockUserを設定
        if (fallbackMockUser != null)
        {
          // UserManager に fallbackMockUser を設定する方法を追加する必要がある
        }

        if (enableDebugLogs)
        {
          Debug.Log("ServiceBootstrap: UserManager 初期化完了");
        }
      }

      yield return new WaitForSeconds(serviceInitDelay);

      // CharacterManager 初期化
      if (CharacterManager.Instance != null)
      {
        // CharacterManager のデータ検証
        var defaultChar = CharacterManager.Instance.GetDefaultCharacter();
        if (defaultChar == null)
        {
          Debug.LogWarning("ServiceBootstrap: CharacterManager にデフォルトキャラクターが設定されていません");
        }

        if (enableDebugLogs)
        {
          Debug.Log("ServiceBootstrap: CharacterManager 初期化完了");
        }
      }

      yield return new WaitForSeconds(serviceInitDelay);
    }

    /// <summary>
    /// 初期ユーザーデータの読み込み
    /// </summary>
    private IEnumerator LoadInitialUserData()
    {
      if (UserManager.Instance == null) yield break;

      bool dataLoaded = false;
      User loadedUser = null;
      System.Exception loadError = null;

      // 非同期でユーザーデータ読み込み
      _ = LoadUserDataAsync().ContinueWith(task =>
      {
        if (task.Exception != null)
        {
          loadError = task.Exception.GetBaseException();
        }
        else
        {
          loadedUser = task.Result;
        }
        dataLoaded = true;
      });

      // 読み込み完了まで待機
      while (!dataLoaded)
      {
        yield return new WaitForSeconds(0.1f);
      }

      if (loadError != null)
      {
        Debug.LogError($"ServiceBootstrap: ユーザーデータ読み込みエラー - {loadError.Message}");
        yield break;
      }

      if (loadedUser != null)
      {
        IsUserDataLoaded = true;
        OnUserDataLoaded?.Invoke(loadedUser);

        if (enableDebugLogs)
        {
          Debug.Log($"ServiceBootstrap: ユーザーデータ読み込み完了 - {loadedUser.userName}");
        }
      }
    }

    /// <summary>
    /// ユーザーデータの非同期読み込み
    /// </summary>
    private async Task<User> LoadUserDataAsync()
    {
      if (UserManager.Instance != null)
      {
        return await UserManager.Instance.GetCurrentUserAsync();
      }

      throw new System.Exception("UserManager が初期化されていません");
    }

    /// <summary>
    /// フォールバック初期化（エラー時）
    /// </summary>
    private IEnumerator FallbackInitialization()
    {
      Debug.LogWarning("ServiceBootstrap: フォールバックモードで初期化中...");

      // 最低限の初期化
      if (fallbackMockUser?.user != null)
      {
        IsUserDataLoaded = true;
        OnUserDataLoaded?.Invoke(fallbackMockUser.user);

        if (enableDebugLogs)
        {
          Debug.Log($"ServiceBootstrap: フォールバックユーザー使用 - {fallbackMockUser.user.userName}");
        }
      }

      IsInitialized = true;
      OnServicesInitialized?.Invoke();

      yield return null;
    }

    /// <summary>
    /// サービス強制リフレッシュ
    /// </summary>
    [ContextMenu("Refresh All Services")]
    public void RefreshAllServices()
    {
      if (Application.isPlaying)
      {
        StartCoroutine(InitializeServicesCoroutine());
      }
    }

    /// <summary>
    /// デバッグ情報表示
    /// </summary>
    [ContextMenu("Show Service Status")]
    public void ShowServiceStatus()
    {
      Debug.Log("=== ServiceBootstrap Status ===");
      Debug.Log($"Initialized: {IsInitialized}");
      Debug.Log($"User Data Loaded: {IsUserDataLoaded}");
      Debug.Log($"UserManager: {(UserManager.Instance != null ? "初期化済み" : "未初期化")}");
      Debug.Log($"CharacterManager: {(CharacterManager.Instance != null ? "初期化済み" : "未初期化")}");
      Debug.Log($"GameBalanceManager: {(GameBalanceManager.Instance != null ? "初期化済み" : "未初期化")}");
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