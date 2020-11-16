# BlazorRouter [![nuget](https://img.shields.io/nuget/vpre/Hez2010.BlazorRouter.svg)](https://www.nuget.org/packages/Hez2010.BlazorRouter)
#### An awesome router for blazor

## Installation
Via nuget package:
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

For example:
```html
<Switch>
    <Route Template="/">
        <Home />
    </Route>
    <Route Template="/login">
        <Account Action="Login" />
    </Route>
    <Route Template="/register">
        <Account Action="Register" />
    </Route>
    <Route Template="/user/{id:string}">
        <User />
    </Route>
    <Route>
        <p>404</p>
    </Route>
</Switch>
```

Besides, you can nested `Switch` to use nested routing:

For example:
```html
<Switch>
    <Route Template="/">
        <Home />
    </Route>
    <Route Template="/login">
        <Account Action="Login" />
    </Route>
    <Route Template="/register">
        <Account Action="Register" />
    </Route>
    <Route Template="/user/{id:string}/*">
        <Switch>
            <Route Template="/user/{id:string}/edit">
                <User Action="Edit" />
            </Route>
            <Route Template="/user/{id:string}/delete">
                <User Action="Delete" />
            </Route>
            <Route>
                <p>404 in /user/id</p>
            </Route>
        </Switch>
    </Route>
    <Route>
        <p>404</p>
    </Route>
</Switch>
```

`*` represents one (can be zero if it's the last segment in template) segment, and you can use `**` to match multiple segments (>= 0).

Note: `**` can only be the last segment.

```html
<Switch>
    <Route Template="/">
        <Home />
    </Route>
    <Route Template="/login">
        <Account Action="Login" />
    </Route>
    <Route Template="/register">
        <Account Action="Register" />
    </Route>
    <Route Template="/user/*/edit">
        <User Action="Edit" />
    </Route>
    <Route Template="/user/*/delete">
        <User Action="Delete" />
    </Route>
    <Route>
        <p>404</p>
    </Route>
</Switch>
```

## Live Example
[Visit Sample](https://hez2010.github.io/BlazorRouter)  
For source, please check out `BlazorRouter.Sample`.
