using UnityEngine;

public class JeepOrbitCamera : MonoBehaviour
{
    [Header("Camera Points")]
    public Transform driverViewPoint; 
    public Transform jeepCenter;     

    [Header("Distance & Limits")]
    public float thirdPersonDistance = 5.0f; 
    public float lookSensitivity = 2f;
    public float pitchMin = -20f; 
    public float pitchMax = 60f;  
    public float yawLimitFP = 150f; 

    [Header("Vibration Settings (NEW)")]
    [Tooltip("How much the camera shakes. Try 0.01 to 0.04.")]
    public float shakeIntensity = 0.02f;
    [Tooltip("How fast the shake vibrates.")]
    public float shakeSpeed = 20f;
    [Tooltip("Check this if you want shake in 3rd person too")]
    public bool shakeInThirdPerson = false;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool isFirstPerson = true;
    private Rigidbody jeepRigidbody;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rotationY = 0f;
        rotationX = 0f;

        // Find the Rigidbody to detect movement speed for the shake
        if (jeepCenter != null && jeepCenter.root.GetComponent<Rigidbody>())
        {
            jeepRigidbody = jeepCenter.root.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.C))
        {
            isFirstPerson = !isFirstPerson;
        }

        rotationY += Input.GetAxis("Mouse X") * lookSensitivity;
        rotationX -= Input.GetAxis("Mouse Y") * lookSensitivity;

        if (isFirstPerson)
        {
            rotationY = Mathf.Clamp(rotationY, -yawLimitFP / 2f, yawLimitFP / 2f);
            rotationX = Mathf.Clamp(rotationX, -45f, 45f);
        }
        else
        {
            rotationX = Mathf.Clamp(rotationX, pitchMin, pitchMax);
        }
    }

    void LateUpdate()
    {
        Vector3 shakeOffset = Vector3.zero;

        // Calculate shake if moving
        if (jeepRigidbody != null && jeepRigidbody.velocity.magnitude > 0.1f)
        {
            if (isFirstPerson || shakeInThirdPerson)
            {
                shakeOffset = CalculateShake();
            }
        }

        if (isFirstPerson)
        {
            // Apply First Person with Shake
            transform.position = driverViewPoint.position + shakeOffset;
            transform.rotation = driverViewPoint.rotation * Quaternion.Euler(rotationX, rotationY, 0f);
        }
        else
        {
            // Apply Orbit Logic
            Quaternion rotation = jeepCenter.rotation * Quaternion.Euler(rotationX, rotationY, 0f);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -thirdPersonDistance);
            
            // Apply shake to the final position
            transform.position = (rotation * negDistance + jeepCenter.position) + shakeOffset;
            transform.rotation = rotation;
        }
    }

    Vector3 CalculateShake()
    {
        float time = Time.time * shakeSpeed;
        float x = (Mathf.PerlinNoise(time, 0f) * 2f - 1f) * shakeIntensity;
        float y = (Mathf.PerlinNoise(0f, time) * 2f - 1f) * shakeIntensity;
        float z = (Mathf.PerlinNoise(time, time) * 2f - 1f) * (shakeIntensity * 0.5f);
        return new Vector3(x, y, z);
    }
}