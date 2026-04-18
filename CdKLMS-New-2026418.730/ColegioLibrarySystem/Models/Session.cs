using System;

namespace ColegioLibrarySystem.Models
{
    public static class Session
    {
        public static int UserID { get; set; }
        public static string Username { get; set; }
        public static string Role { get; set; }
        public static string FullName { get; set; }

        public static void Clear()
        {
            UserID = 0;
            Username = null;
            Role = null;
            FullName = null;
        }
    }
}