using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    Manager_UI manager_UI;
    PlayerController player;

    bool hasBeenActivated;
    bool hasCutScene;
    Image cutsceneImage;

    [Header("Item Specific Settings.")]
    public string itemName = "";
    public AudioClip optionalPickupSound;

    [Header("Optional Door Settings")]
    public int DoorIndexToUnlock = -1;

    [Header("Misc. Settings you probably won't need to change.")]
    public float despawnTime = 1f;
    public float pickUpDistance = 0.5f;
    float shrinkagePercentage = 1;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        manager_UI = GameObject.Find("_Canvas").GetComponent<Manager_UI>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (hasBeenActivated)
        {
            transform.localScale = Vector3.one * shrinkagePercentage;
            shrinkagePercentage -= Time.deltaTime / despawnTime;

            if (hasCutScene)
            {
                cutsceneImage.color = new Color(1, 1, 1,  1 - shrinkagePercentage);
                print(cutsceneImage.name);  
            }

            return;
        }

        if (Vector2.Distance(transform.position, player.transform.position) < pickUpDistance)
            OnPickup();
    }

    void OnPickup()
				{
        bool pickup = true;

        if (itemName.Length > 0) // It has a name
        {
            itemName = itemName.ToLower();

            if (itemName == "crate")
            {
                if (player.currentWeapon.name != "Crowbar" || player.currentWeapon.cooldown > 0)
                    return;

                GetComponent<Collider2D>().isTrigger = true;
                pickup = false;
            }

            if (itemName == "park door")
            {
                if (!player.haveParkKey) // The crowbar
                    return;

                GetComponent<Collider2D>().isTrigger = true;
                pickup = false;
            }

            if (itemName == "key_prison")
            {
                player.havePrisonKey = true;
            }

            if (itemName == "key_park")
            {
                hasCutScene = true;
                manager_UI.OnSelectingBlueprint();

                player.haveParkKey = true;
                manager_UI.Button_Blueprint_Close.transform.GetChild(2).gameObject.SetActive(true);
                cutsceneImage = manager_UI.Button_Blueprint_Close.transform.GetChild(2).GetComponent<Image>();
            }

            if (itemName == "crowbar")
            {
                player.weapons[1].isUnlocked = true;
                player.currentWeapon = player.weapons[1];

                manager_UI.SwitchingToMelee();
            }

            if (itemName == "pistol")
            {
                player.weapons[2].isUnlocked = true;
                player.currentWeapon = player.weapons[2];

                manager_UI.SwitchingToFireArm();
            }

            if (itemName == "battery")
            {
                hasCutScene = true;
                manager_UI.OnSelectingBlueprint();

                manager_UI.Button_Blueprint_Close.transform.GetChild(1).gameObject.SetActive(true);
                cutsceneImage = manager_UI.Button_Blueprint_Close.transform.GetChild(1).GetComponent<Image>();
            }
        }

        hasBeenActivated = true;
        GameObject.Destroy(gameObject, despawnTime);

        if (pickup)
        {
            transform.parent = player.transform;
            transform.localPosition = Vector2.up * 1;
        }
        else
            GetComponent<SpriteRenderer>().sortingOrder = -1;

        if (optionalPickupSound != null)
            gameObject.AddComponent<AudioSource>().PlayOneShot(optionalPickupSound);

            if (DoorIndexToUnlock != -1)
            {
                Manager_Doorways manager_Doorways = GameObject.Find("_ScriptManager").GetComponent<Manager_Doorways>();

                manager_Doorways.doorways[DoorIndexToUnlock].isLocked = false;
            }  
				}
}


