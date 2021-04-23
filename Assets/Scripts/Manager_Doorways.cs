using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_Doorways : MonoBehaviour
{
   [System.Serializable]
    public struct Doorway
    {
        public bool isLocked;
        [Header("Scene switching Doorways")]
        public bool doorSwitchesScene;
        public string sceneName;
  
        [Header("Doorway One")]
        public Vector2 Enterance1;
        [Header("Doorway Two")]
        public Vector2 Enterance2;


    }

    public Doorway[] doorways = new Doorway[1];

    PlayerController player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < doorways.Length; i++)
        {
            if (doorways[i].isLocked)
                continue;


            if (Vector2.Distance(player.transform.position, doorways[i].Enterance1) < 0.5f)
            {
                Vector3 directionToDoorway = ((Vector3)doorways[i].Enterance1) - player.transform.position;

                if (Vector2.Dot(player.velocity, directionToDoorway) > 0)
                    player.transform.position = doorways[i].Enterance2;

                if (doorways[i].doorSwitchesScene)
                    SceneManager.LoadScene(doorways[i].sceneName);

            }

            if (Vector2.Distance(player.transform.position, doorways[i].Enterance2) < 0.5f)
            {
                Vector3 directionToDoorway = ((Vector3)doorways[i].Enterance2) - player.transform.position;

                if (Vector2.Dot(player.velocity, directionToDoorway) > 0)
                    player.transform.position = doorways[i].Enterance1;

                if (doorways[i].doorSwitchesScene)
                    SceneManager.LoadScene(doorways[i].sceneName);
            }
        }
    }
}
