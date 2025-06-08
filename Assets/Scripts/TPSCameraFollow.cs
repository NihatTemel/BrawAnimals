using UnityEngine;

public class TPSCameraFollow : MonoBehaviour
{
    public Transform target;           // Karakterin transformu
    public Vector3 offset = new Vector3(0, 2, -5); // Kamera mesafesi
    public float rotationSpeed = 5f;

    public float minY = -35f;
    public float maxY = 60f;

    private float currentYaw = 0f;
    private float currentPitch = 10f;

    void LateUpdate()
    {
        target = transform.root.GetComponent<OnlinePrefabLobbyController>().currentCharacter.transform;
        if (target == null) return;

        // Fare giri�i
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        // Kameray� d�nd�r
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f); // Hafif yukar� bakar
    }
}
