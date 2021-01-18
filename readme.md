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
- how to handle external references? i.e. things outside the resource group like log analytics etc.
