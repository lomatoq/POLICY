using System;
using UnityEngine;

namespace Policy.Data
{
    [Serializable]
    public class PrestigeData
    {
        public string title;
        [TextArea(1, 3)] public string description;
        public int   cost;
        public float incomeMultiplier = 2.4f;
    }
}
