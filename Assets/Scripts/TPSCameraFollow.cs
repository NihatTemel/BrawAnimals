using UnityEngine;

public class TPSCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public Vector3 aimOffset = new Vector3(1.182f, 1.46f, -1.767f);

    public float rotationSpeed = 5f;
    public float minY = -15f;
    public float maxY = 50f;

    private float currentYaw = 0f;
    private float currentPitch = 10f;

    public bool aiming = false;
    public GameObject AimingPosition;

    public float Looky = 1.5f;
    public float Lookz = 0f;

    private bool changingaim = false;

    private float transitionSpeed = 5f;
    private float transitionProgress = 0f;

    private Vector3 currentOffset;
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
                aimCamTarget = bowControl.AimCamPosition.transform;
            }
        }

        currentOffset = offset; // Baþlangýç offset (TPS)
    }

    void LateUpdate()
    {
        // Aiming deðiþtiyse transition baþlat
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

            Vector3 targetOffset = aiming ? aimOffset : offset;
            currentOffset = Vector3.Lerp(transitionStartOffset, targetOffset, transitionProgress);
        }

        if (aiming)
            SetAimingView();
        else
            SetThirdPersonView();


        var character = transform.root.GetComponent<OnlinePrefabLobbyController>().currentCharacter;
        Ray ray = GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        foreach (var h in hits)
        {
            if (h.collider.transform.root == character.transform) continue;

            targetPoint = h.point;
            break;
        }

        if (targetPoint == Vector3.zero)
        {
            targetPoint = ray.origin + ray.direction * 100f;
        }





    }

    public Vector3 targetPoint;


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
