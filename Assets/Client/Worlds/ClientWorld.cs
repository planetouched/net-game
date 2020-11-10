﻿using System;
using System.Collections.Generic;
using Client.Entities;
using Client.Entities._Base;
using Client.Factories;
using Shared.Enums;
using UnityEngine;

namespace Client.Worlds
{
    public class ClientWorld
    {
        private readonly Dictionary<uint, ClientEntityBase> _entities = new Dictionary<uint, ClientEntityBase>();
        private readonly List<WorldSnapshotWrapper> _snapshotsHistory = new List<WorldSnapshotWrapper>();

        public float currentServerTime { get; private set; }
        public event Action<ClientEntityBase> onAddLocalPlayer;

        public ClientEntityBase FindEntity(uint objectId)
        {
            if (_entities.TryGetValue(objectId, out var entity))
            {
                return entity;
            }

            return null;
        }

        public T FindEntity<T>(uint objectId, GameEntityType type) where T : ClientEntityBase
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
            float snapshotDeltaTime = 0;

            if (_snapshotsHistory.Count > 0)
            {
                currentServerTime = _snapshotsHistory[_snapshotsHistory.Count - 1].serverTime;
                snapshotDeltaTime = snapshotWrapper.serverTime - currentServerTime;
            }

            _snapshotsHistory.Add(snapshotWrapper);

            foreach (var clientEntity in _entities.Values)
            {
                clientEntity.UnUse();
            }

            FindEntity(ClientLocalPlayer.localObjectId)?.Use();

            foreach (var pair in snapshotWrapper.snapshotEntities)
            {
                uint objectId = pair.Key;
                var sharedEntity = pair.Value;

                if (_entities.TryGetValue(objectId, out var foundEntity))
                {
                    //update entity
                    foundEntity.Use();
                    foundEntity.SetCurrentEntity(sharedEntity);
                    foundEntity.SetSnapshotDeltaTime(snapshotDeltaTime);
                }
                else
                {
                    //new entity
                    var clientEntity = ClientEntityFactory.Create(sharedEntity, this);
                    
                    if (ClientLocalPlayer.localObjectId == clientEntity.objectId)
                    {
                        onAddLocalPlayer?.Invoke(clientEntity);
                    }
                    
                    clientEntity.Use();
                    clientEntity.SetSnapshotDeltaTime(snapshotDeltaTime);
                    clientEntity.Create();
                    _entities.Add(objectId, clientEntity);
                }
            }

            var entitiesCopy = new List<ClientEntityBase>(_entities.Values);

            for (int i = 0; i < entitiesCopy.Count; i++)
            {
                if (!entitiesCopy[i].isUsed)
                {
                    _entities.Remove(entitiesCopy[i].objectId);
                    entitiesCopy[i].Drop();
                }
            }

            if (_snapshotsHistory.Count > 256)
            {
                _snapshotsHistory.RemoveAt(0);
            }
        }

        //calls each frame
        public void Process()
        {
            currentServerTime += Time.deltaTime;

            foreach (var entity in _entities.Values)
            {
                entity.Process();
            }
            
            /*
            if (Physics.autoSimulation)
            {
                throw new Exception("Physics.autoSimulation is true");
            }
            
            Physics.Simulate(Time.deltaTime);*/
        }

        public void Clear()
        {
            foreach (var entity in _entities.Values)
            {
                entity.Drop();
            }

            _entities.Clear();

            onAddLocalPlayer = null;
        }
    }
}