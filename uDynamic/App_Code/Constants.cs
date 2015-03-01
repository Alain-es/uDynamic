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
            public static readonly string CheckboxList = "uDynamic.CheckboxList";
            public static readonly string DropdownList = "uDynamic.DropdownList";
            public static readonly string DropdownListSql = "uDynamic.DropdownListSql";
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