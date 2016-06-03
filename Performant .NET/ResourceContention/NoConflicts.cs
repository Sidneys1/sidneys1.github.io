using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ResourceContention {
    internal class NoConflicts {
        private readonly ConcurrentQueue<string> _hashMe = new ConcurrentQueue<string>();
        private static readonly ConcurrentBag<string> Hashes = new ConcurrentBag<string>();

        private bool _done;

        internal void Run(string root) {
            var threads = new Thread[Program.ThreadCount];

            Console.WriteLine("Walking....");
            WalkFs(root);
            var count = _hashMe.Count;
            Console.WriteLine($"Done! Found {count:N0} files!");
            Console.WriteLine("Press enter to start...");
            Console.ReadLine();

            var start = DateTime.Now;
            for (var i = 0; i < Program.ThreadCount; i++) {
                threads[i] = new Thread(ThreadWorker);
                threads[i].Start();
            }

            do {
                int c;
                if ((c = _hashMe.Count) != 0) {
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

        private void WalkFs(string root) {
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
                            _hashMe.Enqueue(result);
                }
                catch {
                    // ignored
                }
            }
        }

        private void ThreadWorker() {
            using (var md5 = new MD5CryptoServiceProvider()) {
                while (!_done) {
                    string file;
                    if (_hashMe.TryDequeue(out file)) {
                        var hash = md5.ComputeHash(Encoding.Default.GetBytes(file));
                        Hashes.Add(
                            $"'{file}' MD5: {BitConverter.ToString(hash).Replace("-", string.Empty).ToLower()}");
                        continue;
                    }

                    Thread.Sleep(100);
                }
            }
        }
    }
}