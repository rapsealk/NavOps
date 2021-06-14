﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geometry
{
    public static float GetAngleBetween(Vector3 a, Vector3 b)
    {
        Vector3 diff = b - a;
        float radian = Mathf.Atan2(diff.x, diff.z);
        //return radian * Mathf.Rad2Deg;
        return radian / Mathf.PI * 180;
    }

    public static Vector2 GetTrigonometricAngle(float angle)
    {
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public static float GetDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.z - a.z, 2));
    }

    /*
    public static bool ObjectExists(Vector3 position, int layerMask = int.MaxValue)
    {
        return Physics.OverlapSphere(position, 0.01f, layerMask).Length > 0;
    }
    */
}
