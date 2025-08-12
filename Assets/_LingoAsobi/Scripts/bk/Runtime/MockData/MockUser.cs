using UnityEngine;
using Scripts.Runtime.DataModels;

namespace Scripts.Runtime.MockData
{
  [CreateAssetMenu(fileName = "MockUser", menuName = "MockData/MockUser")]
  public class MockUser : ScriptableObject
  {
    public User user;
  }
}