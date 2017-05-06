using System;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	internal class SQLServer : DatabasePlugin
	{
#region Provider support
		// we must use new because there are no overrides on static methods, see e.g. http://stackoverflow.com/q/7839691
		new static internal string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "system.data.sqlclient":
					return "System.Data.SqlClient.SqlClientFactory";

				default:
					return null;
			}
		}
		#endregion

		#region SQL
		override public string BuildLimitOffsetSelect(string columns, string tablesAndJoins, string where, string orderBy, int limit, int offset)
		{
			return BuildRowNumberLimitOffsetSelect(columns, tablesAndJoins, where, orderBy, limit, offset);
		}
		#endregion

		#region Table info
		// This code from Massive - see CREDITS file
		override public object GetColumnDefault(dynamic columnInfo)
		{
			string defaultValue = columnInfo.COLUMN_DEFAULT;
			if(string.IsNullOrEmpty(defaultValue))
			{
				return null;
			}
			dynamic result;
			switch(defaultValue)
			{
				case "getdate()":
				case "(getdate())":
					result = DateTime.Now;
					break;
				case "newid()":
					result = Guid.NewGuid().ToString();
					break;
				default:
					result = defaultValue.Replace("(", "").Replace(")", "");
					break;
			}
			return result;
		}
#endregion

#region Keys and sequences
		override public string IdentityRetrievalFunction { get; protected set; } = "SCOPE_IDENTITY()";
#endregion

#region Prefix/deprefix parameters
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			return (cmd != null) ? rawName : ("@" + rawName);
		}
#endregion
	}
}