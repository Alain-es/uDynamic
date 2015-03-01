﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Core;
using Examine;
using Examine.SearchCriteria;
using Examine.Providers;
using Examine.LuceneEngine;
using Examine.Config;
using UmbracoExamine;

using uDynamic.EmbeddedAssembly;


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
            ExamineManager.Instance.IndexProviderCollection["ExternalIndexer"].GatheringNodeData += ExamineEvents_GatheringNodeData;

        }

        private void ExamineEvents_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {

            // Check whether it is a content type
            if (e.IndexType != IndexTypes.Content || (e.IndexType == IndexTypes.Content && e.NodeId < 1)) return;

            // Check whether the doctype contains at least one property which type is "uDynamic.DropdownListSql"
            var contentType = ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes().Where(ct => ct.Alias == e.Node.ExamineNodeTypeAlias()).FirstOrDefault();
            var properties = contentType.PropertyTypes.Where(pt => pt.PropertyEditorAlias == "uDynamic.DropdownListSql");
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

    }
}
