using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int inventorySize = 5; // Haggle er 3, maskin pistol 2, pistol 1.
    int currentCapacity = 0;

    public Items[] items = new  Items[10];

    public struct Items
    {
        public float fade;
        public Transform transform;

        public Items(Transform item)
        {
            transform = item;
            fade = 1f;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (items[i].fade > 0)
            {
                items[i].fade -= Time.deltaTime;
                items[i].transform.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, items[i].fade);
            }
        }
    }

    void AddItem(int itemSize)
    {
        if (itemSize + currentCapacity > inventorySize)
        {
            Debug.Log("Inventory is full.");
            return;
        }
        else
            currentCapacity += itemSize;
    }
}
