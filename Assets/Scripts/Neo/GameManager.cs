using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Warship player1;
    public Warship player2;
    public Text player1PositionText;
    public Text player1RotationText;
    public Text player1HpText;
    public Text player1AmmoText;
    public Slider player1FuelSlider;
    public Text player2PositionText;
    public Text player2RotationText;
    public Text player2HpText;
    // Speed Level
    public Text[] speedLevelTexts;
    // Steer Level
    public Text[] steerLevelTexts;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position1 = player1.transform.position;
        Vector3 rotation1 = player1.transform.rotation.eulerAngles;
        player1PositionText.text = string.Format("({0:F2}, {1:F2})", position1.x, position1.z);
        player1RotationText.text = string.Format("{0:F2}", rotation1.y);
        player1HpText.text = string.Format("{0:F2}", player1.CurrentHealth);
        player1AmmoText.text = string.Format("Ammo: {0}", player1.weaponSystemsOfficer.Ammo);
        player1FuelSlider.value = player1.Engine.Fuel / Engine.maxFuel;
        Vector3 position2 = player2.transform.position;
        Vector3 rotation2 = player2.transform.rotation.eulerAngles;
        player2PositionText.text = string.Format("({0:F2}, {1:F2})", position2.x, position2.z);
        player2RotationText.text = string.Format("{0:F2}", rotation2.y);
        player2HpText.text = string.Format("{0:F2}", player2.CurrentHealth);

        for (int i = 0; i < speedLevelTexts.Length; i++)
        {
            speedLevelTexts[i].color = Color.gray;
        }
        speedLevelTexts[player1.Engine.SpeedLevel+2].color = Color.green;

        for (int i = 0; i < steerLevelTexts.Length; i++)
        {
            steerLevelTexts[i].color = Color.gray;
        }
        steerLevelTexts[player1.Engine.SteerLevel+2].color = Color.green;
    }
}
