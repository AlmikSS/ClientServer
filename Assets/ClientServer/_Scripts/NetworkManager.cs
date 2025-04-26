using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace VH.Multiplayer
{
    public class NetworkManager : MonoBehaviour
    {
        [SerializeField] private List<NetworkObject> _prefabs = new();

        private UdpClient _client;
        private IPEndPoint _server;
        private SpawnManager _spawnManager;
        private Dictionary<int, NetworkObject> _spawnedObjects = new();
        private int _clientId;

        public static NetworkManager Instance { get; private set; }
        public SpawnManager SpawnManager => _spawnManager;
        public UdpClient Client => _client;
        public IPEndPoint Server => _server;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        
        private void Start()
        {
            ConnectToServer(7777, "127.0.0.1");
        }
        
        private async void Update()
        {
            try
            {
                var result = await _client.ReceiveAsync();

                var packetType = (PacketType)result.Buffer[0];

                switch (packetType)
                {
                    case PacketType.ConnectResponse:
                        HandleConnectionResponse(result);
                        break;
                    case PacketType.SpawnObjectResponse:
                        HandleSpawnObjectResponse(result);
                        break;
                    case PacketType.UpdatePositionResponse:
                        HandelUpdatePositionResponse(result);
                        break;
                    default:
                        Debug.LogError($"Unhandled packet type: {packetType}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private void HandelUpdatePositionResponse(UdpReceiveResult result)
        {
            var objectId = (int)result.Buffer[1];
            var bytes = result.Buffer.Skip(2).ToArray();
            var json = Encoding.UTF8.GetString(bytes);
            var newPos = JsonConvert.DeserializeObject<Vector3>(json);
            var pos = new UnityEngine.Vector3(newPos.x, newPos.y, newPos.z);

            foreach (var (key, value) in _spawnedObjects)
            {
                if (key == objectId)
                {
                    value.transform.position = pos;
                    break;
                }
            }
        }

        private void OnClientConnected()
        {
            _spawnManager = new SpawnManager(_server, _client);
            
            _spawnManager.SpawnObject("TestPrefab", new UnityEngine.Vector3(), UnityEngine.Quaternion.identity);
        }
        
        private void HandleSpawnObjectResponse(UdpReceiveResult result)
        {
            var packet = result.Buffer.Skip(1).ToArray();
            var json = Encoding.UTF8.GetString(packet);
            var serverObject = JsonConvert.DeserializeObject<ServerObject>(json);

            foreach (var obj in _prefabs)
            {
                if (obj.gameObject.name == serverObject.PrefabName)
                {
                    var position = new UnityEngine.Vector3(serverObject.Position.x, serverObject.Position.y,
                        serverObject.Position.z);
                    var rotation = new UnityEngine.Quaternion(serverObject.Rotation.x, serverObject.Rotation.y,
                        serverObject.Rotation.z, serverObject.Rotation.w);

                    var newObj = Instantiate(obj, position, rotation);
                    newObj.Setup(serverObject.OwnerId, serverObject.Id);
                    newObj.OnNetworkSpawn();
                    _spawnedObjects.Add(serverObject.Id, newObj);
                    break;
                }
            }
        }

        private void HandleConnectionResponse(UdpReceiveResult result)
        {
            var id = BitConverter.ToInt32(result.Buffer, 1);
            _clientId = id;
            Debug.Log($"Client connected to server with id {_clientId}");
            OnClientConnected();
        }

        private async void ConnectToServer(int port, string ip)
        {
            _client = new UdpClient(0);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Client.Connect(IPAddress.Parse(ip), port);

            _server = new IPEndPoint(IPAddress.Parse(ip), port);

            var packet = new[] { (byte)PacketType.ConnectRequest, };
            await _client.SendAsync(packet, packet.Length, _server);
        }
    }

    public enum PacketType : byte
    {
        ConnectRequest = 0,
        ConnectResponse = 1,
        SpawnObjectRequest = 2,
        SpawnObjectResponse = 3,
        UpdatePositionRequest = 4,
        UpdatePositionResponse = 5,
        UpdateRotationRequest = 6,
        UpdateRotationResponse = 7,
    }
}