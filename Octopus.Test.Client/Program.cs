using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

using Octopus.Management.Commons;
using Octopus.Management.Commons.Jobs;

namespace Octopus.Test.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var dummy1 = new DummyTest(1, "test1", 4244);
            var dummy2 = new DummyTest(3, "test2", 23498);
            var dummy3 = new DummyTest(1, "test1", 4244);
            var dummy4 = new DummyTest(1, "test1", 4244);
            var dummy5 = new DummyTest(673, "test5", 2348);
            var dummy6 = new DummyTest(8, "test6", 8553);
            var dummy7 = new DummyTest(51, "test7", 576);
            var dummy8 = new DummyTest(3, "test2", 23498);
            var dummy9 = new DummyTest(3331, "test9", 30044);
            var dummy10 = new DummyTest(1, "test1", 4244);
            var dummy11 = new DummyTest(3, "test2", 23498);
            var dummy12 = new DummyTest(3331, "test9", 30044);

            var st1 = new Sticky<DummyTest>(DoWhile2, dummy1) {IsCacheable = true};
            var st2 = new Sticky<DummyTest>(DoWhile2, dummy2) {IsCacheable = true};
            var st3 = new Sticky<DummyTest>(DoWhile2, dummy3) {IsCacheable = true};
            var st4 = new Sticky<DummyTest>(DoWhile2, dummy4) {IsCacheable = true};
            var st5 = new Sticky<DummyTest>(DoWhile2, dummy5) {IsCacheable = true};
            var st6 = new Sticky<DummyTest>(DoWhile2, dummy6);
            var st7 = new Sticky<DummyTest>(DoWhile2, dummy7);
            var st8 = new Sticky<DummyTest>(DoWhile2, dummy8) {IsCacheable = true};
            var st9 = new Sticky<DummyTest>(DoWhile2, dummy9) {IsCacheable = true};
            var st10 = new Sticky<DummyTest>(DoWhile2, dummy10);
            var st11 = new Sticky<DummyTest>(DoWhile2, dummy11);
            var st12 = new Sticky<DummyTest>(DoWhile2, dummy12);

            st6.MillisecondsDelay = 10000;
            st7.MillisecondsDelay = 10000;

            long tm1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            st9.PriorityPercent = 35;
            st7.PriorityPercent = 65;

            WorkUtils.AddWork(st1);
            WorkUtils.AddWork(st2);
            WorkUtils.AddWork(st3);
            WorkUtils.AddWork(st4);
            WorkUtils.AddWork(st5);
            WorkUtils.AddWork(st6);
            WorkUtils.AddWork(st7);
            
            //Hal.StopSonar();

            WorkUtils.AddWork(st8);
            WorkUtils.AddWork(st9);

            using(var exeCxt = new ExecutionContext<DummyTest>())
            {
                WorkUtils.AddWork(st10, true);
                WorkUtils.AddWork(st11, exeCxt);
                WorkUtils.AddWork(st12, exeCxt);    
            }


            Hal.WaitEnd();
            
            long tm2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            long octopusTm = tm2- tm1;

            Console.WriteLine("Ejecucion Octopus: {0}ms", octopusTm);
           
            Console.Read();
        }

        static object DoWhile2(Token<DummyTest> token)
        {
            int max = 300000000;

            for (int i = 1; i <= max; i++)
            {
                bool canProcess = token.Signal((int)(((float)i / max) * 100));

                if(!canProcess)
                {
                    break;
                }
            }

            return null;
        }
    }
}
