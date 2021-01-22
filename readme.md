# mktf

This is my project to create Terraform files from resource groups in Azure. It's basically getting the resources in a resource group and transcoding the JSON into TF. I am making some assumptions that all the resources in a group are related. So if one resource relates to another it will contain a reference to a resource in the same module. If the resource group doesn't match the code considers it to be an external resource and will add a data source for the foreign resource.

The project is using Blazor WASM and runs completely in the browser. Initially I had some issues with the dotnet client libraries for Azure because the authentication failed looking for cached credentials. "Not supported on this platform" - so I moved the Azure Service to a blazor server project. However due to limitations in the fluent libraries I opted for a REST approach instead. This works in the browser so all processing is done on the client.

To configure the client you need to set three environment variables:

`AZURE_TENANT_ID` the tenant ID for Azure AD that you use to authenticate
`AZURE_CLIENT_ID` the client ID of the service principal you will use to query Azure
`AZURE_CLIENT_SECRET` the client secret for the above service principal

I would like to support device authentication and even interactive authentication if this project gets hosted. For now environment variables suit me.



===

Some notes for myself on this project:

create new project with `dotnet new blazorserver`
copy `\wwwroot\lib` from working mvc proejct
modify _Host.cshtml and add:

```
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
```
prior to
```
    <script src="_framework/blazor.server.js"></script>
```


To add Monaco Editor:
https://github.com/serdarciplak/BlazorMonaco


Graph code lifted from:
https://docs.microsoft.com/en-us/previous-versions/ms379574(v=vs.80)


ToDo:
- process each resource type
- output script
- add resource group filter
- add resource filter from graph
- naming convention helper
- code emission templates
- emission strategy - one resource per file, one big file?
- variables and tfvars files or hard-coded?
- how to handle external references? i.e. things outside the resource group like log analytics etc.
- create terraform import code to reconcile state
- define variables for things like location etc.