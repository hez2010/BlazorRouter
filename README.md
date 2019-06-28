# BlazorRouter
#### An awesome router for blazor

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

The order of routing is from the top to the bottom in `<Switch>`, and it will use the first matched item.  
If `Template` was not defined or defined as empty, it will match any path. 

## Example
Please check out `BlazorRouter.Sample`.
