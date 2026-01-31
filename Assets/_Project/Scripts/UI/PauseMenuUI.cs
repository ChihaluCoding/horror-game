using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private MonoBehaviour[] disableWhilePaused;
        [SerializeField] private GameObject settingsPanel;

        [Header("Auto Create")]
        [SerializeField] private bool autoCreateMenu = true;
        [SerializeField] private Vector2 menuSize = new Vector2(460f, 280f);
        [SerializeField] private bool autoCreateSettingsButton = true;

        [Header("Input")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

        [Header("Behavior")]
        [SerializeField] private bool pauseTime = true;
        [SerializeField] private bool lockCursorWhenUnpaused = true;
        [SerializeField] private bool showCursorWhenPaused = true;

        private bool isPaused;
        private float previousTimeScale = 1f;
        private CursorLockMode previousLockState;
        private bool previousCursorVisible;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (menuRoot == null && autoCreateMenu)
            {
                menuRoot = CreateDefaultMenu();
            }

            if (menuRoot == null)
            {
                menuRoot = gameObject;
            }

            if (canvasGroup == null)
            {
                canvasGroup = menuRoot.GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = menuRoot.AddComponent<CanvasGroup>();
            }

            SetMenuVisible(false, true);
        }

        private void OnDisable()
        {
            if (isPaused)
            {
                Resume();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (isPaused)
            {
                return;
            }

            isPaused = true;
            previousTimeScale = Time.timeScale;
            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            if (pauseTime)
            {
                Time.timeScale = 0f;
            }

            if (showCursorWhenPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            SetComponentsEnabled(false);
            SetMenuVisible(true, false);
        }

        public void Resume()
        {
            if (!isPaused)
            {
                return;
            }

            isPaused = false;

            if (pauseTime)
            {
                Time.timeScale = previousTimeScale;
            }

            if (lockCursorWhenUnpaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = previousLockState;
                Cursor.visible = previousCursorVisible;
            }

            SetComponentsEnabled(true);
            SetMenuVisible(false, false);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private void SetMenuVisible(bool visible, bool force)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else if (menuRoot != null && menuRoot != gameObject)
            {
                menuRoot.SetActive(visible);
            }
            else if (menuRoot == gameObject && force == false)
            {
                // If menuRoot is the same object and there's no CanvasGroup,
                // we avoid deactivating it so Update still runs.
                // Users should add a CanvasGroup to control visibility.
            }
        }

        private void SetComponentsEnabled(bool enabled)
        {
            if (disableWhilePaused == null)
            {
                return;
            }

            for (int i = 0; i < disableWhilePaused.Length; i++)
            {
                if (disableWhilePaused[i] != null)
                {
                    disableWhilePaused[i].enabled = enabled;
                }
            }
        }

        private GameObject CreateDefaultMenu()
        {
            Canvas targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                GameObject canvasGo = new GameObject("PauseMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                targetCanvas = canvasGo.GetComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            EnsureEventSystem();

            GameObject root = new GameObject("PauseMenuRoot", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            root.transform.SetParent(targetCanvas.transform, false);

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image dim = root.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.4f);
            dim.raycastTarget = true;

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = menuSize;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.06f, 0.06f, 0.08f, 0.92f);

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(panel.transform, false);

            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(menuSize.x - 40f, menuSize.y - 40f);

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateText(content.transform, "Paused", 32, TextAnchor.MiddleCenter);

            Button resumeButton = CreateButton(content.transform, "Resume");
            resumeButton.onClick.AddListener(Resume);

            if (autoCreateSettingsButton)
            {
                Button settingsButton = CreateButton(content.transform, "Settings");
                settingsButton.onClick.AddListener(OpenSettings);
            }

            Button quitButton = CreateButton(content.transform, "Quit");
            quitButton.onClick.AddListener(QuitGame);

            return root;
        }

        private void EnsureEventSystem()
        {
            EventSystem existing = FindObjectOfType<EventSystem>();
            if (existing != null)
            {
                return;
            }

            GameObject eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemGo.transform.SetParent(transform, false);
        }

        private Text CreateText(Transform parent, string text, int fontSize, TextAnchor alignment)
        {
            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(parent, false);

            Text uiText = textGo.GetComponent<Text>();
            uiText.text = text;
            uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            uiText.fontSize = fontSize;
            uiText.alignment = alignment;
            uiText.color = Color.white;

            return uiText;
        }

        private Button CreateButton(Transform parent, string label)
        {
            GameObject buttonGo = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);

            RectTransform rect = buttonGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 48f);

            Image image = buttonGo.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);

            Button button = buttonGo.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.25f, 0.95f);
            colors.highlightedColor = new Color(0.28f, 0.28f, 0.35f, 1f);
            colors.pressedColor = new Color(0.12f, 0.12f, 0.16f, 1f);
            colors.selectedColor = colors.normalColor;
            button.colors = colors;

            GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(buttonGo.transform, false);

            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text labelText = labelGo.GetComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 20;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;

            return button;
        }
    }
}
