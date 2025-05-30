using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Steamworks;

public class SteamLobbyManager : NetworkManager
{
    [Header("UI Elements")]
    public Button hostButton;
    public Button joinButton;
    public Button hostLocalButton;
    public Button joinLocalButton;
    public TMP_Text statusText;
    public TMP_Text playerCountText;

    [Header("Transports")]
    public Transport steamTransport;
    public Transport localTransport;

    public static CSteamID currentLobbyID;
    private bool usingLocalHost = false;

    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    private Callback<LobbyEnter_t> lobbyEntered;

    public override void Awake()
    {
        System.Environment.SetEnvironmentVariable("SteamAppId", "480");
        System.Environment.SetEnvironmentVariable("SteamGameId", "480");

        base.Awake();

        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam başlatılamadı. Steam açık mı?");
            if (statusText != null)
                statusText.text = "Steam başlatılamadı!";
            return;
        }

        // Steam callback'leri
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        // Buton listener'ları
        if (hostButton != null)
            hostButton.onClick.AddListener(CreateLobby);

        if (joinButton != null)
            joinButton.onClick.AddListener(() => SteamMatchmaking.JoinLobby(currentLobbyID));

        if (hostLocalButton != null)
            hostLocalButton.onClick.AddListener(HostLocalGame);

        if (joinLocalButton != null)
            joinLocalButton.onClick.AddListener(JoinLocalGame);

        if (statusText != null)
            statusText.text = "Steam bağlantısı başarılı!";
    }

    // Steam ile lobi oluştur
    void CreateLobby()
    {
        usingLocalHost = false;
        Transport.active = steamTransport;

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);

        if (statusText != null)
            statusText.text = "Lobi oluşturuluyor...";
    }

    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            if (statusText != null)
                statusText.text = "Lobi oluşturulamadı.";
            return;
        }

        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(currentLobbyID, "HostAddress", SteamUser.GetSteamID().ToString());

        StartHost();

        if (statusText != null)
            statusText.text = "Host başlatıldı, arkadaşlar katılabilir.";
    }

    void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        currentLobbyID = callback.m_steamIDLobby;

        if (statusText != null)
            statusText.text = "Davet alındı, lobiye katılınıyor...";
    }

    void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (!usingLocalHost)
        {
            StartCoroutine(WaitAndJoin());
        }

        StartCoroutine(UpdatePlayerCountUI());
    }

    IEnumerator WaitAndJoin()
    {
        yield return new WaitForSeconds(1f);

        string hostAddress = SteamMatchmaking.GetLobbyData(currentLobbyID, "HostAddress");

        Transport.active = steamTransport;
        networkAddress = hostAddress;

        StartClient();

        if (statusText != null)
            statusText.text = "Sunucuya bağlanılıyor...";
    }

    // Local Test: Host
    void HostLocalGame()
    {
        usingLocalHost = true;
        Transport.active = localTransport;
        networkAddress = "localhost";

        StartHost();

        if (statusText != null)
            statusText.text = "🖥️ Local sunucu başlatıldı (Host).";
    }

    // Local Test: Client
    void JoinLocalGame()
    {
        usingLocalHost = true;
        Transport.active = localTransport;
        networkAddress = "localhost";

        StartClient();

        if (statusText != null)
            statusText.text = "💻 Local sunucuya bağlanılıyor (Client)...";
    }

    IEnumerator UpdatePlayerCountUI()
    {
        while (true)
        {
            if (playerCountText != null && SteamManager.Initialized && currentLobbyID.IsValid())
            {
                int count = SteamMatchmaking.GetNumLobbyMembers(currentLobbyID);
                playerCountText.text = $"Oyuncu Sayısı: {count}";
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        if (statusText != null)
            statusText.text = "✅ Bağlantı başarılı!";
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        if (statusText != null)
            statusText.text = "❌ Bağlantı kesildi.";
    }
}