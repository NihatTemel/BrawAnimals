using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
public class EndGameLobbyController : MonoBehaviour
{
    public GameObject PlayerListRoot;
    public GameObject PlayerHighScoreRoot;
    public OnlinePrefabLobbyController[] Players;
    public OnlinePlayerGameSceneVariables[] PlayerVariables;

    public GameObject StartGameButton;

    public int TotalScore;
    public List<int> scores = new List<int>();

    public bool ReadyToListLeaders=false;
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

        if (Players == null || Players.Length == 0)
        {
            Invoke("LoadTotalScore", 0.5f);
            Debug.Log("load score test 2");
            return;
        }

        Debug.Log("load score test 3");

        int n = PlayerVariables.Length;
        

        for (int i = 0; i < n; i++)
        {
            scores.Add(PlayerVariables[i].LastSceneScore);
            Debug.Log("Player score " + PlayerVariables[i].LastSceneScore);
        }

        // Skorlarý azalan þekilde sýrala
        List<int> sortedScores = scores.OrderByDescending(s => s).ToList();

        for (int i = 0; i < n; i++)
        {
            if (PlayerVariables[i].GetComponent<OnlinePrefabLobbyController>().isLocalPlayer)
            {
                int myScore = PlayerVariables[i].LastSceneScore;
                int myRank = sortedScores.IndexOf(myScore); // ayný skor varsa ilk bulduðunu alýr

                Debug.Log("Benim sýram: " + (myRank + 1));

                int extraScore = Mathf.Clamp(10 - myRank, 3, 10);

                PlayerPrefs.SetInt("GameScore", PlayerPrefs.GetInt("GameScore") + extraScore);

                Debug.Log("Game Score -> " + PlayerPrefs.GetInt("GameScore"));

                Debug.Log("Ekstra puaným: " + extraScore);

                PlayerVariables[i].GameScore = PlayerPrefs.GetInt("GameScore");
                PlayerVariables[i].ScoreUpdate = true;
                break;
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

        int nn = 0;
        foreach (var item in PlayerVariables)
        {
            if (item.ScoreUpdate == false)
                nn = 1;
        }
        if (nn == 0)
            ReadyToListLeaders = true;

        LeadersListUpdate();

    }


    void LeadersListUpdate()
    {
        if (!ReadyToListLeaders)
            return;

        PlayerVariables = FindObjectsByType<OnlinePlayerGameSceneVariables>(FindObjectsSortMode.None);

        List<(string name, int score)> leaderboard = new List<(string name, int score)>();

        foreach (var item in PlayerVariables)
        {
            string playerName = item.GetComponent<OnlinePrefabLobbyController>().playerName;
            int score = item.GameScore;
            leaderboard.Add((playerName, score));
        }

        // Skora göre azalan sýrala
        leaderboard = leaderboard.OrderByDescending(p => p.score).ToList();


        Debug.Log("List childcount -> " + PlayerHighScoreRoot.transform.childCount);

        for (int i = 0; i < leaderboard.Count && i < PlayerHighScoreRoot.transform.childCount; i++)
        {
            PlayerHighScoreRoot.transform.GetChild(i).gameObject.SetActive(true);
            Transform playerEntry = PlayerHighScoreRoot.transform.GetChild(i);

            // 1. sýradaki için: "1. Murat"
            string nameText = $"{i + 1}. {leaderboard[i].name}";
            string scoreText = leaderboard[i].score.ToString();

            // Ýsim ve sýra
            playerEntry.GetComponent<TMP_Text>().text = nameText;

            // Skor
            playerEntry.GetChild(0).GetComponent<TMP_Text>().text = scoreText;
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

        sceneIndex = 3;

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
