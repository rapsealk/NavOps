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
    /*
    public Warship player1;
    public Warship player2;
    public Text player1PositionText;
    public Text player1RotationText;
    public Slider player1HpSlider;
    public Text player1AmmoText;
    public Slider player1FuelSlider;
    public Text player2PositionText;
    public Text player2RotationText;
    public Slider player2HpSlider;
    // Speed Level
    public Text[] speedLevelTexts;
    public Text speedKnotText;
    // Steer Level
    public Text[] steerLevelTexts;
    */

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
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

        /*
        Vector3 position1 = player1.transform.position;
        Vector3 rotation1 = player1.transform.rotation.eulerAngles;
        player1PositionText.text = string.Format("({0:F2}, {1:F2})", position1.x, position1.z);
        player1RotationText.text = string.Format("{0:F2}", rotation1.y);
        player1HpSlider.value = (player1.CurrentHealth - Mathf.Epsilon) / Warship.k_MaxHealth;
        player1AmmoText.text = string.Format("Ammo: {0}", player1.weaponSystemsOfficer.Ammo);
        player1FuelSlider.value = player1.Engine.Fuel / Engine.maxFuel;
        Vector3 position2 = player2.transform.position;
        Vector3 rotation2 = player2.transform.rotation.eulerAngles;
        player2PositionText.text = string.Format("({0:F2}, {1:F2})", position2.x, position2.z);
        player2RotationText.text = string.Format("{0:F2}", rotation2.y);
        player2HpSlider.value = (player2.CurrentHealth - Mathf.Epsilon) / Warship.k_MaxHealth;

        for (int i = 0; i < speedLevelTexts.Length; i++)
        {
            speedLevelTexts[i].color = Color.black;
        }
        speedLevelTexts[player1.Engine.SpeedLevel+2].color = Color.white;

        for (int i = 0; i < steerLevelTexts.Length; i++)
        {
            steerLevelTexts[i].color = Color.gray;
        }
        steerLevelTexts[player1.Engine.SteerLevel+2].color = Color.green;

        speedKnotText.text = string.Format("{0:F1} kts", (player1.Engine.IsBackward ? -1 : 1) * player1.rb.velocity.magnitude);
        */
    }
}
