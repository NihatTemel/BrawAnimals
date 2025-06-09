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

            // Ready Image bul (panel altýndaki Image, nameText dýþýndaki ilk Image)
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

            // Alternatif olarak, hazýr UI düzenine göre ilk child'ýn image'sini de kullanabilirsin
            if (readyImage == null && panel.transform.childCount > 0)
            {
                readyImage = panel.transform.GetChild(0).GetComponent<Image>();
            }

            if (readyImage != null)
                readyImage.color = player.isReady ? Color.green : Color.red;


            if(StartGameButton.activeInHierarchy)
                StartGameButton.GetComponent<Button>().onClick.AddListener(OnStartGameClicked);  // koþul aktif edildiðinde silinecek satýr
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


    public void OnStartGameClicked()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("Sadece host oyunu baþlatabilir!");
            return;
        }

        Debug.Log("Oyun baþlatýlýyor...");
        // Sahne deðiþikliði (host tarafýndan)
       
        NetworkManager.singleton.ServerChangeScene("Demo with terrain");
    }

    public void OnStartNextGameClicked() 
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("Sadece host iþlem yapabilir!");
            return;
        }

        int sceneIndex = PlayerPrefs.GetInt("SceneIndex");

        sceneIndex = 1;

        // Build Settings'teki sahnelerin index aralýðýný kontrol et
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            NetworkManager.singleton.ServerChangeScene(sceneName);
        }
        else
        {
            Debug.LogError($"Geçersiz sahne indexi: {sceneIndex}");
        }
    }

}
