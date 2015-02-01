angular.module('umbraco.resources').factory('uDynamicResource', function ($q, $http) {
    return {

        getDropdownListSqlItems: function (dbTableName, dbTextColumnName, dbKeyColumnName) {
            return $http.get("backoffice/uDynamic/uDynamicApi/GetDropdownListSqlItems", {
                params: { dbTableName: dbTableName, dbTextColumnName: dbTextColumnName, dbKeyColumnName: dbKeyColumnName }
            });
        }

    };
})

