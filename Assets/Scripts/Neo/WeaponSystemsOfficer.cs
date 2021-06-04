using UnityEngine;

/*
public class WeaponSystemsOfficer : MonoBehaviour
{
    [HideInInspector] public int PlayerId;
    [HideInInspector] public int TeamId;
    public GameObject torpedoPrefab;
    [HideInInspector] public GameObject torpedoInstance = null;
    [HideInInspector] public const float m_TorpedoReloadTime = 40f;
    [HideInInspector] public bool isTorpedoReady { get; private set; } = true;
    [HideInInspector] public float torpedoCooldown { get; private set; } = 0f;
    [HideInInspector] public const uint maxAmmo = 192;
    [HideInInspector] public uint Ammo {
        get => _ammo;
        private set { _ammo = value; }
    }

    private Turret[] m_Turrets;
    private uint _ammo;

    public void Reset()
    {
        if (torpedoInstance != null)
        {
            Destroy(torpedoInstance);
            torpedoInstance = null;
        }

        isTorpedoReady = true;
        torpedoCooldown = 0f;

        for (int i = 0; i < m_Turrets.Length; i++)
        {
            m_Turrets[i].Reset();
        }

        Ammo = maxAmmo;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Turrets = GetComponentsInChildren<Turret>();

        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTorpedoReady)
        {
            torpedoCooldown += Time.deltaTime;

            if (torpedoCooldown >= m_TorpedoReloadTime)
            {
                isTorpedoReady = true;
                torpedoCooldown = 0f;
            }
        }

        // if (torpedoInstance != null)
        // {
        //     Debug.Log($"WSO: Torpedo: {torpedoInstance.transform.position}");
        // }
    }

    public void Assign(int teamId, int playerId)
    {
        TeamId = teamId;
        PlayerId = playerId;

        m_Turrets = GetComponentsInChildren<Turret>();
        for (int i = 0; i < m_Turrets.Length; i++)
        {
            m_Turrets[i].PlayerId = PlayerId;
            m_Turrets[i].TeamId = TeamId;
        }
    }

    public void Aim(Quaternion target)
    {
        for (int i = 0; i < m_Turrets.Length; i++)
        {
            m_Turrets[i].Rotate(target);
        }
    }

    private Vector2 mainBatteryOffset = Vector2.zero;

    public uint FireMainBattery(Vector2 offset = new Vector2())
    {
        uint previousAmmo = Ammo;

        for (int i = 0; i < m_Turrets.Length; i++)
        {
            if (Ammo == 0)
            {
                return 0;
            }

            if (m_Turrets[i].Fire(offset))
            {
                Ammo -= 1;
            }
        }

        return previousAmmo - Ammo;
    }

    public void FireTorpedoAt(Vector3 position)
    {
        if (!isTorpedoReady)
        {
            return;
        }

        isTorpedoReady = false;

        Vector3 releasePoint = transform.position + (position - transform.position).normalized * 8f;
        releasePoint.y = 0f;

        float y = Geometry.GetAngleBetween(transform.position, position);
        Vector3 rotation = new Vector3(90f, y, 0f);

        torpedoInstance = Instantiate(torpedoPrefab, releasePoint, Quaternion.Euler(rotation));
    }

    public class BatterySummary
    {
        public Vector2 rotation;
        public bool isReloaded;
        public float cooldown;
        public bool isDamaged;
        public float repairProgress;

        public void Copy(Turret battery)
        {
            Vector3 batteryRotation = battery.transform.rotation.eulerAngles;
            rotation = new Vector2(batteryRotation.x, batteryRotation.y);
            isReloaded = battery.isReloaded;
            cooldown = Mathf.Min(battery.cooldownTimer / Turret.m_ReloadTime, 1.0f);
            isDamaged = battery.isDamaged;
            repairProgress = battery.repairTimer / Turret.m_RepairTime;
        }
    }

    public BatterySummary[] Summary()
    {
        BatterySummary[] summary = new BatterySummary[m_Turrets.Length];
        for (int i = 0; i < m_Turrets.Length; i++)
        {
            summary[i] = new BatterySummary();
            summary[i].Copy(m_Turrets[i]);
        }
        return summary;
    }
}
*/
