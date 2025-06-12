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

    private Transform aimCamTarget;

    void Start()
    {
        var character = transform.root.GetComponent<OnlinePrefabLobbyController>().currentCharacter;
        if (character != null)
        {
            target = character.transform;
            aimCamTarget = character.GetComponent<BowGameControl>().AimCamPosition.transform;
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

        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        // Kamera karakterin pozisyonuna göre ayarlanýr, sadece yukarý-aþaðý bakar
        transform.position = target.position + target.TransformDirection(AimoffSet);
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
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    public bool Aiming => aiming;
}
