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
  /// すべてのSceneの基底クラス（改善版）
  /// シーン内のどこにあってもViewを検索可能
  /// </summary>
  public abstract class BaseScene : MonoBehaviour
  {
    [Header("Scene Settings")]
    [SerializeField] protected string sceneName;
    [SerializeField] protected bool requiresAuthentication = true;
    [SerializeField] protected bool autoInitialize = true;

    [Header("Search Settings")]
    [SerializeField] protected ViewSearchMode searchMode = ViewSearchMode.SceneWide;
    [SerializeField] protected bool cacheViews = true;

    [Header("UI References (Optional)")]
    [SerializeField] protected Canvas mainCanvas;
    [SerializeField] protected Camera uiCamera;

    // View検索モード
    public enum ViewSearchMode
    {
      ChildrenOnly,      // 子要素のみ検索（従来の動作）
      SceneWide,         // シーン全体から検索（推奨）
      SpecificCanvas,    // 特定のCanvas配下から検索
      Manual            // 手動設定のみ
    }

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

      // メインCanvasの自動検索（オプション）
      if (mainCanvas == null && searchMode == ViewSearchMode.SpecificCanvas)
      {
        mainCanvas = FindMainCanvas();
      }

      // UICameraの自動検索
      if (uiCamera == null)
      {
        uiCamera = Camera.main;
      }
    }

    protected virtual async void Start()
    {
      if (autoInitialize && !isInitialized)
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
        return;
      }

      try
      {
        // 認証チェック
        if (requiresAuthentication)
        {
          bool isAuthenticated = await CheckAuthenticationAsync();
          if (!isAuthenticated)
          {
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
        await InitializeViewsAsync();

        isInitialized = true;
        OnSceneInitialized?.Invoke();

        // シーンをアクティブ化
        await ActivateAsync();
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
      await SceneHelper.Instance.LoadSceneAsync(GameConstants.Scenes.Title);
    }

    /// <summary>
    /// 初期化エラー処理
    /// </summary>
    protected virtual async Task HandleInitializationError(Exception error)
    {
      Debug.LogError($"[{sceneName}] Fatal error: {error.Message}");
      await Task.CompletedTask;
    }

    #endregion

    #region View Management - 改善版

    /// <summary>
    /// メインCanvasを検索
    /// </summary>
    private Canvas FindMainCanvas()
    {
      // "Canvas"という名前のオブジェクトを優先
      GameObject canvasObj = GameObject.Find("Canvas");
      if (canvasObj != null)
      {
        return canvasObj.GetComponent<Canvas>();
      }

      // なければ最初に見つかったCanvasを使用
      return FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
    }

    /// <summary>
    /// Viewを収集（改善版 - 検索モードに応じて動作を変更）
    /// </summary>
    protected virtual void CollectViews()
    {
      BaseView[] foundViews = null;

      switch (searchMode)
      {
        case ViewSearchMode.ChildrenOnly:
          // 従来の動作：子要素のみ検索
          foundViews = GetComponentsInChildren<BaseView>(true);
          break;

        case ViewSearchMode.SceneWide:
          // シーン全体から検索（推奨）
          foundViews = FindObjectsByType<BaseView>(FindObjectsSortMode.None);
          break;

        case ViewSearchMode.SpecificCanvas:
          // 特定のCanvas配下から検索
          if (mainCanvas != null)
          {
            foundViews = mainCanvas.GetComponentsInChildren<BaseView>(true);
          }
          else
          {
            Debug.LogWarning($"[{sceneName}] Canvas not set, falling back to scene-wide search");
            foundViews = FindObjectsByType<BaseView>(FindObjectsSortMode.None);
          }
          break;

        case ViewSearchMode.Manual:
          // 手動設定のみ使用
          return;
      }

      if (foundViews != null)
      {
        foreach (var view in foundViews)
        {
          RegisterView(view);
        }
      }
    }

    /// <summary>
    /// Viewを登録
    /// </summary>
    protected void RegisterView(BaseView view)
    {
      if (view == null) return;

      string viewName = view.GetType().Name;

      if (!views.ContainsKey(viewName))
      {
        views.Add(viewName, view);
      }
    }

    /// <summary>
    /// Viewを手動で登録（Manual mode用）
    /// </summary>
    public void RegisterViewManually(BaseView view)
    {
      if (view == null)
      {
        Debug.LogError($"[{sceneName}] Cannot register null view");
        return;
      }

      RegisterView(view);
    }

    /// <summary>
    /// Viewを初期化
    /// </summary>
    protected virtual async Task InitializeViewsAsync()
    {
      foreach (var view in views.Values)
      {
        view.Initialize();
      }

      await Task.CompletedTask;
    }

    /// <summary>
    /// Viewを取得（改善版）
    /// </summary>
    public T GetView<T>() where T : BaseView
    {
      string viewName = typeof(T).Name;

      // キャッシュから取得
      if (views.ContainsKey(viewName))
      {
        return views[viewName] as T;
      }

      // キャッシュになければ動的に検索
      if (!cacheViews || searchMode != ViewSearchMode.Manual)
      {
        T view = FindView<T>();

        if (view != null && cacheViews)
        {
          RegisterView(view);
        }

        return view;
      }

      return null;
    }

    /// <summary>
    /// Viewを動的に検索
    /// </summary>
    private T FindView<T>() where T : BaseView
    {

      switch (searchMode)
      {
        case ViewSearchMode.ChildrenOnly:
          return GetComponentInChildren<T>(true);

        case ViewSearchMode.SceneWide:
          return FindFirstObjectByType<T>(FindObjectsInactive.Include);

        case ViewSearchMode.SpecificCanvas:
          if (mainCanvas != null)
          {
            return mainCanvas.GetComponentInChildren<T>(true);
          }
          return FindFirstObjectByType<T>(FindObjectsInactive.Include);

        default:
          return null;
      }
    }

    /// <summary>
    /// すべての登録済みViewを取得
    /// </summary>
    public IEnumerable<BaseView> GetAllViews()
    {
      return views.Values;
    }

    /// <summary>
    /// 特定の型のすべてのViewを取得
    /// </summary>
    public T[] GetAllViewsOfType<T>() where T : BaseView
    {
      List<T> result = new List<T>();

      foreach (var view in views.Values)
      {
        if (view is T typedView)
        {
          result.Add(typedView);
        }
      }

      return result.ToArray();
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
        return;
      }

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


      await OnBeforeDeactivate();
      isActive = false;
      await OnAfterDeactivate();

      OnSceneDeactivated?.Invoke();
    }

    protected virtual async Task OnBeforeActivate() => await Task.CompletedTask;
    protected virtual async Task OnAfterActivate() => await Task.CompletedTask;
    protected virtual async Task OnBeforeDeactivate() => await Task.CompletedTask;
    protected virtual async Task OnAfterDeactivate() => await Task.CompletedTask;

    #endregion

    #region Event Handling

    protected virtual void SubscribeToEvents() { }
    protected virtual void UnsubscribeFromEvents() { }

    #endregion

    #region Navigation

    protected virtual async Task NavigateToSceneAsync(string targetSceneName)
    {
      await DeactivateAsync();
      await SceneHelper.Instance.LoadSceneAsync(targetSceneName);
    }

    public virtual async Task NavigateToHomeAsync()
    {
      await NavigateToSceneAsync(GameConstants.Scenes.Home);
    }

    public virtual async Task OnBackButtonPressed()
    {
      await NavigateToHomeAsync();
    }

    #endregion

    #region Cleanup

    protected virtual void Cleanup()
    {
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