using UnityEngine;
using Mirror;
public class ArrowController : NetworkBehaviour
{
    [SerializeField] public float arrowSpeed = 700f;
    [SerializeField] public float destroyAfterSeconds = 5f;

    private Rigidbody rb;

    void Start()
    {
        

        
    }

    


    [Command(requiresAuthority = false)]
    public void ArrowStartHelper(Vector3 spawnPos, Vector3 direction) 
    {
        RpcArrowStartHelper(spawnPos, direction);
    }

    [ClientRpc]
    void RpcArrowStartHelper(Vector3 spawnPos, Vector3 direction) 
    {
        transform.position = spawnPos;
        Quaternion.LookRotation(direction);

        rb = GetComponent<Rigidbody>();

        // Apply force in the arrow's forward direction
        if (rb != null)
        {
            Debug.Log("Arrow " + arrowSpeed);
            rb.AddForce(transform.forward * arrowSpeed);
        }

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




    public void OnTriggerEnter(Collider other)
    {
        // Debug.Log("touch -> " + collision.gameObject.name);
        if (other.tag == "ArenaBlock")
        {
            other.gameObject.transform.parent.GetComponent<FloorBlockController>().CmdArrowBreakBlock();
            
        }
    }


}