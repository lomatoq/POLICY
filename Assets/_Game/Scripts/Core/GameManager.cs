using UnityEngine;
using Policy.Core;

namespace Policy.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] public GameState state;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartGame()
        {
            state.Reset();
            GameEvents.GameStarted();
            GameEvents.StateChanged();
        }
    }
}
