(function () {

    var personsService = function ($http) {

        var apiURL = "api/persons";
        var reservations = [];

        var getPerson = function () {
            return $http.get(apiURL)
        };

        var createPerson = function () {
            return $http.post(apiURL, {});
        };

        var updatePerson = function () {
            return $http.put(apiURL, {});
        };

        var deletePerson = function () {
            return $http.delete(apiURL);
        };

        var allowAnonymous = function () {
            return $http.get(apiURL + '/' + 'allowAnonymous');
        };

        return {
            getPerson: getPerson,
            createPerson: createPerson,
            updatePerson: updatePerson,
            deletePerson: deletePerson,
            allowAnonymous: allowAnonymous
        };
    };
    
    angular.module('app').factory('personsService', personsService);

})();