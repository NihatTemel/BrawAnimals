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

    public bool _isLocal = false;

    void Start()
    {



        GetComponent<PlayerMainController>().Weapon = Kurek;
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


            GetComponent<PlayerMainController>().AttackBow();

            TPSCameraFollow TpsFollow = transform.root.GetComponent<OnlinePrefabController>().TPSCamera.GetComponent<TPSCameraFollow>();

            ShootArrow();

            yield return new WaitForSeconds(0.55f);
            Aiming = false;
            TpsFollow.aiming = false;

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
      //  if (!transform.root.GetComponent<OnlinePrefabController>().isLocalPlayer) return;

        Vector3 cameraOrigin = BowGameCamera.transform.position;
        Vector3 cameraDirection = BowGameCamera.transform.forward;

        Ray ray = new Ray(cameraOrigin, cameraDirection);
        RaycastHit hit;

        Vector3 targetPoint = Physics.Raycast(ray, out hit, 100f) ? hit.point : cameraOrigin + cameraDirection * 100f;

        // spawnPos oku Kurek’ten çýkar
        Vector3 spawnPos = Kurek.transform.position;
        Vector3 direction = (targetPoint - spawnPos).normalized;

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 2f);
        Debug.DrawLine(spawnPos, targetPoint, Color.red, 2f);

        CmdShootArrow(spawnPos, direction);
    }


    [Command(requiresAuthority = false)]
    public void CmdLoseApple()
    {
        canGetHit = false;

        GetComponent<PlayerMainController>().canMove = false;
        int n = applecount - ((applecount) / 4);


        applecount = applecount / 4;
        applecount = Mathf.Max(0, applecount - 1);

        for (int i = 0; i < n; i++)
        {
            SpawnApple();
        }

        //RpcLoseApple();

        Invoke(nameof(ActiveCanGetHit), 2.5f);
    }

    [ClientRpc]
    void RpcLoseApple()
    {
        int n = applecount - ((applecount) / 4);


        applecount = applecount / 4;
        applecount = Mathf.Max(0, applecount - 1);

        for (int i = 0; i < n; i++)
        {
            SpawnApple();
        }
    }

    void SpawnApple()
    {
        if (!isServer) return;

        Debug.Log("spawn apple");

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        float safeDistance = 1f;
        float colliderRadius = capsule != null ? capsule.radius : 0.5f;
        float spawnDistance = colliderRadius + safeDistance;

        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        Vector3 spawnOffset = randomDirection * spawnDistance + Vector3.up * 0.5f;
        Vector3 spawnPos = transform.position + spawnOffset;

        GameObject newApple = Instantiate(appleObj, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(newApple);

        Rigidbody rb = newApple.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 launchForce = randomDirection * Random.Range(2f, 4f) + Vector3.up * Random.Range(2f, 5f);
            rb.AddForce(launchForce, ForceMode.Impulse);
        }
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
            other.gameObject.tag = "Untagged";
            //CmdDestroy(other.gameObject);
            AppleMoveCharacter(other.gameObject);
            CmdCollectApple();
        }
    }

    [Command(requiresAuthority = false)]
    void CmdCollectApple()
    {
        if (canGetHit)
            applecount++;
    }

    [Command(requiresAuthority = false)]
    void CmdDestroy(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }



    [Command(requiresAuthority = false)]
    void AppleMoveCharacter(GameObject apple)
    {
        RpcAppleMoveCharacter(apple);
    }

    [ClientRpc]
    void RpcAppleMoveCharacter(GameObject apple)
    {
        apple.GetComponent<AppleObjController>().HitBoxCollider.enabled = false;
        apple.transform.DOMove(this.transform.position, 0.3f);
        apple.transform.DOScale(Vector3.zero, 0.3f);
        StartCoroutine(Destroylate(apple));

    }

    IEnumerator Destroylate(GameObject apple)
    {
        yield return new WaitForSeconds(0.35f);
        CmdDestroy(apple);
    }


    void OnAppleCountChanged(int oldCount, int newCount)
    {
        int currentStep = newCount / applesPerScaleStep;

        if (currentStep != previousStep)
        {
            float newScaleValue = 1f + (currentStep * scalePerStep);
            Vector3 targetScale = new Vector3(newScaleValue, newScaleValue, newScaleValue);

            // Karakteri yumuþak þekilde büyüt
            transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutBack);

            if (targetScale.y > transform.localScale.y)
            {
                LevelUp.Play();
            }
            else
            {
                LevelDown.Play();
            }


            // Kamera offset ayarý
            TPSCameraFollow cameraFollow = BowGameCamera.GetComponent<TPSCameraFollow>();
            if (cameraFollow != null)
            {
                Vector3 baseOffset = new Vector3(0, 2f, -5f);

                // Daha dengeli bir uzaklaþma (daha az geri çekilme)
                Vector3 targetOffset = baseOffset + new Vector3(0, 1.2f, -1.4f) * (newScaleValue - 1f);


                // DOTween ile offset animasyonu
                DOTween.To(() => cameraFollow.offset, x => cameraFollow.offset = x, targetOffset, 0.5f)
                       .SetEase(Ease.OutSine);
            }

            previousStep = currentStep;
        }

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
