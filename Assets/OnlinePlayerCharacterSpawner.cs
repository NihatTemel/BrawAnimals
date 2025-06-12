using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Mirror;

public class OnlinePlayerCharacterSpawner : NetworkBehaviour
{
    public string[] GameSceneNames;
    private bool hasSpawned = false; // Spawn kontrolü için flag

    private void Awake()
    {
        Spawndelay();
    }

    void Spawndelay()
    {
        if (hasSpawned) return; // Zaten spawnlandýysa tekrar çalýþmasýn

        if (GameSceneNames == null || GameSceneNames.Length == 0)
        {
            Debug.LogError("No game scenes assigned!");
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (GameSceneNames.Contains(currentSceneName))
        {
            if (!isClient)
            {
                Invoke("Spawndelay", 0.1f);
                return;
            }

            // Sadece local player için spawn iþlemi
            if (isLocalPlayer)
            {
                PlayerPrefs.SetInt("LastSceneScore", 0);
                int index = PlayerPrefs.GetInt("selectedCharacter");

                int n = SceneManager.GetActiveScene().buildIndex;

                index = index + ((n - 3) * 2);

                GetComponent<OnlinePrefabLobbyController>().CmdSpawnSelectedCharacter(index);
                hasSpawned = true; // Spawnlandý olarak iþaretle
            }
        }
    }
}