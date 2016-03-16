# Auth Toolbox

The Auth toolbox handles both Authentication and Authorization that can be used in ASP.NET Core 1.0 Web API Projects.

The Auth toolbox supports authentication using a bearer Jwt token. The Jwt token identifies the user making the request.
When a valid token is present in the request authorization header the middleware will request the permissions for the user from a Policy Desision Point (PDP) and enforce these permissions.
The Auth toolbox acts as a Policy Enforcement Point (PEP).

All the permissions received from the PDP (Policy Decision Point) will be added to the claims of the User's principal and are available in the application.

The toolbox also provides Authorization attributes that can be used in the controllers to enforce authorization.  


## Table of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->


- [Auth flow](#auth-flow)
- [Installation](#installation)
- [Configuration in Startup.ConfigureServices](#configuration-in-startupconfigureservices)
  - [Json config file](#json-config-file)
  - [Code](#code)
  - [Aditional claims](#aditional-claims)
- [Configuration in Startup.Configure](#configuration-in-startupconfigure)
- [Authorization usage](#authorization-usage)
  - [AuthorizeByConvention attribute](#authorizebyconvention-attribute)
  - [AuthorizeWith attribute](#authorizewith-attribute)
- [Jwt token](#jwt-token)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Auth flow

![alt text](docs/authflow.png "Flow")

The above picture visualizes the authentication and authorization flow.

Assuming that a request contains a Jwt token in the authorization header, the first step is to read and validate the token (1).
A part of the validation is the check of the token signature. To validate the signature a singing key is required. The toolbox aquires that singing key from the token issuer (1a) and can be cached.

After validation, which authenticates the user, the permissions for the user are set as claims on the user principal. This is done in a claims transformation step (2). 
The permissions for the user are requested to a Policy Desision Point (2a) and can be cached. 

The final step is the authorization for the requested resource. In this step the required permissions for the resource are checked against the permissions from the user.
If the user is not authorized to acces the resource the request will return with an Http status code of 401 (UnAuthorized). 

It is still possible to allow resources to anonymous users using the **AllowAnonymous** attribute.

If no token is present or if the token is invalid this will result in an empty user principal containing no claims, thus authorization will fail for resouces that requires permissions.
That will finaly result in a response with status code 401 (UnAuthorized).
The reason for the 401 can be viewed in the logs.

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
    "PdpCacheDuration": 240,
    "JwtAudience": "audience",
    "JwtIssuer": "issuer",
    "JwtUserIdClaimType": "sub"
  }
}
```

### Code
You can also call the AddAuth method, passing in the needed options directly:
``` csharp
    services.AddAuth(options =>
    {
        options.ApplicationName = "SampleApp";
        options.PdpUrl = "http://pdp.somewhere.com/";
        options.PdpCacheDuration = 240;
        options.JwtAudience = "audience";
        options.JwtIssuer = "JWTIssuer";
    });
```

Following options can be set :

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
ApplicationName              | The name of the application. Required in order to request permissions to the PDP.| 
PdpUrl | The url for the policy decision provider (PDP). |
PdpCacheDuration | The duration in minutes the responses from the PDP are cached. Set to zero to disable caching.| 60  
JwtIssuer | The issuer value used to validate the Jwt token.| 
JwtAudience | The audience url used to validate the Jwt token.| 
JwtSigningKeyProviderUrl | The url to the Jwt signing key endpoint.|
jwtSigningKeyProviderApikey | The api key for the signing key provider authentication.|
JwtSigningKeyCacheDuration | The duration in minutes the Jwt signing key is cached.| 10

### Aditional claims

An optional parameter of type **Dictionary&lt;string, AuthorizePolicy&gt;** is available on both **AddAuth** methods. With this parameter it is possible to add a collection of (Microsoft.AspNet) authorization policies.

Note: These policies are not to be confused with the policies as defined in the Identity Server and used in the PEP / PDP concept. 


## Configuration in Startup.Configure

The Auth middleware has to be added to the pipeline in the **Configure** method in the **Startup** class:

``` csharp
  app.UseAuth();
```
Please note that the order in which middleware is added is the order of execution of the middleware. Thus middleware in the pipeline previous to the Auth middleware will not be able to use the Auth features.

## Authorization usage

To authorize users to access actions on controllers (resources) you can use two types of attributes: 

 - AuthorizeByConvention
 - AuthorizeWith

Additional all existing authorization attributes defined in the asp.net framework are still usable.

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

## Jwt token
 
 The toolbow uses a Jwt (Jason Web Token) to perform its authentication. The token must be present in the request as a authorization header.
 For more details on how a Jwt is structured see https://jwt.io/introduction.  
 For development you can generate a jwt using an online tool like http://jwtbuilder.jamiekurtz.com.
 
 
 ### Structure
 Here is an example of a Jwt after base64 decoding:
 
 ``` json
{
  "typ": "JWT",
  "alg": "HS256"
}
.
{
  "iss": "Online JWT Builder",
  "iat": 1458028899,
  "exp": 1489564899,
  "aud": "www.example.com",
  "sub": "jrocket@example.com"
}
```

In order to validate the token the toolbox performs these checks:
  
 * Issuer validation
 * Token expiration
 * Signature validation

### Issuer validation

This checks if the token is originating from the expected issuer.
The check compares the issuer value ("iss" claim) against a known value from toolbox configuration.
If the values don't match the token is considered invalid.
    
### Token expiration

The value from the expiration claim ("exp") is compared against the current server time. If the value is expired the token is considered invalid.

### Signature validation

In order to check token origin and integrity token signature must be validated. 
The signature from the token is considered to be an H-MAC SHA256 (HS256) encrypted with a symmetric key.

The signing key used to validate the signature will be aquired from the instance that issued the token.
This is done through a call to an endpoint on the issuer service. The url from that endpoint must be set in configuration in the **JwtSigningKeyProviderUrl** property.
The authentication on the endpoint requires an api key which must also be set in configuration using the **jwtSigningKeyProviderApikey** property.

For the api key it is advised not to set the value in code or in a config file but to use the **User secrets** api from Microsoft see: https://docs.asp.net/en/latest/security/app-secrets.html

The signing key can also be cached. The duration of the cache can be set using the **JwtSigningKeyCacheDuration** property in configuration. The default is 10 minutes. Use 0 to disable the cache.



