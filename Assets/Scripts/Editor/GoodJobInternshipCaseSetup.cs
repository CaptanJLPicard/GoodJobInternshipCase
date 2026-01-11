// ============================================================================
// Made By Hakan Emre ÖZKAN
// For more follow my itch.io account (Heodev) - To begin, begin
// ============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GoodJobInternshipCase.Config;
using GoodJobInternshipCase.Rendering;
using GoodJobInternshipCase.Core;
using GoodJobInternshipCase.Feedback;
using TMPro;

namespace GoodJobInternshipCase.Editor
{
    /// <summary>
    /// Editor utility for quick game setup.
    /// Creates all necessary prefabs and assets.
    /// </summary>
    public class GoodJobInternshipCaseSetup : EditorWindow
    {
        private static readonly string[] ColorNames = { "Blue", "Green", "Pink", "Purple", "Red", "Yellow" };

        [MenuItem("Tools/GoodJobInternshipCase/Setup Game")]
        public static void ShowWindow()
        {
            GetWindow<GoodJobInternshipCaseSetup>("GoodJobInternshipCase Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("GoodJobInternshipCase Quick Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This will create:\n" +
                "- Block Prefab (in Assets/Prefabs/)\n" +
                "- ScorePopup Prefab (in Assets/Prefabs/)\n" +
                "- GameConfig asset (in Assets/ScriptableObjects/)\n" +
                "- Sprite assignments will be auto-configured",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Block Prefab", GUILayout.Height(30)))
            {
                CreateBlockPrefab();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Create ScorePopup Prefab", GUILayout.Height(30)))
            {
                CreateScorePopupPrefab();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Create GameConfig", GUILayout.Height(30)))
            {
                CreateGameConfig();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Setup Complete Scene", GUILayout.Height(40)))
            {
                SetupScene();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "After setup, assign:\n" +
                "1. Block Prefab to BlockPool\n" +
                "2. ScorePopup Prefab to ScorePopupPool\n" +
                "3. GameConfig to GameManager\n" +
                "4. References in GameManager inspector",
                MessageType.Warning);

            // Developer Credits
            EditorGUILayout.Space(20);
            DrawDeveloperCredits();
        }

        private void DrawDeveloperCredits()
        {
            // Separator line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Credits box
            GUIStyle creditStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize = 10,
                wordWrap = true
            };

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11
            };

            GUIStyle linkStyle = new GUIStyle(EditorStyles.linkLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Made By Hakan Emre ÖZKAN", titleStyle);
            EditorGUILayout.LabelField("\"To begin, begin\"", creditStyle);
            EditorGUILayout.Space(5);

            // Developer logo/image
            Texture2D logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Me/White on Transparent.png");
            if (logoTexture != null)
            {
                float maxWidth = 80f;
                float aspectRatio = (float)logoTexture.height / logoTexture.width;
                float imageHeight = maxWidth * aspectRatio;

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(logoTexture, GUILayout.Width(maxWidth), GUILayout.Height(imageHeight));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
            }

            // Clickable links
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("heodev.itch.io", linkStyle))
            {
                Application.OpenURL("https://heodev.itch.io/");
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("eaglebyte-games.itch.io", linkStyle))
            {
                Application.OpenURL("https://eaglebyte-games.itch.io/");
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        [MenuItem("Tools/GoodJobInternshipCase/Create Block Prefab")]
        public static void CreateBlockPrefab()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Create block GameObject
            GameObject block = new GameObject("Block");

            // Add SpriteRenderer
            SpriteRenderer sr = block.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 0;

            // Try to load a default sprite
            Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Blue_Default.png");
            if (defaultSprite != null)
            {
                sr.sprite = defaultSprite;
            }

            // Add BlockVisual component
            block.AddComponent<BlockVisual>();

            // Save as prefab
            string prefabPath = "Assets/Prefabs/Block.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(block, prefabPath);

            // Cleanup
            DestroyImmediate(block);

            Debug.Log($"Block prefab created at: {prefabPath}");
            EditorGUIUtility.PingObject(prefab);
        }

        [MenuItem("Tools/GoodJobInternshipCase/Create ScorePopup Prefab")]
        public static void CreateScorePopupPrefab()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Create ScorePopup GameObject
            GameObject popup = new GameObject("ScorePopup");

            // Add RectTransform (required for UI)
            RectTransform rectTransform = popup.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200f, 50f);

            // Add CanvasGroup for alpha fading
            CanvasGroup canvasGroup = popup.AddComponent<CanvasGroup>();

            // Add ScorePopup component
            ScorePopup scorePopup = popup.AddComponent<ScorePopup>();

            // Create Text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(popup.transform);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Add TextMeshProUGUI
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "+100";
            tmp.fontSize = 36;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;

            // Try to add outline for better visibility
            // Note: Outline is added via TMP settings, not as component

            // Save as prefab
            string prefabPath = "Assets/Prefabs/ScorePopup.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(popup, prefabPath);

            // Cleanup
            DestroyImmediate(popup);

            Debug.Log($"ScorePopup prefab created at: {prefabPath}");
            Debug.Log("NOTE: Add this prefab to a Canvas in your scene, then assign it to ScorePopupPool.");
            EditorGUIUtility.PingObject(prefab);
        }

        [MenuItem("Tools/GoodJobInternshipCase/Create GameConfig")]
        public static void CreateGameConfig()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }

            // Create config
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();

            // Set default values
            config.Rows = 8;
            config.Columns = 8;
            config.ColorCount = 4;
            config.ThresholdA = 3;
            config.ThresholdB = 5;
            config.ThresholdC = 8;

            // Setup sprites
            config.ColorSprites = new BlockSpriteSet[6];

            for (int i = 0; i < ColorNames.Length; i++)
            {
                string colorName = ColorNames[i];

                config.ColorSprites[i] = new BlockSpriteSet
                {
                    ColorName = colorName,
                    DefaultSprite = LoadSprite($"{colorName}_Default"),
                    SpriteA = LoadSprite($"{colorName}_A"),
                    SpriteB = LoadSprite($"{colorName}_B"),
                    SpriteC = LoadSprite($"{colorName}_C")
                };
            }

            // Save asset
            string assetPath = "Assets/ScriptableObjects/GameConfig.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"GameConfig created at: {assetPath}");
            EditorGUIUtility.PingObject(config);
        }

        private static Sprite LoadSprite(string name)
        {
            string path = $"Assets/Sprites/{name}.png";
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [MenuItem("Tools/GoodJobInternshipCase/Setup Scene")]
        public static void SetupScene()
        {
            // Find or create necessary objects

            // 1. Find GameManager or create structure
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                // Find Scripts container or create one
                GameObject scriptsObj = GameObject.Find("Scripts");
                if (scriptsObj == null)
                {
                    scriptsObj = new GameObject("Scripts");
                }

                // Find or create GameManager object
                Transform gmTransform = scriptsObj.transform.Find("GameManager");
                GameObject gmObj;
                if (gmTransform == null)
                {
                    gmObj = new GameObject("GameManager");
                    gmObj.transform.SetParent(scriptsObj.transform);
                }
                else
                {
                    gmObj = gmTransform.gameObject;
                }

                gameManager = gmObj.GetComponent<GameManager>();
                if (gameManager == null)
                {
                    gameManager = gmObj.AddComponent<GameManager>();
                }
            }

            // 2. Create Board object with renderer and pool
            GameObject boardObj = GameObject.Find("Board");
            if (boardObj == null)
            {
                boardObj = new GameObject("Board");
            }

            BoardRenderer boardRenderer = boardObj.GetComponent<BoardRenderer>();
            if (boardRenderer == null)
            {
                boardRenderer = boardObj.AddComponent<BoardRenderer>();
            }

            BlockPool blockPool = boardObj.GetComponent<BlockPool>();
            if (blockPool == null)
            {
                blockPool = boardObj.AddComponent<BlockPool>();
            }

            // 3. Create InputHandler
            GameObject inputObj = GameObject.Find("InputHandler");
            if (inputObj == null)
            {
                inputObj = new GameObject("InputHandler");
                inputObj.transform.SetParent(gameManager.transform.parent);
            }

            GoodJobInternshipCase.Core.InputHandler inputHandler = inputObj.GetComponent<GoodJobInternshipCase.Core.InputHandler>();
            if (inputHandler == null)
            {
                inputHandler = inputObj.AddComponent<GoodJobInternshipCase.Core.InputHandler>();
            }

            // 4. Create FeedbackManager
            GameObject feedbackObj = GameObject.Find("FeedbackManager");
            if (feedbackObj == null)
            {
                feedbackObj = new GameObject("FeedbackManager");
                feedbackObj.transform.SetParent(gameManager.transform.parent);
            }

            GoodJobInternshipCase.Feedback.FeedbackManager feedbackManager = feedbackObj.GetComponent<GoodJobInternshipCase.Feedback.FeedbackManager>();
            if (feedbackManager == null)
            {
                feedbackManager = feedbackObj.AddComponent<GoodJobInternshipCase.Feedback.FeedbackManager>();
            }

            // 5. Assign references via SerializedObject
            SerializedObject gmSO = new SerializedObject(gameManager);

            SerializedProperty boardRendererProp = gmSO.FindProperty("_boardRenderer");
            if (boardRendererProp != null)
                boardRendererProp.objectReferenceValue = boardRenderer;

            SerializedProperty inputHandlerProp = gmSO.FindProperty("_inputHandler");
            if (inputHandlerProp != null)
                inputHandlerProp.objectReferenceValue = inputHandler;

            SerializedProperty feedbackManagerProp = gmSO.FindProperty("_feedbackManager");
            if (feedbackManagerProp != null)
                feedbackManagerProp.objectReferenceValue = feedbackManager;

            // Try to find and assign GameConfig
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/ScriptableObjects/GameConfig.asset");
            if (config != null)
            {
                SerializedProperty configProp = gmSO.FindProperty("_config");
                if (configProp != null)
                    configProp.objectReferenceValue = config;
            }

            gmSO.ApplyModifiedProperties();

            // 6. Assign Block Prefab to BlockPool
            GameObject blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Block.prefab");
            if (blockPrefab != null)
            {
                SerializedObject poolSO = new SerializedObject(blockPool);
                SerializedProperty prefabProp = poolSO.FindProperty("_blockPrefab");
                if (prefabProp != null)
                    prefabProp.objectReferenceValue = blockPrefab;
                poolSO.ApplyModifiedProperties();
            }

            // 7. Setup Camera with CameraFit for mobile
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                CameraFit cameraFit = mainCamera.GetComponent<CameraFit>();
                if (cameraFit == null)
                {
                    cameraFit = mainCamera.gameObject.AddComponent<CameraFit>();
                }

                // Assign config to CameraFit
                if (config != null)
                {
                    SerializedObject camFitSO = new SerializedObject(cameraFit);
                    SerializedProperty camConfigProp = camFitSO.FindProperty("_config");
                    if (camConfigProp != null)
                        camConfigProp.objectReferenceValue = config;
                    camFitSO.ApplyModifiedProperties();
                }
            }

            // 8. Add MobileOptimizer
            GameObject optimizerObj = GameObject.Find("MobileOptimizer");
            if (optimizerObj == null)
            {
                optimizerObj = new GameObject("MobileOptimizer");
                optimizerObj.transform.SetParent(gameManager.transform.parent);
            }

            MobileOptimizer mobileOptimizer = optimizerObj.GetComponent<MobileOptimizer>();
            if (mobileOptimizer == null)
            {
                mobileOptimizer = optimizerObj.AddComponent<MobileOptimizer>();
            }

            // 9. Create Canvas for UI (Score Popup)
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // Above other UI

                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // 10. Create ScorePopupPool
            ScorePopupPool scorePopupPool = Object.FindFirstObjectByType<ScorePopupPool>();
            if (scorePopupPool == null)
            {
                GameObject poolObj = new GameObject("ScorePopupPool");
                poolObj.transform.SetParent(canvas.transform);
                scorePopupPool = poolObj.AddComponent<ScorePopupPool>();
            }

            // Assign ScorePopup prefab to pool
            GameObject scorePopupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ScorePopup.prefab");
            if (scorePopupPrefab != null && scorePopupPool != null)
            {
                SerializedObject poolSO = new SerializedObject(scorePopupPool);
                SerializedProperty prefabProp = poolSO.FindProperty("_popupPrefab");
                if (prefabProp != null)
                    prefabProp.objectReferenceValue = scorePopupPrefab.GetComponent<ScorePopup>();
                poolSO.ApplyModifiedProperties();
            }

            // 11. Assign ScorePopupPool to FeedbackManager
            if (feedbackManager != null && scorePopupPool != null)
            {
                SerializedObject fbSO = new SerializedObject(feedbackManager);
                SerializedProperty poolProp = fbSO.FindProperty("_scorePopupPool");
                if (poolProp != null)
                    poolProp.objectReferenceValue = scorePopupPool;
                fbSO.ApplyModifiedProperties();
            }

            // 12. Add URP Optimizer for mobile performance
            URPOptimizer urpOptimizer = Object.FindFirstObjectByType<URPOptimizer>();
            if (urpOptimizer == null)
            {
                GameObject urpObj = new GameObject("URPOptimizer");
                urpObj.transform.SetParent(gameManager.transform.parent);
                urpOptimizer = urpObj.AddComponent<URPOptimizer>();
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("Scene setup complete! Check GameManager inspector for any missing references.");
            Debug.Log("For low-end devices: Set MobileOptimizer to 'Low' tier and URPOptimizer RenderScale to 0.75");
            Selection.activeGameObject = gameManager.gameObject;
        }
    }
}
#endif
