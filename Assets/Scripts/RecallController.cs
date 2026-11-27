using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class RecallController : MonoBehaviour
{
    public static RecallController Instance { get; private set; }

    [Header("Références")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Volume blurVolume;
    [SerializeField] private GameObject notTodayPanel;
    [SerializeField] private CanvasGroup overlay;

    [Header("Rembobinage")]
    [SerializeField] private float requiredSeconds = 2f;
    [SerializeField] private float rewindSpeed = 2f;
    [SerializeField] private float retriggerDelay = 0.25f; // délai avant qu'une nouvelle collision relance le recall

    [Header("Effets")]
    [SerializeField] private float blurFadeInSpeed = 4f;
    [SerializeField] private float overlayFadeSpeed = 6f;

    public bool IsRewinding { get; private set; }

    private float currentSeconds;
    private float targetSeconds;
    private bool finishing;
    private bool isHolding;
    private bool hasHeld;          // Espace a été maintenu au moins une fois pendant ce recall
    private float blurTarget;
    private float blurCurrent;
    private float lastEndTime = -10f;

    private void Awake()
    {
        Instance = this;

        if (blurVolume == null) Debug.LogWarning("RecallController : Blur Volume non assigné.", this);
        if (notTodayPanel == null) Debug.LogWarning("RecallController : Not Today Panel non assigné, le texte ne s'affichera pas.", this);

        if (blurVolume != null) blurVolume.weight = 0f;
        if (notTodayPanel != null) notTodayPanel.SetActive(false);
        if (overlay != null) overlay.alpha = 0f;
    }

    private void Update()
    {
        // Le flou apparaît progressivement ; sa disparition est instantanée (voir EndEffects).
        blurCurrent = Mathf.MoveTowards(blurCurrent, blurTarget, blurFadeInSpeed * Time.deltaTime);
        if (blurVolume != null) blurVolume.weight = blurCurrent;

        if (overlay != null)
        {
            float overlayTarget = (IsRewinding && isHolding && !finishing) ? 1f : 0f;
            overlay.alpha = Mathf.MoveTowards(overlay.alpha, overlayTarget, overlayFadeSpeed * Time.deltaTime);
        }
    }

    // Appelé par le véhicule à la collision
    public void TriggerRecall()
    {
        if (IsRewinding) return;
        if (Time.time - lastEndTime < retriggerDelay) return;

        RewindManager manager = RewindManager.Instance;
        if (manager == null)
        {
            Debug.LogError("RewindManager absent de la scène.");
            return;
        }

        float maxSeconds = manager.HowManySecondsAvailableForRewind - 0.05f;
        if (maxSeconds <= 0.1f) return;

        targetSeconds = Mathf.Min(requiredSeconds, maxSeconds);
        currentSeconds = 0f;
        finishing = false;
        isHolding = false;
        hasHeld = false;
        IsRewinding = true;

        if (player != null) player.BeginRewind();
        manager.StartRewindTimeBySeconds(0f);   // fige la scène à l'instant de la collision

        blurTarget = 1f;
        if (notTodayPanel != null) notTodayPanel.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (!IsRewinding) return;

        RewindManager manager = RewindManager.Instance;

        // Dernière étape : un pas physique a appliqué la valeur finale, on reprend la partie.
        if (finishing)
        {
            manager.StopRewindTimeBySeconds();
            Physics.SyncTransforms();
            if (player != null) player.EndRewind();

            isHolding = false;
            IsRewinding = false;
            lastEndTime = Time.time;
            return;
        }

        bool spaceDown = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        if (!spaceDown)
        {
            isHolding = false;

            // Relâché après avoir rembobiné : on reprend tout de suite à cet instant.
            if (hasHeld)
            {
                manager.SetTimeSecondsInRewind(currentSeconds);
                BeginFinish();
            }
            // Sinon (jamais appuyé) : la scène reste figée en attendant Espace.
            return;
        }

        isHolding = true;
        hasHeld = true;
        currentSeconds += Time.fixedDeltaTime * rewindSpeed;

        if (currentSeconds >= targetSeconds)
        {
            currentSeconds = targetSeconds;
            manager.SetTimeSecondsInRewind(currentSeconds);
            BeginFinish();
            return;
        }

        manager.SetTimeSecondsInRewind(currentSeconds);
    }

    // Coupe les effets immédiatement ; la reprise physique se fait au prochain FixedUpdate.
    private void BeginFinish()
    {
        finishing = true;

        blurTarget = 0f;
        blurCurrent = 0f;
        if (blurVolume != null) blurVolume.weight = 0f;
        if (notTodayPanel != null) notTodayPanel.SetActive(false);
    }
}