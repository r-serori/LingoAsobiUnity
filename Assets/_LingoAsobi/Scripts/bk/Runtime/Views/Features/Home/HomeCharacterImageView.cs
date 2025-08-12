using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Scripts.Runtime.DataModels;
using Scripts.Runtime.Managers;
using Scripts.Runtime.Views.Shared;
using Scripts.Runtime.Services;

using System.Threading.Tasks;

namespace Scripts.Runtime.Views.Features.Home
{
  /// <summary>
  /// HomeScene のキャラクター画像表示と不規則動作を管理
  /// OptimizedImageView を継承して画像読み込み最適化
  /// 標準的なMonoBehaviour Singleton パターンに対応
  /// </summary>
  public class HomeCharacterImageView : OptimizedImageView
  {
    [Header("キャラクター設定")]
    [SerializeField] private CharacterImageType displayImageType = CharacterImageType.FullBody;

    [Header("アニメーション設定")]
    [SerializeField] private bool enableRandomAnimation = true;
    [SerializeField] private float minAnimationInterval = 3.0f;  // 最小間隔
    [SerializeField] private float maxAnimationInterval = 8.0f;  // 最大間隔
    [SerializeField] private float animationDuration = 1.5f;    // アニメーション時間

    [Header("動作設定")]
    [SerializeField] private Vector2 bounceAmount = new Vector2(0, 20f);     // バウンス量
    [SerializeField] private Vector2 wiggleAmount = new Vector2(10f, 0);     // 左右揺れ量
    [SerializeField] private float scaleAmount = 0.1f;                      // スケール変化量
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("テスト設定")]
    [SerializeField] private bool useTestCharacter = false;
    [SerializeField] private string testCharacterId = "char_001";

    private Character currentCharacter;
    private RectTransform imageRectTransform;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Coroutine animationCoroutine;
    private User currentUser;

    protected override void Awake()
    {
      base.Awake();

      imageRectTransform = GetComponent<RectTransform>();
      if (imageRectTransform != null)
      {
        originalPosition = imageRectTransform.localPosition;
        originalScale = imageRectTransform.localScale;
      }
    }

    void Start()
    {
      // 開発環境でのテスト用
      if (useTestCharacter && Application.isEditor)
      {
        SetCharacterById(testCharacterId);
        return;
      }

      // ServiceBootstrap 経由でサービスアクセス（標準的なMonoBehaviour Singleton）
      if (ServiceBootstrap.Instance != null)
      {
        if (ServiceBootstrap.Instance.IsInitialized)
        {
          // 既に初期化済みの場合、即座にデータ取得
          InitializeWithServices();
        }
        else
        {
          // 初期化完了を待機してからデータ取得
          ServiceBootstrap.Instance.OnServicesInitialized += InitializeWithServices;
          ServiceBootstrap.Instance.OnUserDataLoaded += OnUserDataLoadedFromBootstrap;
        }
      }
      else
      {
        // フォールバック: 直接Singleton使用
        LoadUserCharacterData();
      }
    }

    /// <summary>
    /// ServiceBootstrap経由での初期化（標準的なMonoBehaviour Singleton）
    /// </summary>
    private void InitializeWithServices()
    {
      // ユーザーデータが既に読み込み済みかチェック
      if (ServiceBootstrap.Instance.IsUserDataLoaded)
      {
        // 既に読み込み済みの場合は即座に処理
        LoadUserCharacterData();
      }
    }

    /// <summary>
    /// ServiceBootstrap からのユーザーデータ読み込み完了通知
    /// </summary>
    private void OnUserDataLoadedFromBootstrap(User user)
    {
      if (user != null)
      {
        SetUserFavoriteCharacter(user);
      }
    }

    /// <summary>
    /// 現在のユーザーデータを取得
    /// UserManager Singleton経由で統一的にアクセス
    /// </summary>
    private async Task<User> GetCurrentUserData()
    {
      // Singleton経由でデータ取得（標準的なUnityアーキテクチャ）
      if (UserManager.Instance != null)
      {
        return await UserManager.Instance.GetCurrentUserAsync();
      }

      throw new System.Exception("UserManager が初期化されていません。");
    }

    /// <summary>
    /// ユーザーキャラクターデータを動的に読み込み
    /// 本番環境ではサーバーAPIから取得予定
    /// </summary>
    private async void LoadUserCharacterData()
    {
      try
      {
        // 現在は MockUser、将来は UserDataService に置換
        User userData = await GetCurrentUserData();

        if (userData != null)
        {
          SetUserFavoriteCharacter(userData);
        }
        else
        {
          // フォールバック処理
          SetCharacterById("char_001");
          Debug.LogWarning("ユーザーデータ取得失敗：デフォルトキャラクターを表示");
        }
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"キャラクターデータ読み込みエラー: {ex.Message}");
        SetCharacterById("char_001"); // 安全なフォールバック
      }
    }

    /// <summary>
    /// ユーザー情報からお気に入りキャラクターを設定
    /// </summary>
    public void SetUserFavoriteCharacter(User user)
    {
      if (user == null || CharacterManager.Instance == null)
      {
        Debug.LogWarning("HomeCharacterImageView: User または CharacterManager が null です");
        return;
      }

      currentUser = user;
      string characterId = !string.IsNullOrEmpty(user.favoriteCharacterId)
          ? user.favoriteCharacterId
          : CharacterManager.Instance.GetDefaultCharacter()?.characterId ?? "char_001";

      SetCharacterById(characterId);
    }

    /// <summary>
    /// キャラクターIDでキャラクターを設定
    /// </summary>
    public void SetCharacterById(string characterId)
    {
      if (CharacterManager.Instance == null)
      {
        Debug.LogError("HomeCharacterImageView: CharacterManager が初期化されていません");
        return;
      }

      var character = CharacterManager.Instance.GetCharacterById(characterId);
      SetCharacter(character);
    }

    /// <summary>
    /// キャラクターデータを設定して画像を更新
    /// </summary>
    public void SetCharacter(Character character)
    {
      if (character == null)
      {
        Debug.LogWarning("HomeCharacterImageView: Character が null です");
        return;
      }

      currentCharacter = character;

      // ユーザーの画像品質設定を取得
      ImageQuality quality = currentUser?.preferredImageQuality ?? ImageQuality.High;

      // 適切な画像パスを取得
      string imagePath = character.GetImagePath(displayImageType, quality);

      // 画像を設定（OptimizedImageView の機能を使用）
      SetImage(imagePath);

      // アニメーション設定を更新
      if (enableRandomAnimation && character.enableRandomAnimation)
      {
        RestartRandomAnimation();
      }
      else
      {
        StopRandomAnimation();
      }

      Debug.Log($"HomeCharacterImageView: キャラクター設定完了 - {character.characterName} ({imagePath})");
    }

    /// <summary>
    /// ランダムアニメーションを開始
    /// </summary>
    public void StartRandomAnimation()
    {
      if (animationCoroutine != null)
      {
        StopCoroutine(animationCoroutine);
      }

      animationCoroutine = StartCoroutine(RandomAnimationLoop());
    }

    /// <summary>
    /// ランダムアニメーションを停止
    /// </summary>
    public void StopRandomAnimation()
    {
      if (animationCoroutine != null)
      {
        StopCoroutine(animationCoroutine);
        animationCoroutine = null;
      }

      // 元の位置・スケールに戻す
      if (imageRectTransform != null)
      {
        imageRectTransform.localPosition = originalPosition;
        imageRectTransform.localScale = originalScale;
      }
    }

    /// <summary>
    /// ランダムアニメーションを再開
    /// </summary>
    public void RestartRandomAnimation()
    {
      StopRandomAnimation();
      StartRandomAnimation();
    }

    /// <summary>
    /// ランダムアニメーションのメインループ
    /// </summary>
    private IEnumerator RandomAnimationLoop()
    {
      while (true)
      {
        // ランダムな間隔で待機
        float waitTime = Random.Range(minAnimationInterval, maxAnimationInterval);
        yield return new WaitForSeconds(waitTime);

        // ランダムなアニメーションを実行
        yield return StartCoroutine(ExecuteRandomAnimation());
      }
    }

    /// <summary>
    /// ランダムアニメーションを実行
    /// </summary>
    private IEnumerator ExecuteRandomAnimation()
    {
      if (imageRectTransform == null) yield break;

      // アニメーションタイプをランダム選択
      int animationType = Random.Range(0, 4);

      switch (animationType)
      {
        case 0: yield return StartCoroutine(BounceAnimation()); break;
        case 1: yield return StartCoroutine(WiggleAnimation()); break;
        case 2: yield return StartCoroutine(ScaleAnimation()); break;
        case 3: yield return StartCoroutine(CombinedAnimation()); break;
      }
    }

    /// <summary>
    /// バウンスアニメーション
    /// </summary>
    private IEnumerator BounceAnimation()
    {
      Vector3 targetPosition = originalPosition + new Vector3(0, bounceAmount.y, 0);

      // 上に移動
      yield return StartCoroutine(AnimatePosition(originalPosition, targetPosition, animationDuration * 0.5f));
      // 下に戻る
      yield return StartCoroutine(AnimatePosition(targetPosition, originalPosition, animationDuration * 0.5f));
    }

    /// <summary>
    /// 左右揺れアニメーション
    /// </summary>
    private IEnumerator WiggleAnimation()
    {
      Vector3 leftPosition = originalPosition + new Vector3(-wiggleAmount.x, 0, 0);
      Vector3 rightPosition = originalPosition + new Vector3(wiggleAmount.x, 0, 0);

      // 左→右→中央
      yield return StartCoroutine(AnimatePosition(originalPosition, leftPosition, animationDuration * 0.3f));
      yield return StartCoroutine(AnimatePosition(leftPosition, rightPosition, animationDuration * 0.4f));
      yield return StartCoroutine(AnimatePosition(rightPosition, originalPosition, animationDuration * 0.3f));
    }

    /// <summary>
    /// スケールアニメーション
    /// </summary>
    private IEnumerator ScaleAnimation()
    {
      Vector3 targetScale = originalScale * (1f + scaleAmount);

      // 拡大
      yield return StartCoroutine(AnimateScale(originalScale, targetScale, animationDuration * 0.5f));
      // 縮小
      yield return StartCoroutine(AnimateScale(targetScale, originalScale, animationDuration * 0.5f));
    }

    /// <summary>
    /// 複合アニメーション
    /// </summary>
    private IEnumerator CombinedAnimation()
    {
      Vector3 targetPosition = originalPosition + new Vector3(
          Random.Range(-wiggleAmount.x, wiggleAmount.x),
          Random.Range(0, bounceAmount.y),
          0
      );
      Vector3 targetScale = originalScale * Random.Range(1f - scaleAmount, 1f + scaleAmount);

      // 同時にポジションとスケールを変更
      yield return StartCoroutine(AnimateBoth(
          originalPosition, targetPosition,
          originalScale, targetScale,
          animationDuration * 0.6f
      ));

      // 元に戻す
      yield return StartCoroutine(AnimateBoth(
          targetPosition, originalPosition,
          targetScale, originalScale,
          animationDuration * 0.4f
      ));
    }

    /// <summary>
    /// 位置アニメーション
    /// </summary>
    private IEnumerator AnimatePosition(Vector3 from, Vector3 to, float duration)
    {
      float elapsedTime = 0f;

      while (elapsedTime < duration)
      {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;
        float curveT = animationCurve.Evaluate(t);

        imageRectTransform.localPosition = Vector3.Lerp(from, to, curveT);
        yield return null;
      }

      imageRectTransform.localPosition = to;
    }

    /// <summary>
    /// スケールアニメーション
    /// </summary>
    private IEnumerator AnimateScale(Vector3 from, Vector3 to, float duration)
    {
      float elapsedTime = 0f;

      while (elapsedTime < duration)
      {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;
        float curveT = animationCurve.Evaluate(t);

        imageRectTransform.localScale = Vector3.Lerp(from, to, curveT);
        yield return null;
      }

      imageRectTransform.localScale = to;
    }

    /// <summary>
    /// 位置とスケール同時アニメーション
    /// </summary>
    private IEnumerator AnimateBoth(Vector3 fromPos, Vector3 toPos, Vector3 fromScale, Vector3 toScale, float duration)
    {
      float elapsedTime = 0f;

      while (elapsedTime < duration)
      {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;
        float curveT = animationCurve.Evaluate(t);

        imageRectTransform.localPosition = Vector3.Lerp(fromPos, toPos, curveT);
        imageRectTransform.localScale = Vector3.Lerp(fromScale, toScale, curveT);
        yield return null;
      }

      imageRectTransform.localPosition = toPos;
      imageRectTransform.localScale = toScale;
    }

    /// <summary>
    /// デバッグ用: 手動でアニメーション実行
    /// </summary>
    [ContextMenu("Test Random Animation")]
    private void TestRandomAnimation()
    {
      if (Application.isPlaying)
      {
        StartCoroutine(ExecuteRandomAnimation());
      }
    }

    void OnDestroy()
    {
      // イベント購読解除
      if (ServiceBootstrap.Instance != null)
      {
        ServiceBootstrap.Instance.OnServicesInitialized -= InitializeWithServices;
        ServiceBootstrap.Instance.OnUserDataLoaded -= OnUserDataLoadedFromBootstrap;
      }

      StopRandomAnimation();
    }
  }
}