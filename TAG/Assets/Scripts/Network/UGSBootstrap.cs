using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class UGSBootstrap : MonoBehaviour
{
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed In! Player ID:" + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }
}
