using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Scripts.Runtime.Core;
using Scripts.Runtime.Utilities.Helpers;
using Scripts.Runtime.Utilities.Constants;

namespace Scripts.Runtime.Views.Base
{
  /// <summary>
  /// すべてのViewの基底クラス
  /// View共通の機能とライフサイクルを提供
  /// </summary>
  public abstract class BaseView : MonoBehaviour
  {
    [Header("Base View Settings")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] public bool autoInitialize = true;
    [SerializeField] protected float fadeInDuration = 0.3f;
    [SerializeField] protected float fadeOutDuration = 0.3f;

    // View状態
    protected bool isInitialized = false;
    protected bool isVisible = false;
    protected bool isInteractable = true;

    // イベント
    public event Action OnViewInitialized;
    public event Action OnViewShown;
    public event Action OnViewHidden;
    public event Action OnViewDestroyed;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
      // CanvasGroupが設定されていない場合は自動取得
      if (canvasGroup == null)
      {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
          canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
      }

      // 初期状態を非表示に
      SetVisibility(false, true);
    }

    protected virtual void Start()
    {
      if (autoInitialize)
      {
        Initialize();
      }
    }

    protected virtual void OnDestroy()
    {
      OnViewDestroyed?.Invoke();
      Cleanup();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Viewの初期化
    /// </summary>
    public virtual void Initialize()
    {
      if (isInitialized)
      {
        Debug.LogWarning($"[{GetType().Name}] Already initialized");
        return;
      }

      Debug.Log($"[{GetType().Name}] Initializing...");

      // UI要素の参照を取得
      GetUIReferences();

      // イベントリスナーを設定
      SetupEventListeners();

      // 初期データを設定
      SetupInitialData();

      isInitialized = true;
      OnViewInitialized?.Invoke();

      Debug.Log($"[{GetType().Name}] Initialization complete");
    }

    /// <summary>
    /// UI要素の参照を取得（サブクラスで実装）
    /// </summary>
    protected virtual void GetUIReferences()
    {
      // Override in derived classes
    }

    /// <summary>
    /// イベントリスナーを設定（サブクラスで実装）
    /// </summary>
    protected virtual void SetupEventListeners()
    {
      // Override in derived classes
    }

    /// <summary>
    /// 初期データを設定（サブクラスで実装）
    /// </summary>
    protected virtual void SetupInitialData()
    {
      // Override in derived classes
    }

    #endregion

    #region Visibility

    /// <summary>
    /// Viewを表示
    /// </summary>
    public virtual async Task ShowAsync()
    {
      if (!isInitialized)
      {
        Initialize();
      }

      if (isVisible)
      {
        Debug.LogWarning($"[{GetType().Name}] Already visible");
        return;
      }

      Debug.Log($"[{GetType().Name}] Showing view...");

      // 表示前の処理
      await OnBeforeShow();

      // フェードイン
      gameObject.SetActive(true);
      await UIHelper.FadeInAsync(canvasGroup, fadeInDuration);

      isVisible = true;

      // 表示後の処理
      await OnAfterShow();

      OnViewShown?.Invoke();
    }

    /// <summary>
    /// Viewを非表示
    /// </summary>
    public virtual async Task HideAsync()
    {
      if (!isVisible)
      {
        Debug.LogWarning($"[{GetType().Name}] Already hidden");
        return;
      }

      Debug.Log($"[{GetType().Name}] Hiding view...");

      // 非表示前の処理
      await OnBeforeHide();

      // フェードアウト
      await UIHelper.FadeOutAsync(canvasGroup, fadeOutDuration);

      isVisible = false;

      // 非表示後の処理
      await OnAfterHide();

      OnViewHidden?.Invoke();
    }

    /// <summary>
    /// 即座に表示/非表示を切り替え
    /// </summary>
    public void SetVisibility(bool visible, bool immediate = false)
    {
      if (immediate)
      {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible && isInteractable;
        canvasGroup.blocksRaycasts = visible;
        gameObject.SetActive(visible);
        isVisible = visible;
      }
      else
      {
        if (visible)
        {
          _ = ShowAsync();
        }
        else
        {
          _ = HideAsync();
        }
      }
    }

    #endregion

    #region Interactability

    /// <summary>
    /// インタラクト可能状態を設定
    /// </summary>
    public virtual void SetInteractable(bool interactable)
    {
      isInteractable = interactable;

      if (canvasGroup != null)
      {
        canvasGroup.interactable = interactable && isVisible;
        canvasGroup.blocksRaycasts = interactable && isVisible;
      }

      // ボタンの状態も更新
      Button[] buttons = GetComponentsInChildren<Button>();
      foreach (var button in buttons)
      {
        button.interactable = interactable;
      }
    }

    #endregion

    #region View Lifecycle Hooks

    /// <summary>
    /// 表示前の処理（サブクラスでオーバーライド）
    /// </summary>
    protected virtual async Task OnBeforeShow()
    {
      await Task.CompletedTask;
    }

    /// <summary>
    /// 表示後の処理（サブクラスでオーバーライド）
    /// </summary>
    protected virtual async Task OnAfterShow()
    {
      await Task.CompletedTask;
    }

    /// <summary>
    /// 非表示前の処理（サブクラスでオーバーライド）
    /// </summary>
    protected virtual async Task OnBeforeHide()
    {
      await Task.CompletedTask;
    }

    /// <summary>
    /// 非表示後の処理（サブクラスでオーバーライド）
    /// </summary>
    protected virtual async Task OnAfterHide()
    {
      await Task.CompletedTask;
    }

    #endregion

    #region Data Binding

    /// <summary>
    /// データを更新
    /// </summary>
    public virtual void UpdateData(object data)
    {
      // Override in derived classes
    }

    /// <summary>
    /// データをリフレッシュ
    /// </summary>
    public virtual async Task RefreshAsync()
    {
      Debug.Log($"[{GetType().Name}] Refreshing data...");

      // データを再取得して表示を更新
      await LoadDataAsync();
      UpdateDisplay();
    }

    /// <summary>
    /// データを読み込む（サブクラスで実装）
    /// </summary>
    protected virtual async Task LoadDataAsync()
    {
      await Task.CompletedTask;
    }

    /// <summary>
    /// 表示を更新（サブクラスで実装）
    /// </summary>
    protected virtual void UpdateDisplay()
    {
      // Override in derived classes
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// クリーンアップ処理
    /// </summary>
    protected virtual void Cleanup()
    {
      // イベントリスナーを解除
      RemoveEventListeners();

      // リソースを解放
      ReleaseResources();
    }

    /// <summary>
    /// イベントリスナーを解除（サブクラスで実装）
    /// </summary>
    protected virtual void RemoveEventListeners()
    {
      // Override in derived classes
    }

    /// <summary>
    /// リソースを解放（サブクラスで実装）
    /// </summary>
    protected virtual void ReleaseResources()
    {
      // Override in derived classes
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// 子Viewを取得
    /// </summary>
    protected T GetChildView<T>() where T : BaseView
    {
      return GetComponentInChildren<T>();
    }

    /// <summary>
    /// 親Viewを取得
    /// </summary>
    protected T GetParentView<T>() where T : BaseView
    {
      return GetComponentInParent<T>();
    }

    /// <summary>
    /// エラーメッセージを表示
    /// </summary>
    protected virtual void ShowError(string message)
    {
      Debug.LogError($"[{GetType().Name}] {message}");
      // TODO: UIでエラーを表示
    }

    /// <summary>
    /// ローディング表示
    /// </summary>
    protected virtual void ShowLoading(bool show)
    {
      // Override in derived classes to show loading indicator
    }

    #endregion
  }
}