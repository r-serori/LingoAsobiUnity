using UnityEngine;
using System;

namespace Scripts.Runtime.Data.Models.User
{
  [System.Serializable]
  public class CurrencyData
  {
    public int gold;
    public int gem;
    public int coin;
    public DateTime lastUpdated;

    public CurrencyData()
    {
      gold = 0;
      gem = 0;
      coin = 0;
      lastUpdated = DateTime.Now;
    }

    public CurrencyData(int gold, int gem, int coin)
    {
      this.gold = gold;
      this.gem = gem;
      this.coin = coin;
      this.lastUpdated = DateTime.Now;
    }

    public void AddGold(int amount)
    {
      gold = Math.Max(0, gold + amount);
      lastUpdated = DateTime.Now;
    }

    public void AddGem(int amount)
    {
      gem = Math.Max(0, gem + amount);
      lastUpdated = DateTime.Now;
    }

    public void AddCoin(int amount)
    {
      coin = Math.Max(0, coin + amount);
      lastUpdated = DateTime.Now;
    }

    public bool CanAfford(int goldCost, int gemCost, int coinCost)
    {
      return gold >= goldCost && gem >= gemCost && coin >= coinCost;
    }

    public bool SpendCurrency(int goldCost, int gemCost, int coinCost)
    {
      if (!CanAfford(goldCost, gemCost, coinCost))
        return false;

      gold -= goldCost;
      gem -= gemCost;
      coin -= coinCost;
      lastUpdated = DateTime.Now;
      return true;
    }
  }
}
