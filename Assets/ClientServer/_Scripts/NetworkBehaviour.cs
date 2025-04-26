using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace VH.Multiplayer
{
    public class NetworkBehaviour : MonoBehaviour
    {
        public NetworkObject NetworkObject { get; private set; }
        public int OwnerClientId => NetworkObject.OwnerClientId;
        public int ObjectId => NetworkObject.ObjectId;
        
        protected virtual void Awake()
        {
            if (!TryGetComponent(out NetworkObject networkObject))
                Debug.LogError($"{name} требует компонент NetworkObject.", this);
            
            NetworkObject = networkObject;
        }

        protected virtual void Reset()
        {
            if (!GetComponent<NetworkObject>())
                gameObject.AddComponent<NetworkObject>();
        }

        public virtual void OnNetworkSpawn()
        {
        }
    }
}