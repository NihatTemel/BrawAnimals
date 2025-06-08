using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Mirror;

public class OnlinePlayerCharacterSpawner : NetworkBehaviour
{
    public string[] GameSceneNames;
    private bool hasSpawned = false; // Spawn kontrol� i�in flag

    private void Awake()
    {
        Spawndelay();
    }

    void Spawndelay()
    {
        if (hasSpawned) return; // Zaten spawnland�ysa tekrar �al��mas�n

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

            // Sadece local player i�in spawn i�lemi
            if (isLocalPlayer)
            {
                int index = PlayerPrefs.GetInt("selectedCharacter");
                GetComponent<OnlinePrefabLobbyController>().CmdSpawnSelectedCharacter(index);
                hasSpawned = true; // Spawnland� olarak i�aretle
            }
        }
    }
}