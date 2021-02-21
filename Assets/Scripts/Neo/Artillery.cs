using UnityEngine;

public enum TurretType
{
    FRONTAL = 0,
    RIGHT = 1,
    REAR = 2,
    LEFT = 3
}

public class Artillery : MonoBehaviour
{
    public LaserBeam beamPrefab;
    public Transform muzzle;
    public ParticleSystem muzzleFlash;
    [HideInInspector] public int playerId;
    [HideInInspector] public int teamId;
    [HideInInspector] public const float m_Traverse = 120f;
    [HideInInspector] public const float m_SideTraverse = 60f;
    [HideInInspector] public const float m_TraverseSpeed = 15f;
    [HideInInspector] public const float m_ReloadTime = 3f; // 6
    [HideInInspector] public const float m_RepairTime = 30f;
    [HideInInspector] public float cooldownTimer = 0f;
    [HideInInspector] public bool isReloaded = true;
    [HideInInspector] public float repairTimer = 0f;
    [HideInInspector] public bool isDamaged {
        get => _isDamaged;
        private set
        {
            meshRenderer.material.color = value ? Color.cyan : meshRendererColor;
            _isDamaged = value;
        }
    }
    [HideInInspector] public bool IsTargetLocked {
        get => _isTargetLocked;
        private set { _isTargetLocked = value; }
    }
    public const float AttackRange = 1000f;

    private Warship Warship;
    private TurretType m_TurretType;
    private float m_InitialEulerRotation;
    private Vector2 m_FirePower = new Vector2(8000f, 100f);
    private float offsetX = 3f;
    private float offsetY = 5f;
    private bool initialized = false;
    private MeshRenderer meshRenderer;
    private Color meshRendererColor;
    private bool _isDamaged = false;
    private bool _isTargetLocked = false;

    public void Reset()
    {
        if (!initialized)
        {
            initialized = true;

            Initialize();
        }

        cooldownTimer = 0f;
        isReloaded = true;
        repairTimer = 0f;
        isDamaged = false;
        IsTargetLocked = false;

        Vector3 localRotation = transform.localRotation.eulerAngles;
        if (m_TurretType == TurretType.FRONTAL)
        {
            localRotation.y = 0f;
        }
        else if (m_TurretType == TurretType.RIGHT)
        {
            localRotation.y = 90f;
        }
        else if (m_TurretType == TurretType.REAR)
        {
            localRotation.y = 180f;
        }
        else if (m_TurretType == TurretType.LEFT)
        {
            localRotation.y = 270f;
        }
        transform.localRotation = Quaternion.Euler(localRotation);
    }

    private void Initialize()
    {
        Warship = GetComponentInParent<Warship>();

        m_InitialEulerRotation = (transform.localRotation.eulerAngles.y + 360) % 360;

        if (m_InitialEulerRotation <= Mathf.Epsilon)
        {
            m_TurretType = TurretType.FRONTAL;
        }
        else if (m_InitialEulerRotation <= 90f + Mathf.Epsilon)
        {
            m_TurretType = TurretType.RIGHT;
        }
        else if (m_InitialEulerRotation <= 180f + Mathf.Epsilon)
        {
            m_TurretType = TurretType.REAR;
        }
        else
        {
            m_TurretType = TurretType.LEFT;
        }

        meshRenderer = GetComponent<MeshRenderer>();
        meshRendererColor = meshRenderer.material.color;
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        m_InitialEulerRotation = (transform.localRotation.eulerAngles.y + 360) % 360;

        if (m_InitialEulerRotation <= Mathf.Epsilon)
        {
            m_TurretType = TurretType.FRONTAL;
        }
        else if (m_InitialEulerRotation <= 90f + Mathf.Epsilon)
        {
            m_TurretType = TurretType.RIGHT;
        }
        else if (m_InitialEulerRotation <= 180f + Mathf.Epsilon)
        {
            m_TurretType = TurretType.REAR;
        }
        else
        {
            m_TurretType = TurretType.LEFT;
        }
        */

        // Debug.Log($"{GetType().Name}({name} {TurretType}) InitialEulerRotation: {InitialEulerRotation}");
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDamaged)
        {
            repairTimer += Time.deltaTime;

            if (repairTimer >= m_RepairTime)
            {
                isDamaged = false;
                meshRenderer.material.color = meshRendererColor;
            }
        }
        else if (!isReloaded)
        {
            cooldownTimer += Time.deltaTime;

            if (cooldownTimer >= m_ReloadTime)
            {
                isReloaded = true;
            }
        }

        IsTargetLocked = false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, AttackRange))
        {
            if (hit.collider.tag.Equals("Player")
                || hit.collider.tag.Equals("Turret"))
            {
                IsTargetLocked = true;
            }
        }
    }

    public bool Fire(Vector2 offset = new Vector2())
    {
        if (!isReloaded || isDamaged)
        {
            return false;
        }

        /*
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.x = (rotation.x + offset.x * offsetX + 360) % 360;
        if (rotation.x < 180f)
        {
            rotation.x = 0f;
        }
        else if (360 - rotation.x > 60f)
        {
            rotation.x = -60f;
        }
        rotation.y = (rotation.y + offset.y * offsetY + 360) % 360;
        transform.rotation = Quaternion.Euler(rotation);

        Vector3 localRotation = transform.localRotation.eulerAngles;
        localRotation.y = (localRotation.y > 180f) ? (localRotation.y - 360f) : localRotation.y;
        switch (m_TurretType)
        {
            case TurretType.FRONTAL:
                if (Mathf.Abs(localRotation.y) >= m_Traverse + Mathf.Epsilon)
                {
                    localRotation.y = Mathf.Sign(localRotation.y) * m_Traverse;
                    transform.localRotation = Quaternion.Euler(localRotation);
                }
                break;
            case TurretType.REAR:
                if (Mathf.Abs(localRotation.y) <= 180f - (m_Traverse + Mathf.Epsilon))
                {
                    localRotation.y = 180f - Mathf.Sign(localRotation.y) * m_Traverse;
                    transform.localRotation = Quaternion.Euler(localRotation);
                }
                break;
            case TurretType.LEFT:
                if (Mathf.Abs(localRotation.y + 90f) >= m_Traverse + Mathf.Epsilon)
                {
                    localRotation.y = -90f + Mathf.Sign(localRotation.y + 90f) * m_Traverse;
                    transform.localRotation = Quaternion.Euler(localRotation);
                }
                break;
            case TurretType.RIGHT:
                if (Mathf.Abs(localRotation.y - 90f) >= m_Traverse + Mathf.Epsilon)
                {
                    localRotation.y = 90f + Mathf.Sign(localRotation.y - 90f) * m_Traverse;
                    transform.localRotation = Quaternion.Euler(localRotation);
                }
                break;
        }
        */

        float distance = AttackRange;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, AttackRange))
        {
            distance = hit.distance;
            //hit.collider.gameObject.tag

            if (hit.collider.tag.Equals("Player"))
            {
                Debug.Log($"{name} -> {hit.collider.name} ({hit.collider.tag}, {hit.point})");

                hit.collider.GetComponent<Warship>().TakeDamage();
            }
            else if (hit.collider.tag.Equals("Turret"))
            {
                Debug.Log($"{name} -> {hit.collider.name} ({hit.collider.tag}, {hit.point})");

                hit.collider.GetComponent<Artillery>().TakeDamage();
            }
        }

        muzzleFlash.Play();

        LaserBeam beam = Instantiate<LaserBeam>(beamPrefab, muzzle.position + muzzle.forward * 3, muzzle.rotation);
        beam.Distance = distance;
        // GameObject.Destroy(projectile, 3f);

        /* FIXME
        GameObject projectile = Instantiate(shellPrefab, muzzle.position + muzzle.forward * 3, muzzle.rotation);
        projectile.tag = $"Bullet{teamId}";

        Vector3 velocity = muzzle.transform.forward * m_FirePower.x + muzzle.transform.up * m_FirePower.y;
        Rigidbody rigidbody = projectile.GetComponent<Rigidbody>();
        rigidbody.velocity = velocity / rigidbody.mass;
        */

        isReloaded = false;
        cooldownTimer = 0f;

        return true;
    }

    public void Rotate(Quaternion target)
    {
        // TODO: Lock
        // Base: Horizontal, Barrel: Vertical
        Vector3 rotation = target.eulerAngles;

        /*
        float x = (rotation.x + 360) % 360;
        if (x < 180f)
        {
            x = 0f;
        }
        else if (360 - x > 60f)
        {
            x = -60f;
        }
        rotation.x = x;
        */
        rotation.x = 0f;
        rotation.y = (rotation.y + 360) % 360;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotation), m_TraverseSpeed * Time.deltaTime);

        ///
        /// Post-processing (FIXME)
        ///
        Vector3 localRotation = transform.localRotation.eulerAngles;
        localRotation.y = (localRotation.y > 180f + Mathf.Epsilon) ? (localRotation.y - 360f) : localRotation.y;
        switch (m_TurretType)
        {
            case TurretType.FRONTAL:
                if (Mathf.Abs(localRotation.y) >= m_Traverse + Mathf.Epsilon)
                {
                    localRotation.y = Mathf.Sign(localRotation.y) * m_Traverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), m_TraverseSpeed * Time.deltaTime);
                }
                break;
            case TurretType.REAR:
                if (Mathf.Abs(localRotation.y) <= 180f - (m_Traverse + Mathf.Epsilon))
                {
                    localRotation.y = 180f - Mathf.Sign(localRotation.y) * m_Traverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), m_TraverseSpeed * Time.deltaTime);
                }
                break;
            case TurretType.LEFT:
                if (Mathf.Abs(localRotation.y + 90f) >= m_SideTraverse + Mathf.Epsilon)
                {
                    localRotation.y = -90f + Mathf.Sign(localRotation.y + 90f) * m_SideTraverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), m_TraverseSpeed * Time.deltaTime);
                }
                break;
            case TurretType.RIGHT:
                if (Mathf.Abs(localRotation.y - 90f) >= m_SideTraverse + Mathf.Epsilon)
                {
                    localRotation.y = 90f + Mathf.Sign(localRotation.y - 90f) * m_SideTraverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), m_TraverseSpeed * Time.deltaTime);
                }
                break;
        }
    }

    /*
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[{teamId}-{playerId}] Artillery({name}).OnCollisionEnter(collision: {collision.collider.tag})");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log($"Artillery({name}).OnTriggerEnter(other: {other})");
        isDamaged = true;
        repairTimer = 0f;
        meshRenderer.material.color = Color.cyan;
    }
    */

    public void TakeDamage()
    {
        isDamaged = true;
        repairTimer = 0f;
        meshRenderer.material.color = Color.cyan;

        Debug.Log($"Turret({name}).TakeDamage()");
        Warship.TakeDamage();
    }
}
