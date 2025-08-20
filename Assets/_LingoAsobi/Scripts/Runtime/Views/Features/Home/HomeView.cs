using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Repositories;
using Scripts.Runtime.Utilities.Helpers;
using Scripts.Runtime.Core;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Views.Features.Header;

namespace Scripts.Runtime.Views.Features.Home
{
  /// <summary>
  /// ホーム画面のView
  /// ユーザー情報、キャラクター表示、メニューボタンを管理
  /// </summary>
  public class HomeView : BaseView
  {
    [Header("Header")]
    [SerializeField] public ProfileHeaderView profileHeaderView;

    [Header("Character Display")]
    [SerializeField] private Image characterImage;
    // [SerializeField] private TextMeshProUGUI characterNameText;
    // [SerializeField] private Button characterChangeButton;

    [Header("Character Animation Settings")]
    [SerializeField] private bool enableCharacterAnimation = true;
    [SerializeField] private float animationSpeed = 1.0f;
    [SerializeField] private float maxRotation = 5.0f;
    [SerializeField] private float maxScale = 0.1f;
    [SerializeField] private float maxPosition = 10.0f;

    private UserProfile currentUser;
    private CharacterData favoriteCharacter;

    // アニメーション用の変数

    [SerializeField] private float maxVerticalMovement = 15.0f;    // 上下の最大移動量
    [SerializeField] private float maxHorizontalMovement = 20.0f;  // 左右の最大移動量
    [SerializeField] private float verticalSpeed = 0.8f;           // 上下の動作速度
    [SerializeField] private float horizontalSpeed = 0.6f;         // 左右の動作速度
    [SerializeField] private bool enableFloating = true;           // 浮遊効果の有効/無効
    [SerializeField] private bool enableSwaying = true;            // 揺れ効果の有効/無効

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private float animationTime = 0f;
    private float randomSeed;
    private float floatingOffset = 0f;
    private float swayingOffset = 0f;

    #region Initialization

    protected override void GetUIReferences()
    {
      base.GetUIReferences();

      if (profileHeaderView == null)
      {
        profileHeaderView = GetComponentInChildren<ProfileHeaderView>();
      }

      if (characterImage == null)
      {
        characterImage = GetComponentInChildren<Image>();
      }

      // アニメーション用の初期値を保存
      if (characterImage != null)
      {
        originalPosition = characterImage.rectTransform.anchoredPosition;
        originalScale = characterImage.rectTransform.localScale;
        originalRotation = characterImage.rectTransform.localRotation;
        randomSeed = UnityEngine.Random.Range(0f, 1000f);
      }
    }

    protected override void SetupEventListeners()
    {
      base.SetupEventListeners();

      // if (characterChangeButton != null)
      // {
      //   characterChangeButton.onClick.AddListener(OnCharacterChangeClicked);
      // }
    }

    #endregion

    #region Data Management

    /// <summary>
    /// ユーザーデータを設定
    /// </summary>
    public void SetUserData(UserProfile user)
    {
      currentUser = user;
      profileHeaderView.SetUserData(currentUser);
      UpdateDisplay();
    }

    public void SetCharacterData(CharacterData character)
    {
      favoriteCharacter = character;
      UpdateDisplay();
    }

    protected override async Task LoadDataAsync()
    {
    }

    protected override void UpdateDisplay()
    {
      if (currentUser == null) return;
      // キャラクター
      UpdateCharacterDisplay();
    }

    /// <summary>
    /// キャラクター表示を更新
    /// </summary>
    private void UpdateCharacterDisplay()
    {
      if (favoriteCharacter == null)
      {
        return;
      }

      // キャラクター画像を読み込む
      if (characterImage != null && !string.IsNullOrEmpty(favoriteCharacter.fullBodyImagePath))
      {
        var sprite = Resources.Load<Sprite>(favoriteCharacter.fullBodyImagePath);
        if (sprite != null)
        {
          characterImage.sprite = sprite;
        }
      }
    }

    #endregion

    #region Update Loop

    private void Update()
    {
      // キャラクターアニメーションを常に適用
      if (enableCharacterAnimation && characterImage != null)
      {
        ApplyCharacterAnimation();
      }
    }

    #endregion

    #region Character Animation

    /// <summary>
    /// キャラクターに不規則な動作を適用
    /// </summary>
    private void ApplyCharacterAnimation()
    {
      if (characterImage == null) return;

      animationTime += Time.deltaTime * animationSpeed;

      // 不規則な動作を生成（複数の正弦波を組み合わせて自然な動きを作成）
      float time1 = animationTime * 0.5f + randomSeed;
      float time2 = animationTime * 0.3f + randomSeed * 0.7f;
      float time3 = animationTime * 0.7f + randomSeed * 0.3f;
      float time4 = animationTime * 0.4f + randomSeed * 0.5f;
      float time5 = animationTime * 0.6f + randomSeed * 0.2f;

      // 回転の不規則動作
      float rotationX = Mathf.Sin(time1) * Mathf.Cos(time2 * 0.5f) * maxRotation;
      float rotationY = Mathf.Cos(time1 * 0.7f) * Mathf.Sin(time2) * maxRotation;
      float rotationZ = Mathf.Sin(time3) * maxRotation * 0.3f;

      // スケールの不規則動作
      float scaleX = 1f + Mathf.Sin(time2) * Mathf.Cos(time3 * 0.6f) * maxScale;
      float scaleY = 1f + Mathf.Cos(time1 * 0.8f) * Mathf.Sin(time3) * maxScale;
      float scaleZ = 1f;

      // 基本位置の不規則動作（微細な動き）
      float basePosX = originalPosition.x + Mathf.Sin(time3) * Mathf.Cos(time1 * 0.4f) * (maxPosition * 0.3f);
      float basePosY = originalPosition.y + Mathf.Cos(time2 * 0.6f) * Mathf.Sin(time1) * (maxPosition * 0.3f);

      // 上下の浮遊動作
      float verticalMovement = 0f;
      if (enableFloating)
      {
        // 複数の正弦波を組み合わせて自然な浮遊動作
        verticalMovement = Mathf.Sin(time4 * verticalSpeed) * maxVerticalMovement * 0.5f;
        verticalMovement += Mathf.Sin(time5 * verticalSpeed * 0.7f) * maxVerticalMovement * 0.3f;
        verticalMovement += Mathf.Cos(time1 * verticalSpeed * 0.4f) * maxVerticalMovement * 0.2f;

        // 浮遊の高さを調整
        floatingOffset = Mathf.Sin(time4 * 0.3f) * 5f;
        verticalMovement += floatingOffset;
      }

      // 左右の揺れ動作
      float horizontalMovement = 0f;
      if (enableSwaying)
      {
        // 左右にゆっくりと揺れる動作
        horizontalMovement = Mathf.Sin(time5 * horizontalSpeed) * maxHorizontalMovement * 0.6f;
        horizontalMovement += Mathf.Cos(time4 * horizontalSpeed * 0.8f) * maxHorizontalMovement * 0.4f;

        // 揺れの中心位置を調整
        swayingOffset = Mathf.Sin(time5 * 0.2f) * 3f;
        horizontalMovement += swayingOffset;
      }

      // 最終的な位置を計算
      float finalPosX = basePosX + horizontalMovement;
      float finalPosY = basePosY + verticalMovement;

      // アニメーションを適用
      characterImage.rectTransform.localScale = new Vector3(scaleX, scaleY, scaleZ);
      characterImage.rectTransform.anchoredPosition = new Vector2(finalPosX, finalPosY);
    }

    /// <summary>
    /// アニメーション設定を動的に変更（上下左右の動作設定を含む）
    /// </summary>
    public void SetAnimationSettings(
        bool enable,
        float speed = 1.0f,
        float rotation = 5.0f,
        float scale = 0.1f,
        float position = 10.0f,
        float vertical = 15.0f,
        float horizontal = 20.0f,
        float vSpeed = 0.8f,
        float hSpeed = 0.6f,
        bool floating = true,
        bool swaying = true)
    {
      enableCharacterAnimation = enable;
      animationSpeed = speed;
      maxRotation = rotation;
      maxScale = scale;
      maxPosition = position;
      maxVerticalMovement = vertical;
      maxHorizontalMovement = horizontal;
      verticalSpeed = vSpeed;
      horizontalSpeed = hSpeed;
      enableFloating = floating;
      enableSwaying = swaying;
    }

    /// <summary>
    /// 浮遊効果の設定
    /// </summary>
    public void SetFloatingSettings(bool enable, float amount, float speed)
    {
      enableFloating = enable;
      maxVerticalMovement = amount;
      verticalSpeed = speed;
    }

    /// <summary>
    /// 揺れ効果の設定
    /// </summary>
    public void SetSwayingSettings(bool enable, float amount, float speed)
    {
      enableSwaying = enable;
      maxHorizontalMovement = amount;
      horizontalSpeed = speed;
    }

    /// <summary>
    /// 特定の方向の動作を無効化
    /// </summary>
    public void DisableDirectionalMovement(bool disableVertical, bool disableHorizontal)
    {
      if (disableVertical) enableFloating = false;
      if (disableHorizontal) enableSwaying = false;
    }

    /// <summary>
    /// 特定の方向の動作を有効化
    /// </summary>
    public void EnableDirectionalMovement(bool enableVertical, bool enableHorizontal)
    {
      if (enableVertical) enableFloating = true;
      if (enableHorizontal) enableSwaying = true;
    }

    /// <summary>
    /// アニメーションを一時停止/再開
    /// </summary>
    public void ToggleAnimation()
    {
      enableCharacterAnimation = !enableCharacterAnimation;
    }

    /// <summary>
    /// アニメーションをリセット
    /// </summary>
    public void ResetAnimation()
    {
      if (characterImage != null)
      {
        characterImage.rectTransform.anchoredPosition = originalPosition;
        characterImage.rectTransform.localScale = originalScale;
        characterImage.rectTransform.localRotation = originalRotation;
        animationTime = 0f;
      }
    }

    #endregion

    #region Effects


    #endregion

    #region Button Handlers

    private void OnCharacterChangeClicked()
    {
      // キャラクター選択画面を開く
      // TODO: キャラクター選択ポップアップを表示
    }

    private void OnCurrencyClicked(string currencyType)
    {
      // ショップ画面へ遷移
      EventBus.Instance.Publish(new SceneTransitionEvent
      (
        GameConstants.Scenes.Shop,
        SceneTransitionEvent.TransitionPhase.Started
      ));
    }

    #endregion

  }
}