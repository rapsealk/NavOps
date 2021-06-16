using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Transform BattleField;
    [HideInInspector]
    public Vector3 BattleFieldLocalScale;
    public NavOps.TaskForce TaskForceBlue;
    public NavOps.TaskForce TaskForceRed;
    public ControlArea[] ControlAreas;
    public GameObject[] Obstacles;
    public Slider[] TaskForceBlueHpSliders;
    public Slider[] TaskForceBlueFuelSliders;
    public Slider[] TaskForceRedHpSliders;
    public Slider[] TaskForceRedFuelSliders;
    // public Text[] TaskForceBlueTargetIndicators;
    // public Text[] TaskForceRedTargetIndicators;
    public float Reward {
        get {
            float value = _reward;
            SetReward(0f);
            return value;
        }
    }
    public bool EpisodeDone {
        get {
            bool value = _done;
            _done = false;
            return value;
        }
        private set {
            if (value)
            {
                m_EpisodeQueue.Enqueue(true);
            }
            _done = value;
        }
    }

    Queue<bool> m_EpisodeQueue = new Queue<bool>();
    Queue<bool> m_StepQueue = new Queue<bool>();

    NavOps.Grpc.GrpcServer m_GrpcServer;
    int m_GrpcPort = 9090;
    
    private float[] _hpValues;
    private float[] _opponentHpValues;
    private float _reward;
    private bool _done;

    // Start is called before the first frame update
    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            string portArg = args[1];
            if (portArg.StartsWith("--port"))
            {
                m_GrpcPort = int.Parse(portArg.Split('=')[1]);
            }
        }

        Application.targetFrameRate = 60;

        ResetHpValues();

        m_GrpcServer = new NavOps.Grpc.GrpcServer
        {
            GameManager = this
        };
        m_GrpcServer.StartGrpcServer(grpcPort: m_GrpcPort);

        BattleFieldLocalScale = BattleField.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_EpisodeQueue.Count > 0)
        {
            bool _ = m_EpisodeQueue.Dequeue();
            Reset();
            return;
        }

        UpdateGUI();

        CheckControlAreaStatus();

        ControlArea[] notDominatedAreas = ControlAreas.Where(area => area.Dominant != (int) ControlArea.DominantForce.RED).ToArray();
        if (notDominatedAreas.Length > 0)
        {
            foreach (var unit in TaskForceRed.Units)
            {
                unit.TargetControlArea = null;

                float distance = Mathf.Infinity;
                ControlArea targetControlArea = notDominatedAreas[0];
                foreach (var area in notDominatedAreas)
                {
                    float newDistance = Vector3.Distance(unit.transform.position, area.transform.position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        targetControlArea = area;
                    }
                }
                unit.TargetControlArea = targetControlArea;
            }
        }

        RaycastHit hit;
        for (int i = 0; i < TaskForceBlue.Units.Length; i++)
        {
            NavOps.Grpc.Warship taskForceBlueWarship = TaskForceBlue.Units[i];
            taskForceBlueWarship.IsDetected = false;

            Vector3 position = taskForceBlueWarship.transform.position;
            for (int j = 0; j < TaskForceRed.Units.Length; j++)
            {
                NavOps.Grpc.Warship taskForceRedWarship = TaskForceRed.Units[j];
                taskForceRedWarship.IsDetected = false;
                taskForceRedWarship.CurrentState = NavOps.Grpc.Warship.State.PATROL;

                Vector3 targetPosition = TaskForceRed.Units[j].transform.position;
                Vector3 direction = targetPosition - position;
                if (Physics.Raycast(position, direction, out hit, maxDistance: 200f))
                {
                    if (hit.collider.tag != "Player")
                    {
                        continue;
                    }

                    taskForceBlueWarship.IsDetected = true;
                    taskForceBlueWarship.CurrentState = NavOps.Grpc.Warship.State.STALK;
                    taskForceRedWarship.IsDetected = true;
                    taskForceRedWarship.CurrentState = NavOps.Grpc.Warship.State.STALK;

                    taskForceBlueWarship.Target = taskForceRedWarship;
                    taskForceRedWarship.Target = taskForceBlueWarship;
                }
            }
        }

        if (m_StepQueue.Count > 0)
        {
            bool _ = m_StepQueue.Dequeue();
            foreach (var unit in TaskForceRed.Units)
            {
                unit.HeuristicStep();
            }
        }
    }

    private void Reset()
    {
        Debug.Log($"[GameManager] Reset!");

        foreach (var area in ControlAreas)
        {
            area.Reset();
        }

        foreach (var unit in TaskForceBlue.Units.Concat(TaskForceRed.Units))
        {
            unit.Reset();
        }

        ResetHpValues();
    }

    private void ResetHpValues()
    {
        _hpValues = new float[TaskForceBlue.Units.Length];
        for (int i = 0; i < _hpValues.Length; i++)
        {
            _hpValues[i] = NavOps.Grpc.Warship.k_MaxHealth;
        }

        _opponentHpValues = new float[TaskForceRed.Units.Length];
        for (int i = 0; i < _opponentHpValues.Length; i++)
        {
            _opponentHpValues[i] = NavOps.Grpc.Warship.k_MaxHealth;
        }
    }

    private void UpdateGUI()
    {
        NavOps.Grpc.Warship[] blueUnits = TaskForceBlue.Units;
        for (int i = 0; i < blueUnits.Length; i++)
        {
            TaskForceBlueHpSliders[i].value = blueUnits[i].CurrentHealth / NavOps.Grpc.Warship.k_MaxHealth;
            TaskForceBlueFuelSliders[i].value = blueUnits[i].Engine.Fuel / Engine.maxFuel;
        }

        NavOps.Grpc.Warship[] redUnits = TaskForceRed.Units;
        for (int i = 0; i < redUnits.Length; i++)
        {
            TaskForceRedHpSliders[i].value = redUnits[i].CurrentHealth / NavOps.Grpc.Warship.k_MaxHealth;
            TaskForceRedFuelSliders[i].value = redUnits[i].Engine.Fuel / Engine.maxFuel;
        }

        // for (int i = 0; i < TaskForceBlue.Units.Length; i++)
        // {
        //     TaskForceBlueTargetIndicators[i].text = $"C{i+1} -> H{TaskForceBlue.Units[i].Target?.PlayerId-3}";
        // }
        // for (int i = 0; i < TaskForceRed.Units.Length; i++)
        // {
        //     TaskForceRedTargetIndicators[i].text = $"H{i+1} -> C{TaskForceRed.Units[i].Target?.PlayerId}";
        // }
    }

    public NavOps.Grpc.Observation SendActions(float[][] actions)
    {
        // Debug.Log($"[GameManager] SendActions: {actions}");

        for (int i = 0; i < actions.Length; i++)
        {
            TaskForceBlue.Units[i].OnActionReceived(actions[i]);
        }

        m_StepQueue.Enqueue(true);

        RewardShapingFunction();

        CheckEpisodeStatus();

        return CollectObservations();
    }

    private NavOps.Grpc.Observation CollectObservations()
    {
        NavOps.Grpc.Observation observation = new NavOps.Grpc.Observation();
        NavOps.Grpc.Warship blueUnit = TaskForceBlue.Units[0];
        Vector3 bluePosition = blueUnit.Position;
        float blueRadian = blueUnit.Rotation.eulerAngles.y % 360 * Mathf.Deg2Rad;
        observation.Fleets.Add(new NavOps.Grpc.FleetObservation
        {
            TeamId      = (uint) 0,
            Hp          = blueUnit.CurrentHealth / NavOps.Grpc.Warship.k_MaxHealth,
            Fuel        = blueUnit.Engine.Fuel / Engine.maxFuel,
            Destroyed   = blueUnit.IsDestroyed,
            Position    = new NavOps.Grpc.Position
            {
                X = bluePosition.x / BattleFieldLocalScale.x,
                Y = bluePosition.z / BattleFieldLocalScale.z
            },
            Rotation    = new NavOps.Grpc.Rotation
            {
                Cos = Mathf.Cos(blueRadian),
                Sin = Mathf.Sin(blueRadian)
            },
            Timestamp   = (uint) 0
        });
        foreach (var unit in TaskForceBlue.Units.Concat(TaskForceRed.Units))
        {
            Vector3 position = unit.Position;
            float radian = unit.Rotation.eulerAngles.y % 360 * Mathf.Deg2Rad;
            unit.Timestamp = (unit.IsDetected) ? 0 : unit.Timestamp + 1;
            observation.Fleets.Add(new NavOps.Grpc.FleetObservation
            {
                TeamId      = (uint) unit.TeamId,
                Hp          = unit.CurrentHealth / NavOps.Grpc.Warship.k_MaxHealth,
                Fuel        = unit.Engine.Fuel / Engine.maxFuel,
                Destroyed   = unit.IsDestroyed,
                Position    = new NavOps.Grpc.Position
                {
                    X = position.x / BattleFieldLocalScale.x,
                    Y = position.z / BattleFieldLocalScale.z,
                },
                Rotation    = new NavOps.Grpc.Rotation
                {
                    Cos = Mathf.Cos(radian),
                    Sin = Mathf.Sin(radian)
                },
                Timestamp = unit.Timestamp
            });
        }
        foreach (var area in ControlAreas)
        {
            observation.Locations.Add(new NavOps.Grpc.Location
            {
                Dominance   = area.Dominant,
                Position    = new NavOps.Grpc.Position
                {
                    X = area.Position.x / BattleFieldLocalScale.x,
                    Y = area.Position.z / BattleFieldLocalScale.z
                }
            });
        }
        observation.TargetIndexOnehot.Add(1.0f);
        observation.RaycastHits.Add(blueUnit.RaycastHitDistances);
        foreach (var turret in blueUnit.Wizzo.Turrets)
        {
            float radian = turret.Rotation.eulerAngles.y % 360 * Mathf.Deg2Rad;
            observation.Batteries.Add(new NavOps.Grpc.Battery
            {
                Rotation        = new NavOps.Grpc.Rotation
                {
                    Cos = Mathf.Cos(radian),
                    Sin = Mathf.Sin(radian)
                },
                Reloaded        = turret.Reloaded,
                Cooldown        = turret.CooldownTimer / Turret.k_ReloadTime,
                Damaged         = turret.Damaged,
                RepairProgress  = turret.RepairTimer / Turret.k_RepairTime
            });
        }
        observation.Ammo = blueUnit.Wizzo.Ammo / (float) WeaponSystemsOfficer.maxAmmo;

        float[] speedLevelOnehot = new float[Engine.MaxSpeedLevel - Engine.MinSpeedLevel + 1];
        speedLevelOnehot[blueUnit.Engine.SpeedLevel - Engine.MinSpeedLevel] = 1.0f;
        observation.SpeedLevelOnehot.Add(speedLevelOnehot);

        float[] steerLevelOnehot = new float[Engine.MaxSteerLevel - Engine.MinSteerLevel + 1];
        steerLevelOnehot[blueUnit.Engine.SteerLevel - Engine.MinSteerLevel] = 1.0f;
        observation.SteerLevelOnehot.Add(steerLevelOnehot);

        for (int i = 0; i < Obstacles.Length; i++)
        {
            Vector3 obstaclePosition = Obstacles[i].transform.position;
            observation.ObstaclePositions.Add(new NavOps.Grpc.Position
            {
                X = obstaclePosition.x / BattleFieldLocalScale.x,
                Y = obstaclePosition.z / BattleFieldLocalScale.z
            });
        }

        return observation;
    }

    private void RewardShapingFunction()
    {
        // Combat Score
        float hpDiff = 0f;
        for (int i = 0; i < TaskForceBlue.Units.Length; i++)
        {
            float newHp = TaskForceBlue.Units[i].CurrentHealth;
            hpDiff -= _hpValues[i] - newHp;
            _hpValues[i] = newHp;
        }
        for (int i = 0; i < TaskForceRed.Units.Length; i++)
        {
            float newHp = TaskForceRed.Units[i].CurrentHealth;
            hpDiff += _opponentHpValues[i] - newHp;
            _opponentHpValues[i] = newHp;
        }
        AddReward(hpDiff * 0.1f);

        // Dominant Score
        int dominationFactor = ControlAreas.Where(area => area.Dominant == (int) ControlArea.DominantForce.BLUE).ToArray().Length
                             - ControlAreas.Where(area => area.Dominant == (int) ControlArea.DominantForce.RED).ToArray().Length;

        AddReward(0.01f * dominationFactor);

        Vector3 position = TaskForceBlue.Units[0].Position;
        float distanceReward = -Mathf.Pow(position.magnitude, 2f) / 100_000_000f;
        // Debug.Log($"[GameManager] position={position} (reward={distanceReward})");
        AddReward(distanceReward);
    }

    private void CheckControlAreaStatus()
    {
        for (int i = 0; i < ControlAreas.Length; i++)
        {
            float r = ControlAreas[i].transform.localScale.x * 0.75f;

            Vector3 center = ControlAreas[i].transform.position;

            int blueArrived = 0;
            int redArrived = 0;

            for (int j = 0; j < TaskForceBlue.Units.Length; j++)
            {
                Vector3 pos = TaskForceBlue.Units[j].transform.position;
                bool detected = (Mathf.Pow(pos.x - center.x, 2f) + Mathf.Pow(pos.z - center.z, 2f)) <= Mathf.Pow(r, 2f);
                if (detected)
                {
                    blueArrived = 1;
                    break;
                }
            }

            for (int j = 0; j < TaskForceRed.Units.Length; j++)
            {
                Vector3 pos = TaskForceRed.Units[j].transform.position;
                bool detected = (Mathf.Pow(pos.x - center.x, 2f) + Mathf.Pow(pos.z - center.z, 2f)) <= Mathf.Pow(r, 2f);
                if (detected)
                {
                    redArrived = 1;
                    break;
                }
            }

            if (blueArrived == 1 || redArrived == 1)
            {
                int dominantId = blueArrived - redArrived;
                ControlAreas[i].SetDominant(dominantId);
            }
        }
    }

    private void CheckEpisodeStatus()
    {
        // ----------------------------------------------------
        // Check if episode is done..
        // ----------------------------------------------------
        bool blueTerminated = TaskForceBlue.Units.All(unit => unit.IsDestroyed);
        bool redTerminated = TaskForceRed.Units.All(unit => unit.IsDestroyed);

        if (blueTerminated)
        {
            Debug.Log($"[GameManager] Blue Terminated: {blueTerminated}!");

            EpisodeDone = true;
            SetReward(-1.0f);

            return;
        }
        else if (redTerminated)
        {
            Debug.Log($"[GameManager] Red Terminated: {redTerminated}!");

            EpisodeDone = true;
            SetReward(1.0f);

            return;
        }

        //
        // Control Area
        //
        bool blueDominated = ControlAreas.All(area => area.Dominant == (int) ControlArea.DominantForce.BLUE);
        if (blueDominated)
        {
            Debug.Log($"[GameManager] Blue Dominated!");
            EpisodeDone = true;
            SetReward(1.0f);
            return;
        }

        bool redDominated = ControlAreas.All(area => area.Dominant == (int) ControlArea.DominantForce.RED);
        if (redDominated)
        {
            Debug.Log($"[GameManager] Red Dominated!");
            EpisodeDone = true;
            SetReward(-1.0f);
            return;
        }

        //
        // Resource status
        //
        if (TaskForceBlue.Units.All(unit => unit.Engine.Fuel <= Mathf.Epsilon))
        {
            Debug.Log($"[GameManager] Blue resource exhausted!");
            EpisodeDone = true;
            SetReward(-1.0f);
        }
        else if (TaskForceRed.Units.All(unit => unit.Engine.Fuel <= Mathf.Epsilon))
        {
            Debug.Log($"[GameManager] Red resource exhausted!");
            EpisodeDone = true;
            SetReward(1.0f);
        }
    }

    private void SetReward(float reward)
    {
        _reward = reward;
    }

    private void AddReward(float reward)
    {
        _reward += reward;
    }
}
