using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using blazorserver.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.GenericResource.Update;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;

/// This is the base class for Azure resources that will be included in output
// each superclass will implement its own methods to handle references, naming, code emission etc.
public class AzureResource
{
    public IResource Inner { get; set; }
    public Dictionary<string, string> Variables { get; set; }
    
    // TODO: implement sub-blocks

    public AzureResource()
    {
    }

    public virtual IResource Resource()
    {
        return Inner;
    }
    
    public virtual List<string> GetReferences()
    {
        return null;
    }

    public virtual string Emit()
    {
        return $"Unhandled resource: {Inner.Name}, {Inner.Type}\r\n";
    }
}