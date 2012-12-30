using System;

using idTech4.Services;

namespace idTech4
{
	public class idVersion
	{
		public const int BuildCount		= 2048;
		public const string BuildDate	= "20/10/2012";
		public const string BuildTime	= "18:51:16.42";

		public static string ToString(IPlatformService platform)
		{
			return string.Format("%s.%d%s %s-%s %s %s", 
				idLicensee.EngineVersion, 
				BuildCount, 
				platform.IsDebug ? "-debug" : "",
				platform.TagName,
				platform.Is64Bit ? "-x64" : "-x86",
				BuildDate,
				BuildTime);
		}
	}
}