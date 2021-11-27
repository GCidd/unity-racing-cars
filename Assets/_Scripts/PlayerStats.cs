using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // This script provides input to the car controller in the same way that the user control script does.
    // As such, it is really 'driving' the car, with no special physics or animation tricks to make the car behave properly.

    // "wandering" is used to give the cars a more human, less robotic feel. They can waver slightly
    // in speed and direction while driving towards their target.
    
    [Header("Car Controller Options")]
    [SerializeField] public CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
    [SerializeField] public Vector3 m_CentreOfMassOffset = Vector3.zero;
    [SerializeField] public float m_MaximumSteerAngle = 25;
    [Range(0, 1)] [SerializeField] public float m_SteerHelper = 0.774f; // 0 is raw physics , 1 the car will grip in the direction it is facing
    [Range(0, 1)] [SerializeField] public float m_TractionControl = 1f; // 0 is no traction control, 1 is full interference
    [SerializeField] public float m_FullTorqueOverAllWheels = 2000;
    [SerializeField] public float m_ReverseTorque = 150;
    [SerializeField] public float m_MaxHandbrakeTorque = 100000000;
    [SerializeField] public float m_Downforce = 100f;
    [SerializeField] public SpeedType m_SpeedType;
    [SerializeField] public float m_Topspeed = 200;
    [SerializeField] public int NoOfGears = 5;
    [SerializeField] public float m_RevRangeBoundary = 1f;
    [SerializeField] public float m_SlipLimit = 0.4f;
    [SerializeField] public float m_BrakeTorque = 20000;

    [Header("Car customization")]
    public PresetColorOption carColorOption = PresetColorOption.Random;
    public Color mainColor;
    public Color secondaryColor;
    public Color wheelColor;
    public static int bonusTopSpeed;
    public static float bonusAcceleration;

    [Header("Other options")]
    public int startingGold = 5;
    public static int currentGold = -1;
    public static CarCustomization CarCustomization { get; private set; }
    public static List<Color> boughtColors { get; private set; }
    public Boost boost;
    private void Awake()
    {
        if (currentGold == -1)
        {
            CarCustomization = new CarCustomization();
            boughtColors = new List<Color>();
            currentGold = startingGold;

            if (carColorOption == PresetColorOption.Random)
                CarCustomization.SetupRandomColors();
            else
                CarCustomization.SetupColors(mainColor, secondaryColor, wheelColor);

            boughtColors.Add(CarCustomization.mainColor);
            boughtColors.Add(CarCustomization.secondaryColor);
            boughtColors.Add(CarCustomization.wheelColor);
        }
    }
    public static Color RandomColor()
    {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);

        return new Color(r, g, b);
    }

    public void SetCarCustomization(CarCustomization newCustomization)
    {
        CarCustomization = newCustomization;
    }
    public int IncreaseGold(int gold)
    {
        currentGold += gold;
        return currentGold;
    }
    public int DecreaseGold(int gold)
    {
        currentGold -= gold;
        currentGold = Mathf.Max(currentGold, 0);    // don't let gold go below 0
        return currentGold;
    }
    public int IncreaseTopSpeedBonus(int bonus)
    {
        bonusTopSpeed += bonus;
        return bonusTopSpeed;
    }
    public float IncreaseAccelerationBonus(float bonus)
    {
        bonusAcceleration += bonus;
        return bonusAcceleration;
    }
}