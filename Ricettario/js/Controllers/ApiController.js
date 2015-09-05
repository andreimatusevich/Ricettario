var ApiController = function ($scope, $http) {
    $http.get('/Api/Products').success(function (data) {
        $scope.products = data;
    });

    $scope.debugPrint = function (val) {
        val.Label = val.Id + ' - ' + val.Name;
        val.Text = angular.toJson(val, true);
    };
};

ApiController.$inject = ['$scope', '$http'];