using System;
using System.Diagnostics;

using System.Threading;
using System.Threading.Tasks;


using static Nancy.Injector;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            DewIt().GetAwaiter().GetResult();
        }

        static async Task DewIt()
        {
            Stopwatch watch = new Stopwatch();

            Console.WriteLine("Registering Ctor..");

            watch.Start();
            RegisterType<A>(() => new A(1));
            watch.Stop();
            Console.WriteLine($" ({watch.ElapsedMilliseconds}ms)");
            watch.Reset();

            var task = ResolveTypeAsync<A>();
            var nullBoi = NullifyAsync<A>(() => false);
            Console.WriteLine("doing other stuff..(~1000ms)");
            Thread.Sleep(1000);
            Console.WriteLine("done sleeping");

            Console.WriteLine("Resolving ..");

            object a;

            watch.Reset();
            //if (await nullBoi)
            //    Console.WriteLine("deleted shizz");
            watch.Start();
            a = await task;
            watch.Stop();
            Console.Write($"({watch.ElapsedMilliseconds}ms)");

            Console.ReadKey();
        }
    }

    class A
    {
        public int Value { get; set; }
        public A(int v)
        {
            Console.WriteLine($">>[Ctor called (5000ms)] ");
            Value = v;
            Thread.Sleep(5000);
            Console.WriteLine($">>[Ctor finished] ");
        }
        public override string ToString() => $"[{Value}]";
    }
}