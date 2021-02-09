using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public float Distance = float.MaxValue;

    private Vector3 InstantiatedPosition;

    // Start is called before the first frame update
    void Start()
    {
        InstantiatedPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * Time.deltaTime * 256f;

        if (Vector3.Distance(transform.position, InstantiatedPosition) >= Distance)
        {
            Object.Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("LaserBeam collision");
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("LaserBeam trigger");
    }
}
