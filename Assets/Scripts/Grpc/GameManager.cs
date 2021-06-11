using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public NavOps.TaskForce TaskForceBlue;
    public NavOps.TaskForce TaskForceRed;
    public ControlArea[] ControlAreas;
    public Slider[] TaskForceBlueHpSliders;
    public Slider[] TaskForceRedHpSliders;
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

    private NavOps.Grpc.GrpcServer m_GrpcServer;
    private float[] _hpValues;
    private float[] _opponentHpValues;
    private float _reward;
    private bool _done;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        ResetHpValues();

        m_GrpcServer = new NavOps.Grpc.GrpcServer
        {
            GameManager = this
        };
        m_GrpcServer.StartGrpcServer(grpcPort: 9090);

        //Setup();
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

        // Update Target Location
        /*
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            ControlArea[] notDominatedAreasb = ControlAreas.Where(area => area.Dominant != (int) ControlArea.DominantForce.BLUE).ToArray();
            if (notDominatedAreasb.Length > 0)
            {
                foreach (var unit in TaskForceBlue.Units)
                {
                    unit.TargetControlArea = null;

                    // Debug.Log($"[GameManager] Blue.notDominatedAreas: {string.Join("/", notDominatedAreasb.Select(area => area.name).ToArray())}");
                    float distance = Mathf.Infinity;
                    ControlArea targetControlArea = notDominatedAreasb[0];
                    foreach (var area in notDominatedAreasb)
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
        }
        */

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

    void Setup()
    {
        // for (int i = 0; i < TaskForceBlueHpSliders.Length; i++)
        // {
        //     TaskForceBlueHpSliders[i].gameObject.SetActive(false);
        //     TaskForceBlueTargetIndicators[i].gameObject.SetActive(false);
        // }
        // for (int i = 0; i < TaskForceBlue.Units.Length; i++)
        // {
        //     TaskForceBlueHpSliders[i].gameObject.SetActive(true);
        //     TaskForceBlueTargetIndicators[i].gameObject.SetActive(true);
        // }

        // for (int i = 0; i < TaskForceRedHpSliders.Length; i++)
        // {
        //     TaskForceRedHpSliders[i].gameObject.SetActive(false);
        //     TaskForceRedTargetIndicators[i].gameObject.SetActive(false);
        // }
        // for (int i = 0; i < TaskForceRed.Units.Length; i++)
        // {
        //     TaskForceRedHpSliders[i].gameObject.SetActive(true);
        //     TaskForceRedTargetIndicators[i].gameObject.SetActive(true);
        // }
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
        }

        NavOps.Grpc.Warship[] redUnits = TaskForceRed.Units;
        for (int i = 0; i < redUnits.Length; i++)
        {
            TaskForceRedHpSliders[i].value = redUnits[i].CurrentHealth / NavOps.Grpc.Warship.k_MaxHealth;
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

    public void SendActions(float[][] actions)
    {
        Debug.Log($"[GameManager] SendActions: {actions}");

        for (int i = 0; i < actions.Length; i++)
        {
            TaskForceBlue.Units[i].OnActionReceived(actions[i]);
        }

        m_StepQueue.Enqueue(true);

        RewardShapingFunction();

        CheckEpisodeStatus();
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
        }

        bool redDominated = ControlAreas.All(area => area.Dominant == (int) ControlArea.DominantForce.RED);
        if (redDominated)
        {
            Debug.Log($"[GameManager] Red Dominated!");
            EpisodeDone = true;
            SetReward(-1.0f);
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
