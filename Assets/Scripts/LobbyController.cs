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


           /* if(SceneManager.GetActiveScene().name== "Lobby Menu")

            if(StartGameButton.activeInHierarchy)
                StartGameButton.GetComponent<Button>().onClick.AddListener(OnStartGameClicked);  // koþul aktif edildiðinde silinecek satýr*/

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
            // Çýkar: Tüm eþleþmeleri temizle
            currentList = currentList.Replace(indexStr, "");
            Debug.Log("Çýkarýldý: " + indexStr);
        }
        else
        {
            SceneSelectButtonsRoot.transform.GetChild(sceneIndex - 3).GetChild(1).gameObject.SetActive(true);


            currentList += indexStr;
            Debug.Log("Eklendi: " + indexStr);
        }

        PlayerPrefs.SetString("SceneList", currentList);
        PlayerPrefs.Save();

        Debug.Log("Güncel liste: " + currentList);
    }


    public int FindNextScene()
    {
        string currentList = PlayerPrefs.GetString("SceneList", "");

        if (string.IsNullOrEmpty(currentList))
        {
            Debug.LogWarning("SceneList boþ!");
            return -1; // Liste boþsa -1 döner
        }

        // Ýlk karakteri al
        char nextChar = currentList[0];

        // Integer'a çevir
        if (int.TryParse(nextChar.ToString(), out int nextScene))
        {
            // Kalan listeyi al ve kaydet
            string updatedList = currentList.Substring(1);
            PlayerPrefs.SetString("SceneList", updatedList);
            PlayerPrefs.Save();

            Debug.Log("Sonraki sahne: " + nextScene + " | Güncel Liste: " + updatedList);
            return nextScene;
        }
        else
        {
            Debug.LogError("SceneList'teki karakter sayý deðil: " + nextChar);
            return -1;
        }
    }



    public void OnStartGameClicked()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("Sadece host iþlem yapabilir!");
            return;
        }

        int sceneIndex = PlayerPrefs.GetInt("SceneIndex");

        sceneIndex = FindNextScene();


       /* string currentList = PlayerPrefs.GetString("SceneList", "");
        string indexStr = sceneIndex.ToString();

        if (currentList.Contains(indexStr))
        {
            SceneSelectButtonsRoot.transform.GetChild(sceneIndex - 3).GetChild(1).gameObject.SetActive(false);
            // Çýkar: Tüm eþleþmeleri temizle
            currentList = currentList.Replace(indexStr, "");
            Debug.Log("Çýkarýldý: " + indexStr);
        }
       */
        

        Debug.Log("go scene !" +sceneIndex);

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

    public void OnStartNextGameClicked() 
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("Sadece host iþlem yapabilir!");
            return;
        }

        int sceneIndex = PlayerPrefs.GetInt("SceneIndex");

        sceneIndex = FindNextScene();

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
