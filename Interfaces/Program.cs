using Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    class Program
    {
        static void Main(string[] args)
        {
            TestServiceImpl svc = new TestServiceImpl();
            ViewTransformer vt = svc.Transformer;
            
        }
    }
}
