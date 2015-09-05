var LandingPageController = function($scope) {
    $scope.models = {
        helloAngular: 'I work!'
    };

    $scope.navbarProperties = {
        isCollapsed: true
    };

    $scope.gridOptions = {};
    $scope.gridOptions.enableCellEditOnFocus = true;

    $scope.gridOptions.columnDefs = [
      { name: 'firstName', enableCellEdit: true },
      { name: 'lastName', displayName: 'Name (editOnFocus)', enableCellEdit: true },
      { name: 'company', enableCellEditOnFocus: true },
      //{ name: 'employed', enableCellEdit: true }
      { name: 'employed',  enableCellEdit: false, cellTemplate: '<input type="checkbox" value="employed"/>{{MODEL_COL_FIELD}}' }
    ];

    $scope.myData = [
    {
        "firstName": "Cox",
        "lastName": "Carney",
        "company": "Enormo",
        "employed": true
    },
    {
        "firstName": "Lorraine",
        "lastName": "Wise",
        "company": "Comveyer",
        "employed": false
    },
    {
        "firstName": "Nancy",
        "lastName": "Waters",
        "company": "Fuelton",
        "employed": false
    }
    ];

    $scope.gridOptions.data = $scope.myData;
};

LandingPageController.$inject = ['$scope'];