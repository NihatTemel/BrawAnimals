using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;
public class EndGameLobbyController : MonoBehaviour
{
    public GameObject PlayerListRoot;
    public GameObject PlayerHighScoreRoot;
    public OnlinePrefabLobbyController[] Players;
    public OnlinePlayerGameSceneVariables[] PlayerVariables;

    public GameObject StartGameButton;

    public int TotalScore;

    void Start()
    {
        InvokeRepeating(nameof(RefreshPlayers), 0f, 0.5f); // Her 2 saniyede bir kontrol
        Invoke("LoadTotalScore", 0.5f);
    }


    void LoadTotalScore() 
    {
        Players = FindObjectsByType<OnlinePrefabLobbyController>(FindObjectsSortMode.None);
        PlayerVariables = FindObjectsByType<OnlinePlayerGameSceneVariables>(FindObjectsSortMode.None);

        Debug.Log("load score test 1");

        // Oyuncu kontrolü
        if (Players == null || Players.Length == 0)
        {            
            Invoke("LoadTotalScore", 0.5f);
            Debug.Log("load score test 2");

            return;
        }
        Debug.Log("load score test 3");

        int n = Players.Length;
        for (int i = 0; i < n; i++)
        {
            Debug.Log("load score test 4");


            string ListName = PlayerListRoot.transform.GetChild(i).GetComponent<TMP_Text>().text;

            for (int s = 0; s < n; s++)
            {
                Debug.Log("load score test 5" + Players[s].isLocalPlayer );

                if (Players[s].isLocalPlayer  && Players[s].playerName==ListName) 
                {
                    Debug.Log("load score test 6" );

                    Debug.Log("Score rank -> " + s);
                }
            }
            
        }

    }


    void RefreshPlayers()
    {
        // Oyuncu ve deðiþkenleri bul
        Players = FindObjectsByType<OnlinePrefabLobbyController>(FindObjectsSortMode.None);
        PlayerVariables = FindObjectsByType<OnlinePlayerGameSceneVariables>(FindObjectsSortMode.None);

        // Oyuncu kontrolü
        if (Players == null || Players.Length == 0)
        {
            Debug.Log("Hiç oyuncu bulunamadý.");
            StartGameButton.SetActive(false);
            return;
        }

        // PlayerVariables ile Players'ý eþleþtir ve skora göre sýrala
        var sortedPlayers = Players
            .Select(p => new {
                Controller = p,
                Variables = PlayerVariables.FirstOrDefault(v => v.netId == p.netId)
            })
            .OrderByDescending(x => x.Variables?.LastSceneScore ?? 0)
            .ToList();

        // Tüm panelleri kapat
        foreach (Transform child in PlayerListRoot.transform)
        {
            child.gameObject.SetActive(false);
        }

        // Panelleri doldur
        for (int i = 0; i < Mathf.Min(sortedPlayers.Count, PlayerListRoot.transform.childCount); i++)
        {
            var panel = PlayerListRoot.transform.GetChild(i).gameObject;
            panel.SetActive(true);

            var player = sortedPlayers[i].Controller;
            var playerVar = sortedPlayers[i].Variables;

            // Ýsim ve skor ayarla
            TMP_Text nameText = panel.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = player.playerName;
                nameText.color = i == 0 ? Color.yellow : Color.white;
            }

            // Skor yazýsý (2. child'daki TMP_Text)
            if (panel.transform.childCount > 1)
            {
                Transform scoreTransform = panel.transform.GetChild(1);
                if (scoreTransform != null)
                {
                    TMP_Text scoreText = scoreTransform.GetComponent<TMP_Text>();
                    if (scoreText != null)
                    {
                        scoreText.text = playerVar?.LastSceneScore.ToString() ?? "0";
                        scoreText.color = i == 0 ? Color.yellow : Color.white;
                    }
                }
            }

            // Hazýrlýk durumu
            Image readyImage = panel.transform.Find("ReadyImage")?.GetComponent<Image>();
            if (readyImage != null)
            {
                readyImage.color = player.isReady ? Color.green : Color.red;
            }
        }

        // Tüm oyuncular hazýr mý kontrolü
        bool allReady = sortedPlayers.All(x => x.Controller.isReady);
        StartGameButton.SetActive(allReady);

        if (allReady)
        {
            StartGameButton.GetComponent<Button>().onClick.RemoveAllListeners();
            StartGameButton.GetComponent<Button>().onClick.AddListener(OnStartNextGameClicked);
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

        foreach (var item in PlayerVariables)
        {
            Debug.Log("-->> " + item.gameObject.name);
            item.ResetLastSceneScore();
        }


        // Build Settings'teki sahnelerin index aralýðýný kontrol et
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            foreach (var item in PlayerVariables)
            {
                Debug.Log("-->> " + item.gameObject.name);
                item.ResetLastSceneScore();
            }


            NetworkManager.singleton.ServerChangeScene(sceneName);
        }
        else
        {
            Debug.LogError($"Geçersiz sahne indexi: {sceneIndex}");
        }
    }

}
