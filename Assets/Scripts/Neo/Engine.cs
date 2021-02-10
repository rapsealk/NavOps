using UnityEngine;

public class Engine : MonoBehaviour
{
    public const float maxFuel = 10000f;
    public float Fuel {
        get => _fuel;
        private set { _fuel = value; }
    }

    private Rigidbody m_Rigidbody;

    private float HorsePower = 30f;
    private float _fuel = 10000f;

    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void Reset()
    {
        _fuel = maxFuel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Combust(float fuel = 1.0f)
    {
        if (_fuel < Mathf.Epsilon)
        {
            return;
        }

        Fuel -= fuel;
        m_Rigidbody.AddForce(transform.forward * fuel * HorsePower, ForceMode.Acceleration);
    }

    public void Steer(float rudder = 1.0f)
    {
        m_Rigidbody.transform.Rotate(Vector3.up, rudder * 0.1f);
    }
}
