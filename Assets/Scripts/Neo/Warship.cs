using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Warship : Agent
{
    public const int m_Durability = 20;
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
            healthChange = value - _currentHealth;
            _currentHealth = value;
        }
    }
    public Transform battleField;
    [HideInInspector] public Engine Engine { get; private set; }

    private float healthChange = 0f;
    private int _currentHealth = m_Durability;
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
        Engine.Reset();
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
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
        */
    }

    // Update is called once per frame
    void Update()
    {

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

// #if !UNITY_EDITOR
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

    public override void CollectObservations(VectorSensor sensor)   // 54
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

        // Weapon
        WeaponSystemsOfficer.BatterySummary[] batterySummary = weaponSystemsOfficer.Summary();
        for (int i = 0; i < batterySummary.Length; i++)
        {
            WeaponSystemsOfficer.BatterySummary summary = batterySummary[i];
            sensor.AddObservation(Mathf.Cos(summary.rotation.y));
            sensor.AddObservation(Mathf.Sin(summary.rotation.y));
            sensor.AddObservation(summary.IsTargetLocked);
            sensor.AddObservation(summary.isReloaded);
            sensor.AddObservation(summary.cooldown);
            sensor.AddObservation(summary.isDamaged);
            sensor.AddObservation(summary.repairProgress);
        }
        //sensor.AddObservation(weaponSystemsOfficer.isTorpedoReady);
        //sensor.AddObservation(weaponSystemsOfficer.torpedoCooldown / WeaponSystemsOfficer.m_TorpedoReloadTime);

        sensor.AddObservation(weaponSystemsOfficer.Ammo / (float) WeaponSystemsOfficer.maxAmmo);
        sensor.AddObservation(Engine.Fuel / Engine.maxFuel);

        sensor.AddObservation(CurrentHealth / (float) m_Durability);
        sensor.AddObservation(target.CurrentHealth / (float) m_Durability);
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

        // Reward
        AddReward(rewardFuelLoss);

        float distance = Vector3.Distance(transform.position, target.transform.position);
        float penalty = Mathf.Pow(distance, 2f) * rewardDistance;
        AddReward(penalty);
        
        float damageTaken = healthChange / m_Durability;
        healthChange = 0f;
        AddReward(damageTaken * rewardHpChange);

        // EndEpisode
        if (isCollisionWithWarship)
        {
            SetReward(-1f);
            target.SetReward(-1f);
            EndEpisode();
            target.EndEpisode();
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
        else if (Engine.Fuel <= 0f + Mathf.Epsilon)
        {
            SetReward(-1f);
            target.SetReward(1f);
            EndEpisode();
            target.EndEpisode();
        }
        else if (weaponSystemsOfficer.Ammo == 0)
        {
            SetReward(-1f);
            target.SetReward(1f);
            EndEpisode();
            target.EndEpisode();
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        KeyCode[] keyCodes = {
            KeyCode.UpArrow, KeyCode.DownArrow,
            KeyCode.LeftArrow, KeyCode.RightArrow,
            KeyCode.Space
        };
        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (Input.GetKey(keyCodes[i]))
            {
                actionsOut[0] = (float) (i+1);
                return;
            }
        }
        actionsOut[0] = 0f;
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
            isCollisionWithWarship = true;
        }
        else if (collision.collider.tag == "Torpedo")
        {
            CurrentHealth = 0;
        }
        /*
        else if (collision.collider.tag.StartsWith("Bullet")
                 && !collision.collider.tag.EndsWith(teamId.ToString()))
        {
            float damage = collision.rigidbody?.velocity.magnitude ?? 20f;
            CurrentHealth -= damage;
        }
        */
        else if (collision.collider.tag == "Terrain")
        {
            //float damage = rb.velocity.magnitude * rb.mass;
            //CurrentHealth -= damage;
            CurrentHealth = 0;
        }
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

    public void TakeDamage()
    {
        Debug.Log($"Warship({name}).TakeDamage()");
        CurrentHealth -= 1;
    }
}
