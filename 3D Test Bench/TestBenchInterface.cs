using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _3D_Test_Bench
{
    //Use this interface to programmatically insert objects created outside of the 3D Test Bench
    public interface TestBenchInterface
    {        
        event PlainObjectHandler PlainObjectAdded;
        event PlainObjectHandler PlainObjectRemoved;
    }

    public delegate void PlainObjectHandler(_3D_Test_Bench.PlainObjectInterface plainObject);
}
