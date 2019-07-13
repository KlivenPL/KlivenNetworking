using System;

namespace KlivenNetworking {
    public static class KNetLogger {
        public static Action<string> Log = Console.WriteLine;
        public static Action<string> LogWarning = Console.WriteLine;
        public static Action<string> LogError = Console.Error.WriteLine;
    }
}
