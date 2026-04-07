using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using Policy.Core;
using Policy.Systems;
using Policy.UI;
using Policy.Data;

namespace Policy.Editor
{
    /// <summary>
    /// POLICY → Build Scene
    /// Полностью строит сцену: создаёт префабы, GameObjects, компоненты, прописывает все ссылки.
    /// Безопасно запускать повторно — удаляет старое и строит заново.
    /// </summary>
    public static class SceneBuilder
    {
        const string PrefabsPath  = "Assets/Prefabs";
        const string SettingsPath = "Assets/Data/Settings";

        // ─────────────────────────────────────────────────────────
        // ENTRY POINT
        // ─────────────────────────────────────────────────────────

        [MenuItem("POLICY/Build Scene")]
        public static void BuildScene()
        {
            try { BuildSceneInternal(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[POLICY] Build Scene failed: {e}");
                EditorUtility.DisplayDialog("POLICY — Build Error", e.Message, "OK");
            }
        }

        static void BuildSceneInternal()
        {
            // Ensure data assets exist first
            if (LoadAll<CardData>("Assets/Data/Cards").Length == 0)
            {
                bool ok = EditorUtility.DisplayDialog("POLICY",
                    "No card data found.\nRun POLICY → Create All Data Assets first?", "Yes, create them", "Skip");
                if (ok) PolicyDataFactory.CreateAll();
            }

            EnsureFolders();

            // Build prefabs
            var cardPrefab       = BuildCardPrefab();
            var collRowPrefab    = BuildCollateralRowPrefab();
            var effectRowPrefab  = BuildEffectRowPrefab();
            var panelSettings    = EnsurePanelSettings();
            var gameStateAsset   = EnsureGameState();

            // Clean old scene objects
            CleanOld("POLICY_GameManager", "POLICY_Systems", "POLICY_UIDocument", "POLICY_Canvas", "POLICY_EventSystem", "SwipeCard");

            // Build hierarchy
            var gmGO     = BuildGameManager(gameStateAsset);
            var sysGO    = BuildSystems();
            var uiDocGO  = BuildUIDocument(panelSettings);
            var canvasGO = BuildCanvas();
            var deckGO   = BuildCardDeckView(canvasGO.transform, cardPrefab, sysGO.GetComponent<CardDeckSystem>());
            var flashGO  = BuildOutcomeFlash(canvasGO.transform, effectRowPrefab);
            var reportGO = BuildWeeklyReport(canvasGO.transform, collRowPrefab);
            var toastGO  = BuildToast(canvasGO.transform);
            BuildEventSystem();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            Debug.Log("[POLICY] ✓ Scene built successfully. Press Ctrl+S to save.");
            EditorUtility.DisplayDialog("POLICY", "Scene built!\n\nCtrl+S to save the scene.", "OK");
        } // BuildSceneInternal

        // ─────────────────────────────────────────────────────────
        // PREFAB: SwipeCard
        // ─────────────────────────────────────────────────────────

        static GameObject BuildCardPrefab()
        {
            const string path = PrefabsPath + "/SwipeCard.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                AssetDatabase.DeleteAsset(path);

            // Root — card
            var root = new GameObject("SwipeCard", typeof(RectTransform));
            var rootRT = root.GetComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(340, 320);

            var rootCG  = root.AddComponent<CanvasGroup>();
            var rootBg  = root.AddComponent<RoundedImage>();
            rootBg.color        = Color.white;
            rootBg.cornerRadius = 18f;
            rootBg.outline      = true;
            rootBg.outlineColor = new Color(0.87f, 0.85f, 0.82f);
            rootBg.outlineWidth = 1f;

            var swipeView = root.AddComponent<SwipeCardView>();

            // Content container
            var content = Child(root, "Content");
            Stretch(RT(content), 24, 24, 24, 24);
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = vlg.childForceExpandWidth = true;
            vlg.childControlHeight = vlg.childForceExpandHeight = false;
            vlg.spacing = 8f;

            // TypeLabel
            var typeGO  = Child(content, "TypeLabel");
            RT(typeGO).sizeDelta = new Vector2(0, 16);
            var typeTMP = MakeTMP(typeGO, "Decision", 9f, new Color(0.13f, 0.50f, 0.35f));

            // FromLabel
            var fromGO  = Child(content, "FromLabel");
            RT(fromGO).sizeDelta = new Vector2(0, 14);
            var fromTMP = MakeTMP(fromGO, "Source · Context", 10f, new Color(0.53f, 0.53f, 0.53f));

            // TitleLabel (flex)
            var titleGO = Child(content, "TitleLabel");
            RT(titleGO).sizeDelta = new Vector2(0, 80);
            titleGO.AddComponent<LayoutElement>().flexibleHeight = 1f;
            var titleTMP = MakeTMP(titleGO, "Decision title goes here", 15f, new Color(0.07f, 0.07f, 0.07f));

            // Context bg + label
            var ctxBg = Child(content, "ContextBg");
            RT(ctxBg).sizeDelta = new Vector2(0, 60);
            var ctxImg = ctxBg.AddComponent<RoundedImage>();
            ctxImg.color = new Color(0.96f, 0.95f, 0.94f); ctxImg.cornerRadius = 8f;
            var ctxPad = ctxBg.AddComponent<VerticalLayoutGroup>();
            ctxPad.padding = new RectOffset(11, 11, 9, 9);
            ctxPad.childControlWidth = ctxPad.childForceExpandWidth = true;
            ctxPad.childControlHeight = ctxPad.childForceExpandHeight = false;
            var ctxGO  = Child(ctxBg, "ContextLabel");
            RT(ctxGO).sizeDelta = new Vector2(0, 42);
            var ctxTMP = MakeTMP(ctxGO, "Context description", 11f, new Color(0.53f, 0.53f, 0.53f));

            // Overlays (stretch absolute, outside layout)
            var (ovL, ovLcg) = BuildOverlay(root, "OverlayLeft",
                new Color(0.75f, 0.22f, 0.17f, 0.12f), new Color(0.75f, 0.22f, 0.17f), "←", "Decline");
            var (ovR, ovRcg) = BuildOverlay(root, "OverlayRight",
                new Color(0.10f, 0.50f, 0.35f, 0.12f), new Color(0.10f, 0.50f, 0.35f), "→", "Approve");
            var (ovU, ovUcg) = BuildOverlay(root, "OverlayUp",
                new Color(0.72f, 0.41f, 0.04f, 0.12f), new Color(0.72f, 0.41f, 0.04f), "↑", "Escalate");
            var (ovD, ovDcg) = BuildOverlay(root, "OverlayDown",
                new Color(0.40f, 0.40f, 0.40f, 0.10f), new Color(0.53f, 0.53f, 0.53f), "↓", "Ignore");

            // Wire SwipeCardView fields BEFORE saving prefab
            Wire(swipeView, "canvasGroup",   rootCG);
            Wire(swipeView, "rectTransform", rootRT);
            Wire(swipeView, "typeLabel",     typeTMP);
            Wire(swipeView, "fromLabel",     fromTMP);
            Wire(swipeView, "titleLabel",    titleTMP);
            Wire(swipeView, "contextLabel",  ctxTMP);
            Wire(swipeView, "overlayLeft",   ovLcg);
            Wire(swipeView, "overlayRight",  ovRcg);
            Wire(swipeView, "overlayUp",     ovUcg);
            Wire(swipeView, "overlayDown",   ovDcg);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            AssetDatabase.Refresh();
            Debug.Log("[POLICY] SwipeCard prefab saved.");
            return prefab;
        }

        static (GameObject go, CanvasGroup cg) BuildOverlay(GameObject parent, string name,
            Color bgColor, Color borderColor, string arrow, string label)
        {
            var ov   = Child(parent, name);
            var ovRT = RT(ov);
            Stretch(ovRT);
            var le  = ov.AddComponent<LayoutElement>(); le.ignoreLayout = true;
            var img = ov.AddComponent<RoundedImage>();
            img.color = bgColor; img.cornerRadius = 18f;
            img.outline = true; img.outlineColor = borderColor; img.outlineWidth = 2f;
            var cg  = ov.AddComponent<CanvasGroup>(); cg.alpha = 0f;

            var inner = Child(ov, "Inner");
            Stretch(RT(inner));
            var vg = inner.AddComponent<VerticalLayoutGroup>();
            vg.childAlignment = TextAnchor.MiddleCenter;
            vg.childControlWidth = vg.childControlHeight = false;
            vg.childForceExpandWidth = vg.childForceExpandHeight = false;
            vg.spacing = 4f;

            var arrowGO = Child(inner, "Arrow");
            RT(arrowGO).sizeDelta = new Vector2(200, 40);
            var at = MakeTMP(arrowGO, arrow, 30f, borderColor);
            at.alignment = TextAlignmentOptions.Center;

            var labelGO = Child(inner, "Label");
            RT(labelGO).sizeDelta = new Vector2(200, 22);
            var lt = MakeTMP(labelGO, label, 14f, borderColor);
            lt.alignment  = TextAlignmentOptions.Center;
            lt.fontStyle = FontStyles.Bold;

            return (ov, cg);
        }

        // ─────────────────────────────────────────────────────────
        // PREFAB: EffectRow (outcome flash effects)
        // ─────────────────────────────────────────────────────────

        static GameObject BuildEffectRowPrefab()
        {
            const string path = PrefabsPath + "/EffectRow.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) AssetDatabase.DeleteAsset(path);

            // Root: RoundedImage background only (no TMP on same GO — two Graphics conflict)
            var go  = new GameObject("EffectRow", typeof(RectTransform));
            RT(go).sizeDelta = new Vector2(248, 28);
            var bg = go.AddComponent<RoundedImage>();
            bg.color = new Color(0.07f, 0.07f, 0.07f); bg.cornerRadius = 6f;

            // Child "Label": TMP here (NOT on root) — two Graphic components on same GO crash TMP
            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(go.transform, false);
            Stretch(RT(lblGO));
            var tmp = lblGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 11f;
            tmp.color    = new Color(0.47f, 0.47f, 0.47f);
            tmp.margin   = new Vector4(8, 5, 8, 5);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ─────────────────────────────────────────────────────────
        // PREFAB: CollateralRow (weekly report rows)
        // ─────────────────────────────────────────────────────────

        static TextMeshProUGUI BuildCollateralRowPrefab()
        {
            const string path = PrefabsPath + "/CollateralRow.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) AssetDatabase.DeleteAsset(path);

            var go  = new GameObject("CollateralRow", typeof(RectTransform));
            RT(go).sizeDelta = new Vector2(300, 22);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 9f; tmp.color = new Color(0.27f, 0.27f, 0.27f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<TextMeshProUGUI>();
        }

        // ─────────────────────────────────────────────────────────
        // ASSETS
        // ─────────────────────────────────────────────────────────

        static PanelSettings EnsurePanelSettings()
        {
            const string path = SettingsPath + "/PolicyPanelSettings.asset";
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (ps != null) return ps;
            ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.scaleMode         = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(390, 844);
            AssetDatabase.CreateAsset(ps, path);
            return ps;
        }

        static GameState EnsureGameState()
        {
            const string path = SettingsPath + "/GameState.asset";
            var gs = AssetDatabase.LoadAssetAtPath<GameState>(path);
            if (gs != null) return gs;
            gs = ScriptableObject.CreateInstance<GameState>();
            AssetDatabase.CreateAsset(gs, path);
            AssetDatabase.SaveAssets();
            return gs;
        }

        // ─────────────────────────────────────────────────────────
        // SCENE OBJECTS
        // ─────────────────────────────────────────────────────────

        static GameObject BuildGameManager(GameState state)
        {
            var go  = new GameObject("POLICY_GameManager");
            var gm  = go.AddComponent<GameManager>();
            Wire(gm, "state", state);
            return go;
        }

        static GameObject BuildSystems()
        {
            var go   = new GameObject("POLICY_Systems");
            var deck = go.AddComponent<CardDeckSystem>();

            var cards = LoadAll<CardData>("Assets/Data/Cards");
            if (cards.Length > 0) SetArray(deck, "allCards", cards);
            else Debug.LogWarning("[SceneBuilder] No CardData assets found in Assets/Data/Cards/");

            go.AddComponent<IncomeTickerSystem>();
            go.AddComponent<PolicySystem>();
            go.AddComponent<WeeklyReportSystem>();
            return go;
        }

        static GameObject BuildUIDocument(PanelSettings ps)
        {
            var go  = new GameObject("POLICY_UIDocument");
            var doc = go.AddComponent<UIDocument>();

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/Main.uxml");
            doc.panelSettings    = ps;
            doc.visualTreeAsset  = uxml;
            EditorUtility.SetDirty(doc);

            if (uxml == null) Debug.LogWarning("[SceneBuilder] Main.uxml not found.");

            var sm = go.AddComponent<ScreenManager>();   Wire(sm, "uiDocument", doc);
            var sp = go.AddComponent<SplashScreen>();    Wire(sp, "uiDocument", doc);
            var gs = go.AddComponent<GameScreen>();      Wire(gs, "uiDocument", doc);
            var sw = go.AddComponent<SwipeScreen>();     Wire(sw, "uiDocument", doc);

            var assetDatas  = LoadAll<AssetData>("Assets/Data/Assets");
            var marketDatas = LoadAll<MarketItemData>("Assets/Data/Market");
            if (assetDatas.Length  > 0) SetArray(gs, "assetCatalog",  assetDatas);
            if (marketDatas.Length > 0) SetArray(gs, "marketCatalog", marketDatas);

            return go;
        }

        static GameObject BuildCanvas()
        {
            var go     = new GameObject("POLICY_Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844);
            scaler.matchWidthOrHeight  = 0f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        static GameObject BuildCardDeckView(Transform parent, GameObject cardPrefab, CardDeckSystem deckSystem)
        {
            var go  = new GameObject("CardDeck", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Stretch(RT(go));
            var view = go.AddComponent<CardDeckView>();
            Wire(view, "cardPrefab", cardPrefab.GetComponent<SwipeCardView>());
            Wire(view, "deckSystem", deckSystem);
            Wire(view, "deckParent", go.GetComponent<RectTransform>());
            return go;
        }

        static GameObject BuildOutcomeFlash(Transform parent, GameObject effectPrefab)
        {
            var root = new GameObject("OutcomeFlash", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch(RT(root));
            var rootCG = root.AddComponent<CanvasGroup>();

            // Dark overlay
            var darkBg = Child(root, "DarkBg");
            Stretch(RT(darkBg));
            var darkImg = darkBg.AddComponent<RoundedImage>();
            darkImg.color = new Color(0, 0, 0, 0.5f);

            // Card panel (340 wide, auto height)
            var card   = Child(root, "Card");
            var cardRT = RT(card);
            cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(300, 280);
            var cardBg = card.AddComponent<RoundedImage>();
            cardBg.color = new Color(0.04f, 0.04f, 0.04f); cardBg.cornerRadius = 14f;
            cardBg.outline = true; cardBg.outlineColor = new Color(0.12f, 0.12f, 0.12f); cardBg.outlineWidth = 1f;
            var cardVLG = card.AddComponent<VerticalLayoutGroup>();
            cardVLG.padding = new RectOffset(26, 26, 22, 16); cardVLG.spacing = 10f;
            cardVLG.childControlWidth = cardVLG.childForceExpandWidth = true;
            cardVLG.childControlHeight = cardVLG.childForceExpandHeight = false;

            var fromGO    = Child(card, "FromLabel");    RT(fromGO).sizeDelta    = new Vector2(0, 14);
            var titleGO   = Child(card, "TitleLabel");   RT(titleGO).sizeDelta   = new Vector2(0, 54);
            var effectsGO = Child(card, "Effects");      RT(effectsGO).sizeDelta = new Vector2(0, 100);
            var collGO    = Child(card, "Collateral");   RT(collGO).sizeDelta    = new Vector2(0, 40);

            effectsGO.AddComponent<VerticalLayoutGroup>().spacing = 4f;

            var fromTMP  = MakeTMP(fromGO,  "✓ Approved · Decision",  9f,  new Color(0.20f, 0.20f, 0.20f));
            var titleTMP = MakeTMP(titleGO, "Card title preview",     14f, Color.white);
            var collTMP  = MakeTMP(collGO,  "Collateral pending.",    10f, new Color(0.33f, 0.33f, 0.33f));

            root.SetActive(false);
            var overlay = root.AddComponent<OutcomeFlashOverlay>();
            Wire(overlay, "canvasGroup",      rootCG);
            Wire(overlay, "fromLabel",        fromTMP);
            Wire(overlay, "titleLabel",       titleTMP);
            Wire(overlay, "effectsContainer", effectsGO.transform);
            Wire(overlay, "collateralLabel",  collTMP);
            Wire(overlay, "effectRowPrefab",  effectPrefab);
            return root;
        }

        static GameObject BuildWeeklyReport(Transform parent, TextMeshProUGUI collRowPrefab)
        {
            var root = new GameObject("WeeklyReport", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch(RT(root));
            var rootCG = root.AddComponent<CanvasGroup>();

            // Dark overlay
            var darkBg = Child(root, "DarkBg");
            Stretch(RT(darkBg));
            darkBg.AddComponent<RoundedImage>().color = new Color(0, 0, 0, 0.8f);

            // Report card
            var card   = Child(root, "ReportCard");
            var cardRT = RT(card);
            cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(340, 480);
            var cardBg = card.AddComponent<RoundedImage>();
            cardBg.color = new Color(0.04f, 0.04f, 0.04f); cardBg.cornerRadius = 16f;
            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 18, 14); vlg.spacing = 8f;
            vlg.childControlWidth = vlg.childForceExpandWidth = true;
            vlg.childControlHeight = vlg.childForceExpandHeight = false;

            TextMeshProUGUI Row(string goName, string t, float size, Color col) {
                var go = Child(card, goName); RT(go).sizeDelta = new Vector2(0, size * 1.7f);
                return MakeTMP(go, t, size, col);
            }
            var weekLbl   = Row("WeekLabel",      "WEEKLY STATEMENT · WEEK 9", 8f,  new Color(0.2f, 0.2f, 0.2f));
            var nwLbl     = Row("NetWorthLabel",  "$48,240",                   20f, Color.white);
            var chgLbl    = Row("WeekChangeLbl",  "+$8,400",                   10f, new Color(0.13f, 0.77f, 0.37f));
            var legLbl    = Row("LegacyLabel",    "71/100",                    12f, Color.white);
            var decLbl    = Row("DecisionsLabel", "0",                         12f, Color.white);
            var affLbl    = Row("AffectedLabel",  "4",                         12f, Color.white);

            Row("CollateralHeader", "COLLATERAL", 8f, new Color(0.2f, 0.2f, 0.2f));
            var collContainer = Child(card, "CollateralContainer");
            RT(collContainer).sizeDelta = new Vector2(0, 80);
            collContainer.AddComponent<VerticalLayoutGroup>().spacing = 5f;

            // Close button
            var closeBtnGO = Child(card, "CloseButton");
            RT(closeBtnGO).sizeDelta = new Vector2(80, 30);
            var closeBg  = closeBtnGO.AddComponent<RoundedImage>();
            closeBg.color = Color.clear; closeBg.cornerRadius = 6f;
            closeBg.outline = true; closeBg.outlineColor = new Color(0.13f, 0.13f, 0.13f);
            var closeBtn = closeBtnGO.AddComponent<UnityEngine.UI.Button>();
            var closeTxtGO = Child(closeBtnGO, "Text");
            Stretch(RT(closeTxtGO));
            var closeTMP = MakeTMP(closeTxtGO, "Close", 10f, new Color(0.2f, 0.2f, 0.2f));
            closeTMP.alignment = TextAlignmentOptions.Center;

            root.SetActive(false);
            var overlay = root.AddComponent<WeeklyReportOverlay>();
            Wire(overlay, "canvasGroup",         rootCG);
            Wire(overlay, "weekLabel",           weekLbl);
            Wire(overlay, "netWorthLabel",       nwLbl);
            Wire(overlay, "weekChangeLabel",     chgLbl);
            Wire(overlay, "legacyLabel",         legLbl);
            Wire(overlay, "decisionsLabel",      decLbl);
            Wire(overlay, "affectedLabel",       affLbl);
            Wire(overlay, "collateralContainer", collContainer.transform);
            Wire(overlay, "collateralRowPrefab", collRowPrefab);
            Wire(overlay, "closeButton",         closeBtn);
            return root;
        }

        static GameObject BuildToast(Transform parent)
        {
            var go = new GameObject("Toast", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = RT(go);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(260, 36);
            rt.anchoredPosition = new Vector2(0, 24);

            var bg = go.AddComponent<RoundedImage>();
            bg.color = new Color(0.07f, 0.07f, 0.07f); bg.cornerRadius = 7f;
            bg.outline = true; bg.outlineColor = new Color(0.13f, 0.13f, 0.13f);

            var cg = go.AddComponent<CanvasGroup>(); cg.alpha = 0f;

            var txtGO = Child(go, "Label");
            Stretch(RT(txtGO), 15, 15);
            var tmp = MakeTMP(txtGO, "Toast message", 11f, Color.white);
            tmp.alignment = TextAlignmentOptions.Center;

            var toast = go.AddComponent<ToastView>();
            Wire(toast, "canvasGroup", cg);
            Wire(toast, "label",       tmp);
            return go;
        }

        static void BuildEventSystem()
        {
            // Remove any old EventSystem (may have wrong input module)
            var existing = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var go = new GameObject("POLICY_EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();

            // Use InputSystemUIInputModule if New Input System is active, else StandaloneInputModule
            var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null) go.AddComponent(t);
            else           go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ─────────────────────────────────────────────────────────
        // UTILITIES
        // ─────────────────────────────────────────────────────────

        static void EnsureFolders()
        {
            foreach (var path in new[] { PrefabsPath, SettingsPath, "Assets/Data", "Assets/Data/Cards", "Assets/Data/Assets", "Assets/Data/Market" })
                if (!AssetDatabase.IsValidFolder(path))
                {
                    var p = path.Split('/');
                    AssetDatabase.CreateFolder(string.Join("/", p[..^1]), p[^1]);
                }
        }

        static void CleanOld(params string[] names)
        {
            foreach (var n in names) { var go = GameObject.Find(n); if (go) Object.DestroyImmediate(go); }
        }

        // Always create UI children with RectTransform via constructor (AddComponent can't replace Transform)
        static GameObject Child(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        static RectTransform RT(GameObject go) => go.GetComponent<RectTransform>();

        static RectTransform Stretch(RectTransform rt, float l = 0, float r = 0, float t = 0, float b = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        static TextMeshProUGUI MakeTMP(GameObject go, string text, float size, Color color)
        {
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            return tmp;
        }

        // Set serialized field via reflection — works on both in-memory and scene objects
        static readonly BindingFlags BF =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        static void Wire(Object target, string fieldName, Object value)
        {
            var fi = target.GetType().GetField(fieldName, BF);
            if (fi == null) { Debug.LogWarning($"[SceneBuilder] '{fieldName}' not found on {target.GetType().Name}"); return; }
            try { fi.SetValue(target, value); EditorUtility.SetDirty(target); }
            catch (System.Exception e) { Debug.LogError($"[SceneBuilder] Wire '{fieldName}' on {target.GetType().Name}: {e.Message}"); }
        }

        static void SetArray<T>(Object target, string fieldName, T[] items) where T : Object
        {
            var fi = target.GetType().GetField(fieldName, BF);
            if (fi == null) { Debug.LogWarning($"[SceneBuilder] array '{fieldName}' not found on {target.GetType().Name}"); return; }
            try { fi.SetValue(target, items); EditorUtility.SetDirty(target); }
            catch (System.Exception e) { Debug.LogError($"[SceneBuilder] SetArray '{fieldName}' on {target.GetType().Name}: {e.Message}"); }
        }

        static T[] LoadAll<T>(string folder) where T : Object =>
            !AssetDatabase.IsValidFolder(folder) ? new T[0] :
            AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToArray();
    }
}
