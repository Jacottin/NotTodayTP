using UnityEngine;

public class RecallController : MonoBehaviour
{
    public static RecallController Instance { get; private set; }

    [SerializeField] private PlayerController player;
    [SerializeField] private float rewindDuration = 3f;  // secondes de jeu à remonter
    [SerializeField] private float rewindSpeed = 3f;     // 3 = recul 3x plus rapide que le temps réel
    [SerializeField] private CanvasGroup overlay;        // optionnel : voile bleu façon Tears of the Kingdom

    public bool IsRewinding { get; private set; }

    private float currentSeconds;
    private float targetSeconds;
    private bool finishing;

    private void Awake()
    {
        Instance = this;
        if (overlay != null) overlay.alpha = 0f;
    }

    public void TriggerRecall()
    {
        if (IsRewinding) return;

        RewindManager manager = RewindManager.Instance;
        if (manager == null)
        {
            Debug.LogError("RewindManager absent de la scène.");
            return;
        }

        targetSeconds = Mathf.Min(rewindDuration, manager.HowManySecondsAvailableForRewind - 0.05f);
        if (targetSeconds <= 0.1f) return;

        IsRewinding = true;
        finishing = false;
        currentSeconds = 0f;

        if (player != null) player.BeginRewind();
        manager.StartRewindTimeBySeconds(0f);
        if (overlay != null) overlay.alpha = 1f;
    }

    private void FixedUpdate()
    {
        if (!IsRewinding) return;

        RewindManager manager = RewindManager.Instance;

        // Étape finale : on laisse un pas physique appliquer la dernière valeur, puis on arrête.
        if (finishing)
        {
            manager.StopRewindTimeBySeconds();
            Physics.SyncTransforms();
            if (player != null) player.EndRewind();
            if (overlay != null) overlay.alpha = 0f;
            IsRewinding = false;
            return;
        }

        currentSeconds += Time.fixedDeltaTime * rewindSpeed;

        if (currentSeconds >= targetSeconds)
        {
            manager.SetTimeSecondsInRewind(targetSeconds);
            finishing = true;
            return;
        }

        manager.SetTimeSecondsInRewind(currentSeconds);
    }
}