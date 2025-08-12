using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// API通信の中央管理クラス
/// </summary>
public class APIClient : MonoBehaviour
{
  [Header("API Configuration")]
  [SerializeField] private string baseURL = "https://api.yourgame.com";
  [SerializeField] private int timeoutSeconds = 30;
  [SerializeField] private int maxRetryAttempts = 3;

  // 認証トークン
  private string authToken;
  private DateTime tokenExpiry;

  // リクエスト制限
  private Dictionary<string, DateTime> lastRequestTimes = new();
  private float rateLimitInterval = 1f; // 1秒に1回まで

  #region Public API Methods

  /// <summary>
  /// GETリクエスト
  /// </summary>
  public async Task<T> GetAsync<T>(string endpoint) where T : class
  {
    return await ExecuteRequestAsync<T>(endpoint, "GET", null);
  }

  /// <summary>
  /// POSTリクエスト
  /// </summary>
  public async Task<T> PostAsync<T>(string endpoint, object data) where T : class
  {
    return await ExecuteRequestAsync<T>(endpoint, "POST", data);
  }

  /// <summary>
  /// PUTリクエスト
  /// </summary>
  public async Task<T> PutAsync<T>(string endpoint, object data) where T : class
  {
    return await ExecuteRequestAsync<T>(endpoint, "PUT", data);
  }

  /// <summary>
  /// DELETEリクエスト
  /// </summary>
  public async Task<T> DeleteAsync<T>(string endpoint) where T : class
  {
    return await ExecuteRequestAsync<T>(endpoint, "DELETE", null);
  }

  /// <summary>
  /// 一括データ取得API
  /// </summary>
  public async Task<BulkDataResponse> GetBulkDataAsync(string[] dataTypes, long lastSyncTimestamp = 0)
  {
    var request = new BulkDataRequest
    {
      RequestedDataTypes = dataTypes,
      LastSyncTimestamp = lastSyncTimestamp
    };

    return await PostAsync<BulkDataResponse>("/api/game/bulk-data", request);
  }

  /// <summary>
  /// データ同期API
  /// </summary>
  public async Task<SyncResponse> SyncDataAsync(string[] watchedDataTypes, long lastSyncTimestamp)
  {
    var request = new SyncRequest
    {
      WatchedDataTypes = watchedDataTypes,
      LastSyncTimestamp = lastSyncTimestamp
    };

    return await PostAsync<SyncResponse>("/api/game/sync", request);
  }

  #endregion

  #region Core Request Execution

  private async Task<T> ExecuteRequestAsync<T>(string endpoint, string method, object data) where T : class
  {
    // レート制限チェック
    await ApplyRateLimit(endpoint);

    // 認証トークンの確認・更新
    await EnsureValidAuthToken();

    // リトライ機能付きでリクエスト実行
    return await ExecuteWithRetry<T>(endpoint, method, data);
  }

  private async Task<T> ExecuteWithRetry<T>(string endpoint, string method, object data) where T : class
  {
    Exception lastException = null;

    for (int attempt = 1; attempt <= maxRetryAttempts; attempt++)
    {
      try
      {
        return await SendRequestAsync<T>(endpoint, method, data);
      }
      catch (Exception ex)
      {
        lastException = ex;

        if (attempt < maxRetryAttempts)
        {
          // 指数バックオフで待機
          var delayMs = Mathf.Pow(2, attempt) * 1000;
          Debug.LogWarning($"API request failed (attempt {attempt}/{maxRetryAttempts}): {ex.Message}. Retrying in {delayMs}ms...");
          await Task.Delay((int)delayMs);
        }
      }
    }

    Debug.LogError($"API request failed after {maxRetryAttempts} attempts: {lastException?.Message}");
    throw lastException;
  }

  private async Task<T> SendRequestAsync<T>(string endpoint, string method, object data) where T : class
  {
    var url = baseURL + endpoint;

    using (var request = new UnityWebRequest(url, method))
    {
      // ヘッダー設定
      SetRequestHeaders(request);

      // ボディ設定（POST/PUTの場合）
      if (data != null && (method == "POST" || method == "PUT"))
      {
        var jsonData = JsonUtility.ToJson(data);
        var bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
      }

      // レスポンス受信設定
      request.downloadHandler = new DownloadHandlerBuffer();
      request.timeout = timeoutSeconds;

      // リクエスト送信
      var operation = request.SendWebRequest();

      // 完了待機
      while (!operation.isDone)
      {
        await Task.Yield();
      }

      // エラーチェック
      if (request.result != UnityWebRequest.Result.Success)
      {
        HandleRequestError(request);
      }

      // レスポンス解析
      var responseJson = request.downloadHandler.text;
      Debug.Log($"API Response [{method} {endpoint}]: {responseJson}");

      // JSON デシリアライズ
      if (typeof(T) == typeof(string))
      {
        return responseJson as T;
      }
      else
      {
        return JsonUtility.FromJson<T>(responseJson);
      }
    }
  }

  #endregion

  #region Authentication

  private async Task EnsureValidAuthToken()
  {
    if (string.IsNullOrEmpty(authToken) || DateTime.Now >= tokenExpiry)
    {
      await RefreshAuthToken();
    }
  }

  private async Task RefreshAuthToken()
  {
    try
    {
      // 保存された認証情報でトークンリフレッシュ
      var refreshRequest = new AuthRefreshRequest
      {
        UserId = GameData.CurrentUserId,
        DeviceId = SystemInfo.deviceUniqueIdentifier
      };

      var authResponse = await SendRequestAsync<AuthResponse>("/api/auth/refresh", "POST", refreshRequest);

      authToken = authResponse.AccessToken;
      tokenExpiry = DateTime.Now.AddSeconds(authResponse.ExpiresIn - 300); // 5分前に期限切れとして扱う

      Debug.Log("Auth token refreshed successfully");
    }
    catch (Exception ex)
    {
      Debug.LogError($"Failed to refresh auth token: {ex.Message}");

      // トークンリフレッシュに失敗した場合は再ログインが必要
      EventBus.Trigger("AuthenticationFailed");
      throw new UnauthorizedAccessException("Authentication failed");
    }
  }

  #endregion

  #region Request Configuration

  private void SetRequestHeaders(UnityWebRequest request)
  {
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Accept", "application/json");
    request.SetRequestHeader("User-Agent", $"UnityGameClient/{Application.version}");

    if (!string.IsNullOrEmpty(authToken))
    {
      request.SetRequestHeader("Authorization", $"Bearer {authToken}");
    }

    // デバイス情報
    request.SetRequestHeader("X-Device-ID", SystemInfo.deviceUniqueIdentifier);
    request.SetRequestHeader("X-Platform", Application.platform.ToString());
    request.SetRequestHeader("X-App-Version", Application.version);
  }

  private async Task ApplyRateLimit(string endpoint)
  {
    if (lastRequestTimes.TryGetValue(endpoint, out var lastRequestTime))
    {
      var timeSinceLastRequest = (float)(DateTime.Now - lastRequestTime).TotalSeconds;
      if (timeSinceLastRequest < rateLimitInterval)
      {
        var waitTime = (rateLimitInterval - timeSinceLastRequest) * 1000;
        await Task.Delay((int)waitTime);
      }
    }

    lastRequestTimes[endpoint] = DateTime.Now;
  }

  private void HandleRequestError(UnityWebRequest request)
  {
    var errorMessage = $"API Error [{request.method} {request.url}]: {request.error}";

    switch (request.responseCode)
    {
      case 400:
        throw new ArgumentException($"Bad Request: {request.downloadHandler.text}");
      case 401:
        throw new UnauthorizedAccessException("Unauthorized access");
      case 403:
        throw new UnauthorizedAccessException("Forbidden");
      case 404:
        throw new InvalidOperationException("Resource not found");
      case 429:
        throw new InvalidOperationException("Rate limit exceeded");
      case 500:
        throw new InvalidOperationException("Internal server error");
      default:
        throw new Exception($"{errorMessage} (Code: {request.responseCode})");
    }
  }

  #endregion

  #region Utility Methods

  /// <summary>
  /// ネットワーク接続状態の確認
  /// </summary>
  public bool IsOnline()
  {
    return Application.internetReachability != NetworkReachability.NotReachable;
  }

  /// <summary>
  /// APIベースURLの設定
  /// </summary>
  public void SetBaseURL(string newBaseURL)
  {
    baseURL = newBaseURL;
    Debug.Log($"API base URL updated to: {baseURL}");
  }

  /// <summary>
  /// 認証トークンの手動設定
  /// </summary>
  public void SetAuthToken(string token, int expiresInSeconds)
  {
    authToken = token;
    tokenExpiry = DateTime.Now.AddSeconds(expiresInSeconds - 300);
    Debug.Log("Auth token set manually");
  }

  #endregion
}

#region Request/Response Models

[System.Serializable]
public class BulkDataRequest
{
  public string[] RequestedDataTypes;
  public long LastSyncTimestamp;
}

[System.Serializable]
public class BulkDataResponse
{
  public UserProfile UserProfile;
  public CharacterData[] Characters;
  public ShopItem[] ShopItems;
  public QuestData[] Quests;
  public InventoryItem[] InventoryItems;
  public long ServerTimestamp;
}

[System.Serializable]
public class SyncRequest
{
  public string[] WatchedDataTypes;
  public long LastSyncTimestamp;
}

[System.Serializable]
public class SyncResponse
{
  public UserProfile UpdatedUserProfile;
  public CharacterData[] UpdatedCharacters;
  public ShopItem[] UpdatedShopItems;
  public QuestData[] UpdatedQuests;
  public string[] DeletedDataIds;
  public long ServerTimestamp;
}

[System.Serializable]
public class AuthRefreshRequest
{
  public string UserId;
  public string DeviceId;
}

[System.Serializable]
public class AuthResponse
{
  public string AccessToken;
  public int ExpiresIn;
  public string TokenType;
}

#endregion