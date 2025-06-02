using UnityEngine;

public class KurekTrigger : MonoBehaviour
{

    public GameObject enemy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCharacter"))
        {
            Debug.Log("hit " +other.gameObject.name);
            enemy = other.gameObject;
        }
        else 
        {
            enemy = null;
        }
    }
}
