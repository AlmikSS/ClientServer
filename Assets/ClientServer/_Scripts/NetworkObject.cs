using UnityEngine;

namespace VH.Multiplayer
{
    public class NetworkObject : NetworkBehaviour
    {
        [SerializeField] private bool _spawnWithScene = true;
        
        public int OwnerClientId { get; private set; }
        public int ObjectId { get; private set; }
        public bool SpawnWithScene => _spawnWithScene;
        public bool IsSpawned => _isSpawned;
        
        private bool _isSpawned = false;
        
        public void Setup(int ownerId, int objectId)
        {
            if (_isSpawned)
                return;
            
            OwnerClientId = ownerId;
            ObjectId = objectId;
            
            _isSpawned = true;
        }
    }
}