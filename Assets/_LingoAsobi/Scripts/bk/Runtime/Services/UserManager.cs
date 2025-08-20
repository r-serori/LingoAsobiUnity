using UnityEngine;
using Scripts.Runtime.DataModels;
using Scripts.Runtime.MockData;
using System.Threading.Tasks;

namespace Scripts.Runtime.Services
{
  /// <summary>
  /// ユーザーデータ管理サービス（MonoBehaviour Singleton パターン）
  /// 全シーンで利用可能で、シーン遷移で破棄されない
  /// 命名統一: UserManager → UserManager
  /// </summary>
  public class UserManager : MonoBehaviour
  {
    #region Singleton Pattern

    private static UserManager _instance;

    [System.Obsolete]
    public static UserManager Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = FindObjectOfType<UserManager>();

          if (_instance == null)
          {
            // 自動生成
            GameObject go = new GameObject("UserManager");
            _instance = go.AddComponent<UserManager>();
            DontDestroyOnLoad(go);
          }
        }
        return _instance;
      }
    }

    #endregion

    [Header("Service Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private MockUser fallbackMockUser;

    private User currentUser;

    // イベント通知システム
    public System.Action<User> OnUserDataUpdated;

    void Awake()
    {
      // Singleton 重複チェック
      if (_instance != null && _instance != this)
      {
        Debug.LogWarning("UserManager の重複インスタンスを破棄します。");
        Destroy(gameObject);
        return;
      }

      _instance = this;
      DontDestroyOnLoad(gameObject);

    }

    /// <summary>
    /// 現在のユーザーデータを取得
    /// </summary>
    public async Task<User> GetCurrentUserAsync()
    {
      if (currentUser != null)
        return currentUser;

      // データ取得処理
      await LoadUserData();
      return currentUser;
    }

    /// <summary>
    /// ユーザーデータを読み込み
    /// </summary>
    private async Task LoadUserData()
    {
      try
      {
        // 本番環境: サーバーAPIから取得
        // currentUser = await ApiClient.GetUserDataAsync();

        // 開発環境: MockUserから取得
        var mockUser = fallbackMockUser ?? FindObjectOfType<MockUser>();
        if (mockUser?.user != null)
        {
          currentUser = mockUser.user;

          if (enableDebugLogs)
          {
          }

          // 初期データ読み込み完了イベント
          OnUserDataUpdated?.Invoke(currentUser);
          return;
        }

        throw new System.Exception("MockUser データが見つかりません");
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"UserManager: ユーザーデータ読み込みエラー - {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// ユーザーデータを強制リフレッシュ
    /// </summary>
    public async Task RefreshUserDataAsync()
    {
      currentUser = null;
      await LoadUserData();
    }

    /// <summary>
    /// お気に入りキャラクターを更新
    /// </summary>
    public async Task UpdateFavoriteCharacterAsync(string characterId)
    {
      if (currentUser != null)
      {
        currentUser.favoriteCharacterId = characterId;

        // 本番環境: サーバーに保存
        // await ApiClient.UpdateUserDataAsync(currentUser);

        // UI更新イベント発火
        OnUserDataUpdated?.Invoke(currentUser);
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