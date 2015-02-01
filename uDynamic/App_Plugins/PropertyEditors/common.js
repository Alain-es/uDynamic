function changeVisibilityItem(scope, item, selected) {
    // Find the item
    var index = scope.items.indexOf(item);
    // Hide/show tabs/properties
    if (index > -1) {
        var tabs = scope.items[index].tabs;
        hideShowTabs(tabs, selected);
        var properties = scope.items[index].properties;
        hideShowProperties(properties, selected);
    }
}

// Hide/show tabs
function hideShowTabs(tabsList, selected) {
    if (tabsList && tabsList !== '') {
        var tabLabels = tabsList.split(",");
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
    }
}

// Hide/show properties
function hideShowProperties(propertiesList, selected) {
    if (propertiesList && propertiesList !== '') {
        var propertyAliases = propertiesList.split(",");
        angular.forEach(propertyAliases, function (value, key) {
            // Remove the first charater which contains the action (+ show, - Hide, * readonly)
            var propertyAlias = value.substring(1, value.length);
            var action = value.charAt(0);
            // Look for the property div
            var propertyControls = $("div[class*='umb-property']:has(ng-form)");
            // Show/hide the control
            angular.forEach(propertyControls, function (value, key) {
                if ($(value).find(".control-label").attr("for") == propertyAlias) {
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
