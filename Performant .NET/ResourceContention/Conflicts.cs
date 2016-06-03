using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ResourceContention {
    internal static class Conflicts {
        private static readonly Queue<string> HashMe = new Queue<string>(25000);
        private static readonly List<string> Hashes = new List<string>();

        private static bool _done;

        internal static void Run(string root) {
            var threads = new Thread[Program.ThreadCount];

            Console.WriteLine("Walking....");
            WalkFs(root);
            var count = HashMe.Count;
            Console.WriteLine($"Done! Found {count:N0} files!");
            Console.WriteLine("Press enter to start...");
            Console.ReadLine();

            var start = DateTime.Now;
            for (var i = 0; i < Program.ThreadCount; i++) {
                threads[i] = new Thread(ThreadWorker);
                threads[i].Start();
            }

            do {
                var c = HashMe.Count;
                if (c != 0) {
                    Console.Write($"\r{count - c:N0}/{count:N0}");
                    continue;
                }
                _done = true;
                Console.WriteLine("\r                         ");
                break;
            } while (true);

            foreach (var thread in threads)
                thread.Join();

            Console.WriteLine($"Done in {(DateTime.Now - start).TotalMilliseconds:N0}ms!");
        }

        private static void WalkFs(string root) {
            var paths = new Stack<string>();
            paths.Push(root);
            while (paths.Count > 0) {
                var path = paths.Pop();
                try {
                    foreach (var subpath in Directory.GetDirectories(path))
                        paths.Push(subpath);
                }
                catch {
                    // ignored
                }

                try {
                    foreach (var file in Directory.GetFiles(path))
                        foreach (var result in Enumerable.Repeat(file, 10))
                            HashMe.Enqueue(result);
                }
                catch {
                    // ignored
                }
            }
        }

        private static void ThreadWorker() {
            using (var md5 = new MD5CryptoServiceProvider()) {
                while (!_done) {
                    string file = null;
                    lock (HashMe) {
                        if (HashMe.Count > 0)
                            file = HashMe.Dequeue();
                    }

                    if (file != null) {
                        var hash = md5.ComputeHash(Encoding.Default.GetBytes(file));
                        lock (Hashes) {
                            Hashes.Add(
                                $"'{file}' MD5: {BitConverter.ToString(hash).Replace("-", string.Empty).ToLower()}");
                        }
                        continue;
                    }

                    Thread.Sleep(100);
                }
            }
        }
    }
}