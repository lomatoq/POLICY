using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Policy.Core;
using Policy.Systems;
using Policy.Data;

namespace Policy.UI
{
    /// <summary>
    /// Main game screen: topbar, tabs, and tab-content panels.
    /// Refreshes all elements on GameEvents.OnStateChanged.
    /// </summary>
    public class GameScreen : MonoBehaviour
    {
        [SerializeField] private UIDocument    uiDocument;
        [SerializeField] private AssetData[]   assetCatalog;
        [SerializeField] private MarketItemData[] marketCatalog;

        private VisualElement _root;

        // Topbar
        private Label _tbBal, _tbRate, _tbLeg, _tbLevel;

        // Tabs
        private readonly string[] TabNames = { "portfolio", "market", "policy" };
        private string _activeTab = "portfolio";

        // Portfolio
        private VisualElement _assetList;

        // Market
        private VisualElement _marketList;

        // Policy
        private Label         _policyCount;
        private VisualElement _policyBody;

        private GameState State => GameManager.Instance.state;

        private void OnEnable()
        {
            _root = uiDocument.rootVisualElement;

            _tbBal   = _root.Q<Label>("tb-bal");
            _tbRate  = _root.Q<Label>("tb-rate");
            _tbLeg   = _root.Q<Label>("tb-leg");
            _tbLevel = _root.Q<Label>("tb-level");

            _assetList   = _root.Q("asset-list");
            _marketList  = _root.Q("market-list");
            _policyCount = _root.Q<Label>("policy-count");
            _policyBody  = _root.Q("policy-body");

            // Tab buttons
            foreach (var t in TabNames)
            {
                string captured = t;
                var btn = _root.Q<Button>($"tab-{t}");
                if (btn != null) btn.clicked += () => SwitchTab(captured);
            }

            var decBtn = _root.Q<Button>("tab-decisions");
            if (decBtn != null) decBtn.clicked += () => ScreenManager.Instance.ShowSwipe();

            GameEvents.OnStateChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnStateChanged -= Refresh;
        }

        private void SwitchTab(string name)
        {
            _activeTab = name;
            foreach (var t in TabNames)
            {
                var btn     = _root.Q<Button>($"tab-{t}");
                var content = _root.Q($"tab-content-{t}");
                bool active = t == name;
                btn?.EnableInClassList("tab--active", active);
                content?.EnableInClassList("tab-content--active", active);
                content?.EnableInClassList("tab-content--hidden",  !active);
            }
        }

        private void Refresh()
        {
            var s = State;
            if (_tbBal  != null) _tbBal.text  = $"${Mathf.RoundToInt(s.balance):N0}";
            if (_tbRate != null) _tbRate.text  = $"+${Mathf.RoundToInt(s.incomePerHour)}/hr";
            if (_tbLeg  != null)
            {
                _tbLeg.text = s.legacy.ToString();
                _tbLeg.RemoveFromClassList("leg--green");
                _tbLeg.RemoveFromClassList("leg--amber");
                _tbLeg.RemoveFromClassList("leg--red");
                _tbLeg.AddToClassList(s.legacy >= 70 ? "leg--green" : s.legacy >= 40 ? "leg--amber" : "leg--red");
            }
            if (_tbLevel != null) _tbLevel.text = $"{s.GetLevel()} · Week {s.week}";

            RefreshAssets();
            RefreshMarket();
            RefreshPolicy();
        }

        // ── PORTFOLIO ──────────────────────────────────────────────
        private void RefreshAssets()
        {
            if (_assetList == null) return;
            _assetList.Clear();
            foreach (var id in State.ownedAssetIds)
            {
                var data = System.Array.Find(assetCatalog, a => a.assetId == id);
                if (data == null) continue;
                var card = BuildAssetCard(data);
                _assetList.Add(card);
            }
        }

        private VisualElement BuildAssetCard(AssetData a)
        {
            var root = new VisualElement();
            root.AddToClassList("asset-card");

            var row = new VisualElement(); row.AddToClassList("ac-row");

            var icon = new Label(a.iconEmoji); icon.AddToClassList("ac-icon");
            icon.style.backgroundColor = a.iconBackground;

            var info = new VisualElement(); info.AddToClassList("ac-info");
            var name = new Label(a.displayName); name.AddToClassList("ac-name");
            var sub  = new Label(a.subtitle);    sub.AddToClassList("ac-sub");
            info.Add(name); info.Add(sub);

            var right  = new VisualElement(); right.AddToClassList("ac-right");
            var val    = new Label($"${Mathf.RoundToInt(GameManager.Instance.state.balance):N0}");
            val.AddToClassList("ac-val");
            var chg = new Label(a.incomeLabel); chg.AddToClassList("ac-chg"); chg.AddToClassList("chg-g");
            right.Add(val); right.Add(chg);

            row.Add(icon); row.Add(info); row.Add(right);
            root.Add(row);
            return root;
        }

        // ── MARKET ─────────────────────────────────────────────────
        private void RefreshMarket()
        {
            if (_marketList == null) return;
            _marketList.Clear();
            foreach (var item in marketCatalog)
            {
                bool owned  = State.ownedAssetIds.Contains(item.assetId);
                bool locked = item.hasLockCondition && State.balance < item.unlockAtBalance;
                _marketList.Add(BuildMarketCard(item, owned, locked));
            }
        }

        private VisualElement BuildMarketCard(MarketItemData item, bool owned, bool locked)
        {
            var root = new VisualElement(); root.AddToClassList("market-card");

            var top  = new VisualElement(); top.AddToClassList("mc-top");
            var icon = new Label(item.iconEmoji); icon.AddToClassList("mc-icon");
            icon.style.backgroundColor = item.iconBackground;

            var info = new VisualElement(); info.AddToClassList("mc-info");
            new Label(item.displayName) { }.AddToClassList("mc-name"); // inline style

            var nameLabel   = new Label(item.displayName);   nameLabel.AddToClassList("mc-name");
            var sectorLabel = new Label(item.sector);         sectorLabel.AddToClassList("mc-sector");
            info.Add(nameLabel); info.Add(sectorLabel);

            var priceCol = new VisualElement();
            var priceVal = new Label($"${item.price:N0}"); priceVal.AddToClassList("mc-price-val");
            priceCol.Add(priceVal);

            top.Add(icon); top.Add(info); top.Add(priceCol);

            var desc = new Label(locked ? $"{item.description} Unlocks at ${item.unlockAtBalance / 1000:0}K" : item.description);
            desc.AddToClassList("mc-desc");

            var footer = new VisualElement(); footer.AddToClassList("mc-footer");
            var incLabel = new Label(item.incomeLabel); incLabel.AddToClassList("mc-inc");

            var btn = new Button(); btn.AddToClassList("mc-buy");
            if (owned)        { btn.text = "Owned";   btn.AddToClassList("mc-buy--owned");  btn.SetEnabled(false); }
            else if (locked)  { btn.text = "Locked";  btn.AddToClassList("mc-buy--locked"); btn.SetEnabled(false); }
            else              { btn.text = "Acquire"; btn.clicked += () => AcquireAsset(item); }

            footer.Add(incLabel); footer.Add(btn);
            root.Add(top); root.Add(desc); root.Add(footer);
            return root;
        }

        private void AcquireAsset(MarketItemData item)
        {
            var s = State;
            if (s.balance < item.price) { GameEvents.Toast("Insufficient balance"); return; }
            if (s.ownedAssetIds.Contains(item.assetId)) { GameEvents.Toast("Already owned"); return; }
            s.balance -= item.price;
            s.ownedAssetIds.Add(item.assetId);
            s.affected += 80;
            GameEvents.StateChanged();
            GameEvents.Toast($"{item.displayName} acquired!");
        }

        // ── POLICY ─────────────────────────────────────────────────
        private void RefreshPolicy()
        {
            if (_policyBody == null) return;
            var policies = PolicySystem.Instance?.Policies;
            int total    = PolicySystem.Instance?.TotalApplications() ?? 0;
            if (_policyCount != null) _policyCount.text = total.ToString("N0");

            _policyBody.Clear();
            if (policies == null || policies.Count == 0)
            {
                var empty = new Label("Swipe decisions to write policies.");
                empty.AddToClassList("policy-empty");
                _policyBody.Add(empty);
                return;
            }

            foreach (var p in policies)
            {
                var clause = new VisualElement(); clause.AddToClassList("clause-item");

                var text    = new Label(p.text); text.AddToClassList("ci-text");
                var stats   = new VisualElement(); stats.AddToClassList("ci-stats");
                var applied = new Label($"Applied: {p.applicationCount:N0}×"); applied.AddToClassList("ci-s");

                var track = new VisualElement(); track.AddToClassList("ci-bar");
                var fill  = new VisualElement(); fill.AddToClassList("ci-fill");
                float pct = Mathf.Min(92f, 15f + p.applicationCount * 3f);
                fill.style.width = Length.Percent(pct);
                track.Add(fill);

                stats.Add(applied); stats.Add(track);
                clause.Add(text); clause.Add(stats);
                _policyBody.Add(clause);
            }
        }
    }
}
