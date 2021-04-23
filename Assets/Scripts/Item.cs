using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public float despawnTime = 1f;

    float shrinkagePercentage = 1;


    public int DoorIndexToUnlock = -1;

    public Manager_Doorways manager_DoorWay;

    PlayerController player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    bool isCarried;

    // Update is called once per frame
    void Update()
    {
        if (!isCarried && Vector2.Distance(transform.position, player.transform.position) < 0.5f)
            OnPickup();


        if (isCarried)
        {
           

            transform.localScale = Vector3.one * shrinkagePercentage;


            shrinkagePercentage -= Time.deltaTime / despawnTime;
        }
    }


    void OnPickup()
				{
        transform.parent = player.transform;

        transform.localPosition = Vector2.up * 1;

        isCarried = true;

        GameObject.Destroy(gameObject, despawnTime);


        if (DoorIndexToUnlock == -1)
            return;

        manager_DoorWay.doorways[DoorIndexToUnlock].isLocked = false;
        
        
				}
}
