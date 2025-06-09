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

    public GameObject SceneSelectButtonsRoot;

    void Start()
    {
        CheckAndClearSceneList();
        InvokeRepeating(nameof(RefreshPlayers), 0f, 0.5f); // Her 2 saniyede bir kontrol
    }


    void CheckAndClearSceneList()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Lobby Menu")
        {
            PlayerPrefs.DeleteKey("SceneList");
            PlayerPrefs.Save();
            Debug.Log("Lobby Menu sahnesindeyiz. SceneList temizlendi.");
        }
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


            if(StartGameButton.activeInHierarchy)
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


    public void AddListToScneList2()
    {
        //Debug.Log("-> scene name " + test.name);
    }


    public void AddListToSceneList(int sceneIndex)
    {
        string currentList = PlayerPrefs.GetString("SceneList", "");
        string indexStr = sceneIndex.ToString();

        if (currentList.Contains(indexStr))
        {
            SceneSelectButtonsRoot.transform.GetChild(sceneIndex - 3).GetChild(1).gameObject.SetActive(false);
            // ��kar: T�m e�le�meleri temizle
            currentList = currentList.Replace(indexStr, "");
            Debug.Log("��kar�ld�: " + indexStr);
        }
        else
        {
            SceneSelectButtonsRoot.transform.GetChild(sceneIndex - 3).GetChild(1).gameObject.SetActive(true);


            currentList += indexStr;
            Debug.Log("Eklendi: " + indexStr);
        }

        PlayerPrefs.SetString("SceneList", currentList);
        PlayerPrefs.Save();

        Debug.Log("G�ncel liste: " + currentList);
    }


    public int FindNextScene()
    {
        string currentList = PlayerPrefs.GetString("SceneList", "");

        if (string.IsNullOrEmpty(currentList))
        {
            Debug.LogWarning("SceneList bo�!");
            return -1; // Liste bo�sa -1 d�ner
        }

        // �lk karakteri al
        char nextChar = currentList[0];

        // Integer'a �evir
        if (int.TryParse(nextChar.ToString(), out int nextScene))
        {
            // Kalan listeyi al ve kaydet
            string updatedList = currentList.Substring(1);
            PlayerPrefs.SetString("SceneList", updatedList);
            PlayerPrefs.Save();

            Debug.Log("Sonraki sahne: " + nextScene + " | G�ncel Liste: " + updatedList);
            return nextScene;
        }
        else
        {
            Debug.LogError("SceneList'teki karakter say� de�il: " + nextChar);
            return -1;
        }
    }



    public void OnStartGameClicked()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("Sadece host i�lem yapabilir!");
            return;
        }

        int sceneIndex = PlayerPrefs.GetInt("SceneIndex");

        sceneIndex = FindNextScene();

        // Build Settings'teki sahnelerin index aral���n� kontrol et
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            NetworkManager.singleton.ServerChangeScene(sceneName);
        }
        else
        {
            Debug.LogError($"Ge�ersiz sahne indexi: {sceneIndex}");
        }
    }

    public void OnStartNextGameClicked() 
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("Sadece host i�lem yapabilir!");
            return;
        }

        int sceneIndex = PlayerPrefs.GetInt("SceneIndex");

        sceneIndex = FindNextScene();

        // Build Settings'teki sahnelerin index aral���n� kontrol et
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            NetworkManager.singleton.ServerChangeScene(sceneName);
        }
        else
        {
            Debug.LogError($"Ge�ersiz sahne indexi: {sceneIndex}");
        }
    }

}
