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

    public TMP_Text nametext;
    [SyncVar] public string playername;

    [Header("Attack")]

    public bool weaponactive = false;
    public float weaponactivelimit = 5;
    public float weaponactivecurrent = 0;
    public GameObject Weapon;
    public bool isattacking = false;

    [SyncVar] public bool canMove = true;

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

        playername = transform.root.GetComponent<OnlinePrefabLobbyController>().playerName;
        CmdSetPlayerName();
    }


    [Command(requiresAuthority = false)]
    void CmdSetPlayerName() 
    {
        RpcSetPlayerName();
    }

    [ClientRpc]
    void RpcSetPlayerName() 
    {
        nametext.text = playername;
    }

    private void Update()
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;

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
        

        Vector3 cameraForward = CharacterCamera.transform.forward;
        Vector3 cameraRight = CharacterCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

        bool isMoving = moveDirection.magnitude > 0.1f;
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
        bool canRun = shiftPressed && stamina > staminaThreshold && isMoving;

        float currentSpeed = canRun ? runSpeed : walkSpeed;

        // Hareket

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Yöne dön
        if (moveDirection != Vector3.zero)
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

        // Stamina Güncelleme
        if (canRun)
        {
            stamina -= staminaDrainRate * Time.deltaTime;
            stamina = Mathf.Clamp01(stamina);
        }
        else if (!shiftPressed)
        {
            stamina += staminaRecoverRate * Time.deltaTime;
            stamina = Mathf.Clamp01(stamina);
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

        if (nametext != null)
            nametext.transform.LookAt(CharacterCamera.transform);
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

    void AttackPlayer() 
    {
        if (Input.GetMouseButtonDown(0) && !isattacking && weaponactive) 
        {
            weaponactivecurrent = 0;
            PlayAnimationByIndex(5);

            float attackDuration = currentAnimationClips[5].length;

            Invoke("attackEnd", attackDuration);

        }
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
    



}
