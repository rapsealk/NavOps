using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Warship : Agent
{
    public const float m_Durability = 200f;
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
        private set {
            healthChange = value - _currentHealth;
            _currentHealth = value;
        }
    }
    public Transform battleField;
    [HideInInspector] public Engine Engine { get; private set; }

    private float healthChange = 0f;
    private float _currentHealth = 0f;
    private bool isCollisionWithWarship = false;

    private const float rewardFuelLoss = -1 / 21600;
    private const float rewardDistance = -1 / 100000;
    private const float rewardHpChange = 0.5f;

    public void Reset()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        transform.position = startingPoint.position;
        transform.rotation = startingPoint.rotation;

        CurrentHealth = m_Durability;
        isCollisionWithWarship = false;

        weaponSystemsOfficer.Reset();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                weaponSystemsOfficer.FireMainBattery();
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                // TODO: Animation
                weaponSystemsOfficer.FireTorpedoAt(target.transform.position);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                EndEpisode();
                target.EndEpisode();
            }

            float vertical = Input.GetAxisRaw("Vertical");
            float horizontal = Input.GetAxisRaw("Horizontal");

            // engine.Steer(horizontal);
            // engine.Combust(vertical);
        }
        */
    }

    void FixedUpdate()
    {
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
    }

    #region MLAgent
    public override void Initialize()
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

        Reset();
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Player
        sensor.AddObservation(transform.position.x / (battleField.transform.localScale.x / 2) - 1f);
        sensor.AddObservation(transform.position.z / (battleField.transform.localScale.z / 2) - 1f);

        float radian = (transform.rotation.eulerAngles.y % 360) * Mathf.Deg2Rad;
        sensor.AddObservation(Mathf.Cos(radian));
        sensor.AddObservation(Mathf.Sin(radian));

        // Opponent
        // sensor.AddObservation(target.transform.position.x / (battleField.transform.localScale.x / 2) - 1f);
        // sensor.AddObservation(target.transform.position.z / (battleField.transform.localScale.z / 2) - 1f);
        Vector3 relativePosition = target.transform.position - transform.position;
        sensor.AddObservation(relativePosition.x / (battleField.transform.localScale.x / 2) - 1f);
        sensor.AddObservation(relativePosition.z / (battleField.transform.localScale.x / 2) - 1f);

        float targetRadian = (target.transform.rotation.eulerAngles.y % 360) * Mathf.Deg2Rad;
        sensor.AddObservation(Mathf.Cos(targetRadian));
        sensor.AddObservation(Mathf.Sin(targetRadian));

        bool isEnemyTorpedoLaunched = false;
        Vector3 enemyTorpedoPosition = Vector3.zero;
        GameObject torpedo = target.weaponSystemsOfficer.torpedoInstance;
        if (torpedo != null)
        {
            isEnemyTorpedoLaunched = true;
            enemyTorpedoPosition = torpedo.transform.position;
        }
        sensor.AddObservation(isEnemyTorpedoLaunched);
        sensor.AddObservation(enemyTorpedoPosition.x / (battleField.transform.localScale.x / 2) - 1f);
        sensor.AddObservation(enemyTorpedoPosition.z / (battleField.transform.localScale.z / 2) - 1f);

        // Weapon
        WeaponSystemsOfficer.BatterySummary[] batterySummary = weaponSystemsOfficer.Summary();
        for (int i = 0; i < batterySummary.Length; i++)
        {
            WeaponSystemsOfficer.BatterySummary summary = batterySummary[i];
            sensor.AddObservation(Mathf.Cos(summary.rotation.x));
            sensor.AddObservation(Mathf.Sin(summary.rotation.x));
            sensor.AddObservation(Mathf.Cos(summary.rotation.y));
            sensor.AddObservation(Mathf.Sin(summary.rotation.y));
            sensor.AddObservation(summary.isReloaded);
            sensor.AddObservation(summary.cooldown);
            sensor.AddObservation(summary.isDamaged);
            sensor.AddObservation(summary.repairProgress);
        }
        sensor.AddObservation(weaponSystemsOfficer.isTorpedoReady);
        sensor.AddObservation(weaponSystemsOfficer.torpedoCooldown / WeaponSystemsOfficer.m_TorpedoReloadTime);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        float enginePower = Mathf.Clamp(vectorAction[0], -1f, 1f);
        float rudderPower = Mathf.Clamp(vectorAction[1], -1f, 1f);
        bool[] fireMainBatteryCommands = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            fireMainBatteryCommands[i] = (vectorAction[2+i] >= 0.5f);
        }
        Vector2[] aimOffsets = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            aimOffsets[i].x = Mathf.Clamp(vectorAction[i*2+8], -1f, 1f);
            aimOffsets[i].y = Mathf.Clamp(vectorAction[i*2+9], -1f, 1f);
        }
        bool launchTorpedo = (vectorAction[5] >= 0.5f);

        for (int i = 0; i < 6; i++)
        {
            if (fireMainBatteryCommands[i])
            {
                weaponSystemsOfficer.FireMainBattery(i, aimOffsets[i]);
            }
        }

        if (launchTorpedo)
        {
            weaponSystemsOfficer.FireTorpedoAt(target.transform.position);
        }

        Engine.Combust(enginePower);
        Engine.Steer(rudderPower);

        /**
         * Reward
         */
        AddReward(rewardFuelLoss);

        float distance = Vector3.Distance(transform.position, target.transform.position);
        float penalty = Mathf.Pow(distance, 2f) * rewardDistance;
        AddReward(penalty);
        
        float damageTaken = healthChange / m_Durability;
        healthChange = 0f;
        AddReward(damageTaken * rewardHpChange);

        /**
         * EndEpisode
         */
        if (isCollisionWithWarship)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else if (target.CurrentHealth <= 0f + Mathf.Epsilon)
        {
            SetReward(1f);
            EndEpisode();
        }
        else if (CurrentHealth <= 0f + Mathf.Epsilon)
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        //
    }
    #endregion  // MLAgent

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
            isCollisionWithWarship = true;
            return;
        }
        else if (collision.collider.tag == "Torpedo")
        {
            CurrentHealth = 0f;
        }
        else if (collision.collider.tag.StartsWith("Bullet")
                 && !collision.collider.tag.EndsWith(teamId.ToString()))
        {
            float damage = collision.rigidbody?.velocity.magnitude ?? 0f;
            CurrentHealth -= damage;
        }
        else if (collision.collider.tag == "Terrain")
        {
            float damage = rb.velocity.magnitude * rb.mass;
            CurrentHealth -= damage;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        explosion.transform.position = other.transform.position;
        explosion.transform.rotation = other.transform.rotation;
        explosion.Play();
        // explosion.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
