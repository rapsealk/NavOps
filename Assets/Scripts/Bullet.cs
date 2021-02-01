﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //public GameObject m_BattleShip;
    [HideInInspector] public int m_PlayerNumber;

    // Start is called before the first frame update
    void Start()
    {
        //Physics.IgnoreCollision(GetComponent<Collider>(), m_BattleShip.GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"[Bullet::Update] transform.position: ({transform.position.x}, {transform.position.y}, {transform.position.z})");

        if (transform.position.y < 0)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log($"[Bullet::OnCollisionEnter] {collision.collider.gameObject.name} {collision.collider.name} {collision.collider.tag}");
        /*
        if (collision.collider.tag == "Battleship")
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            Destroy(gameObject);
        }
        */
        if (collision.collider.name == "Collider")
        {
            GetComponent<Rigidbody>().mass = 0f;
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        //Debug.Log($"[Bullet.OnTriggerEnter] {collider}");
        Destroy(gameObject);
    }
}
