using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
public class AppleGameControl : NetworkBehaviour
{


    [SyncVar]public int applecount=1;

    int ScaleLimit = 5;

    public GameObject Kurek;

    public bool weaponactive = false;
    public float weaponactivelimit = 5;
    public float weaponactivecurrent = 0;
    public bool isattacking = false;

    public GameObject CanvasGameScene;
    public Image KurekImgFill;
    public TMP_Text KurekFillText;

    [SyncVar] public bool canGetHit = true;

    public GameObject appleObj;

    void Start()
    {
        GetComponent<PlayerMainController>().Weapon = Kurek;
        CanvasGameScene = GameObject.Find("CanvasGameScene");
        KurekImgFill = CanvasGameScene.GetComponent<AppleGameCanvas>().KurekFillImg;
        KurekFillText = CanvasGameScene.GetComponent<AppleGameCanvas>().KurekFillText;
    }

    // Update is called once per frame
    void Update()
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;


        weaponactive = GetComponent<PlayerMainController>().weaponactive;
        weaponactivelimit = GetComponent<PlayerMainController>().weaponactivelimit;
        weaponactivecurrent = GetComponent<PlayerMainController>().weaponactivecurrent;
        isattacking = GetComponent<PlayerMainController>().isattacking;


        if (Input.GetKeyDown(KeyCode.U)) 
        {
            CollectApple();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoseAplleHelper();
        }


        UpdateCooldownUI();
        AttackEnemy();
    }

    

    void AttackEnemy() 
    {
        if (isattacking) 
        {
            if (Kurek.GetComponent<KurekTrigger>().enemy != null) 
            {
                GameObject enemy = Kurek.GetComponent<KurekTrigger>().enemy;
                if (enemy == this.gameObject)
                    enemy = null;

                var control = enemy.GetComponent<AppleGameControl>();
                if (control.canGetHit)
                {
                    control.canGetHit = false;
                    control.LoseApple();
                    //control.GetHit();
                }


                Debug.Log("hit player" + enemy.gameObject.name);
            }
        }
    }


    [Server]
    void SpawnApple()
    {
        // 1. Capsule Collider yarýçapýný al
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        float safeDistance = 1f; // minimum mesafe (ekstra güvenlik payý)

        float colliderRadius = capsule != null ? capsule.radius : 0.5f;
        float spawnDistance = colliderRadius + safeDistance;
        // 2. Rastgele yön belirle
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        // 3. Spawn pozisyonu = karakter pozisyonu + uzaklýk yönünde + biraz yukarý
        Vector3 spawnOffset = randomDirection * spawnDistance + Vector3.up * 0.5f;
        Vector3 spawnPos = transform.position + spawnOffset;

        // 4. Elmayý oluþtur ve network’e bildir
        GameObject newApple = Instantiate(appleObj, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(newApple);

        

        // 5. Kuvvet uygula (o yöne doðru ve yukarý)
        Rigidbody rb = newApple.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 launchForce = randomDirection * Random.Range(2f, 4f) + Vector3.up * Random.Range(2f, 5f);
            rb.AddForce(launchForce, ForceMode.Impulse);
        }
    }




    [Command(requiresAuthority = false)]
    void LoseApple()
    {
        canGetHit = false;


        Debug.Log("we got hit" + this.gameObject.name);

        Invoke("ActiveCanGetHit", 1.5f);

        LoseAplleHelper();

        //applecount--;

    }

    void LoseAplleHelper() 
    {
        int n = applecount / 2;

        for (int i = 0; i < n; i++)
        {
            applecount--;
            SpawnApple();
        }

        
    }


    void ActiveCanGetHit() 
    {
        canGetHit = true;
    }

    void UpdateCooldownUI()
    {
        if (weaponactivecurrent >= weaponactivelimit)
        {
            KurekImgFill.fillAmount = 0f;
            KurekFillText.text = "";
            return;
        }

        float ratio =1-( weaponactivecurrent / weaponactivelimit);
        KurekImgFill.fillAmount = ratio;
        KurekFillText.text = (weaponactivelimit-weaponactivecurrent).ToString("F1");
    }


    public void OnTriggerEnter(Collider other)
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;
        if (other.tag == "Apple")
        {
            if (canGetHit) 
            {
                CmdDestroy(other.gameObject);

                CollectApple();

                Debug.Log("applle");
            }
            
        }

        
    }


    void ScaleUp()
    {

        ScaleLimit--;
        if (ScaleLimit == 0)
        {
            ScaleLimit = 5;
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

   


}
