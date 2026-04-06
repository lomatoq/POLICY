using System.Collections.Generic;
using UnityEngine;

namespace Policy.Data
{
    [CreateAssetMenu(menuName = "POLICY/Asset", fileName = "Asset_New")]
    public class AssetData : ScriptableObject
    {
        [Header("Identity")]
        public string   assetId;
        public string   displayName;
        [TextArea(1, 2)] public string subtitle;
        public string   incomeLabel;      // e.g. "+$68/hr"
        public Color    iconBackground    = Color.white;
        public string   iconEmoji;        // fallback until real art

        [Header("Upgrades")]
        public List<UpgradeData> upgrades = new();

        [Header("Staff")]
        public List<StaffData> staff = new();

        [Header("Prestige")]
        public PrestigeData prestige;

        public void InitRuntime()
        {
            for (int i = 0; i < upgrades.Count; i++)
            {
                upgrades[i].bought    = false;
                upgrades[i].available = i == 0;
            }
        }
    }
}
