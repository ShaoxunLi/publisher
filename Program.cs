using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            OPCInstance OPC = new OPCInstance();
            
            OPC.Run();

            Console.ReadKey();
        }
    }
}
