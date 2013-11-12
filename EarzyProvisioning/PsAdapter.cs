using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.IO;
using System.Collections.ObjectModel;
using System.Configuration;

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

        public void CreateEarzyTenantWebsite(string tenantName)
        {
            //var runspaceConfiguration = RunspaceConfiguration.Create();
            //var runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            //runspace.Open();

            //var pipeline = runspace.CreatePipeline();

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(string.Format(@"C:\\TopSecret\\TenantScript{0}.ps1", tenantName), true))
            {
                file.WriteLine("$secpasswd = ConvertTo-SecureString \"" + ConfigurationManager.AppSettings["gitpwd"] + "\" -AsPlainText -Force");
                file.WriteLine("$mycreds = New-Object System.Management.Automation.PSCredential (\"valeryjacobs\", $secpasswd)");
                file.WriteLine(string.Format("New-AzureWebsite {0} -GitHub -GithubRepository {1} -GithubCredentials $mycreds", tenantName, "valeryjacobs/EarzyTenant"));
                file.WriteLine("Get-AzureWebsiteDeployment " + tenantName);
            };

            //RunPsScript(string.Format(@"C:\\TopSecret\\TenantScript{0}.ps1", tenantName));
        }

        public Collection<PSObject> RunPsScript(string psScriptPath)
        {
            string psScript = string.Empty;
            if (File.Exists(psScriptPath))
                psScript = File.ReadAllText(psScriptPath);
            else
                throw new FileNotFoundException("Wrong path for the script file");

            Runspace runSpace = RunspaceFactory.CreateRunspace();
            runSpace.Open();

            RunspaceInvoke runSpaceInvoker = new RunspaceInvoke(runSpace);
            runSpaceInvoker.Invoke("Set-ExecutionPolicy Unrestricted");

            Pipeline pipeLine = runSpace.CreatePipeline();
            pipeLine.Commands.AddScript(psScript);
            //pipeLine.Commands.Add("Out-String");

            Collection<PSObject> returnObjects = pipeLine.Invoke();

            if (pipeLine.Error.Count > 0)
            {
                var error = pipeLine.Error.Read() as Collection<ErrorRecord>;
                if (error != null)
                {
                    foreach (ErrorRecord er in error)
                    {
                        Console.WriteLine("[PowerShell]: Error in cmdlet: " + er.Exception.Message);
                    }
                }
            }



            runSpace.Close();

            return returnObjects;
        }

        public async Task<int> RunScript(string script)
        {
            // create Powershell runspace
            Runspace runspace = RunspaceFactory.CreateRunspace();
            // open it
            runspace.Open();

            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(script);
            pipeline.Commands.AddScript("#your main script");

            // execute the script
            var results = pipeline.Invoke();
            foreach (var psObject in results)
            {

            }
            // close the runspace
            runspace.Close();

            return results.Count();
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
            //foreach (var argument in arguments)
            //{
            //    myCommand.Parameters.Add(new CommandParameter(argument.Key, argument.Value));
            //}
            pipeline.Commands.Add(myCommand);
            var results = pipeline.Invoke();
            foreach (var psObject in results)
            {

            }
        }
    }
}
