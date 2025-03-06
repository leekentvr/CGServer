using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MrsLibs
{
	/// <summary>
	/// 共通
	/// </summary>
	namespace Common
	{
		/// <summary>
		/// MRSアプリのインターフェース
		/// </summary>
		internal interface IMrsAppFoundation
		{
			/// <summary>
			/// 初期化
			/// </summary>
			/// <param name="args">コマンドライン</param>
			void Initialize(string[] args);

			/// <summary>
			/// 実行
			/// </summary>
			void Run();
		}
	}

	/// <summary>
	/// シグナル関連
	/// </summary>
	namespace Signal
	{
		/// <summary>
		/// 終了シグナルインターフェース
		/// </summary>
		internal interface IPlatformExitSignal
		{
			/// <summary>
			/// 終了シグナルのイベントハンドラー
			/// </summary>
			event EventHandler ExitSignalHandler;

			/// <summary>
			/// シグナル処理のハンドラーの設定
			/// </summary>
			/// <param name="handler">ハンドラー</param>
			/// <param name="value">ハンドラー値</param>
			/// <returns></returns>
			bool SetSignal(EventHandler handler, Int32 value);
		}

		/// <summary>
		/// イベントタイプ
		/// </summary>
		public enum CtrlTypes : Int32
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		/// <summary>
		/// シグナルの列挙子
		/// </summary>
		public enum Signal : Int32
		{
			SIGHUP = 1,
			SIGINT = 2,
			SIGQUIT = 3,
			SIGKILL = 9,
			SIGPIPE = 13,
			SIGTERM = 15,
		}

		/// <summary>
		/// 共通シグナルタイプ
		/// </summary>
		public enum SignalEnums : Int32
		{
			UNKNOWN_SIGNAL = 0x0,

			/// <summary>
			/// Windows用シグナル
			/// </summary>
			WINDOWS_SIGNAL_FIRST		= 0x0,
			WINDOWS_CTRL_C_EVENT		= (WINDOWS_SIGNAL_FIRST | 0x01),
			WINDOWS_CTRL_BREAK_EVENT	= (WINDOWS_SIGNAL_FIRST | 0x02),
			WINDOWS_CTRL_CLOSE_EVENT	= (WINDOWS_SIGNAL_FIRST | 0x04),
			WINDOWS_CTRL_LOGOFF_EVENT	= (WINDOWS_SIGNAL_FIRST | 0x08),
			WINDOWS_CTRL_SHUTDOWN_EVENT = (WINDOWS_SIGNAL_FIRST | 0x10),

			/// <summary>
			/// Linux用シグナル
			/// </summary>
			LINUX_SIGNAL_FIRST = 0x1000,
			LINUX_SIGHUP  = (LINUX_SIGNAL_FIRST | 0x01),
			LINUX_SIGINT  = (LINUX_SIGNAL_FIRST | 0x02),
			LINUX_SIGQUIT = (LINUX_SIGNAL_FIRST | 0x04),
			LINUX_SIGKILL = (LINUX_SIGNAL_FIRST | 0x08),
			LINUX_SIGPIPE = (LINUX_SIGNAL_FIRST | 0x10),
			LINUX_SIGTERM = (LINUX_SIGNAL_FIRST | 0x20),
		}

		/// <summary>
		/// プラットフォームの列挙子
		/// </summary>
		public enum ExitSignal_Platform
		{
			Windows,
			Linux,
			Mac,
		}

		/// <summary>
		/// Win/Linux共通のシグナルイベント受信処理クラス
		/// </summary>
		public class ExitSignal : IDisposable
		{
			/// <summary>
			/// 廃棄フラグ
			/// </summary>
			private bool m_bDisposed = false;

			/// <summary>
			/// シグナルクラスへのハンドル
			/// </summary>
			private IPlatformExitSignal m_platformExitSignal;

			/// <summary>
			/// プラットフォーム
			/// </summary>
			private ExitSignal_Platform m_platform;

			/// <summary>
			/// 定義されたLinux用シグナルイベント
			/// </summary>
			private readonly Int32 m_linuxSignals;

			/// <summary>
			/// Linux用シグナルイベントの全ての合算値
			/// </summary>
			public const Int32 LinuxSignalsAll = (Int32)(SignalEnums.LINUX_SIGHUP | SignalEnums.LINUX_SIGINT | SignalEnums.LINUX_SIGKILL | /*SignalEnums.LINUX_SIGPIPE |*/ SignalEnums.LINUX_SIGQUIT | SignalEnums.LINUX_SIGTERM);

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public ExitSignal(Int32 linuxSignals = LinuxSignalsAll)
			{
				m_linuxSignals = linuxSignals;
				GetPlatform();
			}

			/// <summary>
			/// Linuxか判定する
			/// </summary>
			public bool IsLinux
			{
				get
				{
					int p = (int)Environment.OSVersion.Platform;
					return (p == 4) || (p == 6) || (p == 128);
				}
			}

			/// <summary>
			/// シグナルハンドラーの設定
			/// </summary>
			/// <param name="handler">シグナルハンドラー</param>
			/// <param name="value">設定シグナル値</param>
			/// <returns></returns>
			public bool SetSignal(EventHandler handler)
			{
				if (m_platform == ExitSignal_Platform.Linux || m_platform == ExitSignal_Platform.Mac)
				{
					// Linux用シグナルハンドラーの設定
					if ((m_linuxSignals & (Int32)SignalEnums.LINUX_SIGHUP) == (Int32)SignalEnums.LINUX_SIGHUP)
						m_platformExitSignal.SetSignal(handler, (Int32)Signal.SIGHUP);
					if ((m_linuxSignals & (Int32)SignalEnums.LINUX_SIGINT) == (Int32)SignalEnums.LINUX_SIGINT)
						m_platformExitSignal.SetSignal(handler, (Int32)Signal.SIGINT);
					if ((m_linuxSignals & (Int32)SignalEnums.LINUX_SIGKILL) == (Int32)SignalEnums.LINUX_SIGKILL)
						m_platformExitSignal.SetSignal(handler, (Int32)Signal.SIGKILL);
					if ((m_linuxSignals & (Int32)SignalEnums.LINUX_SIGPIPE) == (Int32)SignalEnums.LINUX_SIGPIPE)
						m_platformExitSignal.SetSignal(handler, (Int32)Signal.SIGPIPE);
					if ((m_linuxSignals & (Int32)SignalEnums.LINUX_SIGQUIT) == (Int32)SignalEnums.LINUX_SIGQUIT)
						m_platformExitSignal.SetSignal(handler, (Int32)Signal.SIGQUIT);
					if ((m_linuxSignals & (Int32)SignalEnums.LINUX_SIGTERM) == (Int32)SignalEnums.LINUX_SIGTERM)
						m_platformExitSignal.SetSignal(handler, (Int32)Signal.SIGTERM);
					return true;
				}
				else if (m_platform == ExitSignal_Platform.Windows)
				{
					// Windows用シグナルハンドラーの設定
					return m_platformExitSignal.SetSignal(handler, -1);
				}
				else
				{
					// 知らないプラットフォーム
					Debug.Assert(false);
					return false;
				}
			}

			/// <summary>
			/// 廃棄処理
			/// </summary>
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// 廃棄処理の実装部
			/// </summary>
			/// <param name="disposed"></param>
			public void Dispose(bool disposed)
			{
				if (m_bDisposed == true)
					return;

				if (m_bDisposed != disposed)
				{
					DisposeExitSignal();
					m_platformExitSignal = null;
				}

				m_bDisposed = disposed;
			}

			/// <summary>
			/// プラットフォーム毎のシグナルクラスを作成する
			/// </summary>
			private void GetPlatform()
			{
				m_platform = IsLinux ? ExitSignal_Platform.Linux : ExitSignal_Platform.Windows;
				switch (m_platform)
				{
				case ExitSignal_Platform.Linux:
				case ExitSignal_Platform.Mac:
					{
						// Linux用シグナルクラスを作成
						m_platformExitSignal = new UnixExitSignal();
					}
					break;

				case ExitSignal_Platform.Windows:
					{
						// Windows用シグナルクラスを作成
						m_platformExitSignal = new WindowsExitSignal();
					}
					break;

				default:
					Debug.Assert(false);
					break;
				}
			}

			/// <summary>
			/// シグナルクラスの廃棄処理
			/// </summary>
			private void DisposeExitSignal()
			{
				if (m_platformExitSignal == null)
					return;

				switch (m_platform)
				{
				case ExitSignal_Platform.Linux:
				case ExitSignal_Platform.Mac:
					{
						// Linux用シグナルクラスを廃棄
						var sig = m_platformExitSignal as UnixExitSignal;
						sig.Dispose();
					}
					break;

				case ExitSignal_Platform.Windows:
					{
						// Windows用シグナルクラスを廃棄
						var sig = m_platformExitSignal as WindowsExitSignal;
						sig.Dispose();
					}
					break;

				default:
					Debug.Assert(false);
					break;
				}
			}
		}

		/// <summary>
		/// Linux用シグナルイベント受信処理クラス
		/// </summary>
		public class UnixExitSignal : IPlatformExitSignal, IDisposable
		{
			/// <summary>
			/// 終了イベントハンドラー
			/// </summary>
			public event EventHandler ExitSignalHandler;

			/// <summary>
			/// 廃棄フラグ
			/// </summary>
			private bool m_bDisposed = false;

			/// <summary>
			/// シグナル受信デリゲート
			/// </summary>
			/// <param name="value"></param>
			private delegate void SignalRoutine(Int32 value);

			/// <summary>
			/// Sigaction_t保存用
			/// </summary>
			private Dictionary<Signal, IntPtr> m_dicSigaction = new Dictionary<Signal, IntPtr>();

			/// <summary>
			/// Sigaction_t保存用
			/// </summary>
			private Dictionary<Signal, GCHandle> m_dicSaHandler = new Dictionary<Signal, GCHandle>();

			/// <summary>
			/// Sigaction_t内の構造体
			/// </summary>
			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			private struct Sigset_t
			{
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = (1024 / (8 * sizeof(UInt64))))]
				UInt64[] val;
			}

			/// <summary>
			/// Sigaction設定用構造体
			/// </summary>
			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			private struct Sigaction_t
			{
				//[MarshalAs(UnmanagedType.FunctionPtr)]
				public IntPtr sa_handler;
				//[MarshalAs(UnmanagedType.FunctionPtr)]
				public IntPtr sigaction;
				//[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Sigset_t))]
				public Sigset_t sa_mask;
				//[MarshalAs(UnmanagedType.I4)]
				public Int32 sa_flags;
				//[MarshalAs(UnmanagedType.FunctionPtr)]
				public IntPtr sa_restorer;
			}

			/// <summary>
			/// シグナルコールバック登録API
			/// </summary>
			/// <param name="sig"></param>
			/// <param name="act"></param>
			/// <param name="oact"></param>
			/// <returns></returns>
			[DllImport("libc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sigaction")]
			private static extern int Sigaction(Signal sig, IntPtr act, IntPtr oact);

			/// <summary>
			/// シグナルのコールバック設定
			/// </summary>
			public bool SetSignal(EventHandler handler, Int32 value)
			{
				Signal sig = (Signal)value;
				ExitSignalHandler = handler;
				if (ExitSignalHandler == null)
					return false;

				Sigaction_t sa_t = new Sigaction_t();
				IntPtr sigRoutine = Marshal.GetFunctionPointerForDelegate<SignalRoutine>((s) => {
					ExitSignalHandler(this, new SignalEventArgs(ExitSignal_Platform.Linux, (Signal)s));
				});

				GCHandle hHandle = GCHandle.Alloc(sigRoutine);
				sa_t.sa_handler = (IntPtr)hHandle.Target;
				m_dicSaHandler[sig] = hHandle;

				IntPtr hSigaction = Marshal.AllocHGlobal(Marshal.SizeOf(sa_t));
				Marshal.StructureToPtr(sa_t, hSigaction, false);
				Sigaction(sig, hSigaction, IntPtr.Zero);
				m_dicSigaction[sig] = hSigaction;

				return true;
			}

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public UnixExitSignal()
			{
			}

			/// <summary>
			/// 廃棄
			/// </summary>
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// 廃棄の実装部
			/// </summary>
			/// <param name="disposed"></param>
			private void Dispose(bool disposed)
			{
				if (m_bDisposed == true)
					return;

				if (m_bDisposed != disposed)
				{
					foreach(var handle in m_dicSaHandler.Values)
					{
						if (handle.IsAllocated)
							handle.Free();
					}
					foreach(var handle in m_dicSigaction.Values)
					{
						if (handle == IntPtr.Zero)
							Marshal.FreeHGlobal(handle);
					}
				}

				m_bDisposed = disposed;
			}
		}

		/// <summary>
		/// Windows用シグナルイベント受信処理クラス
		/// </summary>
		public class WindowsExitSignal : IPlatformExitSignal, IDisposable
		{
			/// <summary>
			/// 終了イベントハンドラー
			/// </summary>
			public event EventHandler ExitSignalHandler;

			/// <summary>
			/// イベントハンドラーの設定
			/// </summary>
			/// <param name="Handler"></param>
			/// <param name="Add"></param>
			/// <returns></returns>
			[DllImport("Kernel32")]
			private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

			/// <summary>
			/// ハンドラーのデリゲート
			/// </summary>
			/// <param name="ctrlType"></param>
			/// <returns></returns>
			private delegate bool HandlerRoutine(CtrlTypes ctrlType);

			/// <summary>
			/// デリゲートのハンドル
			/// </summary>
			private static GCHandle m_hSignal;

			/// <summary>
			/// 廃棄フラグ
			/// </summary>
			private bool m_bDisposed = false;

			/// <summary>
			/// シグナルハンドラーの設定
			/// </summary>
			/// <param name="handler">イベントハンドラー</param>
			public bool SetSignal(EventHandler handler, Int32 value = -1)
			{
				ExitSignalHandler = handler;
				if (ExitSignalHandler == null)
				{
					Debug.Assert(handler != null);
					return false;
				}

				m_hSignal = GCHandle.Alloc(new HandlerRoutine((ctrlType) => {
					ExitSignalHandler(this, new SignalEventArgs(ExitSignal_Platform.Windows, ctrlType));
					return false;
				}), GCHandleType.Normal);

				Debug.Assert(m_hSignal.IsAllocated);
				SetConsoleCtrlHandler(m_hSignal.Target as HandlerRoutine, true);
				return true;
			}

			/// <summary>
			/// 廃棄処理
			/// </summary>
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// 廃棄処理の実装部
			/// </summary>
			/// <param name="disposed"></param>
			public void Dispose(bool disposed)
			{
				if (m_bDisposed == true)
					return;

				if (disposed != m_bDisposed)
				{
					if (m_hSignal.IsAllocated)
						m_hSignal.Free();
					ExitSignalHandler = null;
				}

				m_bDisposed = disposed;
			}
		}

		/// <summary>
		/// 共通シグナルイベント引数クラス
		/// </summary>
		public class SignalEventArgs : EventArgs
		{
			/// <summary>
			/// Linux用シグナルタイプ
			/// </summary>
			private readonly Signal m_signal;

			/// <summary>
			/// Windows用シグナルタイプ
			/// </summary>
			private readonly CtrlTypes m_ctrlTypes;

			/// <summary>
			/// プラットフォーム
			/// </summary>
			private readonly ExitSignal_Platform m_platform;

			/// <summary>
			/// Linux用コンストラクタ
			/// </summary>
			/// <param name="signal">シグナルタイプ</param>
			public SignalEventArgs(ExitSignal_Platform platform, Signal signal)
			{
				m_platform = platform;
				m_signal = signal;
			}

			/// <summary>
			/// Windows用コンストラクタ
			/// </summary>
			/// <param name="ctrlTypes"></param>
			public SignalEventArgs(ExitSignal_Platform platform, CtrlTypes ctrlTypes)
			{
				m_platform = platform;
				m_ctrlTypes = ctrlTypes;
			}

			/// <summary>
			/// Linux用シグナルタイプのプロパティ
			/// </summary>
			private Signal Signal
			{
				get { return m_signal; }
			}

			/// <summary>
			/// Windows用シグナルタイプのプロパティ
			/// </summary>
			private CtrlTypes CtrlTypes
			{
				get { return m_ctrlTypes; }
			}

			/// <summary>
			/// 
			/// </summary>
			public SignalEnums SignalEnums
			{
				get
				{
					if (m_platform == ExitSignal_Platform.Windows)
					{
						switch(m_ctrlTypes)
						{
						case CtrlTypes.CTRL_C_EVENT:
							return SignalEnums.WINDOWS_CTRL_C_EVENT;
						case CtrlTypes.CTRL_BREAK_EVENT:
							return SignalEnums.WINDOWS_CTRL_BREAK_EVENT;
						case CtrlTypes.CTRL_CLOSE_EVENT:
							return SignalEnums.WINDOWS_CTRL_CLOSE_EVENT;
						case CtrlTypes.CTRL_LOGOFF_EVENT:
							return SignalEnums.WINDOWS_CTRL_LOGOFF_EVENT;
						case CtrlTypes.CTRL_SHUTDOWN_EVENT:
							return SignalEnums.WINDOWS_CTRL_SHUTDOWN_EVENT;
						default:
							return SignalEnums.UNKNOWN_SIGNAL;
						}
					}
					else
					{
						switch(m_signal)
						{
						case Signal.SIGHUP:
							return SignalEnums.LINUX_SIGHUP;
						case Signal.SIGINT:
							return SignalEnums.LINUX_SIGINT;
						case Signal.SIGQUIT:
							return SignalEnums.LINUX_SIGQUIT;
						case Signal.SIGKILL:
							return SignalEnums.LINUX_SIGKILL;
						case Signal.SIGPIPE:
							return SignalEnums.LINUX_SIGPIPE;
						case Signal.SIGTERM:
							return SignalEnums.LINUX_SIGTERM;
						default:
							return SignalEnums.UNKNOWN_SIGNAL;
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// MRSサーバー関連
	/// </summary>
	namespace Server
	{
		/// <summary>
		/// 
		/// </summary>
		class MrsServerFoundation : IDisposable
		{
			/// <summary>
			/// 廃棄フラグ
			/// </summary>
			private bool m_bDisposed = false;

			/// <summary>
			/// 
			/// </summary>
			private IntPtr m_Server = IntPtr.Zero;
			public IntPtr Server
			{
				get => m_Server;
			}

			/// <summary>
			/// 
			/// </summary>
			private GCHandle m_hCallback;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public MrsServerFoundation(Mrs.MrsConnectionType connectionType, string addr, ushort port, int backlog)
			{
				// サーバーの作成
				m_Server = Mrs.mrs_server_create(connectionType, addr, port, backlog);
				if (m_Server == IntPtr.Zero)
				{
					string error =	Marshal.PtrToStringAuto(Mrs.mrs_get_error_string(Mrs.mrs_get_last_error()));
					throw new Exception(error);
				}
			}

			/// <summary>
			/// 廃棄処理
			/// </summary>
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// 廃棄処理の実装部
			/// </summary>
			/// <param name="disposed"></param>
			public void Dispose(bool disposed)
			{
				if (m_bDisposed == true)
					return;

				if (m_bDisposed != disposed)
				{
					// コールバックの解放
					if (m_hCallback.IsAllocated)
						m_hCallback.Free();

					// サーバーのクローズ処理
					Mrs.mrs_close(m_Server);
				}

				m_bDisposed = disposed;
			}

			/// <summary>
			/// 接続コールバックの設定
			/// </summary>
			/// <param name="callback">コールバック</param>
			public void SetNewConnectionCallback(Mrs.MrsNewConnectionCallback callback)
			{
				m_hCallback = GCHandle.Alloc(callback);
				Mrs.mrs_server_set_new_connection_callback(m_Server, m_hCallback.Target as Mrs.MrsNewConnectionCallback);
			}
		}
	}

	/// <summary>
	/// MRSクライアント関連
	/// </summary>
	namespace Client
	{
		class MrsClientFoundation : IDisposable
		{
			/// <summary>
			/// 廃棄フラグ
			/// </summary>
			private bool m_bDisposed = false;

			/// <summary>
			/// 接続リスト
			/// </summary>
			private List<IntPtr> m_connect_list = new List<IntPtr>();

			/// <summary>
			/// コールバックのハンドル集
			/// </summary>
			private class MrsCbkHandles
			{
				public GCHandle connHandle;
				public GCHandle discHandle;
				public GCHandle errorHandle;
				public GCHandle readHandle;

				public MrsCbkHandles() { }
				public MrsCbkHandles(GCHandle _connHandle, GCHandle _discHandle, GCHandle _errorHandle, GCHandle _readHandle)
				{
					connHandle = _connHandle;
					discHandle = _discHandle;
					errorHandle = _errorHandle;
					readHandle = _readHandle;
				}
			}

			/// <summary>
			/// 
			/// </summary>
			private Dictionary<IntPtr, MrsCbkHandles> m_dicMrsHandles = new Dictionary<IntPtr, MrsCbkHandles>();

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public MrsClientFoundation(Mrs.MrsConnectionType connectionType, string addr, ushort port, UInt32 timeout_msec, UInt32 connections)
			{
				Debug.Assert(connections > 0);
				for(uint i = 0; i < connections; i++)
				{
					IntPtr client = Mrs.mrs_connect(connectionType, addr, port, timeout_msec);
					if (client == IntPtr.Zero)
					{
						string error = Marshal.PtrToStringAuto(Mrs.mrs_get_error_string(Mrs.mrs_get_last_error()));
						throw new Exception(error);
					}
					m_connect_list.Add(client);
				}
			}

			/// <summary>
			/// 廃棄処理
			/// </summary>
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// 廃棄処理の実装部
			/// </summary>
			/// <param name="disposed"></param>
			public void Dispose(bool disposed)
			{
				if (m_bDisposed == true)
					return;

				if (m_bDisposed != disposed)
				{
					FreeAll();
					m_connect_list.Clear();
					m_connect_list = null;
				}

				m_bDisposed = disposed;
			}

			/// <summary>
			/// コールバックの設定
			/// </summary>
			/// <param name="conn_cbk">接続コールバック</param>
			/// <param name="disc_cbk">切断コールバック</param>
			/// <param name="error_cbk">エラーコールバック</param>
			/// <param name="read_cbk">レコードの読み込みコールバック</param>
			public void SetCallback(Mrs.MrsConnectCallback conn_cbk, Mrs.MrsDisconnectCallback disc_cbk, Mrs.MrsErrorCallback error_cbk, Mrs.MrsReadRecordCallback read_cbk)
			{
				foreach(var handle in m_connect_list)
				{
					MrsCbkHandles handles = new MrsCbkHandles();
					m_dicMrsHandles[handle] = handles;

					handles.connHandle = GCHandle.Alloc(conn_cbk);
					Mrs.mrs_set_connect_callback(handle, handles.connHandle.Target as Mrs.MrsConnectCallback);

					handles.discHandle = GCHandle.Alloc(disc_cbk);
					Mrs.mrs_set_disconnect_callback(handle, handles.discHandle.Target as Mrs.MrsDisconnectCallback);

					handles.errorHandle = GCHandle.Alloc(error_cbk);
					Mrs.mrs_set_error_callback(handle, handles.errorHandle.Target as Mrs.MrsErrorCallback);

					handles.readHandle = GCHandle.Alloc(read_cbk);
					Mrs.mrs_set_read_record_callback(handle, handles.readHandle.Target as Mrs.MrsReadRecordCallback);
				}
			}

			/// <summary>
			/// 解放
			/// </summary>
			public void FreeAll()
			{
				foreach(var handle in m_connect_list)
				{
					var handles = m_dicMrsHandles[handle];
					if (handles.connHandle.IsAllocated)
						handles.connHandle.Free();
					if (handles.discHandle.IsAllocated)
						handles.discHandle.Free();
					if (handles.errorHandle.IsAllocated)
						handles.errorHandle.Free();
					if (handles.readHandle.IsAllocated)
						handles.readHandle.Free();
				}
				m_dicMrsHandles.Clear();
				m_dicMrsHandles = null;
			}

			/// <summary>
			/// クローズ処理
			/// </summary>
			public void CloseAll()
			{
				foreach(var handle in m_connect_list)
				{
					Mrs.mrs_close(handle);
				}
			}
		}
	}

	/// <summary>
	/// パーサー
	/// </summary>
	namespace Parser
	{
		class MrsCmdParser
		{
			/// <summary>
			/// 
			/// </summary>
			private readonly Char m_key_prefix;

			/// <summary>
			/// 
			/// </summary>
			private Dictionary<string, string> m_parsed_key_value = new Dictionary<string, string>();

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="key_prefix"></param>
			public MrsCmdParser(Char key_prefix)
			{
				m_key_prefix = key_prefix;
			}

			/// <summary>
			/// コマンドをパースする
			/// </summary>
			/// <param name="command"></param>
			/// <returns></returns>
			public bool Parse(string[] commands)
			{
				int i = 1;
				foreach(var command in commands)
				{
					string cmd = command.Trim(m_key_prefix);
					string[] key_value = cmd.Split("=");

					Mrs.MRS_LOG_DEBUG( "arg {0}: {1} [{2}]", i++, key_value[0], key_value[1]);
					m_parsed_key_value[key_value[0]] = key_value[1];
				}
				return true;
			}

			/// <summary>
			/// 文字列を取得する
			/// </summary>
			/// <param name="key"></param>
			/// <param name="def"></param>
			/// <returns></returns>
			public string GetString(string key, string def)
			{
				if (m_parsed_key_value.ContainsKey(key))
				{
					return m_parsed_key_value[key];
				}
				return def;
			}

			/// <summary>
			/// 数値を取得する
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="key"></param>
			/// <param name="def"></param>
			/// <returns></returns>
			public T GetNumeric<T>(string key, T def)
			{
				if (m_parsed_key_value.ContainsKey(key))
				{
					string value = m_parsed_key_value[key];
					switch(typeof(T).Name)
					{
					case "UInt16":
						return (T)(object)UInt16.Parse(value);
					case "UInt32":
						return (T)(object)UInt32.Parse(value);
					case "UInt64":
						return (T)(object)UInt64.Parse(value);
					case "Int16":
						return (T)(object)Int16.Parse(value);
					case "Int32":
						return (T)(object)Int32.Parse(value);
					case "Int64":
						return (T)(object)Int64.Parse(value);
					case "Single":
						return (T)(object)float.Parse(value);
					case "Double":
						return (T)(object)double.Parse(value);
					case "Boolean":
						return (T)(object)bool.Parse(value);
					}
				}
				return def;
			}
		}
	}
}
