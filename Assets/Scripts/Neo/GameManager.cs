using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
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
    public Text EpisodeText;
    public Text ObservationText;
    public Text ActionText;
    public Text FrameText;
    public Text TimeText;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
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

        EpisodeText.text = $"Episode: {player1.EpisodeCount}";
        ObservationText.text = $"Observation: {player1.ObservationCount}";
        ActionText.text = $"Action: {player1.ActionCount}";
        FrameText.text = string.Format("Frame: {0} ({1:F2})", player1.FrameCount, 1 / Time.deltaTime);
        TimeText.text = string.Format("Time: {0:F2}", player1.TimeCount);
    }
}
