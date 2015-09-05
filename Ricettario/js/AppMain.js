var AppMain = angular.module('AppMain', ['ngRoute', 'ui.bootstrap', 'ui.grid', 'ui.grid.edit', 'ui.grid.cellNav']);

AppMain.controller('LandingPageController', LandingPageController);
AppMain.controller('ApiController', ApiController);

var configFunction = function($routeProvider) {
    $routeProvider.
        when('/routeOne', {
            templateUrl: 'routesDemo/one'
        })
        .when('/routeTwo/:donuts', {
            templateUrl: function (params) { return 'RoutesDemo/Two?donuts=' + params.donuts; }
        })
        .when('/routeThree', {
            templateUrl: 'poducts.html'
        });
};
configFunction.$inject = ['$routeProvider'];

AppMain.config(['$routeProvider', configFunction]);	