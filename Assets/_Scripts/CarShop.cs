using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarShop : MonoBehaviour
{
    private enum CurrentPart
    {
        Main,
        Secondary,
        Wheels
    }
    public GameObject mainCamera;
    public GameObject shopCamera;
    public GameObject shopUI;
    public GameObject playerUI;
    public PlayerStats playerStats;
    public Text costText;
    public Text playerGoldText;
    public Text noGoldText;
    public Text topSpeedValueText;
    public Text accelerationValueText;
    public float rotationDelta = 100f;
    public int mainColorCost = 10;
    public int secondaryColorCost = 5;
    public int wheelColorCost = 2;
    public int topSpeedCost = 2;
    public int accelerationCost = 1;
    public int topSpeedBonus = 25;
    public float accelerationBonus = 0.25f;
    public AudioSource purchaseSound;
    public AudioSource buzzerSound;

    static public bool shopProcessing = false;
    private CarCustomization playerCurrentCustomization;
    private CurrentPart currentPart = CurrentPart.Main;
    private CarCustomization carCustomization;
    private int totalCost = 0;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Populator.playerCamera;
        playerStats = Populator.player.GetComponent<PlayerStats>();

        playerCurrentCustomization = PlayerStats.CarCustomization;
        carCustomization = new CarCustomization();
        carCustomization.mainColor = playerCurrentCustomization.mainColor;
        carCustomization.secondaryColor = playerCurrentCustomization.secondaryColor;
        carCustomization.wheelColor = playerCurrentCustomization.wheelColor;
        UpdateUI();
        SetupShopCar();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (shopProcessing)
            {
                mainCamera.SetActive(true);
                playerUI.SetActive(true);
                shopCamera.SetActive(false);
                shopUI.SetActive(false);
                shopProcessing = false;
            }
            else
            {
                mainCamera.SetActive(false);
                playerUI.SetActive(false);
                shopCamera.SetActive(true);
                shopUI.SetActive(true);
                shopProcessing = true;
            }
        }

    }

    private void SetupShopCar()
    {
        transform.Find("SkyCarBody").GetComponent<Renderer>().material.color = playerCurrentCustomization.mainColor;

        transform.Find("SkyCarComponents").GetComponent<Renderer>().material.color = playerCurrentCustomization.secondaryColor;
        transform.Find("SkyCarMudGuardFrontLeft").GetComponent<Renderer>().material.color = playerCurrentCustomization.secondaryColor;
        transform.Find("SkyCarMudGuardFrontRight").GetComponent<Renderer>().material.color = playerCurrentCustomization.secondaryColor;

        transform.Find("SkyCarWheelFrontLeft").GetComponent<Renderer>().material.color = playerCurrentCustomization.wheelColor;
        transform.Find("SkyCarWheelFrontRight").GetComponent<Renderer>().material.color = playerCurrentCustomization.wheelColor;
        transform.Find("SkyCarWheelRearLeft").GetComponent<Renderer>().material.color = playerCurrentCustomization.wheelColor;
        transform.Find("SkyCarWheelRearRight").GetComponent<Renderer>().material.color = playerCurrentCustomization.wheelColor;
    }

    public void SwitchToMainColor()
    {
        currentPart = CurrentPart.Main;
    }

    public void SwitchToSecondaryColor()
    {
        currentPart = CurrentPart.Secondary;
    }

    public void SwitchToWheelsColor()
    {
        currentPart = CurrentPart.Wheels;
    }

    private void UpdateUI()
    {
        UpdateStats();
        UpdateCostText();
        UpdateCurrentGoldText();
        GameObject.Find("SceneBroker").GetComponent<BattleStart>().SendMessage("UpdateUI");
    }

    private void UpdateStats()
    {
        topSpeedValueText.text = (PlayerStats.bonusTopSpeed + playerStats.m_Topspeed).ToString();
        accelerationValueText.text = PlayerStats.bonusAcceleration.ToString();
    }

    private void UpdateCostText()
    {
        totalCost = 0;

        if (PlayerStats.boughtColors.IndexOf(carCustomization.mainColor) == -1)
        {
            totalCost += mainColorCost;
        }
        if (PlayerStats.boughtColors.IndexOf(carCustomization.secondaryColor) == -1)
        {
            totalCost += secondaryColorCost;
        }
        if (PlayerStats.boughtColors.IndexOf(carCustomization.wheelColor) == -1)
        {
            totalCost += wheelColorCost;
        }

        costText.text = string.Format("Cost: {0}G", totalCost);

        noGoldText.gameObject.SetActive(false);
    }

    private void UpdateCurrentGoldText()
    {
        noGoldText.gameObject.SetActive(false);
        playerGoldText.text = string.Format("Current Gold: {0}G", PlayerStats.currentGold);
    }

    private Color String2Color(string colorStr)
    {
        switch (colorStr.ToLower())
        {
            case "red":
                return Color.red;
            case "magenta":
                return Color.magenta;
            case "purple":
                return new Color(0.49f, 0, 1f);
            case "blue":
                return Color.blue;
            case "cyan":
                return Color.cyan;
            case "green":
                return Color.green;
            case "light green":
                return new Color(0.45f, 1f, 0.29f);
            case "yellow":
                return Color.yellow;
            case "white":
                return Color.white;
            case "black":
                return Color.black;
            default:
                float r = Random.Range(0f, 1f);
                float g = Random.Range(0f, 1f);
                float b = Random.Range(0f, 1f);
                return new Color(r, g, b);
        }
    }

    public void ColorClick(string colorName)
    {
        Color currentColor = String2Color(colorName);
        switch (currentPart)
        {
            case CurrentPart.Main:
                transform.Find("SkyCarBody").GetComponent<Renderer>().material.color = currentColor;
                carCustomization.mainColor = currentColor;
                break;
            case CurrentPart.Secondary:
                transform.Find("SkyCarComponents").GetComponent<Renderer>().material.color = currentColor;
                transform.Find("SkyCarMudGuardFrontLeft").GetComponent<Renderer>().material.color = currentColor;
                transform.Find("SkyCarMudGuardFrontRight").GetComponent<Renderer>().material.color = currentColor;
                carCustomization.secondaryColor = currentColor;
                break;
            case CurrentPart.Wheels:
                transform.Find("SkyCarWheelFrontLeft").GetComponent<Renderer>().material.color = currentColor;
                transform.Find("SkyCarWheelFrontRight").GetComponent<Renderer>().material.color = currentColor;
                transform.Find("SkyCarWheelRearLeft").GetComponent<Renderer>().material.color = currentColor;
                transform.Find("SkyCarWheelRearRight").GetComponent<Renderer>().material.color = currentColor;
                carCustomization.wheelColor = currentColor;
                break;
        }
        UpdateCostText();
    }

    public void RandomizeColors()
    {
        noGoldText.gameObject.SetActive(false);
        CurrentPart prevPart = currentPart;

        currentPart = CurrentPart.Main;
        ColorClick("random");

        currentPart = CurrentPart.Secondary;
        ColorClick("random");

        currentPart = CurrentPart.Wheels;
        ColorClick("random");

        currentPart = prevPart;
    }

    public void ResetColor()
    {
        switch (currentPart)
        {
            case CurrentPart.Main:
                transform.Find("SkyCarBody").GetComponent<Renderer>().material.color = playerCurrentCustomization.mainColor;
                carCustomization.mainColor = playerCurrentCustomization.mainColor;
                break;
            case CurrentPart.Secondary:
                transform.Find("SkyCarComponents").GetComponent<Renderer>().material.color = playerCurrentCustomization.secondaryColor;
                transform.Find("SkyCarMudGuardFrontLeft").GetComponent<Renderer>().material.color = playerCurrentCustomization.secondaryColor;
                transform.Find("SkyCarMudGuardFrontRight").GetComponent<Renderer>().material.color = playerCurrentCustomization.secondaryColor;
                carCustomization.secondaryColor = playerCurrentCustomization.secondaryColor;
                break;
            case CurrentPart.Wheels:
                transform.Find("SkyCarWheelFrontLeft").GetComponent<Renderer>().material.color = playerCurrentCustomization.wheelColor;
                transform.Find("SkyCarWheelFrontRight").GetComponent<Renderer>().material.color = playerCurrentCustomization.wheelColor;
                transform.Find("SkyCarWheelRearLeft").GetComponent<Renderer>().material.color = playerCurrentCustomization.wheelColor;
                transform.Find("SkyCarWheelRearRight").GetComponent<Renderer>().material.color = playerCurrentCustomization.wheelColor;
                carCustomization.wheelColor = playerCurrentCustomization.wheelColor;
                break;
        }
        UpdateCostText();
    }

    public void ResetAllColors()
    {
        CurrentPart prevPart = currentPart;

        currentPart = CurrentPart.Main;
        ResetColor();

        currentPart = CurrentPart.Secondary;
        ResetColor();

        currentPart = CurrentPart.Wheels;
        ResetColor();

        currentPart = prevPart;
    }

    public void BuyColor()
    {
        if (totalCost > PlayerStats.currentGold)
        {
            noGoldText.gameObject.SetActive(true);
            buzzerSound.Play();
            return;
        }
        else
        {
            purchaseSound.Play();
            playerStats.DecreaseGold(totalCost);
            totalCost = 0;
            if (PlayerStats.boughtColors.IndexOf(carCustomization.mainColor) == -1)
                PlayerStats.boughtColors.Add(carCustomization.mainColor);

            if (PlayerStats.boughtColors.IndexOf(carCustomization.secondaryColor) == -1)
                PlayerStats.boughtColors.Add(carCustomization.secondaryColor);

            if (PlayerStats.boughtColors.IndexOf(carCustomization.wheelColor) == -1)
                PlayerStats.boughtColors.Add(carCustomization.wheelColor);

            playerStats.SetCarCustomization(carCustomization);

            UpdateUI();
        }
    }

    public void BuyTopSpeed()
    {
        if (topSpeedCost > PlayerStats.currentGold)
        {
            noGoldText.gameObject.SetActive(true);
            buzzerSound.Play();
            return;
        }
        else
        {
            purchaseSound.Play();
            playerStats.DecreaseGold(topSpeedCost);
            playerStats.IncreaseTopSpeedBonus(topSpeedBonus);
        }
        UpdateUI();
    }

    public void BuyAcceleration()
    {
        if (accelerationCost > PlayerStats.currentGold)
        {
            noGoldText.gameObject.SetActive(true);
            buzzerSound.Play();
            return;
        }
        else
        {
            purchaseSound.Play();
            playerStats.DecreaseGold(accelerationCost);
            playerStats.IncreaseAccelerationBonus(accelerationBonus);
        }
        UpdateUI();
    }

}
