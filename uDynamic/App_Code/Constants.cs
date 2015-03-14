using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace uDynamic.Constants
{
    public class Datatype
    {
        public class PropertyEditorAlias
        {
            public const string CheckboxList = "uDynamic.CheckboxList";
            public const string CheckboxListSql = "uDynamic.CheckboxListSql";
            public const string DropdownList = "uDynamic.DropdownList";
            public const string DropdownListSql = "uDynamic.DropdownListSql";
        }
    }

    public class Cache
    {
        public class IdConstant
        {
        }

        public class IdPattern
        {
            public static readonly string SqlCommandResult = "uDynamic.CacheId.{0}";
        }
    }

}