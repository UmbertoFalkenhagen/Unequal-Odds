using UnityEngine;
using UnequalOdds.Gameplay;   // where PlayerProfile lives

namespace UnequalOdds.Runtime
{
    /// <summary>
    /// Holds the current run's data across scenes (singleton + DontDestroyOnLoad).
    /// </summary>
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public PlayerProfile CurrentProfile { get; set; }

        private void Awake()
        {
            // classic singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}