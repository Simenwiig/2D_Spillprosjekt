using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Doorways : MonoBehaviour
{
   [System.Serializable]
    public struct Doorway
    {
        [Header("Doorway One")]
        public Vector2 Enterance1;
        [Header("Doorway Two")]
        public Vector2 Enterance2;
    }

    public Doorway[] doorways;

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
            if (Vector2.Distance(player.transform.position, doorways[i].Enterance1) < 0.5f)
            {
                Vector3 directionToDoorway = ((Vector3)doorways[i].Enterance1) - player.transform.position;

                if (Vector2.Dot(player.velocity, directionToDoorway) > 0)
                    player.transform.position = doorways[i].Enterance2;
            }

            if (Vector2.Distance(player.transform.position, doorways[i].Enterance2) < 0.5f)
            {
                Vector3 directionToDoorway = ((Vector3)doorways[i].Enterance2) - player.transform.position;

                if (Vector2.Dot(player.velocity, directionToDoorway) > 0)
                    player.transform.position = doorways[i].Enterance1;
            }
        }
    }
}
