using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkTestUI : MonoBehaviour
{
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TextMeshProUGUI joinCodeDisplay;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void OnJoinButtonPressed()
    {
        string code = joinCodeInputField.text;
        Debug.Log("Join button pressed, code entered: " + joinCodeInputField.text);
        OnClientButtonPressed(code);
    }


    public async void OnHostButtonPressed()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections: 3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();

            joinCodeDisplay.text = joinCode;
            Debug.Log("Join code: " + joinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void OnClientButtonPressed(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            NetworkManager.Singleton.StartClient();

            Debug.Log("Client Joined");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}

    

