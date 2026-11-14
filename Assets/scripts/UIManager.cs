using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject interactImageUI; 
    public float detectionRange = 2.5f; 
    public string interactTag = "Interactable";

    void Start()
    {
        
        if (interactImageUI != null)
        {
            interactImageUI.SetActive(false);
        }
    }

    void Update()
    {
        if (interactImageUI == null) return;

        
        GameObject[] interactables = GameObject.FindGameObjectsWithTag(interactTag);
        bool isNearSomething = false;

        foreach (GameObject obj in interactables)
        {
            float distance = Vector2.Distance(transform.position, obj.transform.position);

            if (distance <= detectionRange)
            {
                isNearSomething = true;
                break;
            }
        }

       
        if (interactImageUI.activeSelf != isNearSomething)
        {
            interactImageUI.SetActive(isNearSomething);
        }
    }
}