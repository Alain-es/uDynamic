angular.module("umbraco").controller("uDynamic.DropdownListController",

function ($scope, $q, $timeout, assetsService) {

    $scope.isLoaded = false;

    var await = [];

    // Check whether to Apply the Chosen jquery plugin
    if ($scope.model.config.useChosen == 1) {
        // Check whether the Chosen library is already loaded before loading it
        if (typeof chosen === "undefined") { // Don't reload the chosen library if it is already loaded
            await.push(assetsService.loadJs('/App_Plugins/uDynamic/ThirdParty/chosen/chosen.jquery.min.js', $scope));
            await.push(assetsService.loadCss('/App_Plugins/uDynamic/ThirdParty/chosen/chosen.min.css', $scope));
        }
    }

    await.push(assetsService.loadJs('/App_Plugins/uDynamic/PropertyEditors/common.js', $scope));

    // Wait for queue to end
    $q.all(await).then(function () {

        // Check whether the model is initialized
        if (!$scope.model.value) {
            $scope.model.value = [];
        }
            // Backward compatibility with older versions 
        else if (!angular.isArray($scope.model.value)) {
            var value = $scope.model.value;
            $scope.model.value = [];
            $scope.model.value.push(value);
        }

        // Used to bind the single value dropdown with the first item of the array
        $scope.modelValueFirstItem = $scope.model.value[0];

        // Remove any emtpy item from the list
        $scope.model.config.items.items = $scope.model.config.items.items.filter(function (item) {
            return item.key !== "" && item.text !== "";
        });

        // Populate the dropdown list
        $scope.items = $scope.model.config.items.items;

        // Change visibility/state of the tabs and properties depending on the dropdown list initial values
        $timeout(function () {
            changeVisibilityAllItems();
            $scope.isLoaded = true;
        }, 0);


        // Item click
        $scope.click = function click() {
            changeVisibilityAllItems();
        };

        // Check whether to Apply the Chosen jquery plugin
        if ($scope.model.config.useChosen == 1) {
            $timeout(function () {
                $("#" + $scope.model.alias).chosen({ width: "95%" });
            }, 0);
        }

        function changeVisibilityAllItems() {
            angular.forEach($scope.items, function (value, key) {
                // Check whether it is a currently selected value
                if ($scope.model && $scope.model.value == value.key) {
                    changeVisibilityItem($scope, value, true);
                }
                else {
                    changeVisibilityItem($scope, value, false);
                }
            });
        }

    });

});
