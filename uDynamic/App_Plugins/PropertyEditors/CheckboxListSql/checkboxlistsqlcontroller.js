angular.module("umbraco").controller("uDynamic.CheckboxListSqlController", function ($scope, $q, $timeout, assetsService, notificationsService, uDynamicResource) {

    $scope.isLoaded = false;

    var await = [];
    await.push(assetsService.loadJs('/App_Plugins/uDynamic/PropertyEditors/common.js', $scope));

    // Wait for queue to end
    $q.all(await).then(function () {

        // Check whether the model is initialized
        if (!$scope.model.value) {
            $scope.model.value = [];
        }

        uDynamicResource.getSqlListItems($scope.model.config.sqlCommand, $scope.model.config.keyColumnName, $scope.model.config.textColumnName, $scope.model.config.tabsColumnName, $scope.model.config.propertiesColumnName, $scope.model.config.cacheDuration).then(
            function (response) {

                // Map the columns with the values retrieved from the database into the same model used for the other uDynamic property editors (prevalues)
                var items = _.map(response.data, function (value, key) {
                    var item = {};
                    item.key = value.columns[0].columnValue;
                    item.text = value.columns[1].columnValue;
                    if (value.columns.length > 2)
                        item.tabs = value.columns[2].columnValue;
                    else
                        item.tabs = '';
                    if (value.columns.length > 3)
                        item.properties = value.columns[3].columnValue;
                    else
                        item.properties = '';
                    return item;
                });

                // Remove any emtpy item from the list
                items = items.filter(function (item) {
                    return item.key !== "" && item.text !== "";
                });

                // Populate the checkbox list
                $scope.items = items;

                // Remove from the model any previously selected item that doesn't exist in the list anymore
                var onlyValidSelectedItems = [];
                angular.forEach($scope.items, function (value, key) {
                    var index = $scope.model.value.indexOf(value.key);
                    if (index > -1) {
                        onlyValidSelectedItems.push(value.key);
                    }
                });
                $scope.model.value = onlyValidSelectedItems;

                $scope.isLoaded = true;


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

            }, function (error) {
                notificationsService.error("Error", "Error loading dropdown list items");
                console.log(error);
            });

    });

});