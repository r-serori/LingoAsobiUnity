using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Core;
using Scripts.Runtime.Utilities.Helpers;
using Scripts.Runtime.Utilities.Constants;

namespace Scripts.Runtime.Views.Base
{
  /// <summary>
  /// すべてのSceneの基底クラス
  /// シーン共通の機能とライフサイクルを提供
  /// </summary>
  public abstract class BaseScene : MonoBehaviour
  {
    [Header("Scene Settings")]
    [SerializeField] protected string sceneName;
    [SerializeField] protected bool requiresAuthentication = true;
    [SerializeField] protected bool autoInitialize = true;

    [Header("UI References")]
    [SerializeField] protected Canvas mainCanvas;
    [SerializeField] protected Camera uiCamera;

    // Scene状態
    protected bool isInitialized = false;
    protected bool isActive = false;

    // View管理
    protected Dictionary<string, BaseView> views = new Dictionary<string, BaseView>();
    protected BaseView currentActiveView;

    // イベント
    public event Action OnSceneInitialized;
    public event Action OnSceneActivated;
    public event Action OnSceneDeactivated;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
      // シーン名が設定されていない場合は自動取得
      if (string.IsNullOrEmpty(sceneName))
      {
        sceneName = gameObject.scene.name;
      }

      // Canvas参照を取得
      if (mainCanvas == null)
      {
        mainCanvas = GetComponentInChildren<Canvas>();
      }

      // UICamera参照を取得
      if (uiCamera == null && mainCanvas != null)
      {
        uiCamera = mainCanvas.worldCamera;
        if (uiCamera == null)
        {
          uiCamera = Camera.main;
        }
      }
    }

    protected virtual async void Start()
    {
      if (autoInitialize)
      {
        await InitializeAsync();
      }
    }

    protected virtual void OnEnable()
    {
      SubscribeToEvents();
    }

    protected virtual void OnDisable()
    {
      UnsubscribeFromEvents();
    }

    protected virtual void OnDestroy()
    {
      Cleanup();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// シーンの非同期初期化
    /// </summary>
    public virtual async Task InitializeAsync()
    {
      if (isInitialized)
      {
        Debug.LogWarning($"[{sceneName}] Already initialized");
        return;
      }

      Debug.Log($"[{sceneName}] Initializing scene...");

      try
      {
        // 認証チェック
        if (requiresAuthentication)
        {
          bool isAuthenticated = await CheckAuthenticationAsync();
          if (!isAuthenticated)
          {
            Debug.LogWarning($"[{sceneName}] Authentication required");
            await HandleAuthenticationRequired();
            return;
          }
        }

        // データマネージャーの初期化
        await DataManager.Instance.InitializeAsync();

        // シーン固有の初期化
        await OnInitializeAsync();

        // Viewの収集と初期化
        CollectViews();
        InitializeViews();

        isInitialized = true;
        OnSceneInitialized?.Invoke();

        // シーンをアクティブ化
        await ActivateAsync();

        Debug.Log($"[{sceneName}] Scene initialization complete");
      }
      catch (Exception e)
      {
        Debug.LogError($"[{sceneName}] Initialization failed: {e.Message}");
        await HandleInitializationError(e);
      }
    }

    /// <summary>
    /// シーン固有の初期化処理（サブクラスで実装）
    /// </summary>
    protected virtual async Task OnInitializeAsync()
    {
      await Task.CompletedTask;
    }

    /// <summary>
    /// 認証チェック
    /// </summary>
    protected virtual async Task<bool> CheckAuthenticationAsync()
    {
      var user = await DataManager.Instance.GetCurrentUserAsync();
      return user != null;
    }

    /// <summary>
    /// 認証が必要な場合の処理
    /// </summary>
    protected virtual async Task HandleAuthenticationRequired()
    {
      Debug.Log($"[{sceneName}] Redirecting to title...");
      await SceneHelper.Instance.LoadSceneAsync(GameConstants.Scenes.Title);
    }

    /// <summary>
    /// 初期化エラー処理
    /// </summary>
    protected virtual async Task HandleInitializationError(Exception error)
    {
      Debug.LogError($"[{sceneName}] Fatal error: {error.Message}");
      // エラーダイアログを表示するなど
      await Task.CompletedTask;
    }

    #endregion

    #region View Management

    /// <summary>
    /// Viewを収集
    /// </summary>
    protected virtual void CollectViews()
    {
      BaseView[] foundViews = GetComponentsInChildren<BaseView>(true);

      foreach (var view in foundViews)
      {
        string viewName = view.GetType().Name;
        if (!views.ContainsKey(viewName))
        {
          views.Add(viewName, view);
          Debug.Log($"[{sceneName}] Found view: {viewName}");
        }
      }
    }

    /// <summary>
    /// Viewを初期化
    /// </summary>
    protected virtual void InitializeViews()
    {
      foreach (var view in views.Values)
      {
        if (!view.autoInitialize)
        {
          view.Initialize();
        }
      }
    }

    /// <summary>
    /// Viewを取得
    /// </summary>
    public T GetView<T>() where T : BaseView
    {
      string viewName = typeof(T).Name;

      if (views.ContainsKey(viewName))
      {
        return views[viewName] as T;
      }

      // 動的に検索
      T view = GetComponentInChildren<T>(true);
      if (view != null)
      {
        views[viewName] = view;
      }

      return view;
    }

    /// <summary>
    /// Viewを表示
    /// </summary>
    public virtual async Task ShowViewAsync<T>() where T : BaseView
    {
      T view = GetView<T>();

      if (view == null)
      {
        Debug.LogError($"[{sceneName}] View not found: {typeof(T).Name}");
        return;
      }

      // 現在のViewを非表示
      if (currentActiveView != null && currentActiveView != view)
      {
        await currentActiveView.HideAsync();
      }

      // 新しいViewを表示
      await view.ShowAsync();
      currentActiveView = view;
    }

    /// <summary>
    /// Viewを非表示
    /// </summary>
    public virtual async Task HideViewAsync<T>() where T : BaseView
    {
      T view = GetView<T>();

      if (view == null)
      {
        Debug.LogError($"[{sceneName}] View not found: {typeof(T).Name}");
        return;
      }

      await view.HideAsync();

      if (currentActiveView == view)
      {
        currentActiveView = null;
      }
    }

    #endregion

    #region Scene Lifecycle

    /// <summary>
    /// シーンをアクティブ化
    /// </summary>
    public virtual async Task ActivateAsync()
    {
      if (isActive)
      {
        Debug.LogWarning($"[{sceneName}] Already active");
        return;
      }

      Debug.Log($"[{sceneName}] Activating scene...");

      await OnBeforeActivate();

      isActive = true;

      await OnAfterActivate();

      OnSceneActivated?.Invoke();
    }

    /// <summary>
    /// シーンを非アクティブ化
    /// </summary>
    public virtual async Task DeactivateAsync()
    {
      if (!isActive)
      {
        Debug.LogWarning($"[{sceneName}] Already inactive");
        return;
      }

      Debug.Log($"[{sceneName}] Deactivating scene...");

      await OnBeforeDeactivate();

      isActive = false;

      await OnAfterDeactivate();

      OnSceneDeactivated?.Invoke();
    }

    /// <summary>
    /// アクティブ化前の処理
    /// </summary>
    protected virtual async Task OnBeforeActivate()
    {
      await Task.CompletedTask;
    }

    /// <summary>
    /// アクティブ化後の処理
    /// </summary>
    protected virtual async Task OnAfterActivate()
    {
      await Task.CompletedTask;
    }

    /// <summary>
    /// 非アクティブ化前の処理
    /// </summary>
    protected virtual async Task OnBeforeDeactivate()
    {
      await Task.CompletedTask;
    }

    /// <summary>
    /// 非アクティブ化後の処理
    /// </summary>
    protected virtual async Task OnAfterDeactivate()
    {
      await Task.CompletedTask;
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// イベントを購読
    /// </summary>
    protected virtual void SubscribeToEvents()
    {
      // Override in derived classes
    }

    /// <summary>
    /// イベント購読を解除
    /// </summary>
    protected virtual void UnsubscribeFromEvents()
    {
      // Override in derived classes
    }

    #endregion

    #region Navigation

    /// <summary>
    /// 他のシーンへ遷移
    /// </summary>
    protected virtual async Task NavigateToSceneAsync(string targetSceneName)
    {
      await DeactivateAsync();
      await SceneHelper.Instance.LoadSceneAsync(targetSceneName);
    }

    /// <summary>
    /// ホームへ戻る
    /// </summary>
    public virtual async Task NavigateToHomeAsync()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Home);
    }

    /// <summary>
    /// 戻るボタンの処理
    /// </summary>
    public virtual async Task OnBackButtonPressed()
    {
      await NavigateToHomeAsync();
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// クリーンアップ
    /// </summary>
    protected virtual void Cleanup()
    {
      // Viewのクリーンアップ
      foreach (var view in views.Values)
      {
        if (view != null)
        {
          Destroy(view.gameObject);
        }
      }

      views.Clear();
      currentActiveView = null;
    }

    #endregion
  }
}