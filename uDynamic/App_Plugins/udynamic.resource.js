﻿angular.module('umbraco.resources').factory('uDynamicResource', function ($q, $http) {
    return {

        getDropdownListSqlItems: function (sqlCommand, keyColumnName, textColumnName, tabsColumnName, propertiesColumnName, tableName, cacheDuration) {
            if (!sqlCommand) sqlCommand = "";
            if (!keyColumnName) keyColumnName = "";
            if (!textColumnName) textColumnName = "";
            if (!tabsColumnName) tabsColumnName = "";
            if (!propertiesColumnName) propertiesColumnName = "";
            if (!tableName) tableName = "";
            if (!cacheDuration) cacheDuration = 0;
            return $http.get("backoffice/uDynamic/uDynamicApi/GetDropdownListSqlItems", {
                params: { sqlCommand: sqlCommand, keyColumnName: keyColumnName, textColumnName: textColumnName, tabsColumnName: tabsColumnName, propertiesColumnName: propertiesColumnName, tableName: tableName, cacheDuration: cacheDuration }
            });
        }

    };
})

