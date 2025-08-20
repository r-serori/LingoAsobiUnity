using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class QuestSectionBox : MonoBehaviour
{
  [Header("UI References")]
  [SerializeField] private Image backgroundImage;
  [SerializeField] private TextMeshProUGUI sectionTitle;
  [SerializeField] private TextMeshProUGUI sectionDescription;

  [Header("Image Settings")]
  [SerializeField] private Sprite defaultSprite;
  [SerializeField] private Sprite selectedSprite;
  [SerializeField] private Sprite pressedSprite;

  public void SetBackgroundImage(Sprite sprite)
  {
    if (backgroundImage != null)
    {
      backgroundImage.sprite = sprite;
    }
  }

  public void SetData(string title, string description)
  {
    if (sectionTitle != null) sectionTitle.text = title;
    if (sectionDescription != null) sectionDescription.text = description;
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
}