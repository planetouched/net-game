using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RealtimeDebug
{
    public class ConsoleMessageFilter
    {
        private HashSet<string> filteredMessages = new HashSet<string>();    

        public bool IsFiltered(string message, string stackTrace)
        {        
            return filteredMessages.Contains(GetHashString(message, stackTrace));
        }

        public void SetFilter(string message, string stackTrace, bool isFiltered)
        {
            string hash = GetHashString(message, stackTrace);

            if (isFiltered && !filteredMessages.Contains(hash))
            {
                filteredMessages.Add(hash);
            }
            else if(!isFiltered && filteredMessages.Contains(hash))
            {
                filteredMessages.Remove(hash);
            }
        
        }

        private byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private string GetHashString(string message, string stackTrace)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(message);
            foreach (byte b in GetHash(stackTrace))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }    
    }
}
