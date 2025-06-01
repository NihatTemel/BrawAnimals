using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class PlayerMainController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Animation")]
    public Animator animator;
    public AnimationClip[] animationClips;
    public int layer = 0;

    public Camera CharacterCamera;
    private void Start()
    {
        


        StartingSettings();

        
    }

    void StartingSettings() 
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local)
        {
            //enabled = false;
            return;
        }
        CharacterCamera = transform.root.GetComponent<OnlinePrefabController>().TPSCamera.GetComponent<Camera>();
        if (transform.root.GetComponent<OnlinePrefabController>()._local)
            CharacterCamera.gameObject.SetActive(true);


       
        controller = GetComponent<CharacterController>();
    }


    private void Update()
    {
        if (!transform.root.GetComponent<OnlinePrefabController>()._local) return;

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Kameraya göre yön belirleme
        Vector3 cameraForward = CharacterCamera.transform.forward;
        Vector3 cameraRight = CharacterCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

        // Hareket
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Yöne dön
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        // Zýplama
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            PlayAnimationByIndex(2); // Jump
        }

        // Yerçekimi
        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);

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
            if (moveDirection.magnitude > 0.1f)
                PlayAnimationByIndex(1); // Run
            else
                PlayAnimationByIndex(0); // Idle
        }
    }

    public void PlayAnimationByIndex(int index)
    {
        if (index < 0 || index >= animationClips.Length) return;

        string stateName = animationClips[index].name;
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName)) // aynýysa tekrar oynatma
            animator.Play(stateName, layer);
    }

    
   
}
