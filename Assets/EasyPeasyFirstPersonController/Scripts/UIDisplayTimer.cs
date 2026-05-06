using UnityEngine;
using UnityEngine.UI;

public class UIDisplayTimer : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup panelCanvasGroup; // Panel avec CanvasGroup
    public CanvasGroup textCanvasGroup;  // Text avec CanvasGroup
    
    [Header("Timer Settings")]
    public float displayDuration = 20f;
    
    private float elapsedTime = 0f;
    private bool isDisplaying = true;

    private void Start()
    {
        // S'assurer que les éléments sont visibles au démarrage
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 1f;
        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 1f;
    }

    private void Update()
    {
        if (!isDisplaying)
            return;

        elapsedTime += Time.deltaTime;

        // Si le temps est écoulé, masquer les éléments
        if (elapsedTime >= displayDuration)
        {
            HideUI();
            isDisplaying = false;
        }
    }

    private void HideUI()
    {
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;
        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 0f;

        // Optionnel : désactiver l'interaction
        if (panelCanvasGroup != null)
            panelCanvasGroup.interactable = false;
        if (textCanvasGroup != null)
            textCanvasGroup.interactable = false;
    }

    // Méthode pour afficher à nouveau si besoin
    public void ShowUI()
    {
        elapsedTime = 0f;
        isDisplaying = true;
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
        }
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = 1f;
            textCanvasGroup.interactable = true;
        }
    }
}
