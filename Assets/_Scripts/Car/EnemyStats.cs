using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    // This script provides input to the car controller in the same way that the user control script does.
    // As such, it is really 'driving' the car, with no special physics or animation tricks to make the car behave properly.

    // "wandering" is used to give the cars a more human, less robotic feel. They can waver slightly
    // in speed and direction while driving towards their target.

    [Header("Car AI Control Options")]
    [SerializeField] [Range(0, 1)] public float m_CautiousSpeedFactor = 0.5f;               // percentage of max speed to use when being maximally cautious
    [SerializeField] [Range(0, 180)] public float m_CautiousMaxAngle = 180f;                  // angle of approaching corner to treat as warranting maximum caution
    [SerializeField] public float m_CautiousMaxDistance = 100f;                              // distance at which distance-based cautiousness begins
    [SerializeField] public float m_CautiousAngularVelocityFactor = 30f;                     // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
    [SerializeField] public float m_SteerSensitivity = 0.01f;                                // how sensitively the AI uses steering input to turn to the desired direction
    [SerializeField] public float m_AccelSensitivity = 1f;                                // How sensitively the AI uses the accelerator to reach the current desired speed
    [SerializeField] public float m_BrakeSensitivity = 1f;                                   // How sensitively the AI uses the brake to reach the current desired speed
    [SerializeField] public float m_LateralWanderDistance = 3f;                              // how far the car will wander laterally towards its target
    [SerializeField] public float m_LateralWanderSpeed = 0.2f;                               // how fast the lateral wandering will fluctuate
    [SerializeField] [Range(0, 1)] public float m_AccelWanderAmount = 0.1f;                  // how much the cars acceleration will wander
    [SerializeField] public float m_AccelWanderSpeed = 0.1f;                                 // how fast the cars acceleration wandering will fluctuate
    [SerializeField] public BrakeCondition m_BrakeCondition = BrakeCondition.TargetDistance; // what should the AI consider when accelerating/braking?
    [SerializeField] public bool m_Driving = true;                                                  // whether the AI is currently actively driving or stopped.
    [SerializeField] public bool m_StopWhenTargetReached = false;                                    // should we stop driving when we reach the target?
    [SerializeField] public float m_ReachTargetThreshold = 2;                                // proximity to target to consider we 'reached' it, and stop driving.

    public float m_RandomPerlin;             // A random value for the car to base its wander on (so that AI cars don't all wander in the same pattern)
    public float m_AvoidOtherCarTime;        // time until which to avoid the car we recently collided with
    public float m_AvoidOtherCarSlowdown;    // how much to slow down due to colliding with another car, whilst avoiding
    public float m_AvoidPathOffset;          // direction (-1 or 1) in which to offset path to avoid other car, whilst avoiding


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
    [SerializeField] public float m_Topspeed = 150;
    [SerializeField] public int NoOfGears = 5;
    [SerializeField] public float m_RevRangeBoundary = 1f;
    [SerializeField] public float m_SlipLimit = 0.4f;
    [SerializeField] public float m_BrakeTorque = 20000;

    [Header("Other Options")]
    public Difficulty difficulty = Difficulty.Easy;
    public int GoldReward = 5;
    public Boost boost { get; set; }
    public new string name { get; private set; }

    public void SetName(string name)
    {
        this.name = name;
    }
    
    public CarCustomization carCustomization { get; private set; }
    public static Color RandomColor()
    {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);

        return new Color(r, g, b);
    }
    private void Awake()
    {
        carCustomization = new CarCustomization();
        carCustomization.mainColor = RandomColor();
        carCustomization.secondaryColor = RandomColor();
        carCustomization.wheelColor = RandomColor();
    }
}