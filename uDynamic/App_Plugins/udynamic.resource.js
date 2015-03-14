angular.module('umbraco.resources').factory('uDynamicResource', function ($q, $http) {
    return {

        getSqlListItems: function (sqlCommand, keyColumnName, textColumnName, tabsColumnName, propertiesColumnName, cacheDuration) {
            return $http.get("backoffice/uDynamic/uDynamicApi/GetSqlListItems", {
                cache: true,
                params: { sqlCommand: sqlCommand, keyColumnName: keyColumnName, textColumnName: textColumnName, tabsColumnName: tabsColumnName, propertiesColumnName: propertiesColumnName, cacheDuration: cacheDuration }
            });
        }

    };
})

