using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 中央データ管理クラス - 全てのデータアクセスのハブ
/// </summary>
public class DataManager : MonoBehaviour
{
  #region Singleton
  public static DataManager Instance { get; private set; }

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
      InitializeRepositories();
    }
    else
    {
      Destroy(gameObject);
    }
  }
  #endregion

  #region Repositories
  public UserRepository Users { get; private set; }
  public CharacterRepository Characters { get; private set; }
  public ShopRepository Shop { get; private set; }
  public QuestRepository Quests { get; private set; }
  public InventoryRepository Inventory { get; private set; }
  #endregion

  #region Core Services
  public APIClient API { get; private set; }
  public DataCache Cache { get; private set; }
  public LocalDataStorage LocalStorage { get; private set; }
  #endregion

  #region Properties
  public bool IsInitialized { get; private set; }
  public bool IsOnline => Application.internetReachability != NetworkReachability.NotReachable;
  #endregion

  #region Events
  public static event Action OnDataInitialized;
  public static event Action<string> OnDataUpdated;
  public static event Action<string> OnDataLoadFailed;
  #endregion

  private void InitializeRepositories()
  {
    // コアサービス初期化
    API = GetComponent<APIClient>() ?? gameObject.AddComponent<APIClient>();
    Cache = new DataCache();
    LocalStorage = new LocalDataStorage();

    // リポジトリ初期化（依存性注入）
    Users = new UserRepository(API, Cache, LocalStorage);
    Characters = new CharacterRepository(API, Cache, LocalStorage);
    Shop = new ShopRepository(API, Cache, LocalStorage);
    Quests = new QuestRepository(API, Cache, LocalStorage);
    Inventory = new InventoryRepository(API, Cache, LocalStorage);

    Debug.Log("DataManager repositories initialized");
  }

  /// <summary>
  /// 初回データロード
  /// </summary>
  public async Task InitializeAsync()
  {
    try
    {
      Debug.Log("Starting data initialization...");

      // Phase 1: 必須データの読み込み
      await LoadCriticalDataAsync();

      // Phase 2: よく使うデータの読み込み
      await LoadFrequentDataAsync();

      IsInitialized = true;
      OnDataInitialized?.Invoke();

      Debug.Log("Data initialization completed");

      // Phase 3: バックグラウンドで残りのデータ読み込み
      _ = LoadRemainingDataAsync();
    }
    catch (Exception ex)
    {
      Debug.LogError($"Data initialization failed: {ex.Message}");
      OnDataLoadFailed?.Invoke("initialization");
      throw;
    }
  }

  private async Task LoadCriticalDataAsync()
  {
    // 最重要データ - アプリ起動に必須
    var tasks = new Task[]
    {
            Users.LoadUserProfileAsync(),
            Users.LoadCurrencyAsync(),
            Users.LoadStaminaAsync()
    };

    await Task.WhenAll(tasks);
    Debug.Log("Critical data loaded");
  }

  private async Task LoadFrequentDataAsync()
  {
    // よく使用されるデータ - ホーム画面表示に必要
    var tasks = new Task[]
    {
            Characters.LoadUserCharactersAsync(),
            Inventory.LoadInventoryAsync(),
            Quests.LoadActiveQuestsAsync()
    };

    await Task.WhenAll(tasks);
    Debug.Log("Frequent data loaded");
  }

  private async Task LoadRemainingDataAsync()
  {
    // その他のデータ - バックグラウンドで読み込み
    try
    {
      await Shop.LoadShopDataAsync();
      await Quests.LoadAllQuestsAsync();
      Debug.Log("Remaining data loaded");
    }
    catch (Exception ex)
    {
      Debug.LogWarning($"Background data loading failed: {ex.Message}");
    }
  }

  /// <summary>
  /// 定期的なデータ同期
  /// </summary>
  public async Task SyncDataAsync()
  {
    if (!IsOnline) return;

    try
    {
      // 変更されやすいデータのみ同期
      var syncTasks = new Task[]
      {
                Users.SyncStaminaAsync(),
                Shop.SyncShopDataAsync(),
                Quests.SyncQuestProgressAsync()
      };

      await Task.WhenAll(syncTasks);
      OnDataUpdated?.Invoke("sync");
      Debug.Log("Data sync completed");
    }
    catch (Exception ex)
    {
      Debug.LogError($"Data sync failed: {ex.Message}");
    }
  }

  /// <summary>
  /// 特定データの強制リフレッシュ
  /// </summary>
  public async Task RefreshDataAsync(string dataType)
  {
    try
    {
      switch (dataType.ToLower())
      {
        case "user":
          await Users.RefreshAllAsync();
          break;
        case "characters":
          await Characters.RefreshAllAsync();
          break;
        case "shop":
          await Shop.RefreshAllAsync();
          break;
        case "quests":
          await Quests.RefreshAllAsync();
          break;
        case "inventory":
          await Inventory.RefreshAllAsync();
          break;
        default:
          Debug.LogWarning($"Unknown data type for refresh: {dataType}");
          return;
      }

      OnDataUpdated?.Invoke(dataType);
      Debug.Log($"{dataType} data refreshed");
    }
    catch (Exception ex)
    {
      Debug.LogError($"Failed to refresh {dataType}: {ex.Message}");
      OnDataLoadFailed?.Invoke(dataType);
    }
  }

  /// <summary>
  /// アプリ終了時のデータ保存
  /// </summary>
  private void OnApplicationPause(bool pauseStatus)
  {
    if (pauseStatus && IsInitialized)
    {
      SaveAllDataToLocal();
    }
  }

  private void OnApplicationFocus(bool hasFocus)
  {
    if (!hasFocus && IsInitialized)
    {
      SaveAllDataToLocal();
    }
  }

  private void SaveAllDataToLocal()
  {
    try
    {
      Users.SaveToLocal();
      Characters.SaveToLocal();
      Shop.SaveToLocal();
      Quests.SaveToLocal();
      Inventory.SaveToLocal();

      Debug.Log("All data saved to local storage");
    }
    catch (Exception ex)
    {
      Debug.LogError($"Failed to save data to local storage: {ex.Message}");
    }
  }
}