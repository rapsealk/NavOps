using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace NavOps.Grpc
{
    public class Warship : MonoBehaviour
    {
        public enum State
        {
            PATROL  = 0,
            STALK   = 1
        }

        public int PlayerId;
        public int TeamId;
        public Transform BattleField;
        public Transform StartingPoint;
        public Color RendererColor;
        public GameObject DetectionMark;
        public const float k_MaxHealth = 10f;

        [HideInInspector]
        public Vector3 Position;
        [HideInInspector]
        public Quaternion Rotation;
        [HideInInspector]
        public Engine Engine { get; private set; }
        [HideInInspector]
        public WeaponSystemsOfficer Wizzo { get; private set; }
        [HideInInspector]
        public float CurrentHealth {
            get => _currentHealth;
            private set { _currentHealth = value; }
        }
        [HideInInspector]
        public bool IsDetected {
            get => _isDetected;
            set {
                DetectionMark.SetActive(value);
                _isDetected = value;
            }
        }
        [HideInInspector]
        public bool IsDestroyed { get => CurrentHealth <= 0f + Mathf.Epsilon; }
        [HideInInspector]
        public Warship Target;
        [HideInInspector]
        public ControlArea TargetControlArea;
        [HideInInspector]
        public State CurrentState;
        [HideInInspector]
        public uint Timestamp;
        public float[] RaycastHitDistances { get => _raycastHitDistances; }

        private float _currentHealth = k_MaxHealth;
        private bool _isDetected = false;
        private float[] _raycastHitDistances;

        public void Initialize()
        {
            Wizzo = GetComponent<WeaponSystemsOfficer>();
            Wizzo.Assign(TeamId, PlayerId);

            Engine = GetComponent<Engine>();

            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material.color = RendererColor;
            }

            _raycastHitDistances = new float[8];
        }

        public void Reset()
        {
            Debug.Log($"[Warship] Reset(TeamId={TeamId}, PlayerId={PlayerId}): {transform.position}");

            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            transform.position = StartingPoint.position;
            transform.rotation = StartingPoint.rotation;

            CurrentHealth = k_MaxHealth;

            CurrentState = State.PATROL;

            Target = null;

            IsDetected = false;
            Timestamp = 0;

            Wizzo.Reset();
            Engine.Reset();

            for (int i = 0; i < _raycastHitDistances.Length; i++)
            {
                _raycastHitDistances[i] = 1.0f;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Initialize();

            Reset();
        }

        // Update is called once per frame
        void Update()
        {
            Position = transform.position;
            Rotation = transform.rotation;

            // if (Application.platform == RuntimePlatform.WindowsEditor)
            // {
            //     HeuristicStep();
            // }
        }

        void FixedUpdate()
        {
            Warship target = Target;
            if (target == null)
            {
                return;
            }

            Vector3 rotation = Vector3.zero;
            rotation.y = Geometry.GetAngleBetween(transform.position, target.transform.position);

            Wizzo.Aim(Quaternion.Euler(rotation));
        }

        public void HeuristicStep()
        {
            if (CurrentState == State.PATROL)
            {
                NavigateTo(TargetControlArea?.transform, radius: 10f);
            }
            else if (CurrentState == State.STALK)
            {
                NavigateTo(Target.transform, radius: 100f);

                float distance = Vector3.Distance(transform.position, Target.transform.position);
                if (distance < Turret.AttackRange)
                {
                    uint _ = Wizzo.FireMainBattery();
                }
            }
        }

        private void NavigateTo(Transform target, float radius = 100f)
        {
            if (target == null)
            {
                return;
            }

            Vector3 heading = transform.rotation.eulerAngles;
            Vector3 opponentHeading = target?.transform.rotation.eulerAngles ?? Vector3.zero;

            float angle = (heading.y - opponentHeading.y + 360f) % 360f;    // (heading.y - opponentHeading.y) % 180f;

            Vector3 position = transform.position;
            Vector3 targetPosition = target.position;
            Vector3 positionGapVector = position - targetPosition;
            float gradient = positionGapVector.z / positionGapVector.x;
            float x = Mathf.Sqrt(Mathf.Pow(radius, 2) / (Mathf.Pow(gradient, 2) + 1));
            float z = gradient * x;

            float distancePositive = Geometry.GetDistance(position, targetPosition + new Vector3(x, 0f, z));
            float distanceNegative = Geometry.GetDistance(position, targetPosition - new Vector3(x, 0f, z));
            Vector3 nextReachPosition = targetPosition - Mathf.Sign(distancePositive - distanceNegative) * new Vector3(x, 0f, z);
            Vector3 nextReachDirection = nextReachPosition - position;

            // Raycast Detection
            float obstacleSafetyDistance = 32.0f;
            RaycastHit hit;
            Vector3 forceRepulsive = Vector3.zero;
            for (int i = 0; i < 8; i++)
            {
                _raycastHitDistances[i] = 1.0f;

                float rad = (45f * i + transform.rotation.eulerAngles.y) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
                if (i == 0)
                    Debug.DrawRay(position, dir * obstacleSafetyDistance, Color.red);
                if (Physics.Raycast(position, dir, out hit, maxDistance: 400f))
                {
                    if (hit.collider.tag == "Terrain" && hit.distance < 40f)
                    {
                        forceRepulsive += (position - hit.point) * 16f;
                        // forceRepulsive += (position - hit.point) * Mathf.Pow(400f / (position - hit.point).magnitude, 2f);
                    }
                    else if (hit.collider.tag == "Player")
                    {
                        forceRepulsive += (position - hit.point) * Mathf.Pow(800f / (position - hit.point).magnitude, 2f);
                    }

                    _raycastHitDistances[i] = hit.distance / (BattleField.localScale.x * 2);
                }
            }

            //nextReachDirection = (nextReachDirection + forceRepulsive).normalized * nextReachDirection.magnitude;
            nextReachDirection = (nextReachDirection.normalized + forceRepulsive.normalized) * nextReachDirection.magnitude;
            nextReachPosition = position + nextReachDirection;

            Debug.DrawRay(position, nextReachDirection, Color.red);

            float frontalDistance = _raycastHitDistances[0] * BattleField.localScale.x * 2;
            float rightDistance = _raycastHitDistances[1] * BattleField.localScale.x * 2;
            float leftDistance = _raycastHitDistances[7] * BattleField.localScale.x * 2;

            bool frontalDetected = frontalDistance < obstacleSafetyDistance;
            bool rightDetected = rightDistance < obstacleSafetyDistance;
            bool leftDetected = leftDistance < obstacleSafetyDistance;

            if (!frontalDetected)
            {
                if (Engine.SpeedLevel < 2)
                {
                    Engine.SpeedLevel += 1;
                    return;
                }

                float y = Geometry.GetAngleBetween(transform.position, nextReachPosition);
                float ydir = (transform.rotation.eulerAngles.y - y + 180f) % 360f - 180f;
                if (ydir > 3f)
                {
                    Engine.SteerLevel -= 1;
                }
                else if (ydir < -3f)
                {
                    Engine.SteerLevel += 1;
                }
            }
            else if (!leftDetected && rightDetected)
            {
                Engine.SteerLevel -= 1;
            }
            else if (rightDetected && !rightDetected)
            {
                Engine.SteerLevel += 1;
            }
            /*
            if (_raycastHitDistances[0] < 1.0f && frontalDistance < obstacleSafetyDistance)
            {
                //if (TeamId == 1)
                //   Debug.Log($"[Warship] frontalDistance: {frontalDistance}");
                Engine.SpeedLevel -= 1;
                Engine.SteerLevel = 0;
            }
            else if (Engine.SpeedLevel < 2)
            {
                Engine.SpeedLevel += 1;
                return;
            }
            else
            {
                float y = Geometry.GetAngleBetween(transform.position, nextReachPosition);
                float yx = Vector2.Angle(new Vector2(transform.position.x, transform.position.z), new Vector3(nextReachPosition.x, nextReachPosition.z));
                float ydir = (transform.rotation.eulerAngles.y - y + 180f) % 360f - 180f;
                if (ydir > 3f && _raycastHitDistances[7] < 1.0f && leftDistance > obstacleSafetyDistance)
                {
                    if (TeamId == 1)
                        Debug.Log($"[Warship] ydir={ydir} LEFT (y={transform.rotation.eulerAngles.y}, yx={yx})");
                    Engine.SteerLevel -= 1;
                }
                else if (ydir < -3f && _raycastHitDistances[1] < 1.0f && rightDistance > obstacleSafetyDistance)
                {
                    if (TeamId == 1)
                        Debug.Log($"[Warship] ydir={ydir} RIGHT (y={transform.rotation.eulerAngles.y}, yx={yx})");
                    Engine.SteerLevel += 1;
                }
            }
            */
        }

        public void OnActionReceived(float[] actions)
        {
            int maneuverActionId = (int) actions[0];
            int attackActionId = (int) actions[1];

            /*
            if (IsDestroyed)
            {
                Engine.SetSpeedLevel(0);
                Engine.SetSteerLevel(0);
                return;
            }
            */

            switch (maneuverActionId)
            {
                case (int) Engine.ManeuverCommandId.IDLE:
                    break;
                case (int) Engine.ManeuverCommandId.FORWARD:
                    Engine.SpeedLevel += 1;
                    break;
                case (int) Engine.ManeuverCommandId.BACKWARD:
                    Engine.SpeedLevel -= 1;
                    break;
                case (int) Engine.ManeuverCommandId.LEFT:
                    Engine.SteerLevel -= 1;
                    break;
                case (int) Engine.ManeuverCommandId.RIGHT:
                    Engine.SteerLevel += 1;
                    break;
            }

            switch (attackActionId)
            {
                case (int) Engine.AttackCommandId.IDLE:
                    break;
                case (int) Engine.AttackCommandId.FIRE:
                    Wizzo.SendFireCommand();
                    //uint usedAmmos = Wizzo.FireMainBattery();
                    //AddReward(-usedAmmos / 10000f);
                    break;
            }

            /*
            // Default Time Penalty
            // FIXME:
            float[] allyHitpoints_t = new float[m_TaskForce.Units.Length];
            for (int i = 0; i < m_TaskForce.Units.Length; i++)
            {
                allyHitpoints_t[i] = m_AllyWarshipModels[i].CurrentHealth;
                m_AllyWarshipModels[i] = m_TaskForce.Units[i].GenerateWarshipModel();
            }

            float[] enemyHitpoint_t = new float[m_TaskForce.TargetTaskForce.Units.Length];
            for (int i = 0; i < m_TaskForce.TargetTaskForce.Units.Length; i++)
            {
                enemyHitpoint_t[i] = m_EnemyWarshipModels[i].CurrentHealth;

                Warship warship = m_TaskForce.TargetTaskForce.Units[i];
                if (warship.IsDetected)
                {
                    m_EnemyWarshipModels[i] = warship.GenerateWarshipModel();
                }
            }

            float attackReward = 0f;
            for (int i = 0; i < m_TaskForce.TargetTaskForce.Units.Length; i++)
            {
                attackReward -= allyHitpoints_t[i] - m_AllyWarshipModels[i].CurrentHealth;
                attackReward += enemyHitpoint_t[i] - m_EnemyWarshipModels[i].CurrentHealth;

                if (allyHitpoints_t[i] >= 0f + Mathf.Epsilon && m_AllyWarshipModels[i].CurrentHealth <= 0f + Mathf.Epsilon)
                {
                    attackReward -= 5f;
                }
                if (enemyHitpoint_t[i] >= 0f + Mathf.Epsilon && m_EnemyWarshipModels[i].CurrentHealth <= 0f + Mathf.Epsilon)
                {
                    attackReward += 5f;
                }
            }
            AddReward(attackReward / 10f);

            CurrentHealth -= AccumulatedDamage;
            // FIXME: GroupReward
            // float hitpointReward = (CurrentHealth - m_PreviousHealth) - (target.CurrentHealth - m_PreviousOpponentHealth);
            // AddReward(hitpointReward / k_MaxHealth);
            // m_PreviousHealth = CurrentHealth;
            // m_PreviousOpponentHealth = target.CurrentHealth;

            // EndEpisode
            if (m_IsCollisionWithWarship)
            {
                CurrentHealth = 0f;
                SetReward(0f);
                //target.SetReward(0f);
                //EndEpisode();
                //target.EndEpisode();
            }

            m_TaskForce.EnvController.NotifyAgentDestroyed();
            */
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.name.StartsWith("Water"))
            {
                return;
            }

            string colliderTag = collision.collider.tag;

            if (colliderTag == "Player")
            {
                CurrentHealth = 0f;
            }
            else if (colliderTag.StartsWith("Bullet")
                     && !colliderTag.EndsWith(TeamId.ToString()))
            {
                CurrentHealth -= 1f;
            }
        }
    }
}
