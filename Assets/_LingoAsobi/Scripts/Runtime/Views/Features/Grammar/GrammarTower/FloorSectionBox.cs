using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FloorSectionBox : MonoBehaviour
{
  [Header("UI References")]
  [SerializeField] private Image backgroundImage;
  [SerializeField] private TextMeshProUGUI floorTitle;
  [SerializeField] private TextMeshProUGUI floorDescription;

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
    if (floorTitle != null) floorTitle.text = title;
    if (floorDescription != null) floorDescription.text = description;
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