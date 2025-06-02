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
            Debug.Log("Hiç oyuncu bulunamadý.");
            return;
        }

        // netId'ye göre sýrala, herkes ayný listeyi görecek
        Players = Players.OrderBy(p => p.netId).ToArray();

        Debug.Log("Güncel Oyuncu Sayýsý: " + Players.Length);

        // Tüm Player panel objelerini kapat
        for (int i = 0; i < PlayerListRoot.transform.childCount; i++)
        {
            PlayerListRoot.transform.GetChild(i).gameObject.SetActive(false);
        }

        // Maksimum 4 oyuncu gösterilecek
        for (int i = 0; i < Players.Length && i < PlayerListRoot.transform.childCount; i++)
        {
            GameObject panel = PlayerListRoot.transform.GetChild(i).gameObject;
            panel.SetActive(true);

            OnlinePrefabLobbyController player = Players[i];

            // NameText bul (panel altýndaki TMP_Text componenti)
            TMP_Text nameText = panel.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = player.playerName;


            TMP_Text ScoreText = panel.transform.GetChild(0).GetComponentInChildren<TMP_Text>();

            ScoreText.text = "" + player.currentCharacterIdentity.gameObject.GetComponent<AppleGameControl>().applecount;

            //ScoreText= player.gameObject.GetComponent<>


            // Ready Image bul (panel altýndaki Image, nameText dýþýndaki ilk Image)





        }
    }
    void Update()
    {
        
    }
}
