using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Grammar;

namespace Scripts.Runtime.Views.ViewData.Grammar
{
  public class GrammarTowerViewData
  {
    public UserProfile CurrentUser { get; set; }
    public GrammarData GrammarData { get; set; }
    public string CharacterImageUrl { get; set; }

    public GrammarTowerViewData(UserProfile currentUser, GrammarData grammarData, string characterImageUrl)
    {
      CurrentUser = currentUser;
      GrammarData = grammarData;
      CharacterImageUrl = characterImageUrl;
    }
  }
}