# Auth Toolbox

The Auth toolbox handles both Authentication and Authorization that can be used in ASP.NET Core 1.0 Web API Projects.

The Auth toolbox supports authentication using a bearer Jwt token. The Jwt token identifies the user making the request.
When a valid token is present in the request authorization header the middleware will request the permissions for the user and enforce these permissions.
The Auth toolbox acts as a Policy Enforcement Point (PEP).

All the permissions received from the PDP (Policy Decision Point) will be added to the claims of the User's principal and are available in the application.

The toolbox also provides Authorization attributes that can be used in the controllers to enforce authorization.  


## Table of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->


- [Installation](#installation)
- [Configuration in Startup.ConfigureServices](#configuration-in-startupconfigureservices)
  - [Json config file](#json-config-file)
  - [Code](#code)
- [Configuration in Startup.Configure](#configuration-in-startupconfigure)
- [Authorization usage](#authorization-usage)
  - [AuthorizeByConvention attribute](#authorizebyconvention-attribute)
  - [AuthorizeWith attribute](#authorizewith-attribute)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->


## Installation

To add the toolbox to a project, you add the package to the project.json :

``` json 
"dependencies": {
    "Toolbox.Auth":  "1.0.0"
 }
``` 

In Visual Studio you can also use the NuGet Package Manager to do this.

## Configuration in Startup.ConfigureServices

The Auth framework is registered in the ConfigureServices method of the Startup class.

There are 2 ways to configure the DataAccess framework : using a json config file or using code

### Json config file
The path to the Json config file has to be given as argument to the AddAuth method, together with the section name where the options are defined:
``` csharp
    services.AddAuth(options =>
    {
        options.FileName = @"configs/authconfig.json";
        options.Section = "Auth";
    });
```

The Auth framework will read the given section of the json file with the following structure:
``` json
{
  "Auth": {
    "ApplicationName": "TestApp",
    "PdpUrl": "http://pdp.somewhere.com/",
    "PdpCacheDuration": 60,
    "JwtAudience": "audience",
    "JwtIssuer": "issuer",
    "JwtUserIdClaimType": "sub"
  }
}
```

### Code
You can also call the AddDataAccess method, passing in the needed options directly:
``` csharp
    services.AddAuth(options =>
    {
        options.ApplicationName = "SampleApp";
        options.PdpUrl = "http://pdp.somewhere.com/";
        options.PdpCacheDuration = 60;
        options.JwtAudience = "audience";
        options.JwtIssuer = "JWTIssuer";
    });
```

Following options can be set :

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
ApplicationName              | The name of the application. | 
PdpUrl | The url for the policy decision provider (PDP). |
PdpCacheDuration | The duration in minutes the responses from the PDP are cached. Set to zero to disable caching.| 0  
JwtIssuer | The issuer value used to validate the Jwt token.| 
JwtAudience | The audience url used to validate the Jwt token.| 
JwtSigningKeyProviderUrl | The url to the Jwt signing key endpoint.|
jwtSigningKeyProviderApikey | The api key for the signing key provider authentication.|
JwtSigningKeyCacheDuration | The duration in minutes the Jwt signing key is cached.| 10
JwtValidatorClockSkew | The clock skew in minutes to apply for the Jwt expiration validation.| 1

## Configuration in Startup.Configure

The Auth middleware has to be added to the pipeline in the **Configure** method in the **Startup** class:

``` csharp
  app.UseAuth();
```
Please note that the order in which middleware is added is the order of execution of the middleware. Thus middleware in the pipeline previous to the Auth middleware will not be able to use the Auth features.

## Authorization usage

To authorize users to access actions on controllers you can use two types of attributes: 

 - AuthorizeByConvention
 - AuthorizeWith


### AuthorizeByConvention attribute

When placing the **AuthorizeByConvention** attribute on a controller or action some conventions are applied to determine which permission the user must have to access the resource.

The convention is that the user must have a permission with following structure: {action}-{resource}.

The {action} part is translated to the http method used for the controller action.
These are the possible mappings from the http methods to the {action} part:

- GET -> read
- POST -> create
- PUT -> update
- DELETE -> delete

The {resource} part is mapped to the controller name (without the word controller).

For example when the **AuthorizeByConvention** attribute is placed on an http GET method inside a controller named "TicketsController" then the user must have a permission "read-tickets".

``` csharp
    public class TicketsController
    {
        [HttpGet]                   // => The Http method determines the first part of the permission, not the action method name!
        [AuthorizeByConvention]     // => a user with permission 'read-tickets' will be allowed
        public IActionResult GetAction()
        {
            ...
        }

        [HttpPost]
        [AuthorizeByConvention]  // => a user with permission 'create-tickets' will be allowed
        public IActionResult PostAction()
        {
            ...
        }

        [HttpPut]
        [AuthorizeByConvention]  // => a user with permission 'update-tickets' will be allowed
        public IActionResult PutAction()
        {
            ...
        }

        [HttpDelete]
        [AuthorizeByConvention]  // => a user with permission 'delete-tickets' will be allowed
        public IActionResult DeleteAction()
        {
            ...
        }
    }
```

### AuthorizeWith attribute

When placing the **AuthorizeWith** attribute on a controller or action it is possible to define the permission the user must have to allow access to the resource.

The attribute property **Permission** has to be set in order to define the permission.
``` csharp
    [HttpGet]
    [AuthorizeWith(Permission = "permission-125")]  // => a user with permission 'permission-125' will be allowed
    public IActionResult GetActionWithCustomPermission()
    {
        ...
    }
```

Alternatively a list of permissions can be set on the **Permissions** property to define multiple permissions where at least one must match.
``` csharp
    [HttpPost]
    [AuthorizeWith(Permissions = new[] { "permission-125", "permission-321" })]  // => a user with permission 'permission-125' or 'permission-321' will be allowed
    public IActionResult PostAction()
    {
        ...
    }
```

Both **AuthorizeByConvention** and **AuthorizeWith** attributes are derived from the default **Microsoft.AspNet.Authorization.Authorize** attribute. This means that the way these attributes can be combined is still the same as for the default **Authorize** attribute.

 
