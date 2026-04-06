using UnityEngine;
using Policy.Core;

namespace Policy.Systems
{
    /// <summary>
    /// Ticks balance every 100ms based on incomePerSec.
    /// Fires GameEvents.StateChanged so UI updates.
    /// </summary>
    public class IncomeTickerSystem : MonoBehaviour
    {
        private const float TickInterval = 0.1f;

        private GameState State => GameManager.Instance.state;

        private void Start()
        {
            InvokeRepeating(nameof(Tick), TickInterval, TickInterval);
        }

        private void Tick()
        {
            var s = State;
            float earned = s.incomePerSec * TickInterval;
            s.balance     += earned;
            s.weekEarned  += earned;
            GameEvents.StateChanged();
        }
    }
}
