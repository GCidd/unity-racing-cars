using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.Vehicles.Car;

public class BattleStart : MonoBehaviour
{

    public GameObject playerUI;
    public GameObject joinedEnemiesContainer;
    public GameObject mainCamera;
    public static List<GameObject> closeJoinEnemies { get; private set; }  // enemies that will join the battle
    public static List<string> closeJoinEnemyNames = new List<string>();    // names of the enemies that joined the battle
    public static List<GameObject> notJoinedEnemies = new List<GameObject>();
    public float maximumJoinDistance = 10;
    private bool startingBattle = false;
    private GameObject player;
    private GameObject resultText;
    private static Vector3 initialPlayerPos;    // initial player position
    private static Vector3 initialCameraPos;    // initial camera position
    private static Vector3 playerPos;   // player position before battle start
    private static Vector3 cameraPos;   // camera position before battle start

    // Start is called before the first frame update
    void Start()
    {
        player = Populator.player;
        mainCamera = Populator.playerCamera;
        resultText = playerUI.transform.Find("Race result").gameObject;

        if (CarRace.result == RaceResult.Win)
        {
            OnPlayerWin();
        }
        else if (CarRace.result == RaceResult.Loss)
        {
            OnPlayerLoss();
        }
        else if (CarRace.result == RaceResult.NoResult)
        {
            initialCameraPos = GameObject.Find("FreeLookCameraRig").transform.position;
            initialPlayerPos = player.transform.position;
            UpdateUI();
            DontDestroyOnLoad(mainCamera);
        }
    }

    void OnPlayerWin()
    {
        player.SetActive(true);
        Populator.playerCamera.SetActive(true);
        // make player come back to scene from a bit higher for effects
        playerPos.y += 2;
        player.transform.position = playerPos;
        // set camera positions
        mainCamera.transform.position = cameraPos;
        int totalGold = 0;  // total gold got from battle
        foreach (GameObject enemy in Populator.enemies.FindAll(e => e.GetComponent<EnemyMovement>().joined))
        {   // find enemies that was in fight, remove them and get the gold they reward
            //GameObject enemyObject = GameObject.Find(enemy);
            enemy.SetActive(false);
            totalGold += enemy.GetComponent<EnemyStats>().GoldReward;
        }

        foreach (GameObject enemy in Populator.enemies.FindAll(e => !e.GetComponent<EnemyMovement>().joined))
        {
            enemy.SetActive(true);
        }
        Populator.enemies.RemoveAll(e => !e.activeSelf && e.GetComponent<EnemyMovement>().joined);
        player.GetComponent<PlayerStats>().IncreaseGold(totalGold); // award player with the gold
        player.GetComponent<ThirdPersonUserControl>().OnBattleReturned();
        closeJoinEnemyNames = new List<string>();  // empty list
        UpdateUI();
        resultText.GetComponent<Text>().text = string.Format("You won! (+{0}G)", totalGold);
        resultText.SetActive(true);
        InvokeRepeating("UpdateResult", 2, 0.01f);
    }

    void OnPlayerLoss()
    {
        player.SetActive(true);
        Populator.playerCamera.SetActive(true);
        // reset position
        player.transform.position = initialPlayerPos;
        mainCamera.transform.position = initialCameraPos;
        player.GetComponent<PlayerStats>().DecreaseGold(closeJoinEnemyNames.Count);    // decrease player gold
        player.GetComponent<ThirdPersonUserControl>().OnBattleReturned();

        foreach (GameObject enemy in Populator.enemies)
        {
            enemy.SetActive(true);
            if (enemy.GetComponent<EnemyMovement>().joined)
                enemy.GetComponent<EnemyMovement>().Return();
        }
        UpdateUI();
        resultText.GetComponent<Text>().text = string.Format("You lost! (-{0}G)", closeJoinEnemyNames.Count);
        resultText.SetActive(true);
        InvokeRepeating("UpdateResult", 2, 0.01f);
        closeJoinEnemyNames = new List<string>();  // empty list
    }

    void UpdateResult()
    {
        Text text = resultText.GetComponent<Text>();

        Color textColor = text.GetComponent<Text>().color;
        text.GetComponent<Text>().color = new Color(
            textColor.r,
            textColor.g,
            textColor.b,
            textColor.a - 0.05f
        );

        if (textColor.a <= 0)
        {
            resultText.SetActive(false);
            text.GetComponent<Text>().color = new Color(
                textColor.r,
                textColor.g,
                textColor.b,
                1.0f
            );
            CancelInvoke("UpdateResult");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startingBattle)
        {
            // get all the enemies that can join the battle but are far away
            List<GameObject> closeEnemies = Populator.enemies.FindAll(enemy => Vector3.Distance(enemy.transform.position, player.transform.position) <= 2);

            // call function for when they join a battle (they got close enough to the player)
            foreach (GameObject joinedEnemy in closeEnemies)
            {
                joinedEnemy.GetComponent<EnemyMovement>().OnJoined();
            }

            if (Populator.enemies.FindAll(e => e.GetComponent<EnemyMovement>().joined).Count == closeJoinEnemies.Count)
            {   // if the enemies came close enough to the player to join the battle
                List<EnemyStats> stats = new List<EnemyStats>();
                foreach (var enemy in closeJoinEnemies)
                {   // get their stats and names
                    stats.Add(enemy.GetComponent<EnemyStats>());
                    closeJoinEnemyNames.Add(enemy.name);
                }
                // save player and camera positions
                playerPos = player.transform.position;
                cameraPos = mainCamera.transform.position;

                // pass info to the car race class
                CarRace.enemyStats = stats;
                CarRace.playerStats = player.GetComponent<PlayerStats>();
                CarRace.raceDifficulty = stats.Select(i => i.difficulty).Max();

                // change scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("CarRace");
            }
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        CarRace.result = RaceResult.NoResult;
        foreach (GameObject enemy in Populator.enemies)
            Destroy(enemy);
        Destroy(Populator.player);
        Destroy(gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }

    void UpdateUI()
    {   // for now it only updates the gold
        playerUI.transform.Find("Total gold").GetComponent<Text>().text = string.Format("{0}G", PlayerStats.currentGold);
    }

    private void OnPlayerBattleInitiate(Collider enemy)
    {   // called when player initiates a battle
        Boost playerBoost = new Boost(50, 10, 1, 0.1f, false);
        Boost enemyBoost = new Boost(50, 5, 0.2f, 0.05f, true);
        player.GetComponent<PlayerStats>().boost = playerBoost;
        enemy.GetComponent<EnemyStats>().boost = enemyBoost;
    }

    private void OnEnemyBattleInitiate(Collider enemy)
    {   // called when enemy initiates a battle
        Boost enemyBoost = new Boost(50, 10, 1, 0.1f, false);
        Boost playerBoost = new Boost(50, 5, 0.2f, 0.05f, true);

        player.GetComponent<PlayerStats>().boost = playerBoost;
        enemy.GetComponent<EnemyStats>().boost = enemyBoost;
    }

    private void Collisioned(Collider collision)
    {
        if (collision.gameObject.tag == "Enemy" && !startingBattle)
        {   // if player collided with an enemy and a battle is not already starting
            joinedEnemiesContainer.SetActive(true);
            GameObject nameUI = Instantiate(joinedEnemiesContainer.transform.Find("Enemy name sample").gameObject, joinedEnemiesContainer.transform);
            nameUI.name = collision.name;
            if (!collision.GetComponent<EnemyMovement>().EnemyInitiated())
            {
                OnPlayerBattleInitiate(collision);
                nameUI.GetComponent<Text>().text = string.Format("Initiated battle on {0}!", collision.name);
                nameUI.GetComponent<Text>().color = Color.green;
            }
            else
            {
                OnEnemyBattleInitiate(collision);
                nameUI.GetComponent<Text>().text = collision.name + " initiated battle!";
                nameUI.GetComponent<Text>().color = Color.red;
            }
            nameUI.SetActive(true);

            // get all the enemies and keep those that are close enough to join the battle (run to the player)
            closeJoinEnemies = Populator.enemies.FindAll(enemy => Vector3.Distance(enemy.transform.position, player.transform.position) <= maximumJoinDistance);
            foreach (GameObject enemy in closeJoinEnemies)
            {   // call function for when enemy starts joining a fight
                enemy.GetComponent<EnemyMovement>().JoinFight();
            }


            // battle is now starting
            startingBattle = true;
        }
    }

    private void AddJoinedName(GameObject enemy)
    {
        GameObject enemyNameUI = Instantiate(joinedEnemiesContainer.transform.Find("Enemy name sample").gameObject, joinedEnemiesContainer.transform);
        enemyNameUI.name = enemy.name;
        enemyNameUI.GetComponent<Text>().text = enemy.name + " joined the race!";
        enemyNameUI.transform.localPosition = new Vector3(
                0,
                // childCount - 2: 1 for the sample text and 1 for the initiated battle text
                enemyNameUI.transform.localPosition.y - (joinedEnemiesContainer.transform.childCount - 2) * enemyNameUI.GetComponent<RectTransform>().rect.height,
                enemyNameUI.transform.localPosition.z
            );
        enemyNameUI.GetComponent<Text>().color = enemy.GetComponent<EnemyStats>().carCustomization.mainColor;
        enemyNameUI.SetActive(true);
    }
}
