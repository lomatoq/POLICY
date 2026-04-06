using System;
using UnityEngine;

namespace Policy.Data
{
    [Serializable]
    public class CardChoice
    {
        [Tooltip("Button label shown in outcome")]
        public string label;

        [Header("State Deltas")]
        public int incomeDelta;
        public int legacyDelta;
        public int riskDelta;
        public int scopeDelta;

        [Header("Narrative")]
        [TextArea(2, 4)] public string policy;       // null = no policy written
        [TextArea(2, 4)] public string collateral;
        [TextArea(1, 2)] public string returnNote;   // for ignore direction

        [Header("Unlock")]
        public string unlocksAssetId;  // e.g. "clinic" — adds to GameState.ownedAssetIds
    }
}
