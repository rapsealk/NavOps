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
    [HideInInspector] public const float k_Traverse = 120f;
    [HideInInspector] public const float k_SideTraverse = 60f;
    [HideInInspector] public const float k_TraverseSpeed = 15f;
    [HideInInspector] public const float k_ReloadTime = 6f; // 6
    [HideInInspector] public const float k_RepairTime = 30f;
    [HideInInspector] public float CooldownTimer = 0f;
    [HideInInspector] public bool Reloaded = true;
    [HideInInspector] public float RepairTimer = 0f;
    [HideInInspector] public bool Damaged {
        get => _damaged;
        private set
        {
            m_MeshRenderer.material.color = value ? Color.cyan : m_MeshRendererColor;
            _damaged = value;
        }
    }
    [HideInInspector] public bool Enabled {
        get => _enabled;
        private set { _enabled = value; }
    }
    public const float AttackRange = 300f;  // 9240
    public const float k_VerticalMax = 15f;
    public const float k_VerticalMin = -60f;
    [HideInInspector] public Quaternion Rotation;

    private NavOps.Grpc.Warship m_Warship;
    private TurretType m_TurretType;
    private float m_InitialEulerRotation;
    private Vector2 m_FirePower = new Vector2(12000f, 400f);    // Vector2(9140f, 800f);    // 12000
    //private float offsetX = 3f;
    //private float offsetY = 5f;
    private bool m_Initialized = false;
    private MeshRenderer m_MeshRenderer;
    private Color m_MeshRendererColor;
    private bool _damaged = false;
    private bool _enabled = false;

    public void Reset()
    {
        if (!m_Initialized)
        {
            m_Initialized = true;

            Initialize();
        }

        CooldownTimer = 0f;
        Reloaded = true;
        RepairTimer = 0f;
        Damaged = false;

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
        Rotation = transform.rotation;

        if (Damaged)
        {
            RepairTimer += Time.deltaTime;

            if (RepairTimer >= k_RepairTime)
            {
                Damaged = false;
                m_MeshRenderer.material.color = m_MeshRendererColor;
            }
        }
        else if (!Reloaded)
        {
            CooldownTimer += Time.deltaTime;

            if (CooldownTimer >= k_ReloadTime)
            {
                Reloaded = true;
            }
        }
    }

    public bool Fire(Vector2 offset = new Vector2())
    {
        //Debug.Log($"[Turret] Team={TeamId}/Player={PlayerId} Fire!");

        if (!Enabled || !Reloaded || Damaged)
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

        Reloaded = false;
        CooldownTimer = 0f;

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

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotation), k_TraverseSpeed * Time.deltaTime);

        ///
        /// Post-processing (FIXME)
        ///
        bool enableTurret = true;
        Vector3 localRotation = transform.localRotation.eulerAngles;
        localRotation.y = (localRotation.y > 180f + Mathf.Epsilon) ? (localRotation.y - 360f) : localRotation.y;
        switch (m_TurretType)
        {
            case TurretType.FRONTAL:
                if (Mathf.Abs(localRotation.y) >= k_Traverse + Mathf.Epsilon)
                {
                    localRotation.y = Mathf.Sign(localRotation.y) * k_Traverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), k_TraverseSpeed * Time.deltaTime);
                    enableTurret = false;
                }
                break;
            case TurretType.REAR:
                if (Mathf.Abs(localRotation.y) <= 180f - (k_Traverse + Mathf.Epsilon))
                {
                    localRotation.y = 180f - Mathf.Sign(localRotation.y) * k_Traverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), k_TraverseSpeed * Time.deltaTime);
                    enableTurret = false;
                }
                break;
            case TurretType.LEFT:
                if (Mathf.Abs(localRotation.y + 90f) >= k_SideTraverse + Mathf.Epsilon)
                {
                    localRotation.y = -90f + Mathf.Sign(localRotation.y + 90f) * k_SideTraverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), k_TraverseSpeed * Time.deltaTime);
                    enableTurret = false;
                }
                break;
            case TurretType.RIGHT:
                if (Mathf.Abs(localRotation.y - 90f) >= k_SideTraverse + Mathf.Epsilon)
                {
                    localRotation.y = 90f + Mathf.Sign(localRotation.y - 90f) * k_SideTraverse;
                    //transform.localRotation = Quaternion.Euler(localRotation);
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(localRotation), k_TraverseSpeed * Time.deltaTime);
                    enableTurret = false;
                }
                break;
        }

        Enabled = enableTurret;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Turret({name}/{TeamId}-{PlayerId}).OnCollisionEnter(collision: {collision.collider.name}/{collision.collider.tag})");
        
        Damaged = true;
        RepairTimer = 0f;
    }
}
