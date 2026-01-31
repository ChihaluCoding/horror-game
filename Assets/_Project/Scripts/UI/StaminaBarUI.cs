using UnityEngine;
using UnityEngine.UI;
using Project.Player;

namespace Project.UI
{
    public class StaminaBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FirstPersonController controller;
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Behavior")]
        [SerializeField] private bool hideWhenFull = true;
        [SerializeField] private float showThreshold = 0.98f;
        [SerializeField] private float fadeSpeed = 8f;
        [SerializeField] private float visibleAlpha = 1f;
        [SerializeField] private float hiddenAlpha = 0f;

        private void Awake()
        {
            if (controller == null)
            {
                controller = FindObjectOfType<FirstPersonController>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (slider == null)
            {
                slider = GetComponent<Slider>();
            }

            if (fillImage == null)
            {
                if (slider != null && slider.fillRect != null)
                {
                    fillImage = slider.fillRect.GetComponent<Image>();
                }
                else
                {
                    fillImage = GetComponentInChildren<Image>();
                }
            }

            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
            }
        }

        private void Update()
        {
            if (controller == null)
            {
                return;
            }

            float normalized = controller.StaminaNormalized;

            if (slider != null)
            {
                slider.value = normalized;
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = normalized;
            }

            if (canvasGroup != null)
            {
                bool shouldShow = !hideWhenFull || normalized < showThreshold;
                float targetAlpha = shouldShow ? visibleAlpha : hiddenAlpha;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
                canvasGroup.interactable = shouldShow;
                canvasGroup.blocksRaycasts = shouldShow;
            }
        }
    }
}
