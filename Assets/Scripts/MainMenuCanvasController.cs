using UnityEngine;
using TMPro;
public class MainMenuCanvasController : MonoBehaviour
{
    public GameObject CharacterSelectPanel;

    public TMP_Text MenuCharacterName;
    public TMP_Text MenuPanelCharacterName;

    public string CharacterName;
    void Start()
    {
        CharacterNameDesign();
    }

    public void ChracterSelectPanelOn() 
    {
        CharacterSelectPanel.SetActive(true);
    }


    public void CharacterSelectPanelOff() 
    {
        CharacterSelectPanel.SetActive(false);
    }

    public void SelectSincap() 
    {
        PlayerPrefs.SetInt("selectedCharacter", 0);
        CharacterNameDesign();
    }

    public void SelectRakun() 
    {
        PlayerPrefs.SetInt("selectedCharacter", 1);
        CharacterNameDesign();
    }


    void CharacterNameDesign() 
    {
        int n = PlayerPrefs.GetInt("selectedCharacter");
        if (n == 0) 
        {
            CharacterName = "Sincap";
            MenuCharacterName.text = CharacterName;
            MenuPanelCharacterName.text = CharacterName;


        }
        else if(n == 1) 
        {
            CharacterName = "Rakun";
            MenuCharacterName.text = CharacterName;
            MenuPanelCharacterName.text = CharacterName;

        }
    }

}
