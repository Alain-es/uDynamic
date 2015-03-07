using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Caching;
using System.Text.RegularExpressions;

using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Core.Logging;

using uDynamic.Models;

namespace uDynamic.Controllers.Api
{
    [PluginController("uDynamic")]
    [IsBackOffice]
    public class uDynamicApiController : UmbracoAuthorizedJsonController
    {

        /// <summary>
        /// [Depracated]
        /// Retrieve all dropdown list items
        /// </summary>
        [System.Web.Http.HttpGet]
        public IEnumerable<DropdownListItem> GetDropdownListSqlItems(string dbTableName, string dbTextColumnName, string dbKeyColumnName)
        {
            return GetDropdownListSqlItems(string.Empty, dbKeyColumnName, dbTextColumnName, string.Empty, string.Empty, string.Empty, 0);
        }

        /// <summary>
        /// Retrieve all dropdown list items
        /// </summary>
        [System.Web.Http.HttpGet]
        public IEnumerable<DropdownListItem> GetDropdownListSqlItems(string sqlCommand, string keyColumnName, string textColumnName, string tabsColumnName, string propertiesColumnName, string tableName, int cacheDuration)
        {
            List<DropdownListItem> result = new List<DropdownListItem>();

            if (string.IsNullOrWhiteSpace(sqlCommand) && string.IsNullOrWhiteSpace(tableName))
                return result;

            if (string.IsNullOrWhiteSpace(sqlCommand))
            {
                if (string.IsNullOrWhiteSpace(keyColumnName) || string.IsNullOrWhiteSpace(textColumnName))
                {
                    return result;
                }

                // Create the SQL command using the table and column names
                string selectClause = string.Format("{0}, {1}", keyColumnName, textColumnName);
                if (!string.IsNullOrWhiteSpace(tabsColumnName))
                {
                    selectClause = string.Format("{0}, {1}", selectClause, tabsColumnName);
                }
                if (!string.IsNullOrWhiteSpace(propertiesColumnName))
                {
                    selectClause = string.Format("{0}, {1}", selectClause, propertiesColumnName);
                }

                sqlCommand = string.Format("SELECT {0} FROM {1}", selectClause, tableName);
            }

            // Check whether is is cached
            string cacheId = string.Format(uDynamic.Constants.Cache.IdPattern.SqlCommandResult, sqlCommand);
            if (cacheDuration > 0)
            {
                var cachedResult = HttpContext.Current.Cache[cacheId] as List<DropdownListItem>;
                if (cachedResult != null)
                {
                    return cachedResult;
                }
            }

            try
            {
                var sqlDataReader = Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteReader(ApplicationContext.DatabaseContext.ConnectionString, System.Data.CommandType.Text, sqlCommand);
                while (sqlDataReader.Read())
                {
                    DropdownListItem item = new DropdownListItem();
                    if (!string.IsNullOrWhiteSpace(keyColumnName))
                    {
                        item.key = sqlDataReader[keyColumnName].ToString();
                    }
                    else
                    {
                        item.key = sqlDataReader.GetValue(0).ToString();
                    }
                    if (!string.IsNullOrWhiteSpace(textColumnName))
                    {
                        item.text = sqlDataReader[textColumnName].ToString();
                    }
                    else
                    {
                        item.text = sqlDataReader.GetValue(1).ToString();
                    }
                    if (!string.IsNullOrWhiteSpace(tabsColumnName))
                    {
                        item.tabs = sqlDataReader[tabsColumnName].ToString();
                    }
                    else
                    {
                        if (sqlDataReader.FieldCount > 2)
                        {
                            item.tabs = sqlDataReader.GetValue(2).ToString();
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(propertiesColumnName))
                    {
                        item.properties = sqlDataReader[propertiesColumnName].ToString();
                    }
                    else
                    {
                        if (sqlDataReader.FieldCount > 3)
                        {
                            item.properties = sqlDataReader.GetValue(3).ToString();
                        }
                    }
                    result.Add(item);
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.Error<uDynamicApiController>(string.Format("Error executing the SQL command '{0}'.", sqlCommand), ex);
            }

            // Cache the result
            if (cacheDuration > 0)
            {
                HttpContext.Current.Cache.Add(cacheId, result, null, DateTime.Now.AddSeconds(cacheDuration), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, null);
            }

            return result;
        }
    }
}




