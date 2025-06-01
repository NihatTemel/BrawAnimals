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

    void Update()
    {
        if (isLocalPlayer)
        {
            _islocal = true;

            int index = PlayerPrefs.GetInt("selectedCharacter");

            if (Input.GetKeyDown(KeyCode.T))
                CmdSpawnSelectedCharacter(index);

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
        }
    }

    [Command]
    void CmdSetPlayerName(string name)
    {
        playerName = name;
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

    [Command]
    void CmdSpawnSelectedCharacter(int index)
    {
        if (!isServer) return;

        //int index = PlayerPrefs.GetInt("selectedCharacter");
        GameObject character = Instantiate(characterList[index], transform.position, transform.rotation);

        NetworkServer.Spawn(character, connectionToClient);

        currentCharacterIdentity = character.GetComponent<NetworkIdentity>();
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

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }
    }
}