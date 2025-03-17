using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        VisualElement rootVisualElement;
        Button hostButton;
        Button clientButton;
        Button serverButton;
        Label statusLabel;
        ScrollView scoreboardScroll;
        Label itStatusLabel;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            rootVisualElement = uiDocument.rootVisualElement;

            hostButton = CreateButton("HostButton", "Host");
            clientButton = CreateButton("ClientButton", "Client");
            serverButton = CreateButton("ServerButton", "Server");
            statusLabel = CreateLabel("StatusLabel", "Not Connected");

            rootVisualElement.Clear();
            rootVisualElement.Add(hostButton);
            rootVisualElement.Add(clientButton);
            rootVisualElement.Add(serverButton);
            rootVisualElement.Add(statusLabel);

            hostButton.clicked += OnHostButtonClicked;
            clientButton.clicked += OnClientButtonClicked;
            serverButton.clicked += OnServerButtonClicked;

            // Create a scoreboard area
            scoreboardScroll = new ScrollView();
            scoreboardScroll.name = "ScoreboardScroll";
            scoreboardScroll.style.width = 240;
            scoreboardScroll.style.height = 200;
            rootVisualElement.Add(scoreboardScroll);

            // Create a label to show "You are It" or not
            itStatusLabel = new Label("It Status");
            itStatusLabel.style.width = 240;
            rootVisualElement.Add(itStatusLabel);
        }

        void Update()
        {
            UpdateUI();
            UpdateScoreboard();
            UpdateItStatus();
        }

        void OnDisable()
        {
            hostButton.clicked -= OnHostButtonClicked;
            clientButton.clicked -= OnClientButtonClicked;
            serverButton.clicked -= OnServerButtonClicked;
        }

        void OnHostButtonClicked() => NetworkManager.Singleton.StartHost();

        void OnClientButtonClicked() => NetworkManager.Singleton.StartClient();

        void OnServerButtonClicked() => NetworkManager.Singleton.StartServer();

        // Disclaimer: This is not the recommended way to create and stylize the UI elements, it is only utilized for the sake of simplicity.
        // The recommended way is to use UXML and USS. Please see this link for more information: https://docs.unity3d.com/Manual/UIE-USS.html
        private Button CreateButton(string name, string text)
        {
            var button = new Button();
            button.name = name;
            button.text = text;
            button.style.width = 240;
            button.style.backgroundColor = Color.white;
            button.style.color = Color.black;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            return button;
        }

        private Label CreateLabel(string name, string content)
        {
            var label = new Label();
            label.name = name;
            label.text = content;
            label.style.color = Color.black;
            label.style.fontSize = 18;
            return label;
        }

        void UpdateUI()
        {
            if (NetworkManager.Singleton == null)
            {
                SetStartButtons(false);
                SetStatusText("NetworkManager not found");
                return;
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                SetStartButtons(true);
                SetStatusText("Not connected");
            }
            else
            {
                SetStartButtons(false);
                UpdateStatusLabels();
            }
        }

        void UpdateScoreboard()
        {
            scoreboardScroll.Clear();

            // If not connected, skip
            if (!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var scoreMgr = ScoreManager.Instance;
            if (scoreMgr == null) return;

            // For each connected client, show local known score
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                int score = scoreMgr.GetLocalScore(clientId);
                Label entry = new Label($"Player {clientId} Score: {score}");
                scoreboardScroll.Add(entry);
            }
        }

        void UpdateItStatus()
        {
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                itStatusLabel.text = "No connection";
                return;
            }

            var tagManager = FindFirstObjectByType<TagManager>();
            if (tagManager == null)
            {
                itStatusLabel.text = "TagManager not found";
                return;
            }

            // local player's clientId
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            if (localClientId == tagManager.currentItPlayerId.Value)
            {
                itStatusLabel.text = "You are IT!";
            }
            else
            {
                itStatusLabel.text = "You are not IT.";
            }
        }

        void SetStartButtons(bool state)
        {
            hostButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            clientButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            serverButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void SetStatusText(string text) => statusLabel.text = text;

        void UpdateStatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
            string transport = "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
            string modeText = "Mode: " + mode;
            SetStatusText($"{transport}\n{modeText}");
        }

        void SubmitNewPosition()
        {
        }
    }
}