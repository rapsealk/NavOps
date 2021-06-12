using System;
using UnityEngine;

public class Engine : MonoBehaviour
{
     public enum ManeuverCommandId
    {
        IDLE = 0,
        FORWARD = 1,
        BACKWARD = 2,
        LEFT = 3,
        RIGHT = 4
    }

    public enum AttackCommandId
    {
        IDLE = 0,
        FIRE = 1,
        //PITCH_UP = 2,
        //PITCH_DOWN = 3
    }

    public const float maxFuel = 10000f;
    public float Fuel {
        get => _fuel;
        private set { _fuel = value; }
    }
    public const int MinSpeedLevel = -2;
    public const int MaxSpeedLevel = 2;
    public int SpeedLevel {
        get => _speedLevel;
        set { _speedLevel = Math.Min(Math.Max(MinSpeedLevel, value), MaxSpeedLevel); }
    }
    public const int MinSteerLevel = -2;
    public const int MaxSteerLevel = 2;
    public int SteerLevel {
        get => _steerLevel;
        set { _steerLevel = Math.Min(Math.Max(MinSteerLevel, value), MaxSteerLevel); }
    }
    public bool IsForward { get => SpeedLevel > 0; }
    public bool IsBackward { get => SpeedLevel < 0; }

    private Rigidbody m_Rigidbody;
    private float m_HorsePower = 60f;

    private float _fuel = 10000f;
    private int _speedLevel = 0;
    private int _steerLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (Application.platform == RuntimePlatform.OSXEditor
            || Application.platform == RuntimePlatform.WindowsEditor)
        {
            // m_HorsePower *= 2f;
        }
    }

    public void Reset()
    {
        SpeedLevel = 0;
        SteerLevel = 0;

        Fuel = maxFuel;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Fuel > 0)
        {
            float horsePower = m_HorsePower * ((SpeedLevel > 0) ? 1f : 0.5f);
            m_Rigidbody.AddForce(transform.forward * horsePower * SpeedLevel, ForceMode.Acceleration);
        }
        m_Rigidbody.transform.Rotate(Vector3.up, SteerLevel * 0.1f);
        */
    }

    void FixedUpdate()
    {
        if (SpeedLevel != 0)
        {
            Fuel -= 1.0f;
        }

        // Fuel -= HorsePower / 20f;
        if (Fuel > 0)
        {
            float horsePower = m_HorsePower * ((SpeedLevel > 0) ? 1f : 0.5f);
            m_Rigidbody.AddForce(transform.forward * horsePower * SpeedLevel, ForceMode.Acceleration);
        }
        m_Rigidbody.transform.Rotate(Vector3.up, SteerLevel * 0.1f);
    }
}
