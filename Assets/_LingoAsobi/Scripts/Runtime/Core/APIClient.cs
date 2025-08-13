using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Scripts.Runtime.Core
{
  /// <summary>
  /// API通信を管理するクライアントクラス
  /// UnityWebRequestを使用したHTTP通信の抽象化
  /// </summary>
  public class APIClient : MonoBehaviour
  {
    // シングルトンインスタンス
    private static APIClient _instance;
    public static APIClient Instance
    {
      get
      {
        if (_instance == null)
        {
          GameObject go = new GameObject("APIClient");
          _instance = go.AddComponent<APIClient>();
          DontDestroyOnLoad(go);
        }
        return _instance;
      }
    }

    // API設定
    [Header("API Configuration")]
    [SerializeField] private string baseUrl = "https://api.example.com";
    [SerializeField] private float timeoutSeconds = 30f;
    [SerializeField] private int maxRetryCount = 3;

    // 認証トークン
    private string _authToken;

    /// <summary>
    /// 認証トークンを設定
    /// </summary>
    public void SetAuthToken(string token)
    {
      _authToken = token;
      Debug.Log("[APIClient] Auth token set");
    }

    /// <summary>
    /// GETリクエスト
    /// </summary>
    public async Task<T> GetAsync<T>(string endpoint) where T : class
    {
      string url = $"{baseUrl}{endpoint}";

      for (int retry = 0; retry < maxRetryCount; retry++)
      {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
          SetHeaders(request);
          request.timeout = (int)timeoutSeconds;

          try
          {
            await SendRequestAsync(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
              return ParseResponse<T>(request.downloadHandler.text);
            }

            if (request.responseCode == 401) // Unauthorized
            {
              Debug.LogError("[APIClient] Authentication failed");
              throw new UnauthorizedAccessException("Authentication failed");
            }

            if (retry < maxRetryCount - 1)
            {
              await Task.Delay(1000 * (retry + 1)); // 指数バックオフ
              continue;
            }

            throw new Exception($"Request failed: {request.error}");
          }
          catch (Exception e)
          {
            Debug.LogError($"[APIClient] GET request failed: {e.Message}");
            if (retry == maxRetryCount - 1) throw;
          }
        }
      }

      return null;
    }

    /// <summary>
    /// POSTリクエスト
    /// </summary>
    public async Task<T> PostAsync<T>(string endpoint, object data) where T : class
    {
      string url = $"{baseUrl}{endpoint}";
      string jsonData = JsonUtility.ToJson(data);

      for (int retry = 0; retry < maxRetryCount; retry++)
      {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
          byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
          request.uploadHandler = new UploadHandlerRaw(bodyRaw);
          request.downloadHandler = new DownloadHandlerBuffer();

          SetHeaders(request);
          request.SetRequestHeader("Content-Type", "application/json");
          request.timeout = (int)timeoutSeconds;

          try
          {
            await SendRequestAsync(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
              return ParseResponse<T>(request.downloadHandler.text);
            }

            if (retry < maxRetryCount - 1)
            {
              await Task.Delay(1000 * (retry + 1));
              continue;
            }

            throw new Exception($"Request failed: {request.error}");
          }
          catch (Exception e)
          {
            Debug.LogError($"[APIClient] POST request failed: {e.Message}");
            if (retry == maxRetryCount - 1) throw;
          }
        }
      }

      return null;
    }

    /// <summary>
    /// PUTリクエスト
    /// </summary>
    public async Task<T> PutAsync<T>(string endpoint, object data) where T : class
    {
      string url = $"{baseUrl}{endpoint}";
      string jsonData = JsonUtility.ToJson(data);

      using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData))
      {
        request.downloadHandler = new DownloadHandlerBuffer();
        SetHeaders(request);
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = (int)timeoutSeconds;

        await SendRequestAsync(request);

        if (request.result == UnityWebRequest.Result.Success)
        {
          return ParseResponse<T>(request.downloadHandler.text);
        }

        Debug.LogError($"[APIClient] PUT request failed: {request.error}");
        return null;
      }
    }

    /// <summary>
    /// DELETEリクエスト
    /// </summary>
    public async Task<bool> DeleteAsync(string endpoint)
    {
      string url = $"{baseUrl}{endpoint}";

      using (UnityWebRequest request = UnityWebRequest.Delete(url))
      {
        SetHeaders(request);
        request.timeout = (int)timeoutSeconds;

        await SendRequestAsync(request);

        if (request.result == UnityWebRequest.Result.Success)
        {
          return true;
        }

        Debug.LogError($"[APIClient] DELETE request failed: {request.error}");
        return false;
      }
    }

    /// <summary>
    /// ヘッダーを設定
    /// </summary>
    private void SetHeaders(UnityWebRequest request)
    {
      if (!string.IsNullOrEmpty(_authToken))
      {
        request.SetRequestHeader("Authorization", $"Bearer {_authToken}");
      }

      request.SetRequestHeader("Accept", "application/json");
    }

    /// <summary>
    /// リクエストを非同期で送信
    /// </summary>
    private async Task SendRequestAsync(UnityWebRequest request)
    {
      var operation = request.SendWebRequest();

      while (!operation.isDone)
      {
        await Task.Yield();
      }
    }

    /// <summary>
    /// レスポンスをパース
    /// </summary>
    private T ParseResponse<T>(string json) where T : class
    {
      try
      {
        return JsonUtility.FromJson<T>(json);
      }
      catch (Exception e)
      {
        Debug.LogError($"[APIClient] Failed to parse response: {e.Message}");
        Debug.LogError($"[APIClient] Response JSON: {json}");
        return null;
      }
    }

    /// <summary>
    /// APIのヘルスチェック
    /// </summary>
    public async Task<bool> HealthCheckAsync()
    {
      try
      {
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/health"))
        {
          request.timeout = 5;
          await SendRequestAsync(request);
          return request.result == UnityWebRequest.Result.Success;
        }
      }
      catch
      {
        return false;
      }
    }
  }
}