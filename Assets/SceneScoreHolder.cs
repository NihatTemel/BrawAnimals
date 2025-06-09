using UnityEngine;
using Mirror;

public class SceneScoreHolder : NetworkBehaviour
{
    // Sunucuda sahne yüklendiðinde çalýþýr
    public override void OnStartServer()
    {
        base.OnStartServer();
        CmdResetScore(); // Ýstemcilere skorlarý sýfýrlamalarýný söyle
    }

    
    [Command(requiresAuthority = false)]
    void CmdResetScore() 
    {
        RpcResetScores();
    }

    [ClientRpc]
    void RpcResetScores()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("OnlinePlayer");
        foreach (GameObject player in players)
        {
            PlayerPrefs.SetInt("LastSceneScore", 0);
        }
        PlayerPrefs.Save(); // Skorlarý kalýcý yap
    }
}
