using UnityEngine;

public class TPSCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float rotationSpeed = 5f;
    public float minY = -35f;
    public float maxY = 60f;

    private float currentYaw = 0f;
    private float currentPitch = 10f;

    public bool aiming = false;

    public GameObject AimingPosition;
    public Vector3 AimoffSet = new Vector3(1, 1, -1);
    public float AimingRight = 1;
    public float AimingUp;

    public Transform aimCamTarget;
    public Vector3 aimOffsetFromCamera;


    public float Looky=1.5f;
    public float Lookz=1.5f;


    void Start()
    {
        var character = transform.root.GetComponent<OnlinePrefabLobbyController>().currentCharacter;
        target = character.transform;

        if (character != null)
        {
            BowGameControl bowControl = character.GetComponent<BowGameControl>();
            if (bowControl != null && bowControl.isActiveAndEnabled)
            {
                target = character.transform;
                aimCamTarget = character.GetComponent<BowGameControl>().AimCamPosition.transform;

                // Kamera ile aimCamTarget arasýndaki farký al
                aimOffsetFromCamera = aimCamTarget.position - transform.position;
            }
        }
    }

    void LateUpdate()
    {
        if (aiming)
            SetAimingView();
        else
            SetThirdPersonView();
    }

    void SetAimingView()
    {
        if (target == null) return;
        Vector3 lookDir = transform.forward;

        Debug.Log("1--> " + lookDir);
        Debug.Log("2--> " + target.rotation);


       


        Debug.Log("3--> " + target.rotation);

        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        aimOffsetFromCamera = new Vector3(1.18200004f, 1.46000004f, -1.76699996f);
        Vector3 aimingPosition = target.position + target.rotation * aimOffsetFromCamera;

        transform.position = aimingPosition;
        transform.rotation = Quaternion.Euler(currentPitch, target.eulerAngles.y, 0f);
    }

    void SetThirdPersonView()
    {
        if (target == null) return;

        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * Looky + Vector3.right*Lookz);
    }

    public bool Aiming => aiming;
}
