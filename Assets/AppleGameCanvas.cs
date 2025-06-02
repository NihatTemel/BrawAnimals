using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;
using UnityEngine.UI;
public class AppleGameCanvas : NetworkBehaviour
{

    

    public GameObject PlayerListRoot;
    public OnlinePrefabLobbyController[] Players;
    public Image KurekFillImg;
    public float KurekFillTimer;  // 1 = full(recently used)  <--->  0 = end
    public TMP_Text KurekFillText;
    void Start()
    {
        InvokeRepeating(nameof(RefreshPlayers), 0f, 0.5f); // Her 2 saniyede bir kontrol
    }

    

    void RefreshPlayers()
    {
        Players = FindObjectsByType<OnlinePrefabLobbyController>(FindObjectsSortMode.None);


        if (Players == null || Players.Length == 0)
        {
            Debug.Log("Hi� oyuncu bulunamad�.");
            return;
        }

        // netId'ye g�re s�rala, herkes ayn� listeyi g�recek
        Players = Players.OrderBy(p => p.netId).ToArray();

        Debug.Log("G�ncel Oyuncu Say�s�: " + Players.Length);

        // T�m Player panel objelerini kapat
        for (int i = 0; i < PlayerListRoot.transform.childCount; i++)
        {
            PlayerListRoot.transform.GetChild(i).gameObject.SetActive(false);
        }

        // Maksimum 4 oyuncu g�sterilecek
        for (int i = 0; i < Players.Length && i < PlayerListRoot.transform.childCount; i++)
        {
            GameObject panel = PlayerListRoot.transform.GetChild(i).gameObject;
            panel.SetActive(true);

            OnlinePrefabLobbyController player = Players[i];

            // NameText bul (panel alt�ndaki TMP_Text componenti)
            TMP_Text nameText = panel.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = player.playerName;


            TMP_Text ScoreText = panel.transform.GetChild(0).GetComponentInChildren<TMP_Text>();

            ScoreText.text = "" + player.currentCharacterIdentity.gameObject.GetComponent<AppleGameControl>().applecount;

            //ScoreText= player.gameObject.GetComponent<>


            // Ready Image bul (panel alt�ndaki Image, nameText d���ndaki ilk Image)





        }
    }
    void Update()
    {
        
    }
}
