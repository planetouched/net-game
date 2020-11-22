using System.Collections.Generic;
using LiteNetLib.Utils;
using Server.Worlds;
using Shared.Enums;
using Shared.Messages._Base;

namespace Shared.Messages.FromServer
{
    public class GetWorldsListMessage : MessageBase
    {
        private readonly Dictionary<int, ServerWorld> _worlds;
        public int[] worlds { get; private set; }
        
        public GetWorldsListMessage(Dictionary<int, ServerWorld> worlds) : base(MessageIds.GetWorldsList)
        {
            _worlds = worlds;
            system = true;
        }
        
        public override NetDataWriter Serialize(NetDataWriter netDataWriter)
        {
            WriteHeader(netDataWriter);

            var sortedWorlds = new List<int>();
            
            foreach (var pair in _worlds)
            {
                if (!pair.Value.isStarted)
                {
                    sortedWorlds.Add(pair.Key);
                }
            }
            
            netDataWriter.Put(sortedWorlds.Count);
            
            foreach (var id in sortedWorlds)
            {
                netDataWriter.Put(id);
            }
            
            return netDataWriter;
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);

            int count = netDataReader.GetInt();
            worlds = new int[count];

            for (int i = 0; i < count; i++)
            {
                worlds[i] = netDataReader.GetInt();
            }
        }
    }
}