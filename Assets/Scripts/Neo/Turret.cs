using System.Collections;
using UnityEngine;

public enum TurretType
{
    FRONTAL = 0,
    RIGHT = 1,
    REAR = 2,
    LEFT = 3
}

public class Turret : MonoBehaviour
{
    public GameObject ShellPrefab;
    public Transform muzzle;
    public ParticleSystem muzzleFlash;
    [HideInInspector] public int PlayerId;
    [HideInInspector] public int TeamId;
    [HideInInspector] public const float m_Traverse = 120f;
    [HideInInspector] public const float m_SideTraverse = 60f;
    [HideInInspector] public const float m_TraverseSpeed = 15f;
    [HideInInspector] public const float m_ReloadTime = 6f; // 6
    [HideInInspector] public const float m_RepairTime = 30f;
    [HideInInspector] public float cooldownTimer = 0f;
    [HideInInspector] public bool isReloaded = true;
    [HideInInspector] public float repairTimer = 0f;
    [HideInInspector] public bool isDamaged {
        get => _isDamaged;
        private set
        {
            m_MeshRenderer.material.color = value ? Color.cyan : m_MeshRendererColor;
            _isDamaged = value;
        }
    }
    [HideInInspector] public bool Enabled {
        get => _enabled;
        private set { _enabled = value; }
    }
    public const float AttackRange = 300f;  // 9240
    public const float k_VerticalMax = 15f;
    public const float k_VerticalMin = -60f;

    private NavOps.Grpc.Warship m_Warship;
    private TurretType m_TurretType;
    private float m_InitialEulerRotation;
    private Vector2 m_FirePower = new Vector2(12000f, 400f);    // Vector2(9140f, 800f);    // 12000
    //private float offsetX = 3f;
    //private float offsetY = 5f;
    private bool m_Initialized = false;
    private MeshRenderer m_MeshRenderer;
    private Color m_MeshRendererColor;
    private bool _isDamaged = false;
    private bool _enabled = false;

    public void Reset()
    {
        if (!m_Initialized)
        {
            m_Initialized = true;

            Initialize();
        }

        cooldownTimer = 0f;
        isReloaded = true;
        repairTimer = 0f;
        isDamaged = false;

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
        m_Warship = GetComponentInParent<NavOps.Grpc.Warship>();

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

        m_MeshRenderer = GetComponent<MeshRenderer>();
        m_MeshRendererColor = m_MeshRenderer.material.color;
    }

    // Start is called before the first frame update
    void Start()
    {
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
                m_MeshRenderer.material.color = m_MeshRendererColor;
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
    }

    public bool Fire(Vector2 offset = new Vector2())
    {
        //Debug.Log($"[Turret] Team={TeamId}/Player={PlayerId} Fire!");

        if (!Enabled || !isReloaded || isDamaged)
        {
            //Debug.Log($"[Turret] Team={TeamId}/Player={PlayerId} Fire FAILED!");
            return false;
        }

        //Debug.Log($"[Turret] Team={TeamId}/Player={PlayerId} Fire SUCCEED!");

        Vector3 firePosition = muzzle.position + muzzle.forward * 3;
        muzzleFlash.transform.position = firePosition;
        muzzleFlash.Play();

        GameObject projectile = Instantiate(ShellPrefab, firePosition, muzzle.rotation);
        projectile.tag = "Bullet" + TeamId.ToString();
        projectile.GetComponent<Shell>().Warship = m_Warship;

        Vector3 velocity = muzzle.transform.forward * m_FirePower.x + muzzle.transform.up * m_FirePower.y;
        Rigidbody rigidbody = projectile.GetComponent<Rigidbody>();
        rigidbody.velocity = velocity / rigidbody.mass;

        isReloaded = false;
        cooldownTimer = 0f;

        StartCoroutine(StopMuzzleFlashAnimation());

        return true;
    }

    private IEnumerator StopMuzzleFlashAnimation()
    {
        yield return new WaitForSeconds(3f);

        if (muzzleFlash.IsAlive())
        {
            muzzleFlash.Stop();
            muzzleFlash.Clear();
        }

        yield return null;
    }

    public void Rotate(Quaternion target)
    {
        // TODO: Lock
        // Base: Horizontal, Barrel: Vertical
        Vector3 rotation = target.eulerAngles;

        float x = (rotation.x + 360) % 360;
        if (x < 180f)
        {
            x = Mathf.Min(x, 15f);  // 0f
        }
        else if (360f - x > 60f)
        {
            x = -60f;
        }
        rotation.x = x; // 0f
        rotation.y = (rotation.y + 360) % 360;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotation), m_TraverseSpeed * Time.deltaTime);

        ///
        /// Post-processing (FIXME)
        ///
        bool enableTurret = true;
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
                    enableTurret = false;
                }
                break;
            case TurretType.REAR:
                if (Mathf.Abs(localRotation.y) <= 180f - (m_Traverse + Mathf.Epsilon))
                {
                    localRotation.y = 180f - Mathf.Sign(localRotation.y) * m_Traverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), m_TraverseSpeed * Time.deltaTime);
                    enableTurret = false;
                }
                break;
            case TurretType.LEFT:
                if (Mathf.Abs(localRotation.y + 90f) >= m_SideTraverse + Mathf.Epsilon)
                {
                    localRotation.y = -90f + Mathf.Sign(localRotation.y + 90f) * m_SideTraverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), m_TraverseSpeed * Time.deltaTime);
                    enableTurret = false;
                }
                break;
            case TurretType.RIGHT:
                if (Mathf.Abs(localRotation.y - 90f) >= m_SideTraverse + Mathf.Epsilon)
                {
                    localRotation.y = 90f + Mathf.Sign(localRotation.y - 90f) * m_SideTraverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), m_TraverseSpeed * Time.deltaTime);
                    enableTurret = false;
                }
                break;
        }

        Enabled = enableTurret;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Turret({name}/{TeamId}-{PlayerId}).OnCollisionEnter(collision: {collision.collider.name}/{collision.collider.tag})");
        
        isDamaged = true;
        repairTimer = 0f;
    }
}
