using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Grammar;

namespace Scripts.Runtime.Views.ViewData.Grammar
{
  public class GrammarFloorViewData
  {
    public UserProfile CurrentUser { get; set; }
    public GrammarFloorData FloorItem { get; set; }
    public string CharacterImageUrl { get; set; }

    public GrammarFloorViewData(UserProfile currentUser, GrammarFloorData floorItem, string characterImageUrl)
    {
      CurrentUser = currentUser;
      FloorItem = floorItem;
      CharacterImageUrl = characterImageUrl;
    }
  }
}