(function () {

    var ticketsService = function ($http) {

        var apiURL = "api/tickets";
        var reservations = [];

        var readTicket = function () {
            return $http.get(apiURL)
        };

        var createTicket = function () {
            return $http.post(apiURL, {});
        };

        var updateTicket = function () {
            return $http.put(apiURL, {});
        };

        var deleteTicket = function () {
            return $http.delete(apiURL);
        };

        var getActionWithCustomPermission = function () {
            return $http.get(apiURL + '/' + 'getActionWithCustomPermission');
        };

        var allowAnonymous = function () {
            return $http.get(apiURL + '/' + 'allowAnonymous');
        };

        return {
            readTicket: readTicket,
            createTicket: createTicket,
            updateTicket: updateTicket,
            deleteTicket: deleteTicket,
            getActionWithCustomPermission: getActionWithCustomPermission,
            allowAnonymous: allowAnonymous
        };
    };

    angular.module('app').factory('ticketsService', ticketsService);

})();