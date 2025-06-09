using UnityEngine;
using Mirror;

public class OnlinePlayerGameSceneVariables : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnLastSceneScoreChanged))]
    public int LastSceneScore;

    [SyncVar(hook = nameof(OnLastSceneLeadershipChanged))]
    public int LastSceneLeadership;

    private void Start()
    {
        if (isLocalPlayer)
        {
            // Yalnýzca lokal oyuncu için PlayerPrefs'ten yükle
            LoadPlayerPrefs();
            Debug.Log("LAST SCENE SCORE " + PlayerPrefs.GetInt("LastSceneScore"));
        }
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

    private void OnLastSceneScoreChanged(int oldValue, int newValue)
    {
        
        LastSceneScore = newValue;
        if (isLocalPlayer)
        {
            Debug.Log("changed score " + newValue);
            PlayerPrefs.SetInt("LastSceneScore", newValue);
            
        }
    }

    // Liderlik puaný deðiþtiðinde otomatik olarak çaðrýlýr
    private void OnLastSceneLeadershipChanged(int oldValue, int newValue)
    {
        LastSceneLeadership = newValue;
        if (isLocalPlayer)
        {
            PlayerPrefs.SetInt("LastSceneLeadership", newValue);
            
        }
    }

    // PlayerPrefs'ten deðerleri yükle
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

    // Skoru sunucuda güncellemek için komut
    [Command]
    private void CmdUpdateScore(int newScore)
    {
        LastSceneScore = newScore;
    }

    // Liderlik puanýný sunucuda güncellemek için komut
    [Command]
    private void CmdUpdateLeadership(int newLeadership)
    {
        LastSceneLeadership = newLeadership;
    }

    // Oyun sonunda çaðrýlacak metod
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