using UnityEngine;

public class TPSCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5); // Third person offset
    public Vector3 aimOffset = new Vector3(1.18200004f, 1.46000004f, -1.76699996f); // Aiming offset

    public float rotationSpeed = 5f;
    public float minY = -35f;
    public float maxY = 60f;

    private float currentYaw = 0f;
    private float currentPitch = 10f;

    public bool aiming = false;
    public GameObject AimingPosition;

    public float Looky = 1.5f;
    public float Lookz = 1.5f;

    bool changingaim = false;

    private float transitionSpeed = 5f;
    private float transitionProgress = 0f;

    private Vector3 currentOffset; // <--- EKLENDÝ
    private Vector3 transitionStartOffset;
    private bool isTransitioning = false;

    public Transform aimCamTarget;

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
            }
        }

        currentOffset = offset; // Baþlangýçta third-person offset
    }

    void LateUpdate()
    {
        if (aiming != changingaim)
        {
            changingaim = aiming;
            isTransitioning = true;
            transitionProgress = 0f;
            transitionStartOffset = currentOffset;
        }

        if (isTransitioning)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;

            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;
            }

            // Smooth offset geçiþi
            Vector3 targetOffset = aiming ? aimOffset : offset;
            currentOffset = Vector3.Lerp(transitionStartOffset, targetOffset, transitionProgress);
        }

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

        Vector3 aimingPosition = target.position + target.rotation * currentOffset;
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
        Vector3 desiredPosition = target.position + rotation * currentOffset;

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * Looky + Vector3.right * Lookz);
    }

    public bool Aiming => aiming;
}
