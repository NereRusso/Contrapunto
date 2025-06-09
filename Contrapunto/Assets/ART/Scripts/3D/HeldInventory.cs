using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HeldInventory : MonoBehaviour
{
    public static HeldInventory Instance;

    public Transform holder; // HeldObjects bajo la cámara
    public float spacing = 0.5f; // Distancia entre objetos agarrados

    private List<AgarrarObjeto> heldObjects = new List<AgarrarObjeto>();
    public int maxObjects = 3;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool AddObject(AgarrarObjeto obj)
    {
        if (heldObjects.Count >= maxObjects)
            return false;

        AgarrarObjeto visualCopy = Instantiate(obj, holder);
        visualCopy.transform.localRotation = Quaternion.Euler(obj.inventoryRotation);
        visualCopy.transform.localScale = Vector3.one * obj.inventoryScale;
        visualCopy.transform.localPosition = Vector3.zero; // <<< Asegurar que su posición local sea correcta inmediatamente
        visualCopy.GetComponent<Collider>().enabled = false;

        heldObjects.Add(visualCopy);

        UpdateSlots(); // Acomodar los objetos

        return true;
    }


    public AgarrarObjeto GetHeldObject(string objectID)
    {
        foreach (var obj in heldObjects)
        {
            if (obj.objectID == objectID)
                return obj;
        }
        return null;
    }

    public void RemoveObject(AgarrarObjeto obj)
    {
        if (heldObjects.Contains(obj))
        {
            heldObjects.Remove(obj);
            Destroy(obj.gameObject);
            UpdateSlots();
        }
    }

    private void UpdateSlots()
    {
        int count = heldObjects.Count;
        if (count == 0) return;

        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < heldObjects.Count; i++)
        {
            Vector3 localPos = new Vector3(startX + i * spacing, 0f, 0f);
            heldObjects[i].transform.localPosition = localPos;
        }
    }

}
