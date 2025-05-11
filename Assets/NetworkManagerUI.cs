using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Net.NetworkInformation;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    
    private void Awake() {
        serverBtn.onClick.AddListener(() => {
            string ip = GetLocalIPAddress();
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() => {
            string ip = GetLocalIPAddress();
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() => {
            string ip = GetLocalIPAddress();
            NetworkManager.Singleton.StartClient();
        });
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (MainMenuHandler.IsHost && !NetworkManager.Singleton.IsHost)
        {
            string ip = GetLocalIPAddress();
            NetworkManager.Singleton.StartHost();
        }
        else if (MainMenuHandler.IsClient && !NetworkManager.Singleton.IsClient)
        {
            string ip = GetLocalIPAddress();
            NetworkManager.Singleton.StartClient();
        }

        // Reset the flags
        MainMenuHandler.IsHost = false;
        MainMenuHandler.IsClient = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private string GetLocalIPAddress()
    {
        string localIP = "";
        foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus == OperationalStatus.Up)
            {
                IPInterfaceProperties properties = networkInterface.GetIPProperties();
                foreach (UnicastIPAddressInformation address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = address.Address.ToString();
                    }
                }
            }
        }
        return localIP;
    }
}
