using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace HumanResourcesManagementSystemFinal.Helpers
{
    public static class DataProtectionHelper
    {
        private static readonly byte[] entropy = Encoding.UTF8.GetBytes("HRMS_Secret_Key");

        public static string Protect(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] protectedBytes = ProtectedData.Protect(dataBytes, entropy, DataProtectionScope.CurrentUser);
            return System.Convert.ToBase64String(protectedBytes);
        }

        public static string Unprotect(string protectedData)
        {
            if (string.IsNullOrEmpty(protectedData)) return string.Empty;

            try
            {
                byte[] protectedBytes = System.Convert.FromBase64String(protectedData);
                byte[] dataBytes = ProtectedData.Unprotect(protectedBytes, entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(dataBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}