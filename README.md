# BlazorRouter
#### An aewsome router for blazor

## Installation
```
dotnet add package Hez2010.BlazorRouter
```
Then append `@using BlazorRouter` to your `_Imports.razor`

## Usage
Put `<Switch>` to the place where you want to route your contents.  
Then use `<Route>` to config routing template with `Template` property.  
In the end, you can put the routed content inside the `<Route>`.  

Parameters in the routing template will be passed as `IDictionary<string, object>` with attribute `[CascadingParameter(Name = "RouteParameters")]`.  

If you are looking for an example, please checkout `BlazorRouter.Sample`.