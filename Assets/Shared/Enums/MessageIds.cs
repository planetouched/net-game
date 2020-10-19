namespace Shared.Enums
{
    public enum MessageIds : byte
    {
        //from client
        PlayerControl,
        EnterGame,
        
        //from server
        ConnectAccepted,
        WorldSnapshot
    }
}