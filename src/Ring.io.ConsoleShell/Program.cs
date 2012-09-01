using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ring.io.ConsoleShell
{
    class Program
    {
        private const int DEFAULT_PORT = 5991;
        private static Timer timer;
        private static List<Node> nodes = new List<Node>();
        private static List<Node> seedNodes = new List<Node>();
        private static string currentCommand;

        static void Main(string[] args)
        {
            Console.SetWindowSize(1, 1);
            Console.SetBufferSize(80, 25);
            Console.SetWindowSize(80, 25);
            Console.Clear();

            timer = new Timer(timer_Callback, null, 1000, 1000);
            DrawScreen();
            
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key == ConsoleKey.Enter)
                {
                    ProcessCommand(currentCommand.Split(' '));
                    currentCommand = string.Empty;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    currentCommand = currentCommand.Remove(currentCommand.Length - 1);
                }
                else
                {
                    currentCommand += Convert.ToString(key.KeyChar);
                }
            }
        }

        private static void timer_Callback(object state)
        {
            DrawScreen();
        }

        private static void DrawScreen()
        {
            Console.Clear();
            Console.WriteLine("Node\tRing");
            Console.WriteLine("--------------------------------------------------------------------------------");
            for (int i = 0; i < nodes.Count; i++)
            {
                Console.SetCursorPosition(0, i + 2);
                Console.Write(i + 1 + "\t");

                lock (nodes[i].Nodes)
                {
                    foreach (var node in nodes[i].Nodes)
                    {
                        int index = nodes.FindIndex(x => x.Entry.NodeId == node.Value.NodeId);
                        Console.Write(index + 1 + " ");
                    }
                }
                Console.WriteLine();
            }

            Console.SetCursorPosition(0, 24);
            Console.Write("rsh> " + currentCommand);
        }

        private static void ProcessCommand(string[] args)
        {
            switch (args[0])
            {
                case "create":
                    int count = 1;
                    if (args.Length > 1)
                    {
                        count = int.Parse(args[1]);
                    }
                    for (int i = 0; i < count; i++)
                    {
                        Node node;
                        if (nodes.Count == 0)
                        {
                            node = new Node();
                            nodes.Add(node);
                            node.Open();
                        }
                        else
                        {
                            int port = DEFAULT_PORT + nodes.Count;
                            node = new Node(port);
                            nodes.Add(node);
                            node.Open();
                        }

                        if (args.Length > 2 && args[2] == "seed")
                        {
                            seedNodes.Add(node);
                        }
                    }

                    foreach (var node in nodes)
                    {
                        foreach (var seed in seedNodes)
                        {
                            node.AddSeedNode(seed.Entry);
                        }
                    }
                    break;
                case "remove":
                    break;
            }
        }
    }
}