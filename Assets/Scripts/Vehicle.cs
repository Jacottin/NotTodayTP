using UnityEngine;

public class Vehicle : MonoBehaviour
{
    private Rigidbody rb;
    private float speed;
    private float destroyX;
    private int direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(int movementDirection, float vehicleSpeed, float vehicleDestroyX)
    {
        direction = movementDirection;
        speed = vehicleSpeed;
        destroyX = vehicleDestroyX;

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
        Vector3 movement = new Vector3(direction, 0f, 0f) * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        if (Mathf.Abs(transform.position.x) >= destroyX)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Collision avec le joueur !");

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                player.DisableControl();
            }
        }
    }
}