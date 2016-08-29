(function () {

    'use strict';

    var interceptorName = "AuthInterceptor";

    theInterceptor.$inject = ['$q', '$cookies'];

    angular.module('app').factory(interceptorName, theInterceptor)
           .config(function ($httpProvider) {
               $httpProvider.interceptors.push('AuthInterceptor');
           });

    function theInterceptor($q, $cookies) {

        function _request(config) {

            config.headers = config.headers || {};

            //var jwt = $cookies.get("jwt");
            var jwt = $("#jwttoken").val();
            if (jwt) {
                config.headers.Authorization = 'Bearer ' + jwt;
            }

            return config;
        };

        return {
            request: _request
        };

    };

})();

