using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Excavator.Service {
  public class Program {
    public static void Main(string[] args) {
      //http://stackoverflow.com/questions/4856403/installing-a-windows-service-programatically
      if (Environment.UserInteractive) {
        ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
      }
      Console.WriteLine("Press any key to continue...");
      Console.ReadKey(true);
    }
  }
}
