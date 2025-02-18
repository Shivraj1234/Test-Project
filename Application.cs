using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AutoNumberConfiguration.Plugins
{
    class Application : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            try
            {
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(IOrganizationService));
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                    if (context.MessageName.ToLower() != "create" && context.Stage != 20)
                    {
                        return;
                    }
                    Entity targetEntity = context.InputParameters["Target"] as Entity;
                    Entity updateAutoNumber = new Entity("au_AutoNumber");

                    StringBuilder autonumber = new StringBuilder();
                    string prefix, suffix, seperator, current;

                    QueryExpression qeAutoNumberConfig = new QueryExpression();
                    {
                        string EntityName = "au_AutoNumber";
                        ColumnSet ColumnSet = new ColumnSet("au_Newcolumn", "au_Prefix", "au_Suffix", "au_CurrentNumber");
                    };
                    EntityCollection ecAutoNumberConfig = service.RetrieveMultiple(qeAutoNumberConfig);
                    if (ecAutoNumberConfig.Entities.Count == 0)
                    {
                        return;
                    }
                    foreach (Entity entity in ecAutoNumberConfig.Entities)
                    {
                        if (entity.Attributes["au_Newcolumn"].ToString().ToLower() == "autonumber1")
                        {
                            prefix = entity.GetAttributeValue<string>("au_Prefix");
                            suffix = entity.GetAttributeValue<string>("au_Suffix");
                            seperator = entity.GetAttributeValue<string>("au_Seperator");
                            current = entity.GetAttributeValue<string>("au_CurrentNumber");
                            int tempcurrent = int.Parse(current);
                            tempcurrent++;
                            current = tempcurrent.ToString("000000");
                            updateAutoNumber.Id = entity.Id;
                            updateAutoNumber["au_currentNumber"] = current;
                            service.Update(updateAutoNumber);
                            autonumber.Append(prefix + seperator + updateAutoNumber + seperator + suffix);
                            break;

                        }
                    }
                    targetEntity["invoice"] = autonumber.ToString();
                }

            }
            catch (Exception ex) {
                throw new InvalidPluginExecutionException(ex.Message);
            }
           
             
        }
    }
}
