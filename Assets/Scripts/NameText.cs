using UnityEngine;
using TMPro;
using Mirror;

public class NameText : MonoBehaviour
{
    [SerializeField] TMP_Text nametext;
    //[SyncVar] public string playername;
    void Start()
    {
       // playername = transform.root.GetComponent<OnlinePrefabLobbyController>().playerName;
    }

    
   /* [Command(requiresAuthority = false)]
    void CmdSetPlayerName() 
    {
        RpcSetPlayerName();
    }

    [ClientRpc]
    void RpcSetPlayerName() 
    {
        nametext.text = playername;
    }


    void Update()
    {
        CmdSetPlayerName();
    }*/
}
