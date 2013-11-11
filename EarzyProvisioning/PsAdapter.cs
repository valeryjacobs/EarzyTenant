using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace EarzyProvisioning
{
    public class PsAdapter
    {
       
        public void Run(string scriptfile)
        {
            Dictionary<string, string> myDict = new Dictionary<string, string>();
            myDict.Add("-subscriptionDataFile", "\"C:\\TopSecret\\azure.publishsettings\"");
            RunPowerShellScript(scriptfile, myDict);
        }
        
        
        internal void RunPowerShellScript(string scriptPath, Dictionary<string, string> arguments)
        {
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();
            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
            Pipeline pipeline = runspace.CreatePipeline();
            //Here's how you add a new script with arguments            
            Command myCommand = new Command(scriptPath);
            foreach (var argument in arguments)
            {
                myCommand.Parameters.Add(new CommandParameter(argument.Key, argument.Value));
            }
            pipeline.Commands.Add(myCommand);
            var results = pipeline.Invoke();
            foreach (var psObject in results)
            {

            }
        }
    }
}
