using System.Collections.Generic;
using UnityEngine;

namespace Policy.Core
{
    [CreateAssetMenu(menuName = "POLICY/GameState", fileName = "GameState")]
    public class GameState : ScriptableObject
    {
        [Header("Economy")]
        public float balance       = 48240f;
        public float incomePerHour = 68f;
        public float incomePerSec  => incomePerHour / 3600f;

        [Header("Meters")]
        [Range(0, 100)] public int legacy = 71;
        [Range(0, 100)] public int risk   = 28;
        [Range(0, 100)] public int scope  = 8;

        [Header("Progress")]
        public int week                = 9;
        public int staff               = 4;
        public int affected            = 4;
        public int decisionsThisWeek   = 0;
        public float weekEarned        = 8400f;

        [Header("Owned")]
        public List<string> ownedAssetIds  = new() { "team-lead" };
        public List<string> activePolicies = new();
        public List<string> collateralLog  = new();

        public string GetLevel()
        {
            if (balance < 10000)  return "Team Lead";
            if (balance < 30000)  return "Senior Manager";
            if (balance < 80000)  return "Director";
            if (balance < 200000) return "VP";
            return "C-Suite";
        }

        public void Reset()
        {
            balance           = 48240f;
            incomePerHour     = 68f;
            legacy            = 71;
            risk              = 28;
            scope             = 8;
            week              = 9;
            staff             = 4;
            affected          = 4;
            decisionsThisWeek = 0;
            weekEarned        = 8400f;
            ownedAssetIds     = new List<string> { "team-lead" };
            activePolicies    = new List<string>();
            collateralLog     = new List<string>();
        }
    }
}
