using System;

namespace log
{
	class MrsMain : Mrs
	{
		private static MrsLogOutputCallback m_OutputCallback = OnOutputCallback;

		public static void Main(string[] args)
		{
			MRS_LOG_DEBUG("setting level: MRS_LOG_LEVEL_DEBUG(default)");
			MRS_LOG_EMERG("EMERG   1");
			MRS_LOG_ALERT("ALERT   1");
			MRS_LOG_CRIT("CRIT    1");
			MRS_LOG_ERR("ERR     1");
			MRS_LOG_WARNING("WARNING 1");
			MRS_LOG_NOTICE("NOTICE  1");
			MRS_LOG_INFO("INFO    1");
			MRS_LOG_DEBUG("DEBUG   1");

			MRS_LOG_DEBUG("setting level: MRS_LOG_LEVEL_WARNING");
			mrs_set_output_log_level(MrsLogLevel.WARNING);
			MRS_LOG_EMERG("EMERG   2");
			MRS_LOG_ALERT("ALERT   2");
			MRS_LOG_CRIT("CRIT    2");
			MRS_LOG_ERR("ERR     2");
			MRS_LOG_WARNING("WARNING 2");
			MRS_LOG_NOTICE("NOTICE  2");
			MRS_LOG_INFO("INFO    2");
			MRS_LOG_DEBUG("DEBUG   2");
			mrs_set_output_log_level(MrsLogLevel.DEBUG);
			MRS_LOG_DEBUG("setting level: MRS_LOG_LEVEL_DEBUG");

			MRS_LOG_DEBUG("setting output callback: output");
			MrsLogOutputCallback default_output = mrs_get_log_callback();
			mrs_set_log_callback(m_OutputCallback);
			MRS_LOG_EMERG("EMERG   3");
			MRS_LOG_ALERT("ALERT   3");
			MRS_LOG_CRIT("CRIT    3");
			MRS_LOG_ERR("ERR     3");
			MRS_LOG_WARNING("WARNING 3");
			MRS_LOG_NOTICE("NOTICE  3");
			MRS_LOG_INFO("INFO    3");
			MRS_LOG_DEBUG("DEBUG   3");
			mrs_set_log_callback(default_output);
			MRS_LOG_DEBUG("setting output callback: default");
		}

		public static void OnOutputCallback(MrsLogLevel level, String msg)
		{
			string str = string.Format("output function called: {0}", msg);
			mrs_console_log(level, str);
		}
	}
}
