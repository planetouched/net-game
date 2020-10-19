﻿using System.Collections.Generic;
using System.Numerics;
using Client.Entities;
using Client.Entities._Base;
using Shared.Enums;

namespace Client.Worlds
{
    public class ClientWorld
    {
        private readonly Dictionary<uint, IClientEntity> _entities = new Dictionary<uint, IClientEntity>();
        private readonly List<WorldSnapshotWrapper> _snapshotsHistory = new List<WorldSnapshotWrapper>();

        public ClientWorld()
        {
            /*
            var c = new ClientLocalPlayer();
            _entities.Add(0, c);
            c.position = new Vector3(0, 1, 0);*/
        }
        
        public IClientEntity FindEntity(uint objectId)
        {
            if (_entities.TryGetValue(objectId, out var entity))
            {
                return entity;
            }

            return null;        
        }

        public T FindEntity<T>(uint objectId, GameEntityType type) where T : class
        {
            var entity = FindEntity(objectId);
            if (entity != null && entity.type == type)
            {
                return (T) entity;
            }

            return null;
        }
        
        public void AddWorldSnapshot(WorldSnapshotWrapper snapshotWrapper)
        {
            foreach (var clientEntity in _entities.Values)
            {
                clientEntity.UnUse();
            }

            FindEntity(ClientLocalPlayer.localObjectId)?.Use();
            
            foreach (var pair in snapshotWrapper.snapshotEntities)
            {
                uint objectId = pair.Key;
                var snapshotEntity = pair.Value;

                if (_entities.TryGetValue(objectId, out var foundEntity))
                {
                    //update entity
                    foundEntity.Use();
                    foundEntity.position = snapshotEntity.position;
                    foundEntity.rotation = snapshotEntity.rotation;
                }
                else
                {
                    //new entity
                    snapshotEntity.Use();
                    _entities.Add(objectId, snapshotEntity);
                    snapshotEntity.Create();
                }
            }
            
            var entitiesCopy = new List<IClientEntity>(_entities.Values);

            for (int i = 0; i < entitiesCopy.Count; i++)
            {
                if (!entitiesCopy[i].isUsed)
                {
                    _entities.Remove(entitiesCopy[i].objectId);
                    entitiesCopy[i].Drop();
                }
            }
            
            _snapshotsHistory.Add(snapshotWrapper);
            
            if (_snapshotsHistory.Count > 256)
            {
                _snapshotsHistory.RemoveAt(0);
            }
        }

        public void Process()
        {
            foreach (var entity in _entities.Values)
            {
                entity.Process();
            }
        }

        public void Clear()
        {
            foreach (var entity in _entities.Values)
            {
                entity.Drop();
            }
            
            _entities.Clear();
        }
    }
}