using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.Views.Base;

namespace Scripts.Runtime.Views.Features.Title
{
    /// <summary>
    /// タイトル画面のView
    /// BaseViewを継承して共通機能を活用
    /// </summary>
    public class TitleView : BaseView
    {
        #region Events

        public event Action OnStartButtonClicked;
        public event Action OnContinueButtonClicked;
        public event Action OnSettingsButtonClicked;
        // public event Action OnCreditsButtonClicked;
        // public event Action OnQuitButtonClicked;

        #endregion

        #region UI References

        [Header("Title View - Text Elements")]
        // [SerializeField] private TextMeshProUGUI subtitleText;
        // [SerializeField] private TextMeshProUGUI versionText;
        // [SerializeField] private TextMeshProUGUI copyrightText;

        [Header("Title View - Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        // [SerializeField] private Button creditsButton;
        // [SerializeField] private Button quitButton;

        [Header("Title View - Effects")]
        [SerializeField] private ParticleSystem backgroundParticles;

        [Header("Title View - Animation")]
        [SerializeField] private float titleAnimationDelay = 0.5f;
        [SerializeField] private float buttonStaggerDelay = 0.1f;

        #endregion

        #region Private Fields

        private bool hasSaveData = false;

        #endregion

        #region Initialization

        /// <summary>
        /// UI要素の参照を取得
        /// </summary>
        protected override void GetUIReferences()
        {
            base.GetUIReferences();

            // // 自動取得を試みる（Inspectorで設定されていない場合）
            // if (titleText == null)
            //     titleText = transform.Find("Header/TitleText")?.GetComponent<TextMeshProUGUI>();

            // if (buttonContainer == null)
            //     buttonContainer = transform.Find("ButtonContainer")?.gameObject;

            ValidateUIReferences();
        }

        /// <summary>
        /// UI参照の検証
        /// </summary>
        private void ValidateUIReferences()
        {
            // // 他のUI要素の状態も確認
        }

        /// <summary>
        /// イベントリスナーの設定
        /// </summary>
        protected override void SetupEventListeners()
        {
            base.SetupEventListeners();


            // ボタンイベントの登録
            if (startButton != null)
            {
                startButton.onClick.AddListener(() => OnStartButtonClicked?.Invoke());
            }
            else
            {
            }

            if (continueButton != null)
                continueButton.onClick.AddListener(() => OnContinueButtonClicked?.Invoke());

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => OnSettingsButtonClicked?.Invoke());

            // if (creditsButton != null)
            //     creditsButton.onClick.AddListener(() => OnCreditsButtonClicked?.Invoke());

            // if (quitButton != null)
            //     quitButton.onClick.AddListener(() => OnQuitButtonClicked?.Invoke());
        }

        /// <summary>
        /// 初期データの設定
        /// </summary>
        protected override void SetupInitialData()
        {
            base.SetupInitialData();

            // // バージョン情報の設定
            // if (versionText != null)
            // {
            //     versionText.text = $"Version {Application.version}";
            // }

            // // コピーライト情報の設定
            // if (copyrightText != null)
            // {
            //     copyrightText.text = $"© {DateTime.Now.Year} LingoAsobi. All rights reserved.";
            // }

            // // タイトルテキストの設定
            // if (titleText != null)
            // {
            //     titleText.text = Application.productName;
            // }

            // セーブデータの存在確認
            CheckSaveDataAvailability();
        }

        #endregion

        #region View Lifecycle

        /// <summary>
        /// 表示前の処理
        /// </summary>
        protected override async Task OnBeforeShow()
        {
            await base.OnBeforeShow();

            // 初期状態の設定
            ResetAnimations();
            SetButtonsInteractable(false);
        }

        /// <summary>
        /// 表示後の処理
        /// </summary>
        protected override async Task OnAfterShow()
        {
            await base.OnAfterShow();

            // エントランスアニメーションの再生
            await PlayEntranceAnimation();

            // ボタンを有効化
            SetButtonsInteractable(true);
        }

        /// <summary>
        /// 非表示前の処理
        /// </summary>
        protected override async Task OnBeforeHide()
        {
            await base.OnBeforeHide();

            // ボタンを無効化
            SetButtonsInteractable(false);

            // 退出アニメーションの再生
            await PlayExitAnimation();
        }

        #endregion

        #region Animations

        /// <summary>
        /// エントランスアニメーションの再生
        /// </summary>
        private async Task PlayEntranceAnimation()
        {
            // // タイトルのアニメーション
            // if (titleText != null)
            // {
            //     await AnimateTextFadeIn(titleText, titleAnimationDelay);
            // }

            // ボタンのスタガーアニメーション
            await AnimateButtonsStagger();

            // パーティクルエフェクトの開始
            if (backgroundParticles != null && !backgroundParticles.isPlaying)
            {
                backgroundParticles.Play();
            }
        }

        /// <summary>
        /// 退出アニメーションの再生
        /// </summary>
        private async Task PlayExitAnimation()
        {
            // フェードアウトアニメーション
            await Task.Delay(100);

            // パーティクルエフェクトの停止
            if (backgroundParticles != null && backgroundParticles.isPlaying)
            {
                backgroundParticles.Stop();
            }
        }

        /// <summary>
        /// テキストフェードインアニメーション
        /// </summary>
        private async Task AnimateTextFadeIn(TextMeshProUGUI text, float delay)
        {
            if (text == null) return;

            var color = text.color;
            color.a = 0f;
            text.color = color;

            await Task.Delay((int)(delay * 1000));

            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                color.a = Mathf.Lerp(0f, 1f, t);
                text.color = color;
                await Task.Yield();
            }

            color.a = 1f;
            text.color = color;
        }

        /// <summary>
        /// ボタンのスタガーアニメーション
        /// </summary>
        private async Task AnimateButtonsStagger()
        {
            // if (buttonContainer == null) return;

            // Button[] buttons = buttonContainer.GetComponentsInChildren<Button>();

            // foreach (var button in buttons)
            // {
            //     if (button.gameObject.activeSelf)
            //     {
            //         await AnimateButtonAppear(button);
            //         await Task.Delay((int)(buttonStaggerDelay * 1000));
            //     }
            // }
        }

        /// <summary>
        /// ボタン出現アニメーション
        /// </summary>
        private async Task AnimateButtonAppear(Button button)
        {
            if (button == null) return;

            var rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            // スケールアニメーション
            rectTransform.localScale = Vector3.zero;

            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(0f, 1f, EaseOutBack(t));
                rectTransform.localScale = Vector3.one * scale;
                await Task.Yield();
            }

            rectTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// アニメーションのリセット
        /// </summary>
        private void ResetAnimations()
        {
            // if (titleText != null)
            // {
            //     var color = titleText.color;
            //     color.a = 0f;
            //     titleText.color = color;
            // }

            // if (buttonContainer != null)
            // {
            //     Button[] buttons = buttonContainer.GetComponentsInChildren<Button>();
            //     foreach (var button in buttons)
            //     {
            //         var rectTransform = button.GetComponent<RectTransform>();
            //         if (rectTransform != null)
            //         {
            //             rectTransform.localScale = Vector3.zero;
            //         }
            //     }
            // }
        }

        /// <summary>
        /// EaseOutBackイージング関数
        /// </summary>
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        #endregion

        #region Button State Management

        /// <summary>
        /// ボタンのインタラクティブ状態を設定
        /// </summary>
        private void SetButtonsInteractable(bool interactable)
        {

            if (startButton != null)
            {
                startButton.interactable = interactable;
            }
            else
            {
            }

            if (continueButton != null)
                continueButton.interactable = interactable && hasSaveData;

            if (settingsButton != null)
                settingsButton.interactable = interactable;

            // if (creditsButton != null)
            //     creditsButton.interactable = interactable;

            // if (quitButton != null)
            //     quitButton.interactable = interactable;
        }

        /// <summary>
        /// セーブデータの存在確認
        /// </summary>
        private void CheckSaveDataAvailability()
        {
            // TODO: 実際のセーブデータ確認処理
            hasSaveData = PlayerPrefs.HasKey("SaveData");

            // 続きからボタンの表示/非表示
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(hasSaveData);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// エラーメッセージの表示
        /// </summary>
        public void ShowError(string message)
        {
            ShowError(message);
            // TODO: UIでのエラー表示実装
        }

        /// <summary>
        /// セーブデータ状態の更新
        /// </summary>
        public void UpdateSaveDataStatus(bool hasData)
        {
            hasSaveData = hasData;

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(hasSaveData);
                continueButton.interactable = hasSaveData && isInteractable;
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// イベントリスナーの解除
        /// </summary>
        protected override void RemoveEventListeners()
        {
            base.RemoveEventListeners();

            if (startButton != null)
                startButton.onClick.RemoveAllListeners();

            if (continueButton != null)
                continueButton.onClick.RemoveAllListeners();

            if (settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();

            // if (creditsButton != null)
            //     creditsButton.onClick.RemoveAllListeners();

            // if (quitButton != null)
            //     quitButton.onClick.RemoveAllListeners();
        }

        #endregion
    }
}