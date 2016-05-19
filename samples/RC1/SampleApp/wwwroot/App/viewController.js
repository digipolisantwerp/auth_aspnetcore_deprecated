(function () {

    function viewController($scope, ticketsService, personsService, authService) {

        var vm = this;
        vm.showPersonActions = false;

        var init = function () {
            vm.token = authService.getToken();
            
            authService.isUserAuthorized("permission-125")
                .then(function (result) {
                    vm.showPersonActions = result;
                });
        };

        init();

         function clearResults () {
            vm.success = '';
            vm.error = '';
        };

         function setSuccess (msg) {
            vm.success = msg;
        };

         function setError (msg) {
            vm.error = msg;
        };

        // **** TICKETS ****
        
        vm.readTicket = function () {
            clearResults();
            ticketsService.readTicket()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        vm.createTicket = function () {
            clearResults();
            ticketsService.createTicket()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        vm.updateTicket = function () {
            clearResults();
            ticketsService.updateTicket()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        vm.deleteTicket = function () {
            clearResults();
            ticketsService.deleteTicket()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        vm.getActionWithCustomPermission = function () {
            clearResults();
            ticketsService.getActionWithCustomPermission()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        vm.allowAnonymous = function () {
            clearResults();
            ticketsService.allowAnonymous()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        // **** PERSON ****

        vm.getPerson = function () {
            clearResults();
            personsService.getPerson()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        vm.createPerson = function () {
            clearResults();
            personsService.createPerson()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        vm.updatePerson = function () {
            clearResults();
            personsService.updatePerson()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        vm.deletePerson = function () {
            clearResults();
            personsService.deletePerson()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        vm.allowAnonymousPerson = function () {
            clearResults();
            personsService.allowAnonymous()
                .then(function (response) {
                    setSuccess(response.data);
                }, function (error) {
                    setError(error.statusText);
                });
        };

        // **** TOKEN ****

        vm.clearToken = function () {
            vm.token = "";
        };
        
        vm.refreshToken = function () {

            vm.token = "";

            authService.refreshToken()
                .then(function () {
                    vm.token = authService.getToken();
                });
        };
    };

    angular.module('app').controller('viewController', viewController);

})();