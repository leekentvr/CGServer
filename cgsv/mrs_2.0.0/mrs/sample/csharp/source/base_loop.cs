using System;

namespace base_loop
{
    class MrsMain : Mrs
    {
        static void Main(string[] args)
        {
			mrs_initialize();

			try
			{
				MRS_LOG_DEBUG("[{0}] start", mrs.DateTime.Now().ToString());
				for (int i = 0; i < 3; ++i)
				{
					mrs_update();
					MRS_LOG_DEBUG("[{0}] zzz {1}", mrs.DateTime.Now().ToString(), i + 1);
					mrs_sleep(1000);
				}
				MRS_LOG_DEBUG("[{0}] end", mrs.DateTime.Now().ToString());
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			finally
			{
				mrs_finalize();
			}
        }
    }
}
