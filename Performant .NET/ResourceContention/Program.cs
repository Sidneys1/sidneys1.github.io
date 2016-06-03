using System;

namespace ResourceContention {
    internal class Program {
        internal static int ThreadCount = Environment.ProcessorCount;
        private const string ROOT = "C:\\Program Files\\";

        private static void Main() {
            Console.WriteLine($"Using {ThreadCount:N0} threads...");

            Conflicts.Run(ROOT);

            Console.WriteLine("Press Enter to try with no resource contentions");
            Console.ReadLine();

            var x = new NoConflicts();
            x.Run(ROOT);

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}