using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using MrsLibs.Signal;

namespace base_server
{
	class MrsMain : Mrs
	{
		/// <summary>
		/// 実行フラグ
		/// </summary>
		private static volatile bool s_bIsRun = true;

		/// <summary>
		/// スリープ時間
		/// </summary>
		private const UInt32 m_nSleepMsec = 1;

		/// <summary>
		/// メイン関数
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			DateTime dateTime = DateTime.Now;

			// ファイル名の作成
			string fileName = string.Format("{0}.log", dateTime.ToString("yyyyMMddHHmmss"));
			using (StreamWriter writer = new StreamWriter(fileName))
			{
				using (WindowsExitSignal sig = new WindowsExitSignal())
				{
					// ログコールバックの作成
					GCHandle hLogCallback = GCHandle.Alloc(new MrsLogOutputCallback((level, msg) => {
						string time_stamp = mrs.DateTime.Now().ToString();
						string log = string.Format("[{0}] {1}", time_stamp, msg);
						mrs_console_log(level, log);
						writer.WriteLine(log);
					}), GCHandleType.Normal);
					if (!hLogCallback.IsAllocated)
					{
						Console.WriteLine("Fail: alloc to delegate.");
						return;
					}

					// 強制終了対応
					sig.SetSignal((obj, e) => {
						SignalEventArgs event_args = e as SignalEventArgs;
						MRS_LOG_DEBUG("Event: {0}", event_args.SignalEnums);
						s_bIsRun = false;
						Thread.Sleep(10); // ここで待ち
					});

					// ログコールバックの登録
					mrs_set_log_callback(hLogCallback.Target as MrsLogOutputCallback);

					// mrsの初期化
					mrs_initialize();
					MRS_LOG_DEBUG("サーバーの開始");

					try
					{
						while (s_bIsRun)
						{
							// mrsの更新
							mrs_update();

							// sleep
							mrs_sleep(m_nSleepMsec);
						}
					}
					finally
					{
						MRS_LOG_DEBUG("サーバーの終了");
						// mrsの終了処理
						mrs_finalize();
						// ログコールバックの解放
						hLogCallback.Free();
					}
				}
			}
		}
	}
}
