# Auth Toolbox

The Auth toolbox handles both Authentication and Authorization that can be used in ASP.NET Core Web API and Web MVC Projects.

The Auth toolbox supports two types of authentication flows.
The first is a "jwt in header" based authentication called **JwtHeaderAuth** and can be used for authentication and authorization in Web API projects.
The second is a cookie based authentication called **CookieAuth** and can be used for authentication and authorization in Web MVC projects.

In both cases the authentication is done using a bearer Jwt token issued by an external party. The Jwt token identifies the user making the request.

When the user is authenticated, the middleware will request the permissions for the user from a Policy Desision Point (PDP) and enforce these permissions.
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
  - [Aditional claims](#aditional-claims)
- [Configuration in Startup.Configure](#configuration-in-startupconfigure)
- [Authorization usage](#authorization-usage)
  - [AuthorizeByConvention attribute](#authorizebyconvention-attribute)
  - [AuthorizeWith attribute](#authorizewith-attribute)
- [How it works](#how-it-works)
  - [Basic auth flow](#basic-auth-flow)
  - [Request flow](#request-flow)
  - [CookieAuth scheme](#cookieauth-scheme)
  - [JwtHeaderAuth scheme](#jwtheaderauth-scheme)
  - [Jwt token](#jwt-token)
    - [Structure](#structure)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->


## Installation

To add the toolbox to a project, you add the package to the project.json :

``` json 
"dependencies": {
    "Toolbox.Auth":  "1.0.0"
 }
``` 

ALWAYS check the latest version [here](https://github.com/digipolisantwerp/auth_aspnetcore/blob/master/src/Toolbox.Auth/project.json) before adding the above line !

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
    "EnableCookieAuth": false,
    "EnableJwtHeaderAuth": true,
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
        options.EnableCookieAuth = true;
        options.PdpUrl = "http://pdp.somewhere.com/";
        options.PdpCacheDuration = 240;
        options.JwtAudience = "audience";
        options.JwtIssuer = "JWTIssuer";
    });
```

Following options can be set :

General options:

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
ApplicationName              | The name of the application. Required in order to request permissions to the PDP.|
PdpUrl | The url for the policy decision provider (PDP). |
PdpCacheDuration | The duration in minutes the responses from the PDP are cached. Set to zero to disable caching.| 60  
JwtIssuer | The issuer value used to validate the Jwt token.| 
JwtAudience | The audience url used to validate the Jwt token.| 
JwtSigningKeyProviderUrl | The url to the Jwt signing key provider endpoint.|
jwtSigningKeyProviderApikey | The api key for the signing key provider authentication.|
JwtSigningKeyCacheDuration | The duration in minutes the Jwt signing key is cached.| 10


Options used for JwtHeaderAuth

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
EnableJwtHeaderAuth              | Set to true to enable the JwtHeaderAuth scheme. (Web Api)| True  

Options used for CookieAuth

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
EnableCookieAuth              | Set to true to enable the CookieAuth scheme. (Web MVC)| False 
ApiAuthUrl | The url of the Api Engine authentication endpoint.| 
ApiAuthIdpUrl | The url of the Idp the Api Engine will redirect the saml request to.|
ApiAuthSpName | The service provider name of the Api Engine.|
ApiAuthSpUrl | The Api Engine callback url where the idp must redirect to.|

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

When placing the **AuthorizeByConvention** attribute on an api controller or action some conventions are applied to determine which permission the user must have to access the resource.

The convention is that the user must have a permission with following structure: {action}-{resource}.

The {action} part is translated to the http method used for the controller action.
These are the possible mappings from the http methods to the {action} part:

- GET -> read
- POST -> create
- PUT -> update
- DELETE -> delete

The {resource} part is mapped to the controller name (without the word controller).

This attribute can only be used with the JwtHeaderAuth scheme (Web Api projects / api controllers).

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

This attribute can be used with both the JwtHeaderAuth and the CookieAuth schemes.
Please note that the default scheme for this attribute is the **JwtHeaderAuth** scheme.

If you want to use this attribute with the **CookieAuth** scheme, the scheme name must be supplied using the **ActiveAuthenticationSchemes** parameter.

``` csharp
    [AuthorizeWith(ActiveAuthenticationSchemes = AuthSchemes.CookieAuth, Permission = "permission-125")]
    public class HomeController : Controller
    {
        //...
    }
```

## How it works


### Basic auth flow

![alt text](docs/basicauthflow.png "Flow")

The above picture visualizes the basic authentication and authorization flow.

The first step is to authenticate the user (1).
Depending on the enabled schemes this is done using a jwt token or a cookie.
After authentication, the permissions for the user are set as claims on the user principal. This is done in a claims transformation step (2). 
The permissions for the user are requested to a Policy Desision Point (2a) and can be cached. 
The final step is the authorization for the requested resource. In this step the required permissions for the resource are checked against the permissions from the user.
If the user is not authorized to acces the resource the request will return with an Http status code of 401 (UnAuthorized). 
It is still possible to allow resources to anonymous users using the **AllowAnonymous** attribute.
If the user is not authenticated it will result in an empty user principal containing no claims, thus authorization will fail for resouces that requires permissions.

### Request flow

![alt text](docs/middleware.png "Flow")

The above picture visualizes the request flow and the middleware's in action.

1. JwtBearerAuthentication middleware (only active when the jwtHeaderAuth scheme is anabled) checks if a jwt token is present in the Authorization header  
	If the token is present, it is validated.
	If the token is valid, the user is authenticated.  

2. CookieAuthentication middleware (only active when the CookieAuth scheme is enabled) checks if a valid user cookie is present  
	If the cookie is present, the user is authenticated.

3. ClaimsTransformation middleware  
	If a user is authenticated, the permissions of the user are added as claims on the User's identity.

4. If an Authorize... attribute is present on a controller or action, the filter checks the required permission for the action.

5. If the user doesn't have the required permission, the request pipeline is interrupted  
	An http status code 403 is set on the response object.

6. If the user has the requested permission, the action is invoked  

7. CookieAuthentication middleware checks if a 401 or 403 status code is set on the response object  
	If it is set and in combination of the authorization schema "CookieAuth" the response is transformed in a redirect response.

### CookieAuth scheme

The **CookieAuth** scheme can be used to secure your Web MVC projects.

All controllers serving html as content can use this scheme. It is also a way to acquire a jwt token needed for the **JwtHeaderAuth** scheme.

The scheme relies on an authentication cookie to authenticate the user. If no cookie is present in the request a redirect based flow is initiated to aquire a jwt token from an external token issuer.
Once the token is received and validated, the user is signed in and two cookies are set. The first is an authentication cookie and is the default asp.net cookie. The second is a cookie containing the jwt token.
This cookie named **jwt** can be used on the client side to extract the jwt token needed for Api calls (see jwtHeaderAuth flow).

### JwtHeaderAuth scheme

The **JwtHeaderAuth** scheme can be used to secure your Web Api projects.

All api controllers serving data can use this scheme.

The scheme relies on a jwt token present in the authorization header. When a jwt token is present it is validated. 
If the user is not authenticated it will result in a response with http status code 401.
If the user us authenticated but the authorization failed due to missing permissions it wil result in a response with http status code 403.

### Jwt token
 
 The toolbox uses a Jwt (JSON Web Token) to perform its authentication.
 For more details on how a Jwt is structured see https://jwt.io/introduction.  
 For development you can generate a jwt using an online tool like http://jwtbuilder.jamiekurtz.com.
 
#### Structure

 Here is an example of a Jwt (without signature) after base64 decoding:
 
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

#### Issuer validation

This checks if the token is originating from the expected issuer.
The check compares the issuer value ("iss" claim) against a known value from toolbox configuration.
If the values don't match the token is considered invalid.
    
#### Token expiration

The value from the expiration claim ("exp") is compared against the current server time. If the value is expired the token is considered invalid.

#### Signature validation

In order to check the token origin and integrity, the token signature must be validated. 
The signature from the token is considered to be an H-MAC SHA256 (HS256) encrypted with a symmetric key.

The signing key used to validate the signature will be aquired from the instance that issued the token.
This is done through a call to an endpoint on the issuer service. The url from that endpoint must be set in configuration in the **JwtSigningKeyProviderUrl** property.
The authentication on the endpoint requires an api key which must also be set in configuration using the **jwtSigningKeyProviderApikey** property.

For the api key it is advised not to set the value in code or in a config file but to use the **User secrets** api from Microsoft see: https://docs.asp.net/en/latest/security/app-secrets.html

The signing key can also be cached. The duration of the cache can be set using the **JwtSigningKeyCacheDuration** property in configuration. The default is 10 minutes. Use 0 to disable the cache.



