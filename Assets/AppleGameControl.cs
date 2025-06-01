using UnityEngine;
using Mirror;
public class AppleGameControl : NetworkBehaviour
{

    [SyncVar]public int applecount;

    int ScaleLimit = 2;

    public GameObject Kurek;

    public bool weaponactive = false;

    public float weaponactivelimit = 5;
    public float weaponactivecurrent = 0;

    // 5 attack
    // 6 attack
    // 7 idle
    // 8 walk
    // 9 jump
    // 10 run

    public Animator animator;
    public AnimationClip[] animationClips;




    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;




        if (Input.GetKeyDown(KeyCode.V) && !weaponactive)
        {
            weaponactive = true;
            CmdSetKurekVisible(true);
        }

        if (!weaponactive) 
        {
            if (weaponactivecurrent < 5) 
            {
                weaponactivecurrent += Time.deltaTime;
            }
            else 
            {
                weaponactive = true;
                CmdSetKurekVisible(true);
            }
            
            
        }

        GetComponent<PlayerMainController>().weaponactive = weaponactive;

    }


    public void OnTriggerEnter(Collider other)
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;
        if (other.tag == "Apple")
        {
           
                CmdDestroy(other.gameObject);
               
            CollectApple();
            
            Debug.Log("applle");
        }
    }


    void ScaleUp()
    {

        ScaleLimit--;
        if (ScaleLimit == 0)
        {
            ScaleLimit = 2;
            Vector3 newScale = transform.localScale * 1.4f;
            transform.localScale = newScale;
            RpcScaleUp(newScale); // client’lara bildir
        }
    }

    [ClientRpc]
    void RpcScaleUp(Vector3 newScale)
    {
        if (isServer) return; // Server zaten uyguladý
        transform.localScale = newScale;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;
    }

    [Command(requiresAuthority = false)]
    void CmdDestroy(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }

    [Command(requiresAuthority = false)]
    void CollectApple() 
    {
        applecount++;
        ScaleUp();
    }

    [Command]
    void CmdSetKurekVisible(bool visible)
    {
        RpcSetKurekVisible(visible);
    }
    [ClientRpc]
    void RpcSetKurekVisible(bool visible)
    {
        if (Kurek != null)
            Kurek.SetActive(visible);
    }


}
