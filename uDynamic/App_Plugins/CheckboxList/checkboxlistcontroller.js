angular.module("umbraco").controller("uDynamic.CheckboxListController", function ($scope, $timeout, assetsService) {

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

    // Remove from the model any previously selected item that doesn't exist in the list (it happens when users change the prevalues)
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
    }, 0);


    // Change visibility/state of the tabs and properties for the checked/unchecked item
    $scope.changeVisibilityForSelection = function changeVisibilityForSelection(item) {
        var index = $scope.model.value.indexOf(item.key);
        if (index > -1) {
            // Is currently selected
            $scope.model.value.splice(index, 1);
            changeVisibilityItem(item, false);
        }
        else {
            // Is newly selected
            $scope.model.value.push(item.key);
            changeVisibilityItem(item, true);
        }
    };

    function changeVisibilityAllItems() {
        angular.forEach($scope.items, function (value, key) {
            var index = $scope.model.value.indexOf(value.key);
            if (index > -1) {
                // Is currently selected
                changeVisibilityItem(value, true);
            }
            else {
                // Is newly selected
                changeVisibilityItem(value, false);
            }
        });
    }

    function changeVisibilityItem(item, selected) {

        // Find the item
        var index = $scope.items.indexOf(item);

        // Hide/show tabs
        var tabs = $scope.items[index].tabs;
        if (tabs && tabs !== '') {
            var tabLabels = tabs.split(",");
            angular.forEach(tabLabels, function (value, key) {
                // Remove the first charater which contains the action (+ show, - Hide)
                var tabLabel = value.substring(1, value.length);
                var action = value.charAt(0);
                // Look for the tab control
                var tabControls = $("a[href^='#tab']");
                // Show/hide the control
                angular.forEach(tabControls, function (value, key) {
                    if (value.text == tabLabel) {
                        switch (action) {
                            case '+':
                                if (selected) {
                                    $(value).show();
    }
                                else {
                                    $(value).hide();
                                }
                                break;
                            case '-':
                                if (selected) {
                                    $(value).hide();
                                }
                                else {
                                    $(value).show();
                                }
                                break;
                            default:
                        }
                    }
                });
            });

            // Hide/show properties
            var properties = $scope.items[index].properties;
            if (properties && properties !== '') {
                var propertyLabels = properties.split(",");
                angular.forEach(propertyLabels, function (value, key) {
                    // Remove the first charater which contains the action (+ show, - Hide, * readonly)
                    var propertyLabel = value.substring(1, value.length);
                    var action = value.charAt(0);
                    // Look for the property div
                    var propertyControls = $("div[class*='umb-property']:has(ng-form)");
                    // Show/hide the control
                    angular.forEach(propertyControls, function (value, key) {
                        if ($(value).find(".control-label").attr("for") == propertyLabel) {
                            switch (action) {
                                case '+':
                                    if (selected) {
                                        $(value).show();
                                    }
                                    else {
                                        $(value).hide();
                                    }
                                    break;
                                case '-':
                                    if (selected) {
                                        $(value).hide();
                                    }
                                    else {
                                        $(value).show();
                                    }
                                    break;
                                case '*':
                                    break;
                                default:
                            }
                        }
                    });
                });

            }
        }
    }

});