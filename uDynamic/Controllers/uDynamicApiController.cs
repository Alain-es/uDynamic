using System.Linq;
using System.Collections.Generic;
using System.Net.Http;

using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

using uDynamic.Models;

namespace uDynamic.Controllers.Api
{
    [PluginController("uDynamic")]
    [IsBackOffice]
    public class uDynamicApiController : UmbracoAuthorizedJsonController
    {

        /// <summary>
        /// Retrieve all dropdown list items
        /// </summary>
        [System.Web.Http.HttpGet]
        public IEnumerable<DropdownListItem> GetDropdownListSqlItems(string dbTableName, string dbTextColumnName, string dbKeyColumnName)
        {
            // TODO: Make this code SQL injection safe
            string sqlCommand = string.Format("SELECT {0}, {1} FROM {2}", dbTextColumnName, dbKeyColumnName, dbTableName);
            var sqlDataReader = Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteReader(ApplicationContext.DatabaseContext.ConnectionString, System.Data.CommandType.Text, sqlCommand);
            List<DropdownListItem> result = new List<DropdownListItem>();
            while (sqlDataReader.Read())
            {
                DropdownListItem item = new DropdownListItem();
                item.text = sqlDataReader.GetValue(0).ToString();
                item.key = sqlDataReader.GetValue(1).ToString();
                result.Add(item);
            }
            return result;
        }
    }
}




