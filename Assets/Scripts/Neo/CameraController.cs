using UnityEngine;

/*
public class CameraController : MonoBehaviour
{
    public Warship TargetObject;
    public Radar Radar;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Mouse X") * Time.deltaTime;
        float vertical = -1 * Input.GetAxis("Mouse Y") * Time.deltaTime;

        transform.RotateAround(TargetObject.transform.position, TargetObject.transform.up, horizontal * 100);
        transform.Rotate(Vector3.right, vertical * 50);

        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.x = (rotation.x + 360) % 360;
        rotation.x = (rotation.x > 180f) ? (rotation.x - 360f) : rotation.x;
        if (Mathf.Abs(rotation.x) > 25f + Mathf.Epsilon)
        {
            rotation.x = Mathf.Sign(rotation.x) * 25f;
            transform.rotation = Quaternion.Euler(rotation);
        }

        Radar.ViewDirection = rotation.y;

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            //string quaternion = string.Format("({0:F2}, {1:F2}, {2:F2}, {3:F2})", transform.rotation.w, transform.rotation.x, transform.rotation.y, transform.rotation.z);
            //Debug.Log($"Transform.Rotation: (Quaternion=({quaternion}), Euler={transform.rotation.eulerAngles})");
            //TargetObject.weaponSystemsOfficer.Aim(transform.rotation);
        }
    }
}
*/
