using Unity.Services.Core;
using UnityEngine;

public class ClientBootstrap : MonoBehaviour
{
    [SerializeField] private bool _enableDebugging = true;
    
    private async void Awake()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (ServicesInitializationException e)
            {
                DebugTools.ShowDebug(e.Message, _enableDebugging, DebugType.Error);
            }
        }
    }
}