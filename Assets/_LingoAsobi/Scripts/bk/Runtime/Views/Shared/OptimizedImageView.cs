using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Scripts.Runtime.Managers;

namespace Scripts.Runtime.Views.Shared
{
    public class OptimizedImageView : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private GameObject loadingIndicator;

        private CancellationTokenSource cancellationToken;
        private string currentImageKey;

        protected virtual void Awake()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();
        }

        public async void SetImage(string imageKey)
        {
            // 同じ画像の場合はスキップ
            if (currentImageKey == imageKey) return;

            // Singleton インスタンスを取得（自動生成される）
            var resourceManager = PerformanceResourceManager.Instance;

            // 前の読み込みをキャンセル
            cancellationToken?.Cancel();
            cancellationToken = new CancellationTokenSource();

            currentImageKey = imageKey;

            // ローディング表示
            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);

            try
            {
                Sprite sprite = await resourceManager.LoadSpriteAsync(imageKey);

                // キャンセルされていないかチェック
                if (!cancellationToken.Token.IsCancellationRequested && sprite != null)
                {
                    targetImage.sprite = sprite;
                }
                else if (sprite == null)
                {
                    Debug.LogWarning($"画像の読み込みに失敗しました: {imageKey}");
                }
            }
            catch (System.OperationCanceledException)
            {
                // キャンセルされた場合は何もしない
                Debug.Log($"画像読み込みがキャンセルされました: {imageKey}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"画像読み込み中にエラーが発生しました: {imageKey}, エラー: {ex.Message}");
            }
            finally
            {
                if (loadingIndicator != null)
                    loadingIndicator.SetActive(false);
            }
        }

        void OnDestroy()
        {
            cancellationToken?.Cancel();
        }
    }
}