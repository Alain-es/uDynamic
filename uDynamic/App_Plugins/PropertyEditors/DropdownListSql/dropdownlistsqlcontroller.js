angular.module("umbraco").controller("uDynamic.DropdownListSqlController",

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
            if ($scope.model.config.multiple == 1) {
                $scope.model.value = [];
            }
            else {
                $scope.model.value = "";
            }
        }
        else if (!angular.isArray($scope.model.value)) {
            if ($scope.model.config.multiple == 1) {
                var value = $scope.model.value;
                $scope.model.value = [];
                $scope.model.value.push(value);
            }
        }
        else if ($scope.model.config.multiple != 1) {
            $scope.model.value = $scope.model.value[0];
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

                // Add an empty item in order to allow users to deselect when the 'chosen' option is ticked
                if ($scope.model.config.useChosen == 1 && $scope.model.config.multiple != 1) {
                    items.push({ key: '', text: '' });
                }

                // Populate the dropdown list
                $scope.items = items;

                // Remove from the model any previously selected item that doesn't exist in the list anymore
                if ($scope.model.config.multiple == 1) {
                    var onlyValidSelectedItems = [];
                    angular.forEach($scope.items, function (value, key) {
                        var index = $scope.model.value.indexOf(value.key);
                        if (index > -1) {
                            onlyValidSelectedItems.push(value.key);
                        }
                    });
                    $scope.model.value = onlyValidSelectedItems;
                }
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
                        $("#" + $scope.model.alias).chosen({ width: "95%", allow_single_deselect: true });
                    }, 0);
                }

                function changeVisibilityAllItems() {
                    var currentlySelectedItemValue = null;
                    angular.forEach($scope.items, function (value, key) {
                        // Check whether it is a currently selected value
                        if ($scope.model && $scope.model.value == value.key) {
                            currentlySelectedItemValue = value;
                        }
                        else {
                            changeVisibilityItem($scope, value, false);
                        }
                    });
                    if (currentlySelectedItemValue) {
                        changeVisibilityItem($scope, value, true);
                    }
                }

            }, function (error) {
                notificationsService.error("Error", "Error loading dropdown list items");
                console.log(error);
            });


    });

});
