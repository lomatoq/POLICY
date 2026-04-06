using UnityEngine;
using Policy.Core;

namespace Policy.Systems
{
    /// <summary>
    /// Fires GameEvents.WeekEnded every 55 real seconds and advances the week counter.
    /// </summary>
    public class WeeklyReportSystem : MonoBehaviour
    {
        [SerializeField] private float weekDuration = 55f;

        private GameState State => GameManager.Instance.state;
        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= weekDuration)
            {
                _timer = 0f;
                State.week++;
                State.decisionsThisWeek = 0;
                GameEvents.WeekEnded();
                GameEvents.StateChanged();
            }
        }
    }
}
