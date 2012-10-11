using System;
using System.Collections.Generic;
using System.Text;

namespace Octopus.Test.Client
{
    public class DummyTest
    {
        public int A { get; set; }
        public string B { get; set; }
        public long C { get; set; }

        public DummyTest(int a, string b, long c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
