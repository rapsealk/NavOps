using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Warship : Agent, DamagableObject
{
    public const int k_MaxHealth = 20;
    public Transform startingPoint;
    public Color rendererColor;
    public ParticleSystem explosion;
    public Warship target;
    [HideInInspector] public Rigidbody rb;
    public int playerId;
    public int teamId;
    [HideInInspector] public WeaponSystemsOfficer weaponSystemsOfficer;
    [HideInInspector] public int CurrentHealth {
        get => _currentHealth;
        private set {
            AccumulatedDamage = value - _currentHealth;
            _currentHealth = value;
        }
    }
    [HideInInspector] public int AccumulatedDamage {
        get {
            int damage = _accumulatedDamage;
            _accumulatedDamage = 0;
            return damage;
        }
        private set { _accumulatedDamage = value; }
    }
    public Transform battleField;
    [HideInInspector] public Engine Engine { get; private set; }
    [HideInInspector] public int EpisodeCount = 0;
    [HideInInspector] public int ObservationCount;
    [HideInInspector] public int ActionCount;
    [HideInInspector] public int FrameCount;
    [HideInInspector] public float TimeCount;

    private int _currentHealth = k_MaxHealth;
    private int _accumulatedDamage = 0;
    private bool m_IsCollisionWithWarship = false;

    private const float k_RewardFuelLoss = -1 / 21600;
    private const float k_RewardDistance = -1 / 100000;
    private const float k_RewardHpChange = 0.5f;
    private Vector3 m_AimingPoint;
    private const float k_AimingPointVerticalMin = -5f;
    private const float k_AimingPointVerticalMax = 3f;
    private Queue<int> m_InputQueue;

    public void Reset()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        transform.position = startingPoint.position;
        transform.rotation = startingPoint.rotation;

        CurrentHealth = k_MaxHealth;
        AccumulatedDamage = 0;
        m_IsCollisionWithWarship = false;

        m_AimingPoint = Vector3.zero;   // new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
        weaponSystemsOfficer.Reset();
        Engine.Reset();

        m_InputQueue.Clear();
        
        EpisodeCount += 1;
        ObservationCount = 0;
        ActionCount = 0;
        FrameCount = 0;
        TimeCount = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerId != 1)
        {
            return;
        }

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (playerId == 2)
            {
                m_InputQueue.Enqueue(Random.Range(0, 10));
                return;
            }

            KeyCode[] keyCodes = new KeyCode[] {
                KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D,
                KeyCode.Space,
                KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow
            };
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    m_InputQueue.Enqueue(i+1);
                    break;
                }
            }

            weaponSystemsOfficer.Aim(Quaternion.Euler(m_AimingPoint + transform.rotation.eulerAngles));
        }

        FrameCount += 1;
        TimeCount += Time.deltaTime;
    }

    void FixedUpdate()
    {
        /*
        Vector3 rotation = Vector3.zero;
        rotation.y = Geometry.GetAngleBetween(transform.position, target.transform.position);

        Vector3 projectilexz = transform.position;
        projectilexz.y = 0f;
        Vector3 targetxz = target.transform.position;
        targetxz.y = 0f;
        float r = Vector3.Distance(projectilexz, targetxz);
        float G = Physics.gravity.y;
        float vz = 8000f;
        rotation.x = Mathf.Atan((G * r) / (vz * 2f)) * Mathf.Rad2Deg;   // max: 140

        weaponSystemsOfficer.Aim(Quaternion.Euler(rotation));
        */
    }

    private void Init()
    {
        weaponSystemsOfficer = GetComponent<WeaponSystemsOfficer>();
        weaponSystemsOfficer.Assign(teamId, playerId);

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        Engine = GetComponent<Engine>();

        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.color = rendererColor;
        }

        m_InputQueue = new Queue<int>();

        Reset();
    }

// #if !UNITY_EDITOR
    #region MLAgent
    public override void Initialize()
    {
        Init();
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }

    public override void CollectObservations(VectorSensor sensor)   // 54
    {
        // Player
        Vector2 playerPosition = new Vector2(transform.position.x / battleField.transform.localScale.x,
                                             transform.position.z / battleField.transform.localScale.z);
        if (teamId == 1)
        {
            sensor.AddObservation(playerPosition);
        }
        else
        {
            sensor.AddObservation(-playerPosition);
        }

        float radian = (transform.rotation.eulerAngles.y % 360) * Mathf.Deg2Rad;
        if (teamId == 1)
        {
            sensor.AddObservation(Mathf.Cos(radian));
            sensor.AddObservation(Mathf.Sin(radian));
        }
        else
        {
            sensor.AddObservation(-Mathf.Cos(radian));
            sensor.AddObservation(-Mathf.Sin(radian));
        }

        // Opponent
        Vector2 opponentPosition = new Vector2(target.transform.position.x / battleField.transform.localScale.x,
                                               target.transform.position.z / battleField.transform.localScale.z);
        /*
        if (teamId == 1)
        {
            sensor.AddObservation(opponentPosition);
        }
        else
        {
            sensor.AddObservation(-opponentPosition);
        }
        */
        Vector2 relativePosition = playerPosition - opponentPosition;
        if (teamId == 1)
        {
            sensor.AddObservation(relativePosition);
        }
        else
        {
            sensor.AddObservation(-relativePosition);
        }

        float targetRadian = (target.transform.rotation.eulerAngles.y % 360) * Mathf.Deg2Rad;
        if (teamId == 1)
        {
            sensor.AddObservation(Mathf.Cos(targetRadian));
            sensor.AddObservation(Mathf.Sin(targetRadian));
        }
        else
        {
            sensor.AddObservation(-Mathf.Cos(targetRadian));
            sensor.AddObservation(-Mathf.Sin(targetRadian));
        }

        /* Torpedo
        bool isEnemyTorpedoLaunched = false;
        Vector3 enemyTorpedoPosition = Vector3.zero;
        GameObject torpedo = target.weaponSystemsOfficer.torpedoInstance;
        if (torpedo != null)
        {
            isEnemyTorpedoLaunched = true;
            enemyTorpedoPosition = torpedo.transform.position;
        }*/
        //sensor.AddObservation(isEnemyTorpedoLaunched);
        //sensor.AddObservation(enemyTorpedoPosition.x / (battleField.transform.localScale.x / 2) - 1f);
        //sensor.AddObservation(enemyTorpedoPosition.z / (battleField.transform.localScale.z / 2) - 1f);
        //sensor.AddObservation(weaponSystemsOfficer.isTorpedoReady);
        //sensor.AddObservation(weaponSystemsOfficer.torpedoCooldown / WeaponSystemsOfficer.m_TorpedoReloadTime);

        // Weapon
        WeaponSystemsOfficer.BatterySummary[] batterySummary = weaponSystemsOfficer.Summary();
        for (int i = 0; i < batterySummary.Length; i++)
        {
            WeaponSystemsOfficer.BatterySummary battery = batterySummary[i];
            sensor.AddObservation(battery.rotation.x / Turret.k_VerticalMin);
            sensor.AddObservation(Mathf.Cos(battery.rotation.y));
            sensor.AddObservation(Mathf.Sin(battery.rotation.y));
            sensor.AddObservation(battery.isReloaded);
            sensor.AddObservation(battery.cooldown);
            sensor.AddObservation(battery.isDamaged);
            sensor.AddObservation(battery.repairProgress);
        }

        Vector2 aimingPoint = new Vector2(m_AimingPoint.x, m_AimingPoint.y);
        sensor.AddOneHotObservation((int) (aimingPoint.x - k_AimingPointVerticalMin), (int) (k_AimingPointVerticalMax - k_AimingPointVerticalMin) + 1);
        sensor.AddObservation(Mathf.Cos(aimingPoint.y * Mathf.Deg2Rad));
        sensor.AddObservation(Mathf.Sin(aimingPoint.y * Mathf.Deg2Rad));

        sensor.AddObservation(weaponSystemsOfficer.Ammo / (float) WeaponSystemsOfficer.maxAmmo);
        sensor.AddObservation(Engine.Fuel / Engine.maxFuel);

        sensor.AddOneHotObservation(Engine.SpeedLevel+2, 5);
        sensor.AddOneHotObservation(Engine.SteerLevel+2, 5);

        sensor.AddObservation(CurrentHealth / (float) k_MaxHealth);
        sensor.AddObservation(target.CurrentHealth / (float) k_MaxHealth);

        ObservationCount += 1;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        float action = vectorAction[0];

        if (action == 0f)
        {
            // NOOP
        }
        else if (action == 1f)
        {
            Engine.SetSpeedLevel(Engine.SpeedLevel + 1);
        }
        else if (action == 2f)
        {
            Engine.SetSpeedLevel(Engine.SpeedLevel - 1);
        }
        else if (action == 3f)
        {
            Engine.SetSteerLevel(Engine.SteerLevel - 1);
        }
        else if (action == 4f)
        {
            Engine.SetSteerLevel(Engine.SteerLevel + 1);
        }
        else if (action == 5f)
        {
            weaponSystemsOfficer.FireMainBattery();
        }
        else if (action == 6f)
        {
            m_AimingPoint.y = (m_AimingPoint.y - 5f) % 360f;
        }
        else if (action == 7f)
        {
            m_AimingPoint.y = (m_AimingPoint.y + 5f) % 360f;
        }
        else if (action == 8f)
        {
            m_AimingPoint.x = Mathf.Max(m_AimingPoint.x - 1f, k_AimingPointVerticalMin);
        }
        else if (action == 9f)
        {
            m_AimingPoint.x = Mathf.Min(m_AimingPoint.x + 1f, k_AimingPointVerticalMax);
        }

        weaponSystemsOfficer.Aim(Quaternion.Euler(m_AimingPoint + transform.rotation.eulerAngles));

        // Default Time Penalty
        Vector2 playerPosition = new Vector2(transform.position.x / battleField.transform.localScale.x,
                                             transform.position.z / battleField.transform.localScale.z);
        Vector2 opponentPosition = new Vector2(target.transform.position.x / battleField.transform.localScale.x,
                                               target.transform.position.z / battleField.transform.localScale.z);
        // float penalty = Mathf.Max(0.0001f, Vector2.Distance(playerPosition, opponentPosition));
        float distance = Vector2.Distance(playerPosition, opponentPosition);
        float penalty = 4 * Mathf.Pow(distance, 2f) / 10000;
        // float penalty = Vector2.Distance(playerPosition, opponentPosition) / 10000;
        AddReward(-penalty);
        Debug.Log(string.Format("Distance: {0:F6}, Penalty: {1:F6}, Distance^2: {2:F6}", distance, penalty, Mathf.Pow(distance, 2f)));

        // EndEpisode
        if (m_IsCollisionWithWarship)
        {
            SetReward(-1f);
            target.SetReward(-1f);
            EndEpisode();
            target.EndEpisode();
        }
        else if (Engine.Fuel <= 0f + Mathf.Epsilon
                 || weaponSystemsOfficer.Ammo == 0)
        {
            // Time-out
            if (CurrentHealth > target.CurrentHealth)
            {
                SetReward(1f);
                target.SetReward(-1f);
                EndEpisode();
                target.EndEpisode();
            }
            else if (CurrentHealth < target.CurrentHealth)
            {
                SetReward(-1f);
                target.SetReward(1f);
                EndEpisode();
                target.EndEpisode();
            }
            else
            {
                SetReward(-1f);
                target.SetReward(-1f);
                EndEpisode();
                target.EndEpisode();
            }
        }
        else if (CurrentHealth <= 0f + Mathf.Epsilon)
        {
            SetReward(-1f);
            target.SetReward(1f);
            EndEpisode();
            target.EndEpisode();
        }
        else if (target.CurrentHealth <= 0f + Mathf.Epsilon)
        {
            SetReward(1f);
            target.SetReward(-1f);
            EndEpisode();
            target.EndEpisode();
        }

        ActionCount += 1;
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0f;

        if (m_InputQueue.Count > 0)
        {
            actionsOut[0] = (float) m_InputQueue.Dequeue();
        }
    }
    #endregion  // MLAgent
// #endif

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name.StartsWith("Water"))
        {
            return;
        }

        explosion.transform.position = collision.transform.position;
        explosion.transform.rotation = collision.transform.rotation;
        explosion.Play();

        if (collision.collider.tag == "Player")
        {
            m_IsCollisionWithWarship = true;
        }
        else if (collision.collider.tag == "Torpedo")
        {
            CurrentHealth = 0;
        }
        else if (collision.collider.tag.StartsWith("Bullet")
                 && !collision.collider.tag.EndsWith(teamId.ToString()))
        {
            //float damage = collision.rigidbody?.velocity.magnitude ?? 20f;
            //CurrentHealth -= damage;
            OnDamageTaken();
        }
        /*
        else if (collision.collider.tag == "Terrain")
        {
            //float damage = rb.velocity.magnitude * rb.mass;
            //CurrentHealth -= damage;
            CurrentHealth = 0;
        }
        */
    }

    /*
    public void OnTriggerEnter(Collider other)
    {
        //explosion.transform.position = other.transform.position;
        //explosion.transform.rotation = other.transform.rotation;
        //explosion.Play();
        // explosion.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        StartCoroutine(DisplayExplosionEffect(other));
    }

    private IEnumerator DisplayExplosionEffect(Collider other)
    {
        explosion.transform.position = other.transform.position;
        explosion.transform.rotation = other.transform.rotation;
        explosion.Play();

        yield return new WaitForSeconds(2f);

        explosion.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        yield break;
    }
    */

    public void OnDamageTaken()
    {
        CurrentHealth -= 1;
    }
}
