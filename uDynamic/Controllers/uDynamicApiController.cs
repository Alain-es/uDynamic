using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Caching; // Requires a reference to System.Runtime.Caching.dll
using System.Text.RegularExpressions;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

using uDynamic.Models;

namespace uDynamic.Controllers
{
    [PluginController("uDynamic")]
    [IsBackOffice]
    public class uDynamicApiController : UmbracoAuthorizedJsonController
    {

        /// <summary>
        /// Retrieve all list items from the database
        /// </summary>
        [System.Web.Http.HttpGet]
        public IEnumerable<ListItem> GetSqlListItems(string sqlCommand = "", string keyColumnName = "", string textColumnName = "", string tabsColumnName = "", string propertiesColumnName = "", int cacheDuration = 0)
        {
            return uDynamicController.GetSqlListItems(sqlCommand, keyColumnName, textColumnName, tabsColumnName, propertiesColumnName, cacheDuration);
        }
    }


    public class uDynamicController
    {
        /// <summary>
        /// Retrieve all list items from the database
        /// </summary>
        public static IEnumerable<ListItem> GetSqlListItems(string sqlCommand = "", string keyColumnName = "", string textColumnName = "", string tabsColumnName = "", string propertiesColumnName = "", int cacheDuration = 0)
        {
            List<ListItem> result = new List<ListItem>();

            if (string.IsNullOrWhiteSpace(sqlCommand))
                return result;

            // Check whether is is cached
            string cacheId = string.Format(uDynamic.Constants.Cache.IdPattern.SqlCommandResult, sqlCommand);
            if (cacheDuration > 0)
            {
                var cachedResult = MemoryCache.Default[cacheId] as List<ListItem>;
                if (cachedResult != null && cachedResult.Count() > 0)
                {
                    return cachedResult;
                }
            }

            try
            {
                using (var sqlDataReader = Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteReader(ApplicationContext.Current.DatabaseContext.ConnectionString, System.Data.CommandType.Text, sqlCommand))
                {
                    while (sqlDataReader.Read())
                    {
                        ListItem dropdownListItem = new ListItem();

                        // Key
                        var column = new Column();
                        if (!string.IsNullOrWhiteSpace(keyColumnName))
                        {
                            column.columnName = keyColumnName;
                            column.columnValue = sqlDataReader[keyColumnName].ToString();
                        }
                        else
                        {
                            column.columnName = sqlDataReader.GetName(0);
                            column.columnValue = sqlDataReader.GetValue(0).ToString();
                        }
                        dropdownListItem.columns.Add(0, column);

                        // Text
                        column = new Column();
                        if (!string.IsNullOrWhiteSpace(textColumnName))
                        {
                            column.columnName = textColumnName;
                            column.columnValue = sqlDataReader[textColumnName].ToString();
                        }
                        else
                        {
                            column.columnName = sqlDataReader.GetName(1);
                            column.columnValue = sqlDataReader.GetValue(1).ToString();
                        }
                        dropdownListItem.columns.Add(1, column);

                        // Tabs
                        column = new Column();
                        if (!string.IsNullOrWhiteSpace(tabsColumnName))
                        {
                            column.columnName = tabsColumnName;
                            column.columnValue = sqlDataReader[tabsColumnName].ToString();
                            dropdownListItem.columns.Add(2, column);
                        }
                        else
                        {
                            column.columnName = string.Empty;
                            column.columnValue = string.Empty;
                            dropdownListItem.columns.Add(2, column);
                        }

                        // Properties
                        column = new Column();
                        if (!string.IsNullOrWhiteSpace(propertiesColumnName))
                        {
                            column.columnName = propertiesColumnName;
                            column.columnValue = sqlDataReader[propertiesColumnName].ToString();
                            dropdownListItem.columns.Add(3, column);
                        }
                        else
                        {
                            column.columnName = string.Empty;
                            column.columnValue = string.Empty;
                            dropdownListItem.columns.Add(3, column);
                        }

                        // Other columns
                        for (int i = 0; i < sqlDataReader.FieldCount; i++)
                        {
                            // Check whether the column was already added
                            var columnName = sqlDataReader.GetName(i);
                            if (dropdownListItem.columns.Where(c => c.Value.columnName.InvariantEquals(columnName)).Count() > 0)
                            {
                                continue;
                            }
                            column = new Column();
                            column.columnName = columnName;
                            column.columnValue = sqlDataReader.GetValue(i).ToString();
                            dropdownListItem.columns.Add(dropdownListItem.columns.Count, column);
                        }

                        result.Add(dropdownListItem);
                    }
                    sqlDataReader.Close();
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.Error<uDynamicController>(string.Format("Error executing the SQL command '{0}'.", sqlCommand), ex);
            }

            // Cache the result
            if (cacheDuration > 0 && result != null && result.Count() > 0)
            {
                MemoryCache.Default.Add(cacheId, result, new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(cacheDuration) });
            }

            return result;
        }
    }
}




