using System.Threading.Tasks;

/// <summary>
/// リポジトリの基底インターフェース
/// </summary>
public interface IRepository
{
  Task RefreshAllAsync();
  void SaveToLocal();
  void LoadFromLocal();
}