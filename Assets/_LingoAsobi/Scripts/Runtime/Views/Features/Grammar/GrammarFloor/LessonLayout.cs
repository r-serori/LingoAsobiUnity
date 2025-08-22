using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LessonLayout : MonoBehaviour
{
  [Header("UI References")]
  [SerializeField] private Button lessonButton;
  [SerializeField] private Image backgroundImage;
  [SerializeField] private TextMeshProUGUI lessonTitle;
  // [SerializeField] private TextMeshProUGUI lessonDescription;

  [Header("Image Settings")]
  [SerializeField] private Sprite defaultSprite;
  [SerializeField] private Sprite selectedSprite;
  [SerializeField] private Sprite pressedSprite;

  private int order;

  public void SetBackgroundImage(Sprite sprite)
  {
    if (backgroundImage != null)
    {
      backgroundImage.sprite = sprite;
    }
  }

  public void SetData(int order, string title)
  {
    this.order = order;
    if (lessonTitle != null) lessonTitle.text = title;
    // if (lessonDescription != null) lessonDescription.text = description;
  }

  public void SetDefaultImage()
  {
    SetBackgroundImage(defaultSprite);
  }

  public void SetSelectedImage()
  {
    SetBackgroundImage(selectedSprite);
  }

  public void SetPressedImage()
  {
    SetBackgroundImage(pressedSprite);
  }

  // lessonButtonにアクセスするためのpublicプロパティ
  public Button LessonButton => lessonButton;

  // ChildAlignmentを設定するメソッド
  public void SetChildAlignment(TextAnchor alignment)
  {
    var horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
    if (horizontalLayoutGroup != null)
    {
      horizontalLayoutGroup.childAlignment = alignment;
    }
  }

  public int GetOrder()
  {
    return order;
  }
}