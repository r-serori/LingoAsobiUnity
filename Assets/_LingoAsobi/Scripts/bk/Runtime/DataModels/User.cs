using UnityEngine;

namespace Scripts.Runtime.DataModels
{
    [System.Serializable]
    public class User
    {
        [Header("User Information")]
        public int userId;
        public string userName;
        public string userIconUrl;
        public string email;
        public string password;

        [Header("Game Progress")]
        public int level = 1;
        public int exp;
        public int gold;
        public int gem;
        public int stamina;
        // staminaMax は削除（動的計算）

        [Header("Character Preferences")]
        public string favoriteCharacterId;
        public bool enableCharacterAnimation = true;


        [Header("Performance Settings")]
        public ImageQuality preferredImageQuality = ImageQuality.High;
        public bool enableImagePreloading = true;

        /// <summary>
        /// 最大スタミナを取得（GameBalanceConfigから計算）
        /// </summary>
        public int GetMaxStamina(GameBalanceManager config)
        {
            return config != null ? config.GetMaxStaminaForLevel(level) : 100;
        }

        /// <summary>
        /// 次のレベルまでの必要経験値を取得
        /// </summary>
        public int GetExpRequiredForNextLevel(GameBalanceManager config)
        {
            if (config == null) return level * 100;

            int currentLevelTotalExp = config.GetTotalExpRequiredForLevel(level);
            int nextLevelTotalExp = config.GetTotalExpRequiredForLevel(level + 1);
            int currentLevelExp = exp - currentLevelTotalExp;
            int requiredForLevel = nextLevelTotalExp - currentLevelTotalExp;

            return requiredForLevel - currentLevelExp;
        }

        /// <summary>
        /// 現在レベル内での経験値進行度を取得 (0.0 ~ 1.0)
        /// </summary>
        public float GetExpProgressRatio(GameBalanceManager config)
        {
            if (config == null) return 0f;

            int currentLevelTotalExp = config.GetTotalExpRequiredForLevel(level);
            int nextLevelTotalExp = config.GetTotalExpRequiredForLevel(level + 1);
            int currentLevelExp = exp - currentLevelTotalExp;
            int requiredForLevel = nextLevelTotalExp - currentLevelTotalExp;

            return requiredForLevel > 0 ? (float)currentLevelExp / requiredForLevel : 0f;
        }

        /// <summary>
        /// スタミナ進行度を取得 (0.0 ~ 1.0)
        /// </summary>
        public float GetStaminaProgressRatio(GameBalanceManager config)
        {
            int maxStamina = GetMaxStamina(config);
            return maxStamina > 0 ? (float)stamina / maxStamina : 0f;
        }
    }
}