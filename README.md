# Auth Toolbox

The Auth toolbox handles both Authentication and Authorization that can be used in ASP.NET Core Web API and Web MVC Projects.

The Auth toolbox supports two types of authentication flows.
The first is a "jwt in header" based authentication called **JwtHeaderAuth** and can be used for authentication and authorization in Web API projects.
The second is a cookie based authentication called **CookieAuth** and can be used for authentication and authorization in Web MVC projects.

In both cases the authentication is done using a bearer Jwt token issued by an external party. The Jwt token identifies the user making the request.

When the user is authenticated, the middleware will request the permissions for the user from a Policy Decision Point (PDP) and enforce these permissions.
The Auth toolbox acts as a Policy Enforcement Point (PEP).

All the permissions received from the PDP (Policy Decision Point) will be added to the claims of the User's principal and are available in the application.

The toolbox also provides Authorization attributes that can be used in the controllers to enforce authorization.  


## Table of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->


- [Installation](#installation)
- [Dependencies](#dependencies)
- [Configuration in Startup.ConfigureServices](#configuration-in-startupconfigureservices)
  - [Json config file](#json-config-file)
  - [Code](#code)
  - [Additional claims](#additional-claims)
- [Configuration in Startup.Configure](#configuration-in-startupconfigure)
- [Authorization usage](#authorization-usage)
  - [AuthorizeByConvention attribute](#authorizebyconvention-attribute)
  - [AuthorizeWith attribute](#authorizewith-attribute)
- [Login and logout](#login-and-logout)
  - [Login](#login)
  - [Logout](#logout)
- [Additional functionality](#additional-functionality)
  - [Token refresh](#token-refresh)
  - [Automatic token refresh](#automatic-token-refresh)
  - [Permissions endpoint](#permissions-endpoint)
  - [Auth service](#auth-service)
  - [Jwt token in cookie](#jwt-token-in-cookie)
  - [Jwt token in session](#jwt-token-in-session)
  - [Development permissions](#development-permissions)
  - [Disabling Jwt signature validation for testing purposes](#disabling-jwt-signature-validation-for-testing-purposes)
- [How it works](#how-it-works)
  - [Basic auth flow](#basic-auth-flow)
  - [Request flow](#request-flow)
  - [CookieAuth scheme](#cookieauth-scheme)
  - [JwtHeaderAuth scheme](#jwtheaderauth-scheme)
  - [Jwt token](#jwt-token)
    - [Structure](#structure)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->


## Installation

To add the toolbox to a project, you add the package to the csproj project file:

```xml
  <ItemGroup>
    <PackageReference Include="Digipolis.Auth" Version="2.5.0" />
  </ItemGroup>
``` 

or if your project still works with project.json :

``` json 
"dependencies": {
    "Digipolis.Auth":  "2.5.0"
 }
```

ALWAYS check the latest version [here](https://github.com/digipolisantwerp/auth_aspnetcore/blob/master/src/Digipolis.Auth/Digipolis.Auth.csproj) before adding the above line !

In Visual Studio you can also use the NuGet Package Manager to do this.

## Dependencies

The toolbox has a dependency on the IApplicationContext provided by the **Digipolis.ApplicationServices** toolbox that can be found [on github.](https://github.com/digipolisantwerp/application_aspnetcore)

Be sure to configure the ApplicationServices in the **ConfigureServices** before configuring the auth services.! At least the **ApplicationId** must be set to a valid Guid.

``` csharp
    services.AddApplicationServices(setup =>
    {
        setup.ApplicationId = "some valid guid here!";
    });
```


## Configuration in Startup.ConfigureServices

The Auth framework is registered in the ConfigureServices method of the Startup class.

There are 2 ways to configure the Auth framework: using a json config file or using code.

### Json config file

The path to the Json config file has to be given as argument to the AddAuth method, together with the section name where the options are defined:
``` csharp
    services.AddAuth(options =>
    {
        options.BasePath = ConfigPath;
        options.FileName = "auth.json";
        options.Section = "Auth";
    });
```

The Auth framework will read the given section of the json file with the following structure:
``` json
{
  "Auth": {
    "ApplicationName": "TESTAPP",
    "EnableCookieAuth": false,
    "EnableJwtHeaderAuth": true,
    "PdpUrl": "http://pdp.somewhere.com/",
    "PdpApiKey": "some api key",
    "PdpCacheDuration": 240,
    "UseDotnetKeystore": true,
    "DotnetKeystore": "connection string",
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
        options.ApplicationName = "SAMPLEAPP";
        options.ApplicationBaseUrl = "https://theappurl.domain";
        options.EnableCookieAuth = true;
        options.PdpUrl = "http://pdp.somewhere.com/";
        options.PdpApiKey = "some api key";
        options.PdpCacheDuration = 240;
        options.UseDotnetKeystore = true;
        options.DotnetKeystore = "connection string";
        options.JwtAudience = "audience";
        options.JwtIssuer = "JWTIssuer";
    });
```

Following options can be set:

General options:

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
ApplicationName              | The name of the application. Required in order to request permissions to the PDP.|
PdpUrl | The url for the policy decision provider (PDP). |
PdpApiKey | The api key for the PDP endpoint. |
PdpCacheDuration | The duration in minutes the responses from the PDP are cached. Set to zero to disable caching.| 60 
JwtIssuer | The issuer value used to validate the Jwt token.| 
JwtAudience | The audience url used to validate the Jwt token.| 
JwtSigningKeyCacheDuration | The duration in minutes the Jwt signing key is cached.| 1440 (24 hours)
TokenRefreshRoute | The route used for the token refresh endpoint.| "auth/token/refresh"
PermissionsRoute | The route used for the permissions endpoint.| "auth/user/permissions"


Options used for JwtHeaderAuth

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
EnableJwtHeaderAuth              | Set to true to enable the JwtHeaderAuth scheme. (Web Api)| True  

Options used for CookieAuth

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
EnableCookieAuth              | Set to true to enable the CookieAuth scheme. (Web MVC)| False 
ApplicationBaseUrl              | The base url for the application, including scheme and eventual port.|
ApiAuthUrl | The url of the Api Engine authentication endpoint.| 
ApiAuthIdpUrl | The url of the Idp the Api Engine will redirect the saml request to.|
ApiAuthSpName | The service provider name of the Api Engine.|
ApiAuthSpUrl | The Api Engine callback url where the idp must redirect to.|
ApiAuthTokenRefreshUrl | The Api Engine authentication token refresh url.|
ApiAuthTokenLogoutUrl | The Api Engine authentication logout url.|
TokenCallbackRoute | The route used for the token callback url.| "auth/token"
AutomaticTokenRefresh | Set to true to enable automatic token refresh.| false
TokenRefreshTime | The amount of minutes before the jwt token expiration time at which to automatically refresh the token.| 5
AccessDeniedPath | The path to redirect when the access is denied. | 
UseDotnetKeystore | Set to true to use a shared (external) dataprotection key store to store the key used by cookie auth.| 
DotnetKeystore | Connection string for the shared dataprotection key store.| 
AddJwtCookie | Set to true to add the jwt token in a cookie. | True
AddJwtToSession | Set to true to add the jwt token to the Http Session. | False
CookieAuthLifeTime | CookieAuth authentication ticket life time. | 480 (8 hours)
JwtTokenSource | Sets the source for the UserToken property in the AuthService. Set to "session" if jwt token source should be the Http Session (requires Sessions to be enabled and configured). Set to "header" if jwt token source should be the Http Authentication header (requires that the header is set by the requesting party). | "session"


### Additional claims

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

This attribute can only be used with the JwtHeaderAuth scheme (Web Api projects / api controllers).

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

If you want to use this attribute with the **CookieAuth** scheme, the scheme name must be supplied using the **ActiveAuthenticationSchemes** parameter.

``` csharp
    [AuthorizeWith(ActiveAuthenticationSchemes = AuthSchemes.CookieAuth, Permission = "permission-125")]
    public class HomeController : Controller
    {
        //...
    }
```

Both **AuthorizeByConvention** and **AuthorizeWith** attributes are derived from the default **Microsoft.AspNet.Authorization.Authorize** attribute. This means that the way these attributes can be combined is still the same as for the default **Authorize** attribute.

This attribute can be used with both the JwtHeaderAuth and the CookieAuth schemes.
Please note that the default scheme for this attribute is the **JwtHeaderAuth** scheme.

## Login and logout

Login and logout is only applicable to the CookieAuth scheme (Web MVC projects).

### Login

Login is automatically handled when placing an Authorize attribute on a controller or action. If no authorization cookie is present on the request the auth toolbox will return a redirect to start the login procedure on the external identity provider. 

### Logout

In order to logout from the application you can call the **LogoutAsync** method on the **IAuthService** implementation. This will return an url where the browser needs to be redirected to start the logout sequence on the identity provider.
You need to pass the controller and action names where the browser needs to be redirected to after completion of the logout sequence.

``` csharp
    //Example: Logout method on a MVC controller.
    public async Task<IActionResult> Logout([FromServices] IAuthService authService)
    {
        var redirectUrl = await authService.LogOutAsync(ControllerContext, "Home", "Index");
        return Redirect(redirectUrl);
    }
```

## Additional functionality

### Token refresh

When using the **CookieAuth** scheme, the toolbox provides an endpoint where a token can be refreshed. The token must not be expired in order to be able to be refreshed.
The default route for the token refresh endpoint is "auth/token/refresh". The route can be modified by changing the value of the **TokenRefreshRoute** options value.

When calling the endpoint using a GET request, the current token must be passed as query parameter with name "token".

GET auth/token/refresh?token=xxxxx

When the jwt token is stored in session storage (AddJwtToSession = true) it is possible to call the token refresh endpoint without parameters. In this case the token from the session storage will be refreshed when about to expire.

### Automatic token refresh

When using the **CookieAuth** scheme it is possible to enable automatic token refresh.
When enabled, the middleware will check if the token is about to expire and refresh the token when the expiration time is within a certain timespan.
That timespan is 5 minutes by default but can be modified by changing the value of the **TokenRefreshTime** options value.

Please note that the middleware is only running when a call is executed by the client. This means that if no call is executed the token still can expire and cannot be refreshed anymore. This automatic token refresh feature can be seen as a sliding expiration behaviour.

Automatic token refresh is disabled by default. To enable it, set the **AutomaticTokenRefresh** options value to true.


### Permissions endpoint

When using the **JwtHeaderAuth** scheme an endpoint is available to request the users permissions.
The default route for the permissions endpoint is "auth/user/permissions". The route can be modified by changing the value of the **PermissionsRoute** options value.

GET auth/user/permissions

This api endpoint uses the **JwtHeaderAuth** scheme. This requires that you send a valid jwt token in the authorization header for the request.

The response is an array of permissions.

``` json
[
    "login-app",
    "permission-123",
    "permission-456"
]
```

### Auth service

An instance of the **IAuthService** is available through dependency injection.
The **AuthService** allows you to access auth related information.

You can access the **User** object through the **AuthService**.

``` csharp
    //request an instance of the IAuthService in your constructor
    public YourClass(IAuthService authService)
    {
        _authService = authService;
    }
    
    public void SomeMethod()
    {
        //Access the User
        var user = _authService.User;
    }
```

To get the user's jwt token you can read the **UserToken** property.  
The token will only be set if session state is enabled and configured and if the **AddJwtToSession** property in the options is set to **True**.

``` csharp
    //Access the User
    var userToken = _authService.UserToken;
```

### Jwt token in cookie

When using the **CookieAuth** scheme the jwt token received after login can be added in a cookie witk key "jwt".  
Using the **AddJwtCookie** setting in the options this can be turned on or off. By default it is turned on.  
If you don't want the jwt token to be sent to the front end (in a cookie) set the option to **false**.

### Jwt token in session

When using the **CookieAuth** scheme the jwt token received after login can be added to the session.  
Using the **AddJwtToSession** setting in the options this can be turned on or off. By default it is turned on.  
Be sure to enable and configure session state when using this feature.

You can extract the jwt token from the session using the key **"auth-jwt"**

``` csharp
    var token = HttpContext.Session.GetString("auth-jwt");
```

### Development permissions

In order to facilitate development you can use a feature called **Development permissions**. When the application runs in **Development** environment or in the environment specified by the **Environment** property of the **DevPermissionsOptions**, it is possible to bypass the PDP request and get the requested permissions from configuration.
This way it is not needed to set up the permissions in the permissions' management infrastructure (IDP). 

In order to use the development permissions the config file must have a section **DevPermissions** with the **UseDevPermissions** property set to **true**. 
This will only work when the application is running in **Development** environment or in the environment specified by the **Environment** property of the **DevPermissionsOptions**

In the **Development** environment the token lifetimevalidation can be disabled by setting  **ValidateTokenLifetime** to false. This enables easy testing without having to refresh the token.

In the **DevPermissions** section of the config file you can set the permissions. These will be added for every user that logs in to the application.

``` json
    "DevPermissions": {
		"Environment": "Testing",
        "UseDevPermissions": true,
        "ValidateTokenLifetime": false,
        "Permissions": [
            "login-app",
            "permission-125",
            "read-tickets",
    ]
  }
```
### Disabling Jwt signature validation for testing purposes

In order to be able to run integration tests against your application it is possible to disable the signature validation for the used jwt token. This can be done by setting the **RequireSignedTokens** property of the **DevPermissionsOptions** to false.
This allows you to use a dummy jwt token during your integration tests. 
The disabling of the signature validation will only work when the application is running in the environment specified by the **Environment** property of the **DevPermissionsOptions**.

## How it works


### Basic auth flow

![alt text](docs/basicauthflow.png "Flow")

The above picture visualizes the basic authentication and authorization flow.

The first step is to authenticate the user (1).
Depending on the enabled schemes this is done using a jwt token or a cookie.
After authentication, the permissions for the user are set as claims on the user principal. This is done in a claims transformation step (2). 
The permissions for the user are requested to a Policy Decision Point (2a) and can be cached. 
The final step is the authorization for the requested resource. In this step the required permissions for the resource are checked against the permissions from the user.
If the user is not authorized to access the resource the request will return with an Http status code of 401 (UnAuthorized). 
It is still possible to allow resources to anonymous users using the **AllowAnonymous** attribute.
If the user is not authenticated it will result in an empty user principal containing no claims, thus authorization will fail for resources that requires permissions.

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

The scheme relies on an authentication cookie to authenticate the user. If no cookie is present in the request a redirect based flow is initiated to acquire a jwt token from an external token issuer.
Once the token is received and validated, the user is signed in and two cookies are set. The first is an authentication cookie and is the default asp.net cookie. The second is a cookie containing the jwt token.
This cookie named **jwt** can be used on the client side to extract the jwt token needed for Api calls (see jwtHeaderAuth flow).

In order to be able to scale the application, the key used to encrypt the authentication cookie by the DataProtection api, can be stored in a shared database. The connection string to that database is set 
in the **DotnetKeystore** property of the **AuthOptions**. To use this key from shared store the **UseDotnetKeystore** property on the **AuthOptions** must be set to true.

### JwtHeaderAuth scheme

The **JwtHeaderAuth** scheme can be used to secure your Web Api projects.

All api controllers serving data can use this scheme.

The scheme relies on a jwt token present in the authorization header. When a jwt token is present it is validated. 
If the user is not authenticated it will result in a response with http status code 401.
If the user us authenticated but the authorization failed due to missing permissions it will result in a response with http status code 403.

### Jwt token
 
 The toolbox uses a Jwt (JSON Web Token) to perform its authentication.
 For more details on how a Jwt is structured see https://jwt.io/introduction.  
 
#### Structure

 Here is an example of a Jwt of an **authenticated user** (without signature) after base64 decoding:
 
 ``` json
{
  "alg": "RS256",
  "x5u": "http://...",
  "typ": "JWT"
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
The signature from the token is considered to be RSA encrypted with an asymmetric public key.

The signing key used to validate the signature will be acquired from the instance that issued the token.
This is done through a call to an endpoint on the issuer service. The url from that endpoint is provided in the token header with the **x5u** property.

The signing key can also be cached. The duration of the cache can be set using the **JwtSigningKeyCacheDuration** property in configuration. The default is 1440 minutes (24 hours). Use 0 to disable the cache.

#### Using serviceaccount permissions

When using app-to-app communication without authentication of a user (e.g.event processing), the permissions are retrieved using the applicationname. 
The example below shows a JWT token without user authentication. The permissions are assigned to the serviceaccounts for **myapplicationname**.

 ``` json
{
  "nbf": 1508506044,
  "X-Credential-Username": "none",
  "X-Consumer-Groups": "x.x.v1, x.x.v1, x.x.v1, x.x.v1",
  "iss": "https://...",
  "aud": "xxx",
  "X-JWT-Issuer": "https://...",
  "X-Consumer-Username": "int-x.myapplicationname.v1",
  "X-Consumer-Custom-ID": "int-x.myapplicationname.v1",
  "X-Authenticated-Userid": "none",
  "X-Authenticated-Scope": "none",
  "X-Host-Override": "none",
  "iat": 1508506344,
  "jti": "0ce77365c7c8421984317403271f51e5",
  "X-Consumer-ID": "494964f6-0c7a-4d37-b131-6442ad7685ae",
  "exp": 1508722344
}
```