using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField] public float arrowSpeed = 700f;
    [SerializeField] public float destroyAfterSeconds = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Apply force in the arrow's forward direction
        if (rb != null)
        {
            Debug.Log("Arrow " + arrowSpeed);
            rb.AddForce(transform.forward * arrowSpeed);
        }

        // Destroy arrow after some time to prevent clutter
       // Destroy(gameObject, destroyAfterSeconds);
    }

    public void ShootArrow(float ShootingPower) 
    {
        rb.AddForce(transform.forward * ShootingPower);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Optional: Add collision logic here
        // For example, stick the arrow to surfaces or deal damage

        // Disable physics after hitting something
        if (rb != null)
        {
           // rb.isKinematic = true;
        }

        // Optionally destroy after hitting something
        // Destroy(gameObject, 2f);
    }
}