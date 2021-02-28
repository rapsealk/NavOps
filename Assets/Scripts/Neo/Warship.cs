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
        sensor.AddObservation(transform.position.x / battleField.transform.localScale.x);
        sensor.AddObservation(transform.position.z / battleField.transform.localScale.z);

        float radian = (transform.rotation.eulerAngles.y % 360) * Mathf.Deg2Rad;
        sensor.AddObservation(Mathf.Cos(radian));
        sensor.AddObservation(Mathf.Sin(radian));
        // Opponent
        // sensor.AddObservation(target.transform.position.x / (battleField.transform.localScale.x / 2) - 1f);
        // sensor.AddObservation(target.transform.position.z / (battleField.transform.localScale.z / 2) - 1f);
        Vector3 relativePosition = target.transform.position - transform.position;
        sensor.AddObservation(relativePosition.x / (battleField.transform.localScale.x * 2));
        sensor.AddObservation(relativePosition.z / (battleField.transform.localScale.x * 2));

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

        sensor.AddOneHotObservation(Engine.SpeedLevel+2, 5);
        sensor.AddOneHotObservation(Engine.SteerLevel+2, 5);

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
        actionsOut[0] = 0f;

        if (playerId == 1)
        {
            KeyCode[] keyCodes = {
                KeyCode.UpArrow, KeyCode.DownArrow,
                KeyCode.LeftArrow, KeyCode.RightArrow,
                KeyCode.Space
            };
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    actionsOut[0] = (float) (i+1);
                    return;
                }
            }
        }
        else
        {
            float radius = 30f;

            Vector3 currentPosition = transform.position;
            Vector3 opponentPosition = target.transform.position;
            Vector3 vector = currentPosition - opponentPosition;
            float gradient = vector.z / vector.x;
            float x = Mathf.Sqrt(Mathf.Pow(radius, 2) / (Mathf.Pow(gradient, 2) + 1));
            float z = gradient * x;

            float distance1 = Geometry.GetDistance(currentPosition, opponentPosition + new Vector3(x, 0f, z));
            float distance2 = Geometry.GetDistance(currentPosition, opponentPosition - new Vector3(x, 0f, z));
            Vector3 targetPosition = Vector3.zero;
            if (distance1 < distance2)
            {
                targetPosition = opponentPosition + new Vector3(x, 0f, z);
            }
            else
            {
                targetPosition = opponentPosition - new Vector3(x, 0f, z);
            }
            Vector3 targetDirection = targetPosition - currentPosition;
            Debug.DrawRay(currentPosition, targetDirection, Color.red);

            if (Geometry.GetDistance(currentPosition, opponentPosition) < radius + 50f)
            {
                //actionsOut[0] = 5f;
                weaponSystemsOfficer.FireMainBattery();
            }

            if (Mathf.Min(distance1, distance2) < radius)
            {
                if (Engine.SpeedLevel > 0)
                {
                    Engine.SetSpeedLevel(Engine.SpeedLevel - 1);
                    //actionsOut[0] = 2f;
                    return;
                }
            }
            else if (Engine.SpeedLevel <= 0)
            {
                Engine.SetSpeedLevel(Engine.SpeedLevel + 1);
                actionsOut[0] = 1f;
                return;
            }

            // #2. Direction
            Vector3 rotation = transform.rotation.eulerAngles;
            float angle = (Geometry.GetAngleBetween(currentPosition, targetPosition) + 360) % 360;
            float gap = angle - rotation.y;
            if ((gap > 0f && gap < 180f) || gap < -180f)
            {
                Engine.SetSteerLevel(Engine.SteerLevel + 1);
                //actionsOut[0] = 4f; // Right
            }
            else
            {
                Engine.SetSteerLevel(Engine.SteerLevel - 1);
                actionsOut[0] = 3f; // Left
            }
            /*
            if (Mathf.Abs(gap) > 90f)
            {
                warship.m_Warship.SetEngineLevel(Warship.EngineLevel.BACKWARD_MAX);
            }
            else
            {
                warship.m_Warship.SetEngineLevel(Warship.EngineLevel.FORWARD_MAX);
            }
            */
            /*
            float distance = Geometry.GetDistance(transform.position, target.transform.position);
            Vector3 dir = target.transform.position - transform.position;
            Debug.DrawRay(transform.position, dir, Color.red, Time.deltaTime);
            Debug.DrawRay(transform.position, dir.normalized * (dir.magnitude - 30f), Color.green, Time.deltaTime);

            actionsOut[0] = 0f;
            if (Engine.SpeedLevel == 0f)
            {
                actionsOut[0] = 1f;
                return;
            }
            else if (distance < 30f)
            {
                actionsOut[0] = 2f;
                return;
            }
            //else if ()
            float angle = Geometry.GetAngleBetween(transform.position, target.transform.position);
            if (angle > 10f)
            {
                actionsOut[0] = 4f; // Right
                //return;
            }
            else if (angle < -10f)
            {
                actionsOut[0] = 3f; // Left
                //return;
            }
            //
            else if (Engine.SteerLevel > 0)
            {
                actionsOut[0] = 3f;
            }
            else if (Engine.SteerLevel < 0)
            {
                actionsOut[0] = 4f;
            }
            else {
                actionsOut[0] = 5f;
            }
            //
            Debug.Log($"Distance: {distance}, Angle: {angle}, Steer: {Engine.SteerLevel}, Action: {actionsOut[0]}");
            */
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

    public void TakeDamage()
    {
        Debug.Log($"Warship({name}).TakeDamage()");
        CurrentHealth -= 1;
    }
}
