using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.Data.Models.Character;

namespace Scripts.Runtime.Views.Features.Shared.Modal
{
  public class StandbyCharacterBox : MonoBehaviour
  {
    [Header("UI References")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Image frameImage;

    public void SetCharacterImage(string spritePath)
    {
      if (characterImage != null)
      {
        characterImage.sprite = Resources.Load<Sprite>(spritePath);
      }
    }

    public void SetFrameImage(CharacterAttribute attribute)
    {
      // TODO: フレーム画像が決定次第、変更
      if (frameImage != null)
      {
        Debug.Log("SetFrameImage: " + attribute);
        frameImage.sprite = attribute switch
        {
          CharacterAttribute.Fire => Resources.Load<Sprite>("UI/shared/modal/mid_frame"),
          CharacterAttribute.Light => Resources.Load<Sprite>("UI/shared/modal/mid_frame"),
          CharacterAttribute.Dark => Resources.Load<Sprite>("UI/shared/modal/mid_frame"),
          CharacterAttribute.Neutral => Resources.Load<Sprite>("UI/shared/modal/mid_frame"),
          CharacterAttribute.Water => Resources.Load<Sprite>("UI/shared/modal/mid_frame"),
          CharacterAttribute.Wood => Resources.Load<Sprite>("UI/shared/modal/mid_frame"),
          _ => Resources.Load<Sprite>("UI/shared/modal/mid_frame"),
        };
      }
    }
  }
}