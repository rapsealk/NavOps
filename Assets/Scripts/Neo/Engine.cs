using System;
using UnityEngine;

public class Engine : MonoBehaviour
{
    public const float maxFuel = 10000f;
    public float Fuel {
        get => _fuel;
        private set { _fuel = value; }
    }
    public const int MinSpeedLevel = -2;
    public const int MaxSpeedLevel = 2;
    public int SpeedLevel {
        get => _speedLevel;
        private set { _speedLevel = value; }
    }
    public const int MinSteerLevel = -2;
    public const int MaxSteerLevel = 2;
    public int SteerLevel {
        get => _steerLevel;
        private set { _steerLevel = value; }
    }
    public bool IsForward { get => SpeedLevel > 0; }
    public bool IsBackward { get => SpeedLevel < 0; }

    private Rigidbody m_Rigidbody;

    private float _fuel = 10000f;
    private int _speedLevel = 0;
    private int _steerLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
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
        if (SpeedLevel != 0)
        {
            Fuel -= 1.0f;
        }

        if (Fuel > 0)
        {
            float horsePower = (SpeedLevel > 0) ? 30f : 15f;
            m_Rigidbody.AddForce(transform.forward * horsePower * SpeedLevel, ForceMode.Acceleration);
        }
        m_Rigidbody.transform.Rotate(Vector3.up, SteerLevel * 0.1f);
    }

    void FixedUpdate()
    {
        // Fuel -= HorsePower / 20f;
    }

    public void SetSpeedLevel(int level)
    {
        SpeedLevel = Math.Min(Math.Max(MinSpeedLevel, level), MaxSpeedLevel);
    }

    public void SetSteerLevel(int level)
    {
        SteerLevel = Math.Min(Math.Max(MinSteerLevel, level), MaxSteerLevel);
    }
}
