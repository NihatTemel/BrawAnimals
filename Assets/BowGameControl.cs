using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class BowGameControl : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnAppleCountChanged))] public int applecount = 1;

    public GameObject Kurek;
    public GameObject Arrow;


    [SyncVar(hook = nameof(OnTrailStateChanged))]
    private bool isTrailActive;
    public GameObject trailObject; // Inspector'dan ata



    public bool weaponactive = false;
    public float weaponactivelimit = 1;
    public float weaponactivecurrent = 0;
    public bool isattacking = false;

    public GameObject CanvasGameScene;
    public Image KurekImgFill;
    public TMP_Text KurekFillText;

    [SyncVar] public bool canGetHit = true;

    public GameObject appleObj;

    private Vector3 baseScale = Vector3.one;
    private float scalePerStep = 0.3f;
    private int applesPerScaleStep = 3;

    private int previousStep = 0;

    public GameObject BowGameCamera;

    [Header("Vfx effects")]
    public ParticleSystem LevelUp;
    public ParticleSystem LevelDown;
    public ParticleSystem BasicHit;

    public GameObject AimCamPosition;

    //public bool _isLocal = false;

    void Start()
    {



        applecount=100;

        CmdSetApple100();


        GetComponent<PlayerMainController>().Weapon = Kurek;
        GetComponent<PlayerMainController>().weaponactivelimit = weaponactivelimit;
        


        trailObject = Kurek.transform.GetChild(0).gameObject;
        CanvasGameScene = GameObject.Find("CanvasGameScene");
        
        KurekImgFill = CanvasGameScene.GetComponent<BowGameCanvas>().KurekFillImg;
        KurekFillText = CanvasGameScene.GetComponent<BowGameCanvas>().KurekFillText;


        CameraStartSettings();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void CameraStartSettings() 
    {
      //  transform.root.GetComponent<OnlinePrefabController>().TPSCamera.SetActive(false);
        BowGameCamera = transform.root.GetComponent<OnlinePrefabController>().TPSCamera;
        if(transform.root.GetComponent<OnlinePrefabController>().isLocalPlayer)
            BowGameCamera.SetActive(true);
        
        //BowGameCamera.GetComponent<Cine>

    }


    void SetAllNames()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("PlayerCharacter");
        int n = Players.Length;

        for (int i = 0; i < n; i++)
        {
            Players[i].GetComponent<PlayerMainController>().nametext.text = Players[i].transform.root.GetComponent<OnlinePrefabLobbyController>().playerName;
        }
    }

    void Update()
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;

        weaponactive = GetComponent<PlayerMainController>().weaponactive;
        //weaponactivelimit = GetComponent<PlayerMainController>().weaponactivelimit;
        weaponactivecurrent = GetComponent<PlayerMainController>().weaponactivecurrent;
        isattacking = GetComponent<PlayerMainController>().isattacking;

       
        UpdateCooldownUI();


        if (Input.GetMouseButtonDown(0))
            SettingAim();
        if (Input.GetMouseButtonUp(0))
            StartCoroutine( AttackEnemy());

        
       

        SetAllNames();
    }


    bool Aiming = false;

    void SettingAim() 
    {
        if (!canGetHit) return;

        if (weaponactivelimit >= weaponactivecurrent) return;
        

        //GameObject TpsCamera = GetComponent<OnlinePrefabController>().TPSCamera;
        TPSCameraFollow TpsFollow = transform.root.GetComponent<OnlinePrefabController>().TPSCamera.GetComponent<TPSCameraFollow>();

        TpsFollow.aiming = true;
        Aiming = true;

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = BowGameCamera.transform.rotation.eulerAngles;

        // Sadece Y ve Z'yi al, X sabit kalsýn
        transform.rotation = Quaternion.Euler(currentEuler.x, targetEuler.y, targetEuler.z);
    }

    bool bowcanattack = true;

    IEnumerator AttackEnemy()
    {

        yield return null;
        if (!canGetHit || isattacking || !Aiming) 
        {
            Debug.Log("return attack !");
        }
        else 
        {
            isattacking = true;
           // GetComponent<PlayerMainController>().isattacking = true;

            GetComponent<PlayerMainController>().AttackBow();

            TPSCameraFollow TpsFollow = transform.root.GetComponent<OnlinePrefabController>().TPSCamera.GetComponent<TPSCameraFollow>();
            if (bowcanattack) 
            {
                bowcanattack = false;
                  ShootArrow();
            }

            yield return new WaitForSeconds(0.55f);
            Aiming = false;
            TpsFollow.aiming = false;
            bowcanattack = true;
        }





    }
    public float arrowForce = 700f;
    [Command(requiresAuthority = false)]
    public void CmdShootArrow(Vector3 spawnPos, Vector3 direction)
    {
        GameObject arrowObj = Instantiate(Arrow, spawnPos, Quaternion.LookRotation(direction));

        NetworkServer.Spawn(arrowObj);

        if (arrowObj.TryGetComponent(out ArrowController arrow))
        {
            arrow.ArrowStartHelper(spawnPos, direction);
        }
    }
    public void ShootArrow()
    {

        

        Vector3 targetPoint = BowGameCamera.GetComponent<TPSCameraFollow>().targetPoint;

        Vector3 spawnPos = Kurek.transform.position;

        // Calculate direction from bow to target point
        Vector3 direction = (targetPoint - spawnPos).normalized;

        // Debug visualization
       
    

        CmdShootArrow(spawnPos, direction);
    }


   
  

    void ActiveCanGetHit()
    {
        canGetHit = true;
        GetComponent<PlayerMainController>().canMove = true;
    }

    void UpdateCooldownUI()
    {
        if (weaponactivecurrent >= weaponactivelimit)
        {
            KurekImgFill.fillAmount = 0f;
            KurekFillText.text = "";
            return;
        }

        float ratio = 1 - (weaponactivecurrent / weaponactivelimit);
        KurekImgFill.fillAmount = ratio;
        KurekFillText.text = (weaponactivelimit - weaponactivecurrent).ToString("F1");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;

        if (other.CompareTag("Apple") && canGetHit && other.GetComponent<AppleObjController>().collectable)
        {
           
        }
    }

    [Command(requiresAuthority = false)]
    void CmdCollectApple()
    {
        if (canGetHit)
            applecount++;
    }

    [Command(requiresAuthority =false)]
    void CmdSetApple100() 
    {
        applecount = 100;    
    }

    [Command(requiresAuthority = false)]
    void CmdDestroy(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }





    void OnAppleCountChanged(int oldCount, int newCount)
    {
        

        if (transform.root.GetComponent<OnlinePrefabLobbyController>().isLocalPlayer)
        {
            this.gameObject.transform.root.GetComponent<OnlinePlayerGameSceneVariables>().LastSceneScore = applecount;
            PlayerPrefs.SetInt("LastSceneScore", newCount);

        }


    }

    private void OnTrailStateChanged(bool oldValue, bool newValue)
    {
        trailObject.SetActive(newValue);
    }

    [Command(requiresAuthority = false)]
    public void CmdSetTrailState(bool active)
    {
        isTrailActive = active;
    }


    [Command(requiresAuthority = false)]
    public void CmdHitEffect()
    {
        RpcHitEffect();
    }


    [ClientRpc]
    void RpcHitEffect()
    {
        BasicHit.Play();
    }

    GameObject touchBlock = null;

    [Command(requiresAuthority = false)]
    void CmdBreakBlock() 
    {
        RpcBreakBlock();
    }
    [ClientRpc]
    void RpcBreakBlock() 
    {
        touchBlock.gameObject.SetActive(false);
        touchBlock.transform.parent.GetChild(0).gameObject.SetActive(true);
    }



    public void OnCollisionEnter(Collision collision)
    {
       // Debug.Log("touch -> " + collision.gameObject.name);
        if (collision.collider.tag == "ArenaBlock") 
        {
            collision.gameObject.transform.parent.GetComponent<FloorBlockController>().CmdBreakBlock();
        }
    }


}
