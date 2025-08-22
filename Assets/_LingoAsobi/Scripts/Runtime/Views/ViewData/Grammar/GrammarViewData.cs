using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Grammar;
using System.Collections.Generic;

namespace Scripts.Runtime.Views.ViewData.Grammar
{
  public class GrammarViewData
  {
    public UserProfile CurrentUser { get; set; }
    public List<GrammarData> GrammarDataList { get; set; }

    public GrammarViewData(UserProfile currentUser, List<GrammarData> grammarDataList)
    {
      CurrentUser = currentUser;
      GrammarDataList = grammarDataList;
    }
  }
}