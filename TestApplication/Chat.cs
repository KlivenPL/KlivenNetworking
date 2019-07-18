using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlivenNetworking;

namespace TestApplication {
    class Chat {
        public static Chat Instance;
        Stopwatch loopSw = new Stopwatch();
        public static Action Update;
        public static Action FixedUpdate;

        public static TelepathyServer server;
        public static TelepathyClient client;

        static void Main(string[] args) {
            Instance = new Chat();
            Instance.Start();
        }

        void Start() {
            client = new TelepathyClient();

            client.Connect("localhost", 2137);
            Loop();
        }

        void Loop() {
            loopSw.Start();
            while (true) {
                if (loopSw.ElapsedTicks / (float)Stopwatch.Frequency >= 0.1f) {
                    FixedUpdate?.Invoke();
                    loopSw.Restart();
                }
                Update?.Invoke();
            }
        }
        string text = "";
        public void ClientUpdate() {
            if (Console.KeyAvailable) {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter) {
                    //Console.WriteLine($"players:{KlivenNet.Players.Count}");
                    //foreach (var player in KlivenNet.Players) {
                    //    Console.WriteLine(player.Name);
                    //}
                    SendTextMessage(text);
                    text = "";
                } else {
                    text += key.KeyChar;
                }
            }
        }


        void SendTextMessage(string text) {
            Console.WriteLine($"sending {text}");
        }

    }
}
