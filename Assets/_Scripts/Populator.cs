using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Populator : MonoBehaviour
{
    public GameObject populateWith;
    public Vector3 regionCenter;
    public int enemiesToCreate = 30;

    public static List<GameObject> enemies { get; private set; } = new List<GameObject>();
    public static GameObject player;
    public static GameObject playerCamera;
    // Start is called before the first frame update
    void Awake()
    {
        if (CarRace.result == RaceResult.NoResult)
        {
            string[] male_names = (Resources.Load("male_names") as TextAsset).text.Split('\n');
            string[] female_names = (Resources.Load("female_names") as TextAsset).text.Split('\n');

            for (int i = 0; i < enemiesToCreate; i++)
            {
                Vector2 randomPosCircle = Random.insideUnitCircle * 27;
                Vector3 randomPos = new Vector3(
                        randomPosCircle.x,
                        1f,
                        randomPosCircle.y
                    );
                GameObject newEnemy = Instantiate(populateWith, randomPos, populateWith.transform.rotation);

                // assign random names to new enemies
                int randomIndex = (int)Random.Range(0f, male_names.Length - 1);
                newEnemy.name = Random.Range(0f, 1f) > 0.5 ? male_names[randomIndex] : female_names[randomIndex];
                newEnemy.GetComponent<EnemyStats>().SetName(newEnemy.name);
                DontDestroyOnLoad(newEnemy);

                enemies.Add(newEnemy);
            }
            populateWith.SetActive(false);
            player = GameObject.Find("Player");
            playerCamera = GameObject.Find("FreeLookCameraRig");
        }
    }
}
