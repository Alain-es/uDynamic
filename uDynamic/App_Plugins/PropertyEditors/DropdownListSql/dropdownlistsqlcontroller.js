angular.module("umbraco").controller("uDynamic.DropdownListControllerSql",

function ($scope, $q, $timeout, assetsService, notificationsService, uDynamicResource) {

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
            $scope.model.value = "";
        }

        //uDynamicResource.getDropdownListSqlItems($scope.model.config.dbTableName, $scope.model.config.dbTextColumnName, $scope.model.config.dbKeyColumnName).then(
        uDynamicResource.getDropdownListSqlItems($scope.model.config.sqlCommand, $scope.model.config.keyColumnName, $scope.model.config.textColumnName, $scope.model.config.tabsColumnName, $scope.model.config.propertiesColumnName, $scope.model.config.tableName, $scope.model.config.cacheDuration).then(
            function (response) {

                var items = response.data;

                // Remove any emtpy item from the list
                items = items.filter(function (item) {
                    return item.key !== "" && item.text !== "";
                });

                // Populate the dropdown list
                $scope.items = items;

                $scope.isLoaded = true;

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
                        // Check whether it is the currently selected value
                        if ($scope.model && $scope.model.value == value.key) {
                            changeVisibilityItem($scope, value, true);
                        }
                        else {
                            changeVisibilityItem($scope, value, false);
                        }
                    });
                }

            }, function (error) {
                notificationsService.error("Error", "Error loading dropdown list items");
                console.log(error);
            });


    });

});
