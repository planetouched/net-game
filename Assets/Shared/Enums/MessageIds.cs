namespace Shared.Enums
{
    public enum MessageIds : byte
    {
        //from client
        PlayerControl,
        Connect,
        Disconnect,
        
        //from server
        ConnectAccepted,
        WorldSnapshot
    }
}