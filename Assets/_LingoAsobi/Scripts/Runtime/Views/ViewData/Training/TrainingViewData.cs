using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Models.Training;
using System.Collections.Generic;

namespace Scripts.Runtime.Views.ViewData.Training
{
  public class TrainingViewData
  {
    public UserProfile CurrentUser { get; set; }
    public TrainingData TrainingData { get; set; }

    public TrainingViewData(UserProfile currentUser, TrainingData trainingData)
    {
      CurrentUser = currentUser;
      TrainingData = trainingData;
    }
  }
}