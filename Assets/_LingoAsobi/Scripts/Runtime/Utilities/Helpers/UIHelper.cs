using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Scripts.Runtime.Utilities.Constants;

namespace Scripts.Runtime.Utilities.Helpers
{
  /// <summary>
  /// UI操作ヘルパークラス
  /// UI要素の操作とアニメーションを簡潔に行う
  /// </summary>
  public static class UIHelper
  {
    #region Fade Animations

    /// <summary>
    /// CanvasGroupをフェードイン
    /// </summary>
    public static async Task FadeInAsync(CanvasGroup canvasGroup, float duration = 0.3f)
    {
      if (canvasGroup == null) return;

      canvasGroup.gameObject.SetActive(true);
      float startAlpha = canvasGroup.alpha;
      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
        await Task.Yield();
      }

      canvasGroup.alpha = 1f;
      canvasGroup.interactable = true;
      canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// CanvasGroupをフェードアウト
    /// </summary>
    public static async Task FadeOutAsync(CanvasGroup canvasGroup, float duration = 0.3f)
    {
      if (canvasGroup == null) return;

      canvasGroup.interactable = false;
      canvasGroup.blocksRaycasts = false;

      float startAlpha = canvasGroup.alpha;
      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
        await Task.Yield();
      }

      canvasGroup.alpha = 0f;
      canvasGroup.gameObject.SetActive(false);
    }

    /// <summary>
    /// Imageをフェード
    /// </summary>
    public static IEnumerator FadeImage(Image image, float targetAlpha, float duration)
    {
      if (image == null) yield break;

      Color startColor = image.color;
      Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        image.color = Color.Lerp(startColor, targetColor, elapsed / duration);
        yield return null;
      }

      image.color = targetColor;
    }

    #endregion

    #region Scale Animations

    /// <summary>
    /// スケールアニメーション（ポップ）
    /// </summary>
    public static async Task PopScaleAsync(Transform transform, float duration = 0.2f)
    {
      if (transform == null) return;

      Vector3 originalScale = transform.localScale;
      transform.localScale = Vector3.zero;

      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;

        // オーバーシュート効果を加える
        float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
        scale = 1f + (scale * 0.1f) - (0.1f * t);

        transform.localScale = originalScale * scale;
        await Task.Yield();
      }

      transform.localScale = originalScale;
    }

    /// <summary>
    /// パルスアニメーション（拡大縮小の繰り返し）
    /// </summary>
    public static IEnumerator PulseScale(Transform transform, float scaleFactor = 1.1f, float duration = 0.5f)
    {
      if (transform == null) yield break;

      Vector3 originalScale = transform.localScale;
      Vector3 targetScale = originalScale * scaleFactor;

      while (true)
      {
        // 拡大
        yield return ScaleToCoroutine(transform, targetScale, duration / 2);
        // 縮小
        yield return ScaleToCoroutine(transform, originalScale, duration / 2);
      }
    }

    private static IEnumerator ScaleToCoroutine(Transform transform, Vector3 target, float duration)
    {
      Vector3 start = transform.localScale;
      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        transform.localScale = Vector3.Lerp(start, target, elapsed / duration);
        yield return null;
      }

      transform.localScale = target;
    }

    #endregion

    #region Position Animations

    /// <summary>
    /// スライドイン（左から）
    /// </summary>
    public static async Task SlideInFromLeftAsync(RectTransform rectTransform, float duration = 0.3f)
    {
      if (rectTransform == null) return;

      Vector2 originalPosition = rectTransform.anchoredPosition;
      Vector2 startPosition = new Vector2(-Screen.width, originalPosition.y);

      rectTransform.anchoredPosition = startPosition;

      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        float t = Mathf.SmoothStep(0, 1, elapsed / duration);
        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, originalPosition, t);
        await Task.Yield();
      }

      rectTransform.anchoredPosition = originalPosition;
    }

    /// <summary>
    /// スライドアウト（右へ）
    /// </summary>
    public static async Task SlideOutToRightAsync(RectTransform rectTransform, float duration = 0.3f)
    {
      if (rectTransform == null) return;

      Vector2 startPosition = rectTransform.anchoredPosition;
      Vector2 targetPosition = new Vector2(Screen.width, startPosition.y);

      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        float t = Mathf.SmoothStep(0, 1, elapsed / duration);
        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
        await Task.Yield();
      }

      rectTransform.anchoredPosition = targetPosition;
    }

    #endregion

    #region UI Creation Helpers

    /// <summary>
    /// ポップアップを作成
    /// </summary>
    public static GameObject CreatePopup(string title, string message, Transform parent = null)
    {
      GameObject popup = new GameObject("Popup");

      if (parent == null)
      {
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas != null) parent = canvas.transform;
      }

      popup.transform.SetParent(parent, false);

      // 背景
      GameObject bg = new GameObject("Background");
      bg.transform.SetParent(popup.transform, false);
      RectTransform bgRect = bg.AddComponent<RectTransform>();
      bgRect.anchorMin = Vector2.zero;
      bgRect.anchorMax = Vector2.one;
      bgRect.sizeDelta = Vector2.zero;
      Image bgImage = bg.AddComponent<Image>();
      bgImage.color = new Color(0, 0, 0, 0.5f);

      // パネル
      GameObject panel = new GameObject("Panel");
      panel.transform.SetParent(popup.transform, false);
      RectTransform panelRect = panel.AddComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0.5f);
      panelRect.anchorMax = new Vector2(0.5f, 0.5f);
      panelRect.sizeDelta = new Vector2(400, 300);
      Image panelImage = panel.AddComponent<Image>();
      panelImage.color = Color.white;

      // タイトル
      GameObject titleObj = new GameObject("Title");
      titleObj.transform.SetParent(panel.transform, false);
      RectTransform titleRect = titleObj.AddComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0.5f, 1f);
      titleRect.anchorMax = new Vector2(0.5f, 1f);
      titleRect.anchoredPosition = new Vector2(0, -30);
      titleRect.sizeDelta = new Vector2(350, 50);
      TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
      titleText.text = title;
      titleText.fontSize = 24;
      titleText.alignment = TextAlignmentOptions.Center;

      // メッセージ
      GameObject messageObj = new GameObject("Message");
      messageObj.transform.SetParent(panel.transform, false);
      RectTransform messageRect = messageObj.AddComponent<RectTransform>();
      messageRect.anchorMin = new Vector2(0.5f, 0.5f);
      messageRect.anchorMax = new Vector2(0.5f, 0.5f);
      messageRect.anchoredPosition = Vector2.zero;
      messageRect.sizeDelta = new Vector2(350, 150);
      TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
      messageText.text = message;
      messageText.fontSize = 16;
      messageText.alignment = TextAlignmentOptions.Center;

      return popup;
    }

    #endregion
  }
}