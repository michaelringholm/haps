using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrubloServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Distribution.CreateDistribution();
            Simulator.Run();
            SocketListener.StartListening();
        }
    }
}
