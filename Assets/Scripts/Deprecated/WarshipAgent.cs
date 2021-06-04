﻿using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Sensors;

/*
public class WarshipAgent : Agent, IWarshipController
{
    public int m_PlayerId;
    [HideInInspector]
    public Warship m_Warship;
    public Warship m_Opponent;
    public DominationManager m_DominationManager = null;

    public const float aggressiveFactor = 1.0f;
    public const float defensiveFactor = 0.5f;

    public enum ActionId
    {
        NOOP = 0,
        FORWARD = 1,
        BACKWARD = 2,
        LEFT = 3,
        RIGHT = 4,
        FIRE = 5
    }

    [Header("Finite State Machine")]
    public bool m_IsFiniteStateMachineBot = false;
    // [HideInInspector]
    private WarshipControllerFSM m_FiniteStateMachine = null;

    private Transform m_OpponentTransform;

    public const float winReward = 1.0f;
    public const float damageReward = 0.1f;
    public const float damagePenalty = -0.1f;

    public override void Initialize()
    {
        m_Warship = GetComponent<Warship>();
        m_Warship.m_PlayerId = m_PlayerId;

        if (m_IsFiniteStateMachineBot)
        {
            m_FiniteStateMachine = new WarshipControllerFSM();
            m_FiniteStateMachine.m_Opponent = m_Opponent;
        }

        // MaxStep = 1000;
    }

    public override void OnEpisodeBegin()
    {
        Reset();

        m_DominationManager?.Reset();
    }

    public void Reset()
    {
        m_Warship.Reset();

        m_Opponent.Reset();
        m_OpponentTransform = m_Opponent.GetComponent<Transform>();

        if (m_FiniteStateMachine != null)
        {
            m_FiniteStateMachine?.TransitionToState(m_FiniteStateMachine.m_IdleState);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 position = m_Warship.m_Transform.localPosition / 160f;
        // sensor.AddObservation(m_Warship.m_Transform.localPosition / 100f);          // 3 (x, y, z)
        sensor.AddObservation(position.x);
        sensor.AddObservation(position.z);
        float radian = m_Warship.m_Transform.rotation.eulerAngles.y / 180 * Mathf.PI;
        // sensor.AddObservation(m_Warship.m_Transform.rotation.eulerAngles.y);        // 1 (x, y, z)
        sensor.AddObservation(Mathf.Cos(radian));
        sensor.AddObservation(Mathf.Sin(radian));
        sensor.AddObservation(m_Warship.m_CurrentHealth / Warship.StartingHealth);  // 1

        for (int i = 0; i < m_Warship.m_Turrets.Length; i++)
        {
            Deprecated.Turret turret = m_Warship.m_Turrets[i];
            sensor.AddObservation(turret.m_IsLoaded);
            sensor.AddObservation(turret.CurrentCooldownTime);      // 6
            sensor.AddObservation(turret.m_IsDamaged);
            sensor.AddObservation(turret.RepairTimeLeft / Deprecated.Turret.repairTime);
        }

        Vector3 opponentPosition = m_OpponentTransform.localPosition / 160;
        // sensor.AddObservation(m_OpponentTransform.localPosition / 100f);            // 3
        sensor.AddObservation(opponentPosition.x);
        sensor.AddObservation(opponentPosition.z);
        float opponentRadian = m_OpponentTransform.rotation.eulerAngles.y / 180 * Mathf.PI;
        // sensor.AddObservation(m_OpponentTransform.rotation.eulerAngles.y);          // 1
        sensor.AddObservation(Mathf.Cos(opponentRadian));
        sensor.AddObservation(Mathf.Sin(opponentRadian));
        sensor.AddObservation(m_Opponent.m_CurrentHealth / Warship.StartingHealth); // 1

                                                                                    // Total: 18

        // Reward
        #region RewardShaping

        //AddReward(-0.0001f);

        //if (m_PlayerId == 1 && m_DominationManager.IsBlueDominating)
        //{
        //    AddReward(0.01f);

        //    if (m_DominationManager.IsDominated)
        //    {
        //        SetReward(winReward);
        //        EndEpisode();
        //        //m_Opponent.SetReward(-winReward);
        //        //m_Opponent.EndEpisode();
        //    }
        //}
        //else if (m_PlayerId == 2 && m_DominationManager.IsRedDominating)
        //{
        //    AddReward(0.01f);

        //    if (m_DominationManager.IsDominated)
        //    {
        //        SetReward(winReward);
        //        EndEpisode();
        //        //m_Opponent.SetReward(-winReward);
        //        //m_Opponent.EndEpisode();
        //    }
        //}

        #endregion
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        for (int i = 0; i < vectorAction.Length; i++)
        {
            if (vectorAction[i] == 1.0f)
            {
                if (i == (int) ActionId.NOOP)
                {
                    // NOOP
                }
                else if (i == (int) ActionId.FORWARD)
                {
                    m_Warship.Accelerate(Direction.up);
                }
                else if (i == (int) ActionId.BACKWARD)
                {
                    m_Warship.Accelerate(Direction.down);
                }
                else if (i == (int) ActionId.LEFT)
                {
                    m_Warship.Steer(Direction.left);
                }
                else if (i == (int) ActionId.RIGHT)
                {
                    m_Warship.Steer(Direction.right);
                }
                else if (i == (int) ActionId.FIRE)
                {
                    m_Warship.Fire();
                }
            }
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        // TODO: Bot
        if (Input.GetKeyDown(KeyCode.W))
        {
            actionsOut[(int)ActionId.FORWARD] = 1;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            actionsOut[(int)ActionId.BACKWARD] = 1;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            actionsOut[(int)ActionId.LEFT] = 1;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            actionsOut[(int)ActionId.RIGHT] = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            actionsOut[(int)ActionId.FIRE] = 1;
        }
    }

    private void TakeDamage(float damage)
    {
        m_Warship.TakeDamage(damage);
    }

    void OnCollisionEnter(Collision collision)
    {
        //if (collision.collider.tag == "Wall")
        //{
        //    TakeDamage(Warship.DefaultDamage);
        //    Debug.Log($"ID#{m_PlayerId} - {collision.collider.tag} -> {m_Warship.m_CurrentHealth}");

        //    AddReward(damageReward);

        //    if (m_Warship.m_CurrentHealth <= 0f)
        //    {
        //        SetReward(-winReward);
        //        EndEpisode();
        //    }
        //}
    }

    void OnTriggerEnter(Collider collider)
    {
        m_Warship.m_ExplosionAnimation?.Play();

        if (collider.tag.Contains("Bullet") && !collider.tag.EndsWith(m_PlayerId.ToString()))
        {
            TakeDamage(WarshipHealth.DefaultDamage);
            Debug.Log($"ID#{m_PlayerId} - {collider.tag} -> {m_Warship.m_CurrentHealth}");

            AddReward(damagePenalty * defensiveFactor);

            if (m_Warship.m_CurrentHealth <= 0f)
            {
                SetReward(-winReward);
                EndEpisode();
            }
        }
        else if (collider.CompareTag("Battleship"))
        {
            Debug.Log($"ID#{m_PlayerId} - Collision with battleship");
            SetReward(-winReward);
            EndEpisode();
        }
    }

    // IWarshipController
    public Transform GetTransform()
    {
        return transform;
    }

    public Warship GetOpponent()
    {
        return m_Opponent;
    }
}
*/