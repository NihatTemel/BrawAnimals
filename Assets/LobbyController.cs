using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;
public class LobbyController : MonoBehaviour
{
    public GameObject PlayerListRoot;
    public OnlinePrefabLobbyController[] Players;

    public GameObject StartGameButton;

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

            // Ready Image bul (panel alt�ndaki Image, nameText d���ndaki ilk Image)
            Image[] images = panel.GetComponentsInChildren<Image>();
            Image readyImage = null;

            foreach (Image img in images)
            {
                if (img.gameObject != panel && img != nameText.GetComponent<Image>())
                {
                    readyImage = img;
                    break;
                }
            }

            // Alternatif olarak, haz�r UI d�zenine g�re ilk child'�n image'sini de kullanabilirsin
            if (readyImage == null && panel.transform.childCount > 0)
            {
                readyImage = panel.transform.GetChild(0).GetComponent<Image>();
            }

            if (readyImage != null)
                readyImage.color = player.isReady ? Color.green : Color.red;

            StartGameButton.GetComponent<Button>().onClick.AddListener(OnStartGameClicked);  // ko�ul aktif edildi�inde silinecek sat�r
            /*if (Players.Length > 1) 
            {
                bool allready = true;
                foreach (var item in Players)
                {
                    if (!item.isReady)
                        allready = false;
                }

                if (allready) 
                {
                    StartGameButton.SetActive(true);
                    StartGameButton.GetComponent<Button>().onClick.AddListener(OnStartGameClicked);
                }
                else
                    StartGameButton.SetActive(false);
            }
            */

        }
    }


    void OnStartGameClicked()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("Sadece host oyunu ba�latabilir!");
            return;
        }

        Debug.Log("Oyun ba�lat�l�yor...");
        // Sahne de�i�ikli�i (host taraf�ndan)
        NetworkManager.singleton.ServerChangeScene("Demo with terrain");
    }

}
