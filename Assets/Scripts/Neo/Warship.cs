using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Warship : Agent, DamagableObject
{
    public const float k_MaxHealth = 10f;
    public Transform startingPoint;
    public Color rendererColor;
    public ParticleSystem explosion;
    public Warship target;
    [HideInInspector] public Rigidbody rb;
    public int playerId;
    public int teamId;
    [HideInInspector] public WeaponSystemsOfficer weaponSystemsOfficer;
    [HideInInspector] public float CurrentHealth {
        get => _currentHealth;
        private set { _currentHealth = value; }
    }
    [HideInInspector] public float AccumulatedDamage {
        get {
            float damage = _accumulatedDamage;
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

    private float _currentHealth = k_MaxHealth;
    private float _accumulatedDamage = 0;
    private bool m_IsCollisionWithWarship = false;
    private float m_PreviousHealth = k_MaxHealth;
    private float m_PreviousOpponentHealth = k_MaxHealth;

    private Vector3 m_AimingPoint;
    private const float k_AimingPointVerticalMin = -2f;
    private const float k_AimingPointVerticalMax = 1f;
    private Queue<int> m_InputQueue;

    public void Reset()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        transform.position = startingPoint.position;
        transform.rotation = startingPoint.rotation;

        CurrentHealth = k_MaxHealth;
        m_PreviousHealth = k_MaxHealth;
        m_PreviousOpponentHealth = k_MaxHealth;
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

        if (Application.platform == RuntimePlatform.WindowsEditor
            || Application.platform == RuntimePlatform.OSXEditor)
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

            // weaponSystemsOfficer.Aim(Quaternion.Euler(m_AimingPoint + transform.rotation.eulerAngles));
        }

        FrameCount += 1;
        TimeCount += Time.deltaTime;
    }

    void FixedUpdate()
    {
        Vector3 rotation = Vector3.zero;
        rotation.y = Geometry.GetAngleBetween(transform.position, target.transform.position);

        /*
        Vector3 projectilexz = transform.position;
        projectilexz.y = 0f;
        Vector3 targetxz = target.transform.position;
        targetxz.y = 0f;
        float r = Vector3.Distance(projectilexz, targetxz);
        float G = Physics.gravity.y;
        float vz = 8000f;
        rotation.x = Mathf.Atan((G * r) / (vz * 2f)) * Mathf.Rad2Deg;   // max: 140
        */
        rotation.x = m_AimingPoint.x;

        weaponSystemsOfficer.Aim(Quaternion.Euler(rotation));
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

        sensor.AddOneHotObservation(Engine.SpeedLevel + 2, 5);
        sensor.AddOneHotObservation(Engine.SteerLevel + 2, 5);

        sensor.AddObservation(CurrentHealth / (float) k_MaxHealth);
        sensor.AddObservation(target.CurrentHealth / (float) k_MaxHealth);

        ObservationCount += 1;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        float movementAction = vectorAction[0];
        float attackAction = vectorAction[1];

        // Movement Actions
        if (0f == movementAction)
        {
            // NOOP
        }
        else if (1f == movementAction)
        {
            Engine.SetSpeedLevel(Engine.SpeedLevel + 1);
        }
        else if (2f == movementAction)
        {
            Engine.SetSpeedLevel(Engine.SpeedLevel - 1);
        }
        else if (3f == movementAction)
        {
            Engine.SetSteerLevel(Engine.SteerLevel - 1);
        }
        else if (4f == movementAction)
        {
            Engine.SetSteerLevel(Engine.SteerLevel + 1);
        }

        // Attack Actions
        if (0f == attackAction)
        {
            // NOOP
        }
        else if (1f == attackAction)
        {
            uint usedAmmos = weaponSystemsOfficer.FireMainBattery();
            AddReward(usedAmmos / 10000f);
        }
        /*
        else if (2f == attackAction)
        {
            m_AimingPoint.y = (m_AimingPoint.y - 5f) % 360f;
        }
        else if (3f == attackAction)
        {
            m_AimingPoint.y = (m_AimingPoint.y + 5f) % 360f;
        }
        */
        else if (2f == attackAction)
        {
            m_AimingPoint.x = Mathf.Max(m_AimingPoint.x - 1f, k_AimingPointVerticalMin);
        }
        else if (3f == attackAction)
        {
            m_AimingPoint.x = Mathf.Min(m_AimingPoint.x + 1f, k_AimingPointVerticalMax);
        }

        // weaponSystemsOfficer.Aim(Quaternion.Euler(m_AimingPoint + transform.rotation.eulerAngles));

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
        // Debug.Log(string.Format("Distance: {0:F6}, Penalty: {1:F6}, Distance^2: {2:F6}", distance, penalty, Mathf.Pow(distance, 2f)));

        CurrentHealth -= AccumulatedDamage;
        float hitpointReward = (CurrentHealth - m_PreviousHealth) - (target.CurrentHealth - m_PreviousOpponentHealth);
        AddReward(hitpointReward / k_MaxHealth);
        m_PreviousHealth = CurrentHealth;
        m_PreviousOpponentHealth = target.CurrentHealth;

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
        /*
        if (playerId == 1)
        {
            actionsOut[0] = 0f;
            actionsOut[1] = 0f;
            if (m_InputQueue.Count > 0)
            {
                actionsOut[0] = (float) m_InputQueue.Dequeue();
            }
            return;
        }
        */

        actionsOut[0] = 0f; // Movement (5)
        actionsOut[1] = 0f; // Attack (4)

        Vector3 heading = transform.rotation.eulerAngles;
        Vector3 opponentHeading = target.transform.rotation.eulerAngles;

        float angle = (heading.y - opponentHeading.y + 360f) % 360f;    // (heading.y - opponentHeading.y) % 180f;

        ///
        const float radius = 100f;
        const float attackRange = 160f;
        const float attackRangeShort = 100f;

        Vector3 position = transform.position;
        Vector3 targetPosition = target.transform.position;
        Vector3 positionGapVector = position - targetPosition;
        float gradient = positionGapVector.z / positionGapVector.x;
        float x = Mathf.Sqrt(Mathf.Pow(radius, 2) / (Mathf.Pow(gradient, 2) + 1));
        float z = gradient * x;

        float distancePositive = Geometry.GetDistance(position, targetPosition + new Vector3(x, 0f, z));
        float distanceNegative = Geometry.GetDistance(position, targetPosition - new Vector3(x, 0f, z));
        Vector3 nextReachPosition = targetPosition - Mathf.Sign(distancePositive - distanceNegative) * new Vector3(x, 0f, z);
        Vector3 nextReachDirection = nextReachPosition - position;
        Debug.DrawRay(position, nextReachDirection, Color.red);

        RaycastHit hit;
        for (int i = 0; i < 8; i++)
        {
            float rad = (45f * i) / 180f * Mathf.PI;
            Vector3 dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            if (Physics.Raycast(position, dir, out hit, maxDistance: 160f))
            {
                Vector3 force = position - hit.point;
                nextReachDirection = (nextReachDirection.normalized + force.normalized) * nextReachDirection.magnitude;
                nextReachPosition = position + nextReachDirection;
            }
        }

        Debug.DrawRay(position, nextReachPosition, Color.white);

        if (Engine.SpeedLevel < 2)
        {
            actionsOut[0] = 1f;
            return;
        }

        float y = Geometry.GetAngleBetween(transform.position, nextReachPosition);
        float ydir = (transform.rotation.eulerAngles.y - y + 180f) % 360f - 180f;
        if (ydir > 0f)
        {
            actionsOut[0] = 3f; // Left
        }
        else if (ydir < 0f)
        {
            actionsOut[0] = 4f; // Right
        }

        /*
        if (m_AimingPoint.x < 1f)
        {
            actionsOut[1] = 3f;
        }
        else*/
        float distance = Vector3.Distance(position, targetPosition);
        if (/*Mathf.Min(distancePositive, distanceNegative)*/ distance < attackRange)
        {
            if (/*Mathf.Min(distancePositive, distanceNegative)*/ distance < attackRangeShort)
            {
                if (m_AimingPoint.x < 1f)
                {
                    actionsOut[1] = 3f; // Down
                }
                else if (m_AimingPoint.x > 1f)
                {
                    actionsOut[1] = 2f; // Up
                }
                else
                {
                    actionsOut[1] = 1f;
                }

                return;
            }

            if (m_AimingPoint.x > 0f)
            {
                actionsOut[1] = 2f;
            }
            else if (m_AimingPoint.x < 0f)
            {
                actionsOut[1] = 3f;
            }
            else
            {
                actionsOut[1] = 1f;
            }
        }
        // Debug.Log($"Distance: {distance} / attackRange: {attackRange} / AimingPoint: {m_AimingPoint.x}");
    }
    #endregion  // MLAgent
// #endif

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name.StartsWith("Water"))
        {
            return;
        }

        //Debug.Log($"Warship.OnCollisionEnter(collision: {collision.collider.tag}, {collision.collider.tag.StartsWith("Bullet")})");

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
        AccumulatedDamage += 1f;
    }
}
