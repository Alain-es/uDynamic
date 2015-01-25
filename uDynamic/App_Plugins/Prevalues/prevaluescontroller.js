﻿angular.module("umbraco")
    .controller("uDynamic.prevaluesController",
    function ($scope, $timeout, assetsService) {

        // Initialize the model
        if (!$scope.model.value) {
            $scope.model.value = {
                items: []
            }
        }

        // Load the css file with the grid's styles
        assetsService.loadCss("/App_Plugins/uDynamic/Prevalues/prevalues.css");

        // Add a new item
        $scope.addItem = function () {
            $scope.model.value.items.push({
                text: "",
                key: "",
                tabs: "",
                properties: ""
            });
        }

        // Remove an item
        $scope.removeItem = function (index) {
            $scope.model.value.items.splice(index, 1);
        }

        // Makes the grid sortable
        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".sortHandle",
            start: function (event, ui) {
                var curTH = ui.helper.closest("table").find("thead").find("tr");
                var itemTds = ui.item.children("td");
                curTH.find("th").each(function (ind, obj) {
                    itemTds.eq(ind).width($(obj).width());
                });
            },
            update: function (ev, ui) {
            }
        };

    });