using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
public class AppleGameControl : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnAppleCountChanged))] public int applecount = 1;

    public GameObject Kurek;



    [SyncVar(hook = nameof(OnTrailStateChanged))]
    private bool isTrailActive;
    public GameObject trailObject; // Inspector'dan ata



    public bool weaponactive = false;
    public float weaponactivelimit = 5;
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

    public GameObject AppleGameCamera;

    [Header("Vfx effects")]
    public ParticleSystem LevelUp;
    public ParticleSystem LevelDown;
    public ParticleSystem BasicHit;


    void Start()
    {
        GetComponent<PlayerMainController>().Weapon = Kurek;
        trailObject = Kurek.transform.GetChild(0).gameObject;
        CanvasGameScene = GameObject.Find("CanvasGameScene");
        KurekImgFill = CanvasGameScene.GetComponent<AppleGameCanvas>().KurekFillImg;
        KurekFillText = CanvasGameScene.GetComponent<AppleGameCanvas>().KurekFillText;

        AppleGameCamera = transform.root.GetComponent<OnlinePrefabController>().TPSCamera;

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
        weaponactivelimit = GetComponent<PlayerMainController>().weaponactivelimit;
        weaponactivecurrent = GetComponent<PlayerMainController>().weaponactivecurrent;
        isattacking = GetComponent<PlayerMainController>().isattacking;

        if (Input.GetKeyDown(KeyCode.N))
        {
            CmdCollectApple();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            CmdLoseApple();
        }

        UpdateCooldownUI();
        AttackEnemy();
        SetAllNames();
    }

    void AttackEnemy()
    {
        if (!canGetHit) return;

        if (!isattacking) 
        {
            CmdSetTrailState(false);
            return;

        }

        CmdSetTrailState(true);
        if (Kurek.GetComponent<KurekTrigger>().enemy != null)
        {
            GameObject enemy = Kurek.GetComponent<KurekTrigger>().enemy;
            if (enemy == this.gameObject) return;
            if (!canGetHit) return;
            var control = enemy.GetComponent<AppleGameControl>();
            if (control.canGetHit)
            {
                enemy.GetComponent<PlayerMainController>().canMove = false;
                enemy.GetComponent<AppleGameControl>().CmdHitEffect();
                control.canGetHit = false;
                control.CmdLoseApple();
                Debug.Log("hit player " + enemy.gameObject.name);
            }
        }
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
        int n = applecount-( (applecount) / 4);
        

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
        if(canGetHit)
        applecount++;
    }

    [Command(requiresAuthority = false)]
    void CmdDestroy(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }



    [Command(requiresAuthority =false)]
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

            // Karakteri yumu�ak �ekilde b�y�t
            transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutBack);

            if(targetScale.y > transform.localScale.y) 
            {
                LevelUp.Play();
            }
            else 
            {
                LevelDown.Play();
            }


            // Kamera offset ayar�
            TPSCameraFollow cameraFollow = AppleGameCamera.GetComponent<TPSCameraFollow>();
            if (cameraFollow != null)
            {
                Vector3 baseOffset = new Vector3(0, 2f, -5f);

                // Daha dengeli bir uzakla�ma (daha az geri �ekilme)
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

}
