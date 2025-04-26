using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace VH.Multiplayer
{
    public class SpawnManager
    {
        private readonly IPEndPoint _server;
        private readonly UdpClient _client;

        public SpawnManager(IPEndPoint server, UdpClient client)
        {
            _server = server;
            _client = client;
        }

        public void SpawnObject(string name, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            SpawnObjectAsync(name, 0, position, rotation);
        }

        public void SpawnObject(string name, uint ownerId, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            SpawnObjectAsync(name, (int)ownerId, position, rotation);
        }

        private async void SpawnObjectAsync(string name, int ownerId, UnityEngine.Vector3 pos, UnityEngine.Quaternion rot)
        {
            var position = new Vector3 { x = pos.x, y = pos.y, z = pos.z };
            var rotation = new Quaternion { x = rot.x, y = rot.y, z = rot.z, w = rot.w };
            
            var so = new ServerObject()
            {
                OwnerId = ownerId,
                PrefabName = name,
                Position = position,
                Rotation = rotation,
            };

            var json = JsonConvert.SerializeObject(so);
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            var packet = new byte[2 + jsonBytes.Length];

            packet[0] = (byte)PacketType.SpawnObjectRequest;
            packet[1] = (byte)ownerId;
            Buffer.BlockCopy(jsonBytes, 0, packet, 2, jsonBytes.Length);

            Debug.Log($"Send spawn request");
            await _client.SendAsync(packet, packet.Length, _server);
        }
    }

    public class ServerObject
    {
        public int Id;
        public int OwnerId;
        public string PrefabName;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }
}