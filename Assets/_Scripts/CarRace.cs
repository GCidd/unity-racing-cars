using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;
using UnityStandardAssets.Utility;

public enum RaceResult
{
    NoResult,
    Win,
    Loss
}

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public class CarRace : MonoBehaviour
{
    public static RaceResult result = RaceResult.NoResult;
    public static Difficulty raceDifficulty = Difficulty.Easy;

    public StartingPositionsOptions startingPositionOptions;
    public Text uiLap;
    public Text uiSpeed;
    public Text uiPosition;
    public Text uiBoost;
    public Text uiResult;
    public Text uiContinue;
    public GameObject finishedNamesContainer;

    public GameObject mainCamera;
    public GameObject raceEndCamera;
    public GameObject cameraPositions;
    public Transform m_Target;

    public static List<EnemyStats> enemyStats = new List<EnemyStats>();
    public static PlayerStats playerStats { get; set; }
    private List<GameObject> enemyCars = new List<GameObject>();
    private GameObject bannerText;
    private GameObject playerCar;
    public static List<GameObject> allCars;
    private float carWidth = 0;
    private float carLength = 0;
    private Vector3 startingPosition;
    private int playerPosition = 0;
    public int maxLaps = 1;
    private bool raceEnded = false;

    private void Awake()
    {
        foreach (GameObject enemy in Populator.enemies)
        {
            enemy.SetActive(false);
        }
        Populator.player.SetActive(false);
        Populator.playerCamera.SetActive(false);
        result = RaceResult.NoResult;
    }

    // Start is called before the first frame update
    private void Start()
    {
        playerCar = GameObject.Find("Player Car");
        bannerText = GameObject.Find("Banner Text");
        startingPosition = GameObject.Find("StartingPosition").transform.position;
        SetupPositions();
        SetupUI();
    }
    
    private void SetupPositions()
    {
        allCars = new List<GameObject>();
        GameObject enemyCar = GameObject.Find("EnemyCar");
        int carIndex = 0;   // car's position

        if (raceDifficulty == Difficulty.Normal)
        {
            playerPosition = Random.Range(0, startingPositionOptions.totalPerRow);
        }
        else if (raceDifficulty == Difficulty.Hard)
        {
            playerPosition = Random.Range(startingPositionOptions.totalPerRow, enemyStats.Count);
        }

        carWidth = playerCar.transform.Find("Colliders").transform.Find("ColliderBottom").GetComponent<MeshRenderer>().bounds.size.x;
        carLength = playerCar.transform.Find("Colliders").transform.Find("ColliderBottom").GetComponent<MeshRenderer>().bounds.size.z;
        
        allCars.Add(playerCar);
        PositionCar(playerCar, playerPosition);
        playerCar.GetComponent<CarController>().SetupStats(playerStats);

        foreach (EnemyStats enemy in enemyStats)
        {
            if (carIndex == playerPosition)
                carIndex++;

            GameObject newCar = Instantiate(enemyCar);
            newCar.GetComponent<CarController>().SetupStats(enemy);
            newCar.GetComponent<CarAIControl>().SetupStats(enemy);
            newCar.name = enemy.name;

            PositionCar(newCar, carIndex);

            carIndex++;
            enemyCars.Add(newCar);
        }

        allCars.AddRange(enemyCars);
        enemyCar.SetActive(false);
    }

    private void PositionCar(GameObject car, int index)
    {
        float offsetX = (index % StartingPositionsOptions.TotalPerRow) * (index + StartingPositionsOptions.DistanceBetweenCars);
        float offsetZ = (index / StartingPositionsOptions.TotalPerRow) * (index + StartingPositionsOptions.DistanceBetweenRows);

        car.transform.position = new Vector3(
            startingPosition.x + offsetX,
            startingPosition.y,
            startingPosition.z - offsetZ
        );
    }

    private void SetupUI()
    {
        uiLap.text = string.Format("{0}/{1}", 1, maxLaps);
        uiPosition.text = "1st";
    }

    // Update is called once per frame
    void Update()
    {
        // update UI here maybe
        UpdateUI();

        foreach (GameObject car in allCars.FindAll(c => c.GetComponent<WaypointProgressTracker>().lap == maxLaps + 1 && !c.GetComponent<WaypointProgressTracker>().finishedRace))
        {   // someone finished the game
            car.GetComponent<WaypointProgressTracker>().FinishedRace();
            if (result == RaceResult.NoResult)
            {   // run only once when someone finished race
                if (playerCar.GetComponent<WaypointProgressTracker>().finishedRace)
                    OnPlayerWin();
                else
                    OnPlayerLoss();
            }

            AddFinisheddName(car);
        }
        if (!raceEnded)
        {
            CheckBannerFinish();

            if (playerCar.GetComponent<WaypointProgressTracker>().finishedRace)
            {
                mainCamera.SetActive(false);
                raceEndCamera.SetActive(true);
                raceEnded = true;

                if (result == RaceResult.Win)
                {
                    uiResult.text = "You won!";
                    uiResult.gameObject.SetActive(true);
                }
                else if (result == RaceResult.Loss)
                {
                    uiResult.text = "You lost!";
                    uiResult.gameObject.SetActive(true);
                }
                uiContinue.gameObject.SetActive(true);

                playerCar.AddComponent<CarAIControl>();
                playerCar.GetComponent<CarAIControl>().SetTarget(m_Target);
                Destroy(playerCar.GetComponent<CarUserControl>());
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
            int playerWaypoint = playerCar.GetComponent<WaypointProgressTracker>().waypoint;
            
            if (playerWaypoint <= 2)
            {
                raceEndCamera.transform.position = cameraPositions.transform.Find("Camera Position 1").transform.position;
            }
            else if (playerWaypoint <= 6)
            {
                raceEndCamera.transform.position = cameraPositions.transform.Find("Camera Position 2").transform.position;
            } 
            else if (playerWaypoint <= 12)
            {
                raceEndCamera.transform.position = cameraPositions.transform.Find("Camera Position 3").transform.position;
            }
            else if (playerWaypoint <= 16)
            {
                raceEndCamera.transform.position = cameraPositions.transform.Find("Camera Position 4").transform.position;
            }
            else if (playerWaypoint <= 19)
            {
                raceEndCamera.transform.position = cameraPositions.transform.Find("Camera Position 5").transform.position;
            }
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            result = RaceResult.NoResult;
            foreach (GameObject enemy in Populator.enemies)
                Destroy(enemy);
            foreach (GameObject car in allCars)
                Destroy(car);
            Destroy(Populator.playerCamera);
            Destroy(Populator.player);
            Destroy(gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
        }
    }

    void OnPlayerWin()
    {
        Debug.Log("PLAYER WON RACE.");
        result = RaceResult.Win;
    }

    void OnPlayerLoss()
    {
        Debug.Log("PLAYER LOST RACE.");
        result = RaceResult.Loss;
    }

    void UpdateUI()
    {
        int[] places = DeterminePlaces();
        int playerPlace = places[allCars.IndexOf(playerCar)];
        uiPosition.text = (playerPlace + 1).ToString() + (playerPlace == 0 ? "st" : playerPlace == 1 ? "nd" : playerPlace == 2 ? "rd" : "th");
        uiSpeed.text = string.Format("{0} kmh", playerCar.GetComponent<CarController>().CurrentSpeed.ToString("0"));
        uiLap.text = string.Format("{0}/{1}", playerCar.GetComponent<WaypointProgressTracker>().lap, maxLaps);
        
        if (playerCar.GetComponent<CarController>().boostEffect == 0)
            uiBoost.gameObject.SetActive(false);
        else
        {
            uiBoost.gameObject.SetActive(true);
            if (playerCar.GetComponent<CarController>().boostEffect == 1)
            {
                uiBoost.text = "Boost active!";
            }
            else
            {
                uiBoost.text = "Negative boost active!";
            }
        }
    }

    int[] DeterminePlaces()
    {
        List<int> places = new List<int>();
        
        List<GameObject> sortedCars = allCars.OrderByDescending(car => car.GetComponent<WaypointProgressTracker>().progressDistance).ToList();
        foreach(GameObject car in allCars)
        {
            // total waypoints car has passed
            places.Add(sortedCars.IndexOf(car));
        }

        return places.ToArray();
    }

    void CheckBannerFinish()
    {
        if (bannerText.GetComponent<TMPro.TextMeshPro>().text == "FINISH")
            return;

        if (playerCar.GetComponent<WaypointProgressTracker>().lap == maxLaps)
        {
            bannerText.GetComponent<TMPro.TextMeshPro>().text = "FINISH";
        }
    }

    private void AddFinisheddName(GameObject car)
    {
        GameObject enemyNameUI = Instantiate(finishedNamesContainer.transform.Find("Name sample").gameObject, finishedNamesContainer.transform);
        enemyNameUI.name = car == playerCar ? "You" : car.name;
        int finishedCars = finishedNamesContainer.transform.childCount - 1; // -1 for the sample text
        Debug.Log(car.name + " " + finishedCars);
        string place = finishedCars.ToString() + (finishedCars == 1 ? "st" : finishedCars == 2 ? "nd" : finishedCars == 3 ? "rd" : "th");
        enemyNameUI.GetComponent<Text>().text = string.Format("{0} {1}", place, enemyNameUI.name);
        enemyNameUI.transform.localPosition = new Vector3(
                0,
                enemyNameUI.transform.localPosition.y - finishedNamesContainer.transform.childCount * enemyNameUI.GetComponent<RectTransform>().rect.height,
                enemyNameUI.transform.localPosition.z
            );
        enemyNameUI.SetActive(true);
    }
}
