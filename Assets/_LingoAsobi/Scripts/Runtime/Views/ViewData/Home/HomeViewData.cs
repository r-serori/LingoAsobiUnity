
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Grammar;
using Scripts.Runtime.Data.Models.Character;

namespace Scripts.Runtime.Views.ViewData.Home
{
  public class HomeViewData
  {
    public UserProfile CurrentUser { get; set; }
    public CharacterData FavoriteCharacter { get; set; }
    public HomeViewData(UserProfile currentUser, CharacterData favoriteCharacter)
    {
      CurrentUser = currentUser;
      FavoriteCharacter = favoriteCharacter;
    }
  }
}