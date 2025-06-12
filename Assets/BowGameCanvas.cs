using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;
public class BowGameCanvas : NetworkBehaviour
{

    public int GameTimer = 15;
    [SyncVar(hook = nameof(OnTimerChanged))] public int Timer = 0;
    [SyncVar(hook = nameof(OnCountdownChanged))] public int Countdown = 3;
    public TMP_Text timerText;
    public TMP_Text countdownText;

    private Tween scaleTween;
    private Coroutine countdownCoroutine;
    private bool gameStarted = false;

    public GameObject PlayerListRoot;
    public OnlinePrefabLobbyController[] Players;
    public AppleGameControl[] Players2;
    public Image KurekFillImg;
    public float KurekFillTimer;
    public TMP_Text KurekFillText;



    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateTimerText(Timer);
        countdownText.gameObject.SetActive(false);
    }

    void Start()
    {
        InvokeRepeating(nameof(RefreshPlayers), 0f, 0.15f);
    }





    void RefreshPlayers()
    {
        Players = FindObjectsByType<OnlinePrefabLobbyController>(FindObjectsSortMode.None);
        Players2 = FindObjectsByType<AppleGameControl>(FindObjectsSortMode.None);

        if (Players == null || Players.Length == 0)
        {
            Debug.Log("Hi� oyuncu bulunamad�.");
            return;
        }

        // E�er t�m oyuncular spawnland�ysa ve oyun ba�lamad�ysa
        if (isServer && !gameStarted && Players.Length == Players2.Length && Players.Length > 0)
        {
            gameStarted = true;
            StartCountdown();
        }

        // Oyuncular� skorlar�na g�re azalan s�rada s�rala
        Players = Players.OrderByDescending(p =>
        {
            if (p.currentCharacterIdentity != null &&
                p.currentCharacterIdentity.gameObject.GetComponent<AppleGameControl>() != null)
            {
                return p.currentCharacterIdentity.gameObject.GetComponent<AppleGameControl>().applecount;
            }
            return 0; // Skor bilgisi yoksa 0 kabul et
        }).ToArray();

        // T�m Player panel objelerini kapat
        for (int i = 0; i < PlayerListRoot.transform.childCount; i++)
        {
            PlayerListRoot.transform.GetChild(i).gameObject.SetActive(false);
        }

        // Maksimum 4 oyuncu g�sterilecek
        for (int i = 0; i < Players.Length && i < PlayerListRoot.transform.childCount; i++)
        {
            GameObject panel = PlayerListRoot.transform.GetChild(i).gameObject;
            panel.SetActive(true);

            OnlinePrefabLobbyController player = Players[i];

            TMP_Text nameText = panel.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = player.playerName;

            TMP_Text ScoreText = panel.transform.GetChild(0).GetComponentInChildren<TMP_Text>();
            if (player.currentCharacterIdentity != null &&
                player.currentCharacterIdentity.gameObject.GetComponent<AppleGameControl>() != null)
            {
                int score = player.currentCharacterIdentity.gameObject.GetComponent<AppleGameControl>().applecount;
                ScoreText.text = score.ToString();

                // �ste�e ba�l�: Skor rengini vurgulamak i�in
                if (i == 0) // En y�ksek skor
                {
                    ScoreText.color = Color.yellow; // Veya ba�ka bir vurgu rengi
                    nameText.color = Color.yellow;
                }
                else
                {
                    ScoreText.color = Color.white;
                    nameText.color = Color.white;
                }
            }
        }
    }

    [Server]
    void StartCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        // Ba�lang�� geri say�m� (3, 2, 1, GO!)
        Countdown = 4;
        while (Countdown > 0)
        {
            yield return new WaitForSeconds(1f);
            Countdown--;
        }

        // GO! mesaj�n� g�ster
        RpcShowCountdown("GO!");
        yield return new WaitForSeconds(1f);
        RpcHideCountdown();

        // As�l oyun geri say�m�
        Timer = GameTimer; // �rnek olarak 60 saniye verdim, istedi�iniz de�eri kullanabilirsiniz
        while (Timer > 0)
        {
            yield return new WaitForSeconds(1f);
            Timer--;
        }

        // Oyun bitti
        EndGame();
    }

    [ClientRpc]
    void RpcShowCountdown(string message)
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = message;
        countdownText.color = Color.green;
        countdownText.transform.localScale = Vector3.zero;
        countdownText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    [ClientRpc]
    void RpcHideCountdown()
    {
        timerText.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(false);
        //SceneAppleSpawner.GetComponent<AppleGameSceneSpawner>().enabled = true;
    }




    void OnCountdownChanged(int oldValue, int newValue)
    {
        if (newValue > 0)
        {
            RpcShowCountdown(newValue.ToString());
        }
    }

    void OnTimerChanged(int oldValue, int newValue)
    {
        UpdateTimerText(newValue);
    }

    void UpdateTimerText(int value)
    {
        if (timerText == null) return;

        timerText.text = value.ToString();

        if (value <= 3)
        {
            timerText.DOColor(Color.red, 0.2f);
            scaleTween?.Kill();
            scaleTween = timerText.transform
                .DOScale(Vector3.one * 1.5f, 0.5f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);
        }
        else
        {
            timerText.color = Color.white;
            timerText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    [Server]
    void EndGame()
    {
        Debug.Log("Oyun bitti!");



        NetworkManager.singleton.ServerChangeScene("EndGameScene");

    }
}