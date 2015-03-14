using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Core;
using Umbraco.Core.Logging;
using UmbracoExamine;
using Examine;
using Examine.SearchCriteria;
using Examine.Providers;
using Examine.LuceneEngine;
using Examine.Config;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using uDynamic.EmbeddedAssembly;
using uDynamic.Extensions;
using uDynamic.Models;

namespace uDynamic.Events
{

    public class UmbracoStartupEvent : IApplicationEventHandler
    {

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //LogHelper.Info(typeof(UmbracoStartupEvent), string.Format("Startup event ..."));

            // Register routes for embedded files
            //RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Hooks Examine's gathering data event
            try
            {
                // Get all datatypes that are "uDynamic.DropdownListSql" or "uDynamic.CheckboxListSql"
                var dataTypes = ApplicationContext.Current.Services.DataTypeService.GetAllDataTypeDefinitions()
                    .Where(dt => dt.PropertyEditorAlias == uDynamic.Constants.Datatype.PropertyEditorAlias.DropdownListSql || dt.PropertyEditorAlias == uDynamic.Constants.Datatype.PropertyEditorAlias.CheckboxListSql);
                // Get the search indexes from the prevalues
                List<string> indexProviders = new List<string>();
                foreach (var dataType in dataTypes)
                {
                    var prevalues = ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);
                    if (prevalues.PreValuesAsDictionary.ContainsKey("searchIndexProviderNames")
                            && prevalues.PreValuesAsDictionary["searchIndexProviderNames"].Value != null
                            && prevalues.PreValuesAsDictionary.ContainsKey("searchIndexAddFields")
                            && prevalues.PreValuesAsDictionary["searchIndexAddFields"].Value != null
                            && prevalues.PreValuesAsDictionary["searchIndexAddFields"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Count() > 0)
                    {
                        indexProviders.AddRangeUniqueWithTrimming(prevalues.PreValuesAsDictionary["searchIndexProviderNames"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                }
                // Hooks event
                foreach (var indexProvider in indexProviders)
                {
                    if (ExamineManager.Instance.IndexProviderCollection.Where(ip => ip.Name.InvariantEquals(indexProvider.Trim())).Count() > 0)
                    {
                        ExamineManager.Instance.IndexProviderCollection[indexProvider].GatheringNodeData += ExamineEvents_GatheringNodeData;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbracoStartupEvent>("Error hooking Examine's gathering data event for the property editors 'uDynamic.DropdownListSql' and/or 'uDynamic.CheckboxListSql'", ex);
            }

        }

        private void ExamineEvents_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {

            try
            {
                // Get the properties which type is "uDynamic.DropdownListSql" or "uDynamic.CheckboxListSql"
                var contentType = ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes().FirstOrDefault(ct => ct.Alias == e.Node.ExamineNodeTypeAlias());
                if (contentType == null) return;
                var properties = contentType.PropertyTypes
                    .Where(pt => pt.PropertyEditorAlias == uDynamic.Constants.Datatype.PropertyEditorAlias.DropdownListSql || pt.PropertyEditorAlias == uDynamic.Constants.Datatype.PropertyEditorAlias.CheckboxListSql);
                if (properties.Count() < 1) return;

                // Add new fields to the index 
                var content = ApplicationContext.Current.Services.ContentService.GetById(e.NodeId);
                foreach (var property in properties)
                {
                    // Get the prevalues
                    var prevalues = ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(property.DataTypeDefinitionId);

                    // Check whether the prevalues contain all the required settings                   
                    if (prevalues.PreValuesAsDictionary.ContainsKey("sqlCommand") && prevalues.PreValuesAsDictionary["sqlCommand"].Value != null
                        && prevalues.PreValuesAsDictionary.ContainsKey("dbKeyColumnName")
                        && prevalues.PreValuesAsDictionary.ContainsKey("dbTextColumnName")
                        && prevalues.PreValuesAsDictionary.ContainsKey("dbTabsColumnName")
                        && prevalues.PreValuesAsDictionary.ContainsKey("dbPropertiesColumnName")
                        && prevalues.PreValuesAsDictionary.ContainsKey("cacheDuration")
                        && prevalues.PreValuesAsDictionary.ContainsKey("searchIndexAddFields") && prevalues.PreValuesAsDictionary["searchIndexAddFields"].Value != null
                        )
                    {
                        List<string> propertyValues = new List<string>();
                        // Check whether the property contains a value
                        if (content.GetValue(property.Alias) == null || string.IsNullOrWhiteSpace(content.GetValue(property.Alias).ToString())) continue;
                        // For backward compatibility with the previous versions of SQLDropdownList, we convert the single value into an array
                        if (!content.GetValue(property.Alias).ToString().Contains("["))
                        {
                            propertyValues.Add(content.GetValue(property.Alias).ToString());
                        }
                        else
                        {
                            propertyValues = JsonConvert.DeserializeObject<IEnumerable<string>>(content.GetValue(property.Alias).ToString()).ToList();
                        }
                        if (propertyValues.Count() < 1) continue;

                        // Get the values from database (list items)
                        int cacheDuration = 0;
                        int.TryParse(prevalues.PreValuesAsDictionary["cacheDuration"].Value, out cacheDuration);
                        IEnumerable<ListItem> listItems = uDynamic.Controllers.uDynamicController.GetSqlListItems(prevalues.PreValuesAsDictionary["sqlCommand"].Value, prevalues.PreValuesAsDictionary["dbKeyColumnName"].Value, prevalues.PreValuesAsDictionary["dbTextColumnName"].Value, prevalues.PreValuesAsDictionary["dbTabsColumnName"].Value, prevalues.PreValuesAsDictionary["dbPropertiesColumnName"].Value, cacheDuration);
                        // Lookup in the list the item that matches the id (key) saved in content node property (if it is a checkbox list then it could be more than one id)
                        // Each listItem is a list of columns in which the Column[0] contains the id (key)
                        listItems = listItems.Where(item => propertyValues.InvariantContains(item.columns[0].columnValue));
                        if (listItems.Count() < 1) continue;

                        // Add fields to the index
                        var indexFields = prevalues.PreValuesAsDictionary["searchIndexAddFields"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var indexField in indexFields)
                        {
                            string indexFieldValue = string.Empty;
                            foreach (var listItem in listItems)
                            {
                                var value = listItem.columns.Values.FirstOrDefault(c => c.columnName == indexField.Trim());
                                if (value != null)
                                {
                                    indexFieldValue += value.columnValue + " ";
                                }
                            }
                            var indexFielName = property.Alias + indexField.Trim();
                            if (e.Fields.ContainsKey(indexFielName))
                            {
                                e.Fields[indexFielName] = indexFieldValue.Trim();
                            }
                            else
                            {
                                e.Fields.Add(indexFielName, indexFieldValue.Trim());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbracoStartupEvent>("Error adding values to the search index for a property editor 'uDynamic.DropdownListSql' or 'uDynamic.CheckboxListSql'", ex);
            }

        }

    }
}
