using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarCamera : MonoBehaviour
{
    public GameManager GameManager;
    public NavOps.Grpc.Warship Player;

    private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // transform.position = Player.transform.position + Player.transform.forward * -7.5f + Player.transform.up * 20f;
        // transform.rotation = Player.transform.rotation;

        NavOps.Grpc.Warship unit = GameManager.TaskForceRed.Units[0];
        if (unit.IsDetected)
        {
            int layerMask = LayerMask.NameToLayer("Opponent");
            camera.cullingMask = camera.cullingMask | (1 << layerMask);

            /*
            Vector3 targetDirection = unit.transform.position - transform.position;

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, 20 * Time.deltaTime, 0.0f);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);
            */
            transform.position = Player.transform.position + (unit.transform.position - Player.transform.position).normalized * -7.5f + Player.transform.up * 20f;

            Vector3 rotation = Vector3.zero;
            rotation.x = 15f;
            rotation.y = Geometry.GetAngleBetween(transform.position, unit.transform.position);
            transform.rotation = Quaternion.Euler(rotation);
        }
        else
        {
            int layerMask = LayerMask.NameToLayer("Opponent");
            camera.cullingMask = int.MaxValue ^ (1 << layerMask);

            transform.position = Player.transform.position + Player.transform.forward * -7.5f + Player.transform.up * 20f;

            Vector3 rotation = Player.transform.rotation.eulerAngles;
            rotation.x = 15f;
            transform.rotation = Quaternion.Euler(rotation);
            // transform.rotation = Player.transform.rotation;
        }
    }
}
