using System.Collections.Generic;
using UnityEngine;

public class WeaponSystemsOfficer : MonoBehaviour
{
    [HideInInspector] public int PlayerId;
    [HideInInspector] public int TeamId;
    /*
     * Torpedo related..
    public GameObject torpedoPrefab;
    [HideInInspector] public GameObject torpedoInstance = null;
    [HideInInspector] public const float m_TorpedoReloadTime = 40f;
    [HideInInspector] public bool isTorpedoReady { get; private set; } = true;
    [HideInInspector] public float torpedoCooldown { get; private set; } = 0f;
    */
    [HideInInspector] public const uint maxAmmo = 192;
    [HideInInspector] public uint Ammo {
        get => _ammo;
        private set { _ammo = value; }
    }
    [HideInInspector] public Turret[] Turrets;

    private uint _ammo;
    private Queue<bool> _fireCommandQueue = new Queue<bool>();

    public void Reset()
    {
        /*
        if (torpedoInstance != null)
        {
            Destroy(torpedoInstance);
            torpedoInstance = null;
        }

        isTorpedoReady = true;
        torpedoCooldown = 0f;
        */

        for (int i = 0; i < Turrets.Length; i++)
        {
            Turrets[i].Reset();
        }

        Ammo = maxAmmo;

        _fireCommandQueue.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        Turrets = GetComponentsInChildren<Turret>();

        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (_fireCommandQueue.Count > 0)
        {
            bool _ = _fireCommandQueue.Dequeue();

            FireMainBattery();
        }
        /*
        if (!isTorpedoReady)
        {
            torpedoCooldown += Time.deltaTime;

            if (torpedoCooldown >= m_TorpedoReloadTime)
            {
                isTorpedoReady = true;
                torpedoCooldown = 0f;
            }
        }
        */

        // if (torpedoInstance != null)
        // {
        //     Debug.Log($"WSO: Torpedo: {torpedoInstance.transform.position}");
        // }
    }

    public void Assign(int teamId, int playerId)
    {
        TeamId = teamId;
        PlayerId = playerId;

        Turrets = GetComponentsInChildren<Turret>();
        for (int i = 0; i < Turrets.Length; i++)
        {
            Turrets[i].PlayerId = PlayerId;
            Turrets[i].TeamId = TeamId;
        }
    }

    public void Aim(Quaternion target)
    {
        for (int i = 0; i < Turrets.Length; i++)
        {
            Turrets[i].Rotate(target);
        }
    }

    public void SendFireCommand()
    {
        _fireCommandQueue.Enqueue(true);
    }

    private Vector2 mainBatteryOffset = Vector2.zero;

    public uint FireMainBattery(Vector2 offset = new Vector2())
    {
        if (Ammo > 0)
        {
            Debug.Log($"[Wizzo] Team={TeamId}/Player={PlayerId} Fire!");
        }

        uint previousAmmo = Ammo;

        for (int i = 0; i < Turrets.Length; i++)
        {
            if (Ammo == 0)
            {
                return 0;
            }

            if (Turrets[i].Fire(offset))
            {
                Ammo -= 1;
            }
        }

        return previousAmmo - Ammo;
    }

    /*
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
    */

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
            isReloaded = battery.Reloaded;
            cooldown = Mathf.Min(battery.CooldownTimer / Turret.k_ReloadTime, 1.0f);
            isDamaged = battery.Damaged;
            repairProgress = battery.RepairTimer / Turret.k_RepairTime;
        }
    }

    public BatterySummary[] Summary()
    {
        BatterySummary[] summary = new BatterySummary[Turrets.Length];
        for (int i = 0; i < Turrets.Length; i++)
        {
            summary[i] = new BatterySummary();
            summary[i].Copy(Turrets[i]);
        }
        return summary;
    }
}
