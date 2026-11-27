using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [Header("Déplacement")]
    [SerializeField] private float speed = 6f;

    [Header("Disparition")]
    [SerializeField] private float destroyX = 15f;

    [Header("Rembobinage")]
    [SerializeField] private float rewindSeconds = 2f;

    private Rigidbody rb;
    private int direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(
        int movementDirection,
        float vehicleSpeed,
        float vehicleDestroyX,
        float vehicleRewindSeconds
    )
    {
        direction = movementDirection;
        speed = vehicleSpeed;
        destroyX = vehicleDestroyX;
        rewindSeconds = vehicleRewindSeconds;

        if (direction > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        Vector3 movement = new Vector3(direction, 0f, 0f) * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        bool isOutOfBounds =
            (direction > 0 && transform.position.x >= destroyX) ||
            (direction < 0 && transform.position.x <= -destroyX);

        if (isOutOfBounds)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Le joueur a été touché : rembobinage du temps.");

        RewindAfterCollision();
    }

    private void RewindAfterCollision()
    {
        if (RewindManager.Instance == null)
        {
            Debug.LogError("RewindManager est absent de la scène.");
            return;
        }

        float availableTime = RewindManager.Instance.HowManySecondsAvailableForRewind;

        if (availableTime <= 0f)
        {
            Debug.LogWarning("Aucun historique n'est encore disponible pour le rembobinage.");
            return;
        }

        float timeToRewind = Mathf.Min(rewindSeconds, availableTime);

        Debug.Log($"Rembobinage de {timeToRewind:F2} secondes.");

        RewindManager.Instance.InstantRewindTimeBySeconds(timeToRewind);
    }
}