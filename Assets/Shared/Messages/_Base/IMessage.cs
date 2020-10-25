using Shared.CommonInterfaces;
using Shared.Enums;

namespace Shared.Messages._Base
{
    public interface IMessage : ISerializable, IDeserializable
    {
        uint messageNum { get; }
        int gameId { get; }
        MessageIds messageId { get; }
        uint objectId { get; }
        //float time { get; }

        //IMessage SetTime(float t);
        IMessage SetGameId(int id);
        IMessage SetObjectId(uint id);
        IMessage SetMessageNum(uint num);
    }
}