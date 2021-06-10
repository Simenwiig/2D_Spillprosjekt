using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering.Universal;

public class Item : MonoBehaviour
{
    Manager_UI manager_UI;
    PlayerController player;

    bool hasBeenActivated;
    bool hasCutScene;
    Image cutsceneImage;
    AudioSource audioSource;

    [Header("Item Specific Settings.")]
    public string itemName = "";
    public AudioClip optionalPickupSound;
    public AudioClip optionalPickupSound_2;
    public GameObject optionalGodray;
    public Sprite optionalSprite;
    public GameObject optionalGameObject;
    public GameObject optionalRequiredObject;


    [Header("Optional Door Settings")]
    public string nameOfDoorIUnlock = "None";

    [Header("Misc. Settings you probably won't need to change.")]
    public float despawnTime = 1f;
    public float pickUpDistance = 0.5f;
    float shrinkagePercentage = 1;

    bool hadRequiredItem;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        manager_UI = Manager_UI.GetManager();


        if (itemName.ToLower().Contains("tutorial_"))
            GetComponentInChildren<SpriteRenderer>().enabled = false;

        if (itemName == "phone controller" && !Application.isEditor)
            gameObject.SetActive(false);


        if (optionalPickupSound != null || optionalPickupSound_2 != null)
            audioSource = gameObject.AddComponent<AudioSource>();

            hadRequiredItem = optionalRequiredObject != null;
      }


    // Update is called once per frame
    void Update()
    {
        if (hasBeenActivated)
        {
            transform.localScale = Vector3.one * shrinkagePercentage;
            shrinkagePercentage -= Time.deltaTime / despawnTime;



            if (optionalGodray != null)
            {
                optionalGodray.GetComponent<Light2D>().intensity = shrinkagePercentage;
                optionalGodray.transform.GetChild(0).GetComponent<Light2D>().intensity = shrinkagePercentage;
            }


            if (hasCutScene)
            {
                transform.localScale = Vector3.zero;
                cutsceneImage.color = new Color(1, 1, 1,  1 - shrinkagePercentage);
            }

            return;
        }

        if (Vector2.Distance(transform.position, player.transform.position) < pickUpDistance && !player.isFalling)
            OnPickup();
    }

    void OnPickup()
				{
        bool isPickup = true;


        if (itemName.Length > 0) // It has a name
        {
            itemName = itemName.ToLower();

            if (itemName == "crate")
            {
                bool usingWrongTool = player.currentWeapon.name != "Crowbar";
                bool isSwinging = !player.currentWeapon.readyToFire;

                if (usingWrongTool || !isSwinging)
                {
                    if (usingWrongTool && isSwinging && optionalPickupSound_2 != null)
                        audioSource.PlayOneShot(optionalPickupSound_2, PlayerPrefs.GetFloat("MainVolume") * PlayerPrefs.GetFloat("SFXVolume"));

                    return;
                }

                GetComponent<Collider2D>().enabled = false;
                isPickup = false;
            }

            


            if (itemName == "key_unlockpark")
            {
                isPickup = false;
                hasCutScene = true;
                manager_UI.OnSelectingBlueprint();
                manager_UI.UI_Blueprints.transform.GetChild(1).gameObject.SetActive(true);
                cutsceneImage = manager_UI.UI_Blueprints.transform.GetChild(1).GetComponent<Image>();
            }


            if (itemName == "key_helipad")
            {
                GameObject.Find("door_helipad").GetComponent<Item>().itemName = "door_openable";
            }

            if (itemName == "crowbar")
            {
                player.weapons[1].isUnlocked = true;
                player.currentWeapon = player.weapons[1];

                if (player.weapons[2].isUnlocked)
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
                isPickup = false;
                hasCutScene = true;
                manager_UI.OnSelectingBlueprint();

                manager_UI.UI_Blueprints.transform.GetChild(0).gameObject.SetActive(true);
                cutsceneImage = manager_UI.UI_Blueprints.transform.GetChild(0).GetComponent<Image>();

            }

            if (itemName == "food" || itemName == "water")
            {
                float value = itemName == "food" ? 50f : 33f;

                if (player.hungerLevel > 100 - value * 0.1f) // If only 10% of the food would be spent, you ignore the water.
                    return;

                player.hungerLevel += Mathf.Min(value, 100 - player.hungerLevel);
            }


            if (itemName == "god water")
            {
                player.healthLevel = 100;
                player.hungerLevel = 100;

                player.isInGodmode = true;
            }

            if (itemName == "noclip beans")
            {
                player.hungerLevel = 100;

                player.GetComponentInChildren<Rigidbody2D>().isKinematic = true;
                player.moveSpeed *= 3;
            }

            if (itemName == "phone controller")
            {
                player.playingOnPC = !player.playingOnPC;
                player.damageTimer = 0.25f;
                Debug.Log("Changing control scheme to " + (player.playingOnPC ? "Mouse & Keyboard mode." : "Touch Screen mode."));

                
            }

            if (itemName.Contains("blueprint"))
            {
                isPickup = false;
                manager_UI.OnSelectingBlueprint();

                Manager_UI.GetManager().Button_Blueprint.interactable = true;
                Manager_UI.GetManager().Button_Blueprint.image.enabled = true;
            }


                if (itemName.Contains("tutorial_"))
            {
                int index = int.Parse(itemName.Remove(0, 9));



                if (index == 4) // The Radio Tower
                {
                    manager_UI.OnSelectingBlueprint();
                    bool hasItem = manager_UI.UI_Blueprints.transform.GetChild(0).gameObject.activeInHierarchy;
                    manager_UI.OnUnpausing();

                    if (!hasItem)
                        return;
                }

                if (index == 5) // The Radio Tower
                {
                    manager_UI.OnSelectingBlueprint();
                    bool hasItem = manager_UI.UI_Blueprints.transform.GetChild(0).gameObject.activeInHierarchy;
                    manager_UI.OnUnpausing();

                    if (hasItem)
                        return;
                }




                Manager_UI.GetManager().ActivateTutorialElement(index);


                if (index == 0)
                {
                    Manager_UI.GetManager().Button_Blueprint.interactable = false;
                    Manager_UI.GetManager().Button_Blueprint.image.enabled = false;

                    player.healthLevel = 90;
                    player.hungerLevel = 0;
                }
            }
        }




        hasBeenActivated = true;
        GameObject.Destroy(gameObject, despawnTime);


        if (hadRequiredItem && optionalGameObject == null)
        {
            GetComponent<Collider2D>().isTrigger = true;
            isPickup = false;
        }





        if (optionalGodray != null)
        {
            optionalGodray.transform.parent = null;
            GameObject.Destroy(optionalGodray, despawnTime);
        }

        if (optionalGameObject != null)
            optionalGameObject.SetActive(!optionalGameObject.activeInHierarchy);

        if (isPickup)
        {
            transform.parent = player.transform;
            transform.localPosition = Vector2.up * 1;
        }
        else
            GetComponent<SpriteRenderer>().sortingOrder = -1;

        if (optionalSprite != null)
            GetComponent<SpriteRenderer>().sprite = optionalSprite;

        if (optionalPickupSound != null)
            audioSource.PlayOneShot(optionalPickupSound, PlayerPrefs.GetFloat("MainVolume") * PlayerPrefs.GetFloat("SFXVolume"));

        if (nameOfDoorIUnlock != "None")
        {
            Manager_Door manager_Door = GameObject.Find("_ScriptManager").GetComponent<Manager_Door>();

            Manager_Door.DoorSet doorToUnlock = Manager_Door.DoorSet.GetDoor(manager_Door.Doors, nameOfDoorIUnlock, null);
            if (doorToUnlock != null)
                doorToUnlock.Lock(true);
        }


        
				}
}


