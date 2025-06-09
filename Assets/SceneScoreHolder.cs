using UnityEngine;
using Mirror;

public class SceneScoreHolder : NetworkBehaviour
{
    // Sunucuda sahne y�klendi�inde �al���r
    public override void OnStartServer()
    {
        base.OnStartServer();
        CmdResetScore(); // �stemcilere skorlar� s�f�rlamalar�n� s�yle
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
        PlayerPrefs.Save(); // Skorlar� kal�c� yap
    }
}
