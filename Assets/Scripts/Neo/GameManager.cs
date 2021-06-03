using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TaskForce TaskForceBlue;
    public TaskForce TaskForceRed;
    public Slider[] TaskForceBlueHpSliders;
    public Slider[] TaskForceRedHpSliders;
    public Text[] TaskForceBlueTargetIndicators;
    public Text[] TaskForceRedTargetIndicators;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        float[] taskForceBlueHpValues = TaskForceBlue.HpValues;
        for (int i = 0; i < taskForceBlueHpValues.Length; i++)
        {
            TaskForceBlueHpSliders[i].value = taskForceBlueHpValues[i] / Warship.k_MaxHealth;
        }
        float [] taskForceRedHpValues = TaskForceRed.HpValues;
        for (int i = 0; i < taskForceRedHpValues.Length; i++)
        {
            TaskForceRedHpSliders[i].value = taskForceRedHpValues[i] / Warship.k_MaxHealth;
        }

        for (int i = 0; i < TaskForceBlue.Units.Length; i++)
        {
            TaskForceBlueTargetIndicators[i].text = $"C{i+1} -> H{TaskForceBlue.Units[i].Target?.PlayerId-3}";
        }
        for (int i = 0; i < TaskForceRed.Units.Length; i++)
        {
            TaskForceRedTargetIndicators[i].text = $"H{i+1} -> C{TaskForceRed.Units[i].Target?.PlayerId}";
        }
    }

    void Setup()
    {
        for (int i = 0; i < TaskForceBlueHpSliders.Length; i++)
        {
            TaskForceBlueHpSliders[i].gameObject.SetActive(false);
            TaskForceBlueTargetIndicators[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < TaskForceBlue.Units.Length; i++)
        {
            TaskForceBlueHpSliders[i].gameObject.SetActive(true);
            TaskForceBlueTargetIndicators[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < TaskForceRedHpSliders.Length; i++)
        {
            TaskForceRedHpSliders[i].gameObject.SetActive(false);
            TaskForceRedTargetIndicators[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < TaskForceRed.Units.Length; i++)
        {
            TaskForceRedHpSliders[i].gameObject.SetActive(true);
            TaskForceRedTargetIndicators[i].gameObject.SetActive(true);
        }
    }
}
