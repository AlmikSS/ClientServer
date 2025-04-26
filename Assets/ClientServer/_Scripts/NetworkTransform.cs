using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace VH.Multiplayer
{
    public class NetworkTransform : NetworkBehaviour
    {
        private UnityEngine.Vector3 _previousPosition;
        private UnityEngine.Quaternion _previousRotation;
        
        private void Update()
        {
            var client = NetworkManager.Instance.Client;
            var server = NetworkManager.Instance.Server;
            
            if (_previousPosition != transform.position)
            {
                var newPos = new Vector3 { x = transform.position.x, y = transform.position.y, z = transform.position.z };
                SendRequest(newPos, server, client, PacketType.UpdatePositionRequest);
            }

            if (_previousRotation != transform.rotation)
            {
                var newRot = new Quaternion { x = transform.rotation.x, y = transform.rotation.y, z = transform.rotation.z, w = transform.rotation.w };
                SendRequest(newRot, server, client, PacketType.UpdateRotationRequest);
            }
            
            _previousPosition = transform.position;
            _previousRotation = transform.rotation;
        }
        
        private async void SendRequest(object request, IPEndPoint server, UdpClient client, PacketType type)
        {
            var json = JsonConvert.SerializeObject(request);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var packet = new byte[2 + jsonBytes.Length];
                
            packet[0] = (byte)PacketType.UpdatePositionRequest;
            packet[1] = (byte)ObjectId;
            Buffer.BlockCopy(jsonBytes, 0, packet, 2, jsonBytes.Length);
                
            await client.SendAsync(packet, packet.Length, server);
        }
    }
}