angular.module("umbraco").controller("uDynamic.CheckboxListController", function ($scope, $q, $timeout, assetsService) {

    $scope.isLoaded = false;

    var await = [];
    await.push(assetsService.loadJs('/App_Plugins/uDynamic/PropertyEditors/common.js', $scope));

    // Wait for queue to end
    $q.all(await).then(function () {

        // Check whether the model is initialized
        if (!$scope.model.value) {
            $scope.model.value = [];
        }

        // Remove any emtpy item from the list
        $scope.model.config.items.items = $scope.model.config.items.items.filter(function (item) {
            return item.key !== "" && item.text !== "";
        });

        // Populate the checkbox list
        $scope.items = $scope.model.config.items.items;

        // Remove from the model any previously selected item that doesn't exist in the list (it happens when prevalues have been modified)
        var onlyValidSelectedItems = [];
        angular.forEach($scope.model.config.items.items, function (value, key) {
            var index = $scope.model.value.indexOf(value.key);
            if (index > -1) {
                onlyValidSelectedItems.push(value.key);
            }
        });
        $scope.model.value = onlyValidSelectedItems;

        // Change visibility/state of the tabs and properties depending on the checkbox list initial values
        $timeout(function () {
            changeVisibilityAllItems();
            $scope.isLoaded = true;
        }, 0);


        // Item click
        $scope.click = function click(item) {
            var index = $scope.model.value.indexOf(item.key);
            if (index > -1) {
                // Is currently selected
                $scope.model.value.splice(index, 1);
                changeVisibilityItem($scope, item, false);
            }
            else {
                // Is newly selected
                $scope.model.value.push(item.key);
                changeVisibilityItem($scope, item, true);
            }
        };

        function changeVisibilityAllItems() {
            angular.forEach($scope.items, function (value, key) {
                var index = $scope.model.value.indexOf(value.key);
                if (index > -1) {
                    // Is currently selected
                    changeVisibilityItem($scope, value, true);
                }
                else {
                    // Is newly selected
                    changeVisibilityItem($scope, value, false);
                }
            });
        }

    });
});