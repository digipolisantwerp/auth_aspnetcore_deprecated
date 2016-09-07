(function () {

    var authService = function ($http, $cookies, $timeout, $q, utilsService) {

        var apiURL = "myroute/token";
        var userPermissions = [];
        var permissionsLoaded = false;

        // ******** Init ********

        function Init() {
            //setupTokenRefresh();
        };

        Init();

        // ******** Public ********

        var getToken = function () {
            return $cookies.get("jwt");
        };

        var refreshToken = function () {

            var token = $cookies.get("jwt");

            getTokenExpiration(token);

            return $http.get(apiURL + "/refresh?token=" + token)
                 .then(function (response) {
                     $cookies.put("jwt", response.data);
                     setupTokenRefresh();
                 }, function (error) {

                 });
        };

        var getUserPermissions = function () {
            var deferred = $q.defer();

            if (permissionsLoaded) {
                deferred.resolve(userPermissions);
            } else {

                $http.get("myauth/user/permissions")
                    .then(function (response) {
                        userPermissions = response.data;
                        permissionsLoaded = true;
                        deferred.resolve(userPermissions);
                    });
            }
            return deferred.promise;
        };

        var isUserAuthorized = function (requiredPermission) {

            return getUserPermissions()
                .then(function () {
                    return userHasPermission(requiredPermission);
                });
        };

        return {
            getToken: getToken,
            refreshToken: refreshToken,
            getUserPermissions: getUserPermissions,
            isUserAuthorized: isUserAuthorized
        };

        // ******** Private ********

        function userHasPermission(requiredPermission) {
            var permission = userPermissions.find(function (p) {
                return p == requiredPermission;
            });
            return permission != undefined;
        };

        function setupTokenRefresh() {

            var delay = 0;   //miliseconds
            var tokenRefreshTime = 5;       //minutes before token expiration at which to refresh

            var token = $cookies.get("jwt");
            var tokenExpiration = getTokenExpiration(token) * 1000;
            var now = new Date().getTime();
            var refreshTime = tokenExpiration - (tokenRefreshTime * 60000);

            delay = refreshTime - now;

            $timeout(function () {
                refreshToken();
            }, delay);
        };

        function getTokenExpiration(token) {
            var tokenSubs = token.split('.');
            var payload = tokenSubs[1];
            var payloadData = utilsService.base64Decode(payload);
            var claims = JSON.parse(payloadData);
            var exp = claims.exp;
            var expDate = new Date(exp * 1000);
            return exp;
        };
    };

    angular.module('app').factory('authService', authService);

})();