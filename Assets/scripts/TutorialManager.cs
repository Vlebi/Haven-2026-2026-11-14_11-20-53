using UnityEngine;
using TMPro; 

public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject tutorialPanel;      
    public TextMeshProUGUI tutorialText;  

    [Header("Tutorial Strings")]
    [TextArea(2, 5)] 
    public string[] tutorialLines;

    private int currentLineIndex = 0;

    void Start()
    {
        
        if (tutorialLines.Length > 0 && tutorialText != null)
        {
            tutorialText.text = tutorialLines[0];
        }
    }

    
    public void NextLine()
    {
        currentLineIndex++;

        
        if (currentLineIndex < tutorialLines.Length)
        {
            tutorialText.text = tutorialLines[currentLineIndex];
        }
        else
        {
           
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }
        }
    }
}