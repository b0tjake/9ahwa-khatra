using UnityEngine;
using QahwaKhatra.Utils;

namespace QahwaKhatra.Core
{
    public enum GameState
    {
        TutorialCleaning,
        CafeOpen,
        CraftingMiniGame,
        DayEndSummary,
        ShopMenu,
        Paused
    }

    public class GameManager : Singleton<GameManager>
    {
        [Header("State")]
        [SerializeField] private GameState _currentState = GameState.TutorialCleaning;

        [Header("Mobile Target Framerate")]
        [SerializeField] private int _targetFPS = 60;

        public GameState CurrentState => _currentState;

        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = _targetFPS;
            QualitySettings.vSyncCount = 0;
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
        }
    }
}
