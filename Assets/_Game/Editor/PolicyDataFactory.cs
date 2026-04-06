using System.IO;
using UnityEditor;
using UnityEngine;
using Policy.Data;

namespace Policy.Editor
{
    /// <summary>
    /// Creates all ScriptableObject assets from the HTML prototype data.
    /// Menu: POLICY → Create All Data Assets
    /// </summary>
    public static class PolicyDataFactory
    {
        [MenuItem("POLICY/Create All Data Assets")]
        public static void CreateAll()
        {
            EnsureFolders();
            CreateCards();
            CreateAssets();
            CreateMarket();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[POLICY] All data assets created successfully.");
        }

        // ── FOLDERS ────────────────────────────────────────────────
        private static void EnsureFolders()
        {
            CreateFolder("Assets/Data");
            CreateFolder("Assets/Data/Cards");
            CreateFolder("Assets/Data/Assets");
            CreateFolder("Assets/Data/Market");
        }

        private static void CreateFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts  = path.Split('/');
                var parent = string.Join("/", parts[..^1]);
                AssetDatabase.CreateFolder(parent, parts[^1]);
            }
        }

        // ── HELPER ─────────────────────────────────────────────────
        private static T CreateSO<T>(string path) where T : ScriptableObject
        {
            if (File.Exists(Application.dataPath + path.Replace("Assets", "")))
                return AssetDatabase.LoadAssetAtPath<T>(path);

            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, path);
            return so;
        }

        private static CardChoice Choice(string label, int income, int legacy, int risk, int scope,
                                         string policy = null, string collateral = null,
                                         string returnNote = null, string unlocksAsset = null)
            => new()
            {
                label         = label,
                incomeDelta   = income,
                legacyDelta   = legacy,
                riskDelta     = risk,
                scopeDelta    = scope,
                policy        = policy,
                collateral    = collateral,
                returnNote    = returnNote,
                unlocksAssetId = unlocksAsset,
            };

        // ── CARDS ──────────────────────────────────────────────────
        private static void CreateCards()
        {
            // 1. Attendance Policy
            {
                var c = CreateSO<CardData>("Assets/Data/Cards/Card_AttendancePolicy.asset");
                c.type        = CardType.Decision;
                c.from        = "HR Portal · Team Alert";
                c.title       = "Alex has been late 3 times this month. Apply the attendance policy you wrote in week 3?";
                c.context     = "Same rule that deducted $340 from Daniel. You wrote it.";
                c.accentColor = new Color(0.13f, 0.50f, 0.35f);
                c.approve     = Choice("Apply. Rules are rules.",         +340,   -6, +3, +1,
                    policy:      "Attendance breach → automatic bonus deduction. No exceptions.",
                    collateral:  "Alex: −$340. Did not appeal. Stopped attending team socials.");
                c.decline     = Choice("Waive this time.",                 0,    +2,  0,  0,
                    policy:      "Attendance discretion left to manager judgement.",
                    collateral:  "Alex finished the quarter top performer.");
                c.escalate    = Choice("Send to HR committee.",            0,     0, -2, +2,
                    policy:      "Attendance appeals handled by HR, not line managers.",
                    collateral:  "Process takes 3 weeks. Alex waits. Productivity dips.");
                c.ignore      = Choice("File it. Deal later.",             0,     0, +5,  0,
                    returnNote:  "Returns week 12 — now 3 employees affected.");
                EditorUtility.SetDirty(c);
            }

            // 2. Health Clinic Investment
            {
                var c = CreateSO<CardData>("Assets/Data/Cards/Card_HealthClinic.asset");
                c.type        = CardType.Investment;
                c.from        = "Market Opportunity · Healthcare";
                c.title       = "Acquire 40% stake in HealthFirst Private Clinic.";
                c.context     = "$8,000 upfront. +$142/hr passive. Decisions involve patient care vs margins.";
                c.accentColor = new Color(0.10f, 0.43f, 0.34f);
                c.approve     = Choice("Acquire.",        +142,   0, +8, +50,
                    collateral:    "340 patients under your policy framework. First decision arrives in 3 days.",
                    unlocksAsset:  "clinic");
                c.decline     = Choice("Pass.",              0,   0,  0,   0,
                    collateral:    "Competitor acquired it instead. Now worth $14K.");
                c.escalate    = Choice("Request full audit first.", 0, +2, -3, 0,
                    collateral:    "Audit reveals 12 malpractice claims. You decline. Wise.");
                c.ignore      = Choice("Maybe later.",       0,   0,  0,   0,
                    returnNote:    "Returns week 11 — price is now $11,000.");
                EditorUtility.SetDirty(c);
            }

            // 3. Availability Tracking Policy
            {
                var c = CreateSO<CardData>("Assets/Data/Cards/Card_AvailabilityTracking.asset");
                c.type        = CardType.Policy;
                c.from        = "HR Team Proposal";
                c.title       = "Implement mandatory 6am–10pm availability tracking for all staff.";
                c.context     = "Estimated +$24/hr efficiency gain. 47 employees affected.";
                c.accentColor = new Color(0.47f, 0.47f, 0.87f);
                c.approve     = Choice("Implement.",         +24, -14, +12, +47,
                    policy:      "All staff must maintain active status 6am–10pm weekdays.",
                    collateral:  "14 employees began job searching week 1. 3 resigned by month end.");
                c.decline     = Choice("Reject.",              0,  +4,  -2,   0,
                    policy:      "Extended availability tracking: rejected.",
                    collateral:  "Team morale +8 in next survey. HR notes 'unusual positive trend.'");
                c.escalate    = Choice("Trial with volunteers only.", +8, +1, +2, +6,
                    policy:      "Voluntary availability tracking pilot — 6 participants.",
                    collateral:  "Results inconclusive. Pilot becomes permanent for volunteers.");
                c.ignore      = Choice("Table it.",            0,   0,  +2,   0,
                    returnNote:  "Returns week 10 — now mandatory per new company directive.");
                EditorUtility.SetDirty(c);
            }

            // 4. AI Layoffs Event
            {
                var c = CreateSO<CardData>("Assets/Data/Cards/Card_AILayoffs.asset");
                c.type        = CardType.Event;
                c.from        = "Breaking · Market Event";
                c.title       = "AI replaced 12% of your department's workload overnight. Board wants you to act.";
                c.context     = "Option to eliminate 3 roles. Save $180K/yr. Or retrain. Or ignore report.";
                c.accentColor = new Color(0.44f, 0.17f, 0.07f);
                c.approve     = Choice("Eliminate the 3 roles.",          +180, -20, +15, +3,
                    policy:      "AI efficiency gains to be passed to shareholders, not retained staff.",
                    collateral:  "Maria, James, and Priya 2.0 received letters Friday. Effective 30 days.");
                c.decline     = Choice("Retrain the team.",                -20, +10,  -5, +3,
                    policy:      "AI displacement must include retraining programme.",
                    collateral:  "6-month program. 2 of 3 successfully upskilled. 1 left voluntarily.");
                c.escalate    = Choice("Propose hybrid: 1 role + retraining.", +60, -4, +4, +3,
                    policy:      "AI transitions to follow 1/3 elimination, 2/3 retraining ratio.",
                    collateral:  "Board satisfied. Team partially satisfied. Press calls it 'pragmatic.'");
                c.ignore      = Choice("Delay the decision.",               0,   0,  +8,  0,
                    returnNote:  "Returns week 11 — board now demands full elimination.");
                EditorUtility.SetDirty(c);
            }

            // 5. Whistleblower Event
            {
                var c = CreateSO<CardData>("Assets/Data/Cards/Card_Whistleblower.asset");
                c.type        = CardType.Event;
                c.from        = "Anonymous · Encrypted";
                c.title       = "Someone inside your clinic leaked patient data usage to a journalist. Story drops tomorrow.";
                c.context     = "You can get ahead of it, or wait and see.";
                c.accentColor = new Color(0.24f, 0.21f, 0.54f);
                c.approve     = Choice("Issue statement. Cooperate fully.", -40000, +18, -20, 0,
                    policy:      "Proactive disclosure standard for any data incident.",
                    collateral:  "TechCrunch: 'Rare corporate transparency.' Clinic reputation +12.");
                c.decline     = Choice("Lawyer up. Say nothing.",                0, -12, +25, 0,
                    policy:      "All media inquiries handled exclusively by legal.",
                    collateral:  "Story runs. 'Company refuses comment.' −18% patient trust.");
                c.escalate    = Choice("Find the leaker. Fire them first.",    -8000, -22, +10, 0,
                    policy:      "Unauthorised disclosure grounds for immediate termination.",
                    collateral:  "Leaker identified and fired. Second journalist picked up the story.");
                c.ignore      = Choice("Hope it goes away.",                     0,   0, +18, 0,
                    returnNote:  "Returns in 48 hours with a documentary crew.");
                EditorUtility.SetDirty(c);
            }

            // 6. Meme Coin Investment
            {
                var c = CreateSO<CardData>("Assets/Data/Cards/Card_MemeCoin.asset");
                c.type        = CardType.Investment;
                c.from        = "Market Alert · Crypto";
                c.title       = "MEME token pump underway. 2,140 retail holders. Peak estimated in 4 hours.";
                c.context     = "Dump now: +$28K. Hold: risk −60% if peak passes.";
                c.accentColor = new Color(0.39f, 0.22f, 0.02f);
                c.approve     = Choice("Dump now. Take the $28K.",           +28000, -22, +8, +2140,
                    collateral:  "2,140 holders averaged −$847 loss. Your gain: $28,000.");
                c.decline     = Choice("Don't touch it. Integrity intact.",       0,  +5, -3,     0,
                    collateral:  "Token crashed anyway. You avoided both profit and blame.");
                c.escalate    = Choice("Sell 50%, hold 50%.",                +14000, -10, +4, +1070,
                    collateral:  "Net: +$14K. 1,070 holders affected at −$423 average.");
                c.ignore      = Choice("Watch and wait.",                         0,   0, +10,    0,
                    returnNote:  "Returns at peak. Same choice. Less time.");
                EditorUtility.SetDirty(c);
            }

            // 7. Prestige — Director Offer
            {
                var c = CreateSO<CardData>("Assets/Data/Cards/Card_PrestigeDirector.asset");
                c.type        = CardType.Prestige;
                c.from        = "Board of Directors · Confidential";
                c.title       = "Director role offered. Resign your current position. Scope: 200 people. Income ×2.4.";
                c.context     = "Your policies travel with you. Your legacy score does too.";
                c.accentColor = new Color(0.96f, 0.62f, 0.07f);
                c.approve     = Choice("Accept. Go bigger.",           0,  0,  0, +200,
                    collateral:  "PRESTIGE — income ×2.4. New decisions arrive at scale.");
                c.decline     = Choice("Stay. Consolidate.",         +20, +4, -5,    0,
                    collateral:  "You optimised your current role. Stable growth.");
                c.escalate    = Choice("Negotiate: keep current team.", 0, +2,  0, +100,
                    collateral:  "Board agrees. Half the scope. 60% of the multiplier.");
                c.ignore      = Choice("Think about it.",              0,  0,  0,    0,
                    returnNote:  "Offer expires week 13. Someone else accepts it.");
                EditorUtility.SetDirty(c);
            }
        }

        // ── PORTFOLIO ASSETS ───────────────────────────────────────
        private static void CreateAssets()
        {
            // Team Lead
            {
                var a = CreateSO<AssetData>("Assets/Data/Assets/Asset_TeamLead.asset");
                a.assetId      = "team-lead";
                a.displayName  = "Team Lead Role";
                a.subtitle     = "4 direct reports · NeuralFlow Inc.";
                a.incomeLabel  = "+$68/hr";
                a.iconBackground = new Color(0.90f, 0.95f, 0.98f);
                a.iconEmoji    = "💼";
                a.upgrades = new System.Collections.Generic.List<UpgradeData>
                {
                    new() { id = "u1", upgradeName = "Performance reviews",  effectDescription = "+$24/hr",            cost = 2000, incomeDelta = 24,  legacyDelta = 0,   available = true  },
                    new() { id = "u2", upgradeName = "Remote tracking",      effectDescription = "+$18/hr · −8 legacy", cost = 4500, incomeDelta = 18,  legacyDelta = -8,  available = false },
                    new() { id = "u3", upgradeName = "Mandatory overtime",   effectDescription = "+$44/hr · −22 legacy",cost = 8000, incomeDelta = 44,  legacyDelta = -22, available = false },
                };
                a.staff = new System.Collections.Generic.List<StaffData>
                {
                    new() { role = "Analyst",     count = 2, hireCost = 800,  locked = false },
                    new() { role = "Coordinator", count = 1, hireCost = 1200, locked = false },
                    new() { role = "Intern",      count = 1, hireCost = 200,  locked = false },
                    new() { role = "Compliance",  count = 0, hireCost = 3000, locked = true  },
                };
                a.prestige = new PrestigeData
                {
                    title            = "Promotion to Director",
                    description      = "Reset upgrades. Keep legacy. Income ×2.4.",
                    cost             = 20000,
                    incomeMultiplier = 2.4f,
                };
                EditorUtility.SetDirty(a);
            }

            // HealthFirst Clinic
            {
                var a = CreateSO<AssetData>("Assets/Data/Assets/Asset_HealthClinic.asset");
                a.assetId      = "clinic";
                a.displayName  = "HealthFirst Clinic";
                a.subtitle     = "340 patients · 94% occupancy";
                a.incomeLabel  = "+$142/hr";
                a.iconBackground = new Color(0.88f, 0.96f, 0.93f);
                a.iconEmoji    = "🏥";
                a.upgrades = new System.Collections.Generic.List<UpgradeData>
                {
                    new() { id = "uc1", upgradeName = "Premium upsell",   effectDescription = "+$58/hr",             cost = 5000, incomeDelta = 58, legacyDelta = 0,   available = true  },
                    new() { id = "uc2", upgradeName = "Budget protocol",  effectDescription = "+$84/hr · −18 legacy", cost = 9000, incomeDelta = 84, legacyDelta = -18, available = false },
                };
                a.staff = new System.Collections.Generic.List<StaffData>
                {
                    new() { role = "Nurse",    count = 4, hireCost = 1500,  locked = false },
                    new() { role = "Admin",    count = 2, hireCost = 900,   locked = false },
                    new() { role = "Lobbyist", count = 0, hireCost = 12000, locked = true  },
                };
                a.prestige = new PrestigeData
                {
                    title            = "Acquire competitor",
                    description      = "Monopolize market. ×3.2.",
                    cost             = 50000,
                    incomeMultiplier = 3.2f,
                };
                EditorUtility.SetDirty(a);
            }
        }

        // ── MARKET ITEMS ───────────────────────────────────────────
        private static void CreateMarket()
        {
            MakeMarket("Market_HealthClinic",
                "clinic", "HealthFirst Clinic", "Healthcare",
                "Private healthcare provider. Decisions: patient care vs profit.",
                8000, "+$142/hr", new Color(0.88f, 0.96f, 0.93f), "🏥",
                false, 0f);

            MakeMarket("Market_MemeCoin",
                "meme", "Meme Coin Fund", "Crypto · High Risk",
                "Launch and manage a memecoin. Comes with its own swipe card.",
                3000, "volatile", new Color(0.99f, 0.95f, 0.88f), "📈",
                false, 0f);

            MakeMarket("Market_DataVault",
                "data", "DataVault Inc.", "Data · Privacy",
                "Data brokerage. Trade user data to advertisers. Unlocks at $60K.",
                15000, "+$280/hr", new Color(0.92f, 0.95f, 0.98f), "🗄",
                true, 60000f);

            MakeMarket("Market_ReachMedia",
                "media", "Reach Media", "Media",
                "Algorithm, controversial content, advertiser pressure. Unlocks at $100K.",
                25000, "+$420/hr", new Color(0.95f, 0.91f, 1.00f), "📻",
                true, 100000f);
        }

        private static void MakeMarket(string fileName, string assetId, string displayName, string sector,
                                        string description, int price, string incomeLabel,
                                        Color iconBg, string emoji, bool hasLock, float unlockAt)
        {
            var m = CreateSO<MarketItemData>($"Assets/Data/Market/{fileName}.asset");
            m.assetId           = assetId;
            m.displayName       = displayName;
            m.sector            = sector;
            m.description       = description;
            m.price             = price;
            m.incomeLabel       = incomeLabel;
            m.iconBackground    = iconBg;
            m.iconEmoji         = emoji;
            m.hasLockCondition  = hasLock;
            m.unlockAtBalance   = unlockAt;
            EditorUtility.SetDirty(m);
        }
    }
}
