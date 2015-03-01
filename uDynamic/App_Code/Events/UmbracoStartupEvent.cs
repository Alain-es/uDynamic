using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Examine;
using Examine.SearchCriteria;
using Examine.Providers;
using Examine.LuceneEngine;
using Examine.Config;
using UmbracoExamine;

using uDynamic.EmbeddedAssembly;
using uDynamic.Extensions;

namespace uDynamic.Events
{
    public class UmbracoStartupEvent : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            //LogHelper.Info(typeof(UmbracoStartupEvent), string.Format("Startup event ..."));

            // Register routes for embedded files
            //RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Hooks Examine's gathering data event
            try
            {
                // Get all datatypes that are "uDynamic.DropdownListSql"
                var dataTypes = ApplicationContext.Current.Services.DataTypeService.GetAllDataTypeDefinitions().Where(dt => dt.PropertyEditorAlias == uDynamic.Constants.Datatype.PropertyEditorAlias.DropdownListSql);
                // Get the search indexes from the prevalues
                List<string> indexProviders = new List<string>();
                foreach (var dataType in dataTypes)
                {
                    var prevalues = ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);
                    if (prevalues.PreValuesAsDictionary["searchIndexAdd"].Value.InvariantEquals("1") || prevalues.PreValuesAsDictionary["searchIndexAdd"].Value.InvariantEquals("true"))
                    {
                        indexProviders.AddRangeUnique(prevalues.PreValuesAsDictionary["searchIndexNames"].Value.Split(','));
                    }
                }
                // Hooks event
                foreach (var indexProvider in indexProviders)
                {
                    if (ExamineManager.Instance.IndexProviderCollection.Where(ip => ip.Name == indexProvider).Count() > 0)
                    {
                        ExamineManager.Instance.IndexProviderCollection[indexProvider].GatheringNodeData += ExamineEvents_GatheringNodeData;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbracoStartupEvent>("Error hooking Examine's gathering data event.", ex);
            }

        }

        private void ExamineEvents_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {

            // Check whether it is a content type
            if (e.IndexType != IndexTypes.Content || (e.IndexType == IndexTypes.Content && e.NodeId < 1)) return;

            try
            {
                // Check whether the doctype contains at least one property which type is "uDynamic.DropdownListSql"
                var contentType = ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes().Where(ct => ct.Alias == e.Node.ExamineNodeTypeAlias()).FirstOrDefault();
                var properties = contentType.PropertyTypes.Where(pt => pt.PropertyEditorAlias == uDynamic.Constants.Datatype.PropertyEditorAlias.DropdownListSql);
                if (properties.Count() < 1) return;

                // Get the prevalues
                var prevalues = ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(properties.FirstOrDefault().DataTypeDefinitionId);

                // Get the values
                var apiController = new uDynamic.Controllers.Api.uDynamicApiController();
                int cacheDuration = 0;
                int.TryParse(prevalues.PreValuesAsDictionary["cacheDuration"].Value, out cacheDuration);
                var values = apiController.GetDropdownListSqlItems(prevalues.PreValuesAsDictionary["sqlCommand"].Value, prevalues.PreValuesAsDictionary["dbKeyColumnName"].Value, prevalues.PreValuesAsDictionary["dbTextColumnName"].Value, prevalues.PreValuesAsDictionary["dbTabsColumnName"].Value, prevalues.PreValuesAsDictionary["dbPropertiesColumnName"].Value, prevalues.PreValuesAsDictionary["dbTableName"].Value, cacheDuration);
                if (values.Count() < 1) return;

                // Add new fields to the index with the text
                var content = ApplicationContext.Current.Services.ContentService.GetById(e.NodeId);
                foreach (var property in properties)
                {
                    var value = values.Where(p => p.key == content.GetValue(property.Alias).ToString());
                    if (value.Count() < 1) continue;
                    e.Fields.Add(property.Alias + "_uDynamicText", value.FirstOrDefault().text);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbracoStartupEvent>("Error adding the text value to the search index.", ex);
            }

        }

    }
}
