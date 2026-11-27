using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private float destroyX = 15f;

    private Rigidbody rb;
    private int direction = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Le 4e paramètre est conservé pour rester compatible avec VehicleSpawner, mais n'est plus utilisé.
    public void Initialize(int movementDirection, float vehicleSpeed, float vehicleDestroyX, float unusedRewindSeconds = 0f)
    {
        direction = movementDirection;
        speed = vehicleSpeed;
        destroyX = vehicleDestroyX;

        transform.rotation = Quaternion.Euler(0f, direction > 0 ? 90f : -90f, 0f);
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        if (RewindManager.Instance != null && RewindManager.Instance.IsBeingRewinded) return;

        Vector3 movement = new Vector3(direction, 0f, 0f) * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        bool isOutOfBounds =
            (direction > 0 && transform.position.x >= destroyX) ||
            (direction < 0 && transform.position.x <= -destroyX);

        if (isOutOfBounds)
        {
            // Le RewindManager détruira l'objet (mode DisableDestroy, voir le spawner).
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (RecallController.Instance == null || RecallController.Instance.IsRewinding) return;

        RecallController.Instance.TriggerRecall();
    }
}