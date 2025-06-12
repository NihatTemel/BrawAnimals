using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
[RequireComponent(typeof(CharacterController))]
public class PlayerMainController : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float jumpForce = 8f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Animation")]
    public Animator animator;
    public AnimationClip[] animationClips;
    public int layer = 0;

    public AnimationClip[] shovelAnimationClips;
    private AnimationClip[] currentAnimationClips;

    public Camera CharacterCamera;

    [Header("Stamina")]
    public Image StaminaImg;
    public float stamina = 1f;
    public float staminaDrainRate = 0.2f; // saniyede drain
    public float staminaRecoverRate = 0.1f; // boþta geri kazaným hýzý
    public float staminaThreshold = 0.1f;
    public GameObject SprintVfx;

    public TMP_Text nametext;
    [SyncVar] public string playername;

    [Header("Attack")]

    public bool weaponactive = false;
    public float weaponactivelimit = 5;
    public float weaponactivecurrent = 0;
    public GameObject Weapon;
    public bool isattacking = false;

    [SyncVar] public bool canMove = true;
    public bool aimining;
    public Transform RigBody;


    private void Start()
    {
        StartingSettings();

    }

    void StartingSettings()
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local)
            return;

        CharacterCamera = transform.root.GetComponent<OnlinePrefabController>().TPSCamera.GetComponent<Camera>();
        if (transform.root.GetComponent<OnlinePrefabController>()._local)
            CharacterCamera.gameObject.SetActive(true);


        StaminaImg = GameObject.Find("Stamina").GetComponent<Image>();
        controller = GetComponent<CharacterController>();

        aimining=transform.root.GetComponent<OnlinePrefabController>().TPSCamera.GetComponent<TPSCameraFollow>().aiming;

        

        playername = transform.root.GetComponent<OnlinePrefabLobbyController>().playerName;
        CmdSetPlayerName();
    }


    [Command(requiresAuthority = false)]
    void CmdSetPlayerName() 
    {
        nametext.text = playername;
        RpcSetPlayerName();
    }


    [ClientRpc]
    void RpcSetPlayerName() 
    {
        nametext.text = playername;
    }

    private float xRotation = 0f;



    private void Update()
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;

        float mouseSensitivity = 1f;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        bool isAiming = CharacterCamera.GetComponent<TPSCameraFollow>().Aiming;

        if (isAiming)
        {
            // FPS gibi yukarý-aþaðý bakýþ
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);
            CharacterCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX); // FPS gibi dönüþ
        }

        // Zemin kontrolü
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (!canMove)
        {
            horizontal = 0;
            vertical = 0;
        }

        Vector3 moveDirection;

        if (isAiming)
        {
            RigBody.localRotation = Quaternion.Euler(-90, 40, 0);
            moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;

             
            walkSpeed = 1.5f;
            runSpeed = 3f;

        }
        else
        {
            RigBody.localRotation = Quaternion.Euler(-90, 0, 0);
            // TPS tarzý kamera yönü bazlý hareket
            Vector3 cameraForward = CharacterCamera.transform.forward;
            Vector3 cameraRight = CharacterCamera.transform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

            walkSpeed = 5f;
            runSpeed = 9f;


        }

        bool isMoving = moveDirection.magnitude > 0.1f;
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
        bool canRun = shiftPressed && stamina > staminaThreshold && isMoving;
        float currentSpeed = canRun ? runSpeed : walkSpeed;

        // Hareket uygula
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // TPS modunda yürürken karakter kamera yönüne dönsün
        if (moveDirection != Vector3.zero && !isAiming)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        // Zýplama
        if (Input.GetButtonDown("Jump") && isGrounded && canMove)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            PlayAnimationByIndex(2); // Jump
        }

        // Yerçekimi
        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

        // Stamina
        if (canRun)
        {
            stamina -= staminaDrainRate * Time.deltaTime;
            stamina = Mathf.Clamp01(stamina);
            CmdSetVfxVisible(true);
        }
        else if (!shiftPressed)
        {
            stamina += staminaRecoverRate * Time.deltaTime;
            stamina = Mathf.Clamp01(stamina);
            CmdSetVfxVisible(false);
        }

        if (StaminaImg != null)
            StaminaImg.fillAmount = stamina;

        // Animasyonlar
        if (!isGrounded)
        {
            if (velocity.y > 0.1f)
                PlayAnimationByIndex(2); // Jump
            else
                PlayAnimationByIndex(3); // Fall
        }
        else
        {
            if (isMoving)
            {
                if (canRun)
                    PlayAnimationByIndex(4); // Run
                else
                    PlayAnimationByIndex(1); // Walk
            }
            else
            {
                PlayAnimationByIndex(0); // Idle
            }
        }

        // Ýsim etiketi
        if (nametext != null)
        {
            nametext.transform.LookAt(CharacterCamera.transform);
            CmdSetPlayerName();
        }

        AttackActive();
        AttackPlayer();
    }




    void AttackActive() 
    {
       

        if (!weaponactive)
        {
            if (weaponactivecurrent < weaponactivelimit)
            {
                weaponactivecurrent += Time.deltaTime;
            }
            else
            {
                weaponactive = true;
                CmdSetWeaponVisible(true);
            }


        }
    }

    public void AttackPlayer() 
    {
        AppleGameControl appleGameControl = GetComponent<AppleGameControl>();

        
       
        if (Input.GetMouseButtonDown(0) && !isattacking && weaponactive && appleGameControl.enabled) 
        {
            weaponactivecurrent = 0;
            PlayAnimationByIndex(5);

            float attackDuration = currentAnimationClips[5].length;

            Invoke("attackEnd", attackDuration);

        }
    }

    public void AttackBow() 
    {
        if (!isattacking && weaponactive) 
        {
            Debug.Log("test bow");

            weaponactivecurrent = 0;
            PlayAnimationByIndex(5);

            float attackDuration = currentAnimationClips[5].length;

            Invoke("BowHide", 1);

           
        }
            
    }

    

    void AttackBowEnd() 
    {
        isattacking = false;

       
        
    }

    void BowHide() 
    {
        weaponactive = false;
        CmdSetWeaponVisible(false);
        Invoke("AttackBowEnd", 4);
    }


    void attackEnd() 
    {
        isattacking = false;
        weaponactive = false;
        CmdSetWeaponVisible(false);
    }

    public void PlayAnimationByIndex(int index)
    {
        currentAnimationClips = weaponactive ? shovelAnimationClips : animationClips;

        if (index < 0 || index >= currentAnimationClips.Length) return;

        string stateName = currentAnimationClips[index].name;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && !isattacking) 
        {
            if(index>4)
                isattacking = true;
            animator.Play(stateName, layer);

        }
    }

    [Command]
    void CmdSetWeaponVisible(bool visible)
    {
        RpcSetWeaponVisible(visible);
    }
    [ClientRpc]
    void RpcSetWeaponVisible(bool visible)
    {
        if (Weapon != null)
            Weapon.SetActive(visible);
    }


    [Command]
    void CmdSetVfxVisible(bool visible)
    {
        RpcSetVfxVisible(visible);
    }
    [ClientRpc]
    void RpcSetVfxVisible(bool visible)
    {
        if (SprintVfx != null)
            SprintVfx.SetActive(visible);
    }

}
