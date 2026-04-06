using System;
using UnityEngine;

namespace Policy.Data
{
    [Serializable]
    public class UpgradeData
    {
        public string id;
        public string upgradeName;
        [TextArea(1, 2)] public string effectDescription;
        public int cost;
        public int incomeDelta;
        public int legacyDelta;

        [HideInInspector] public bool bought  = false;
        [HideInInspector] public bool available = false;
    }
}
