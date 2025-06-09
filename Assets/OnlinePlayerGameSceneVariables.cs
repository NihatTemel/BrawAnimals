using UnityEngine;
using Mirror;

public class OnlinePlayerGameSceneVariables : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnLastSceneScoreChanged))]
    public int LastSceneScore;

    [SyncVar(hook = nameof(OnLastSceneLeadershipChanged))]
    public int LastSceneLeadership;

    [SyncVar(hook = nameof(OnLastSceneLeadershipChanged))] 
    public int GameScore;

    [SyncVar] public bool ScoreUpdate = false;
    private void Start()
    {
        if (isLocalPlayer)
        {
            // Yaln�zca lokal oyuncu i�in PlayerPrefs'ten y�kle
            LoadPlayerPrefs();
            Debug.Log("LAST SCENE SCORE " + PlayerPrefs.GetInt("LastSceneScore"));
            Invoke("CallGameScore", 1);
        }
    }

    void CallGameScore() 
    {
        int n = GameScore;
        CmdGameScore(n);
    }

    [Command(requiresAuthority=false)]
    void CmdGameScore(int n) 
    {
        RpcGameScore(n);
    }

    [ClientRpc]
    void RpcGameScore(int n) 
    {
        GameScore = n;
        ScoreUpdate = true;
    }

    
    public void ResetLastSceneScore() 
    {
        if (isLocalPlayer)
        {
            Debug.Log("Local Scene Score Resetted");
            LastSceneScore = 0;
            PlayerPrefs.SetInt("LastSceneScore", 0);

        }
    }

    private void OnLastGameScoreChanged(int oldValue, int newValue)
    {

        GameScore = newValue;
        if (isLocalPlayer)
        {
            Debug.Log("changed score " + newValue);
            PlayerPrefs.SetInt("GameScore", newValue);

        }
    }


    private void OnLastSceneScoreChanged(int oldValue, int newValue)
    {
        
        LastSceneScore = newValue;
        if (isLocalPlayer)
        {
            Debug.Log("changed score " + newValue);
            PlayerPrefs.SetInt("LastSceneScore", newValue);
            
        }
    }

    // Liderlik puan� de�i�ti�inde otomatik olarak �a�r�l�r
    private void OnLastSceneLeadershipChanged(int oldValue, int newValue)
    {
        LastSceneLeadership = newValue;
        if (isLocalPlayer)
        {
            PlayerPrefs.SetInt("LastSceneLeadership", newValue);
            
        }
    }

    // PlayerPrefs'ten de�erleri y�kle
    private void LoadPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("LastSceneScore"))
        {
            LastSceneScore = PlayerPrefs.GetInt("LastSceneScore");
            CmdUpdateScore(LastSceneScore);
        }

        if (PlayerPrefs.HasKey("LastSceneLeadership"))
        {
            LastSceneLeadership = PlayerPrefs.GetInt("LastSceneLeadership");
            CmdUpdateLeadership(LastSceneLeadership);
        }
    }

    // Skoru sunucuda g�ncellemek i�in komut
    [Command]
    private void CmdUpdateScore(int newScore)
    {
        LastSceneScore = newScore;
    }

    // Liderlik puan�n� sunucuda g�ncellemek i�in komut
    [Command]
    private void CmdUpdateLeadership(int newLeadership)
    {
        LastSceneLeadership = newLeadership;
        
    }

    // Oyun sonunda �a�r�lacak metod
    public void SaveFinalScores(int finalScore, int finalLeadership)
    {
        if (isLocalPlayer)
        {
            LastSceneScore = finalScore;
            LastSceneLeadership = finalLeadership;

            PlayerPrefs.SetInt("LastSceneScore", finalScore);
            PlayerPrefs.SetInt("LastSceneLeadership", finalLeadership);
            PlayerPrefs.Save();

            CmdUpdateScore(finalScore);
            CmdUpdateLeadership(finalLeadership);
        }
    }
}