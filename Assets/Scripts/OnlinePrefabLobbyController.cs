using UnityEngine;
using Mirror;
using Steamworks;

public class OnlinePrefabLobbyController : NetworkBehaviour
{
    public GameObject[] characterList;

    [SyncVar(hook = nameof(OnCharacterChanged))]
    public NetworkIdentity currentCharacterIdentity;

    public GameObject currentCharacter;

    [SyncVar] public bool isReady;
    public bool _islocal;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    public string localName;
    [SyncVar] public int index;
    void Update()
    {
        if (isLocalPlayer)
        {
            _islocal = true;

            

            /*if (Input.GetKeyDown(KeyCode.T))
                CmdSpawnSelectedCharacter();
            */
            if (Input.GetKeyDown(KeyCode.R))
                CmdSetReady(true);
        }
        else
        {
            _islocal = false;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (SteamManager.Initialized)
        {
            string steamName = SteamFriends.GetPersonaName();
            localName = steamName;
            CmdSetPlayerName(steamName);
            index = PlayerPrefs.GetInt("selectedCharacter");
        }
    }

    [Command]
    void CmdSetPlayerName(string name)
    {
        playerName = name;
        index = PlayerPrefs.GetInt("selectedCharacter");
    }

    void OnNameChanged(string oldName, string newName)
    {
        Debug.Log($"[Sync] Oyuncu adý güncellendi: {newName}");
    }

    [Command]
    void CmdSetReady(bool readyState)
    {
        isReady = readyState;
        Debug.Log($"[Server] Oyuncu {netId} ready durumu: {readyState}");
    }

    [Command(requiresAuthority = false)]
    public void CmdSpawnSelectedCharacter(int index)
    {
        // Eðer bu connection için zaten bir karakter varsa spawnlama
        if (currentCharacterIdentity != null && currentCharacterIdentity.connectionToClient == connectionToClient)
        {
            Debug.LogWarning("Character already spawned for this connection!");
            return;
        }
        

        Debug.Log("Spawning character for connection: " + connectionToClient.connectionId);

        
        GameObject character = Instantiate(characterList[index], transform.position, transform.rotation);

        NetworkServer.Spawn(character, connectionToClient);
        currentCharacterIdentity = character.GetComponent<NetworkIdentity>();

        Debug.Log("Character spawned successfully for: " + connectionToClient.identity.netId);
        RpcParentCharacter(currentCharacterIdentity);
    }

    [ClientRpc]
    void RpcParentCharacter(NetworkIdentity characterNetId)
    {
        if (characterNetId == null || !isActiveAndEnabled) return;

        GameObject character = characterNetId.gameObject;

        character.transform.SetParent(transform);
        character.transform.localPosition = Vector3.zero;
        character.transform.localRotation = Quaternion.identity;

        if (isLocalPlayer)
        {
            currentCharacter = character;
        }
    }

    void OnCharacterChanged(NetworkIdentity oldCharacter, NetworkIdentity newCharacter)
    {
        if (!isActiveAndEnabled) return;

        if (newCharacter != null && isClient)
        {
            // Sadece istemci tarafýnda parentlama yap
            GameObject character = newCharacter.gameObject;
            character.transform.SetParent(transform);
            character.transform.localPosition = Vector3.zero;
            character.transform.localRotation = Quaternion.identity;

            if (isLocalPlayer)
            {
                currentCharacter = character;
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void CharacterSpawnHelper() 
    {
        Debug.Log("spawn test 1-1");
       // CmdSpawnSelectedCharacter();
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }
    }
}