using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using MrsLibs.Common;
using MrsLibs.Signal;
using MrsLibs.Client;
using MrsLibs.Parser;

namespace echo_client
{
	using MrsConnection = IntPtr;

	/// <summary>
	/// エコークライアントクラス
	/// </summary>
	public class EchoClient : Mrs, IMrsAppFoundation, IDisposable
	{
		/// <summary>
		/// 廃棄フラグ
		/// </summary>
		private bool m_bDisposed = false;

		/// <summary>
		/// 
		/// </summary>
		private static UInt16 m_nRecordOptions = 0;

		/// <summary>
		/// スリープ時間
		/// </summary>
		private UInt32 m_SleepMsec = 1;

		/// <summary>
		/// 鍵交換をする
		/// </summary>
		private static bool m_canKeyExchange = true;

		/// <summary>
		/// 暗号化する
		/// </summary>
		private static bool m_canEncryptRecords = true;

		/// <summary>
		/// エコーデータの書き込み長
		/// </summary>
		private static UInt32 m_nWriteDataLen = 1024;

		/// <summary>
		/// 書き込み回数
		/// </summary>
		private static UInt32 m_nWriteCount = 10;

		/// <summary>
		/// 接続数
		/// </summary>
		private static UInt32 m_nConnections = 1;

		/// <summary>
		/// 読み込み数
		/// </summary>
		private static UInt32 m_nReadCount = 0;

        /// <summary>
        /// レコード受信用
        /// </summary>
        private static bool m_canValidRecord = true;

        /// <summary>
        /// コネクションパス
        /// </summary>
        private static string m_ConnectionPath = "/";

        /// <summary>
        /// 接続タイプ
        /// </summary>
        private MrsConnectionType m_connectionType;

		/// <summary>
		/// サーバーアドレス
		/// </summary>
		private string m_server_addr = "127.0.0.1";

		/// <summary>
		/// サーバーのポート番号
		/// </summary>
		private ushort m_server_port = 22222;

		/// <summary>
		/// タイムアウト値
		/// </summary>
		private UInt32 m_timeout_msec = 5000;

		/// <summary>
		/// ループフラグ
		/// </summary>
		private static volatile bool m_bLoop = true;

		/// <summary>
		/// 接続
		/// </summary>
		private static mrs.Connect g_Connect = new mrs.Connect();

		private static MrsConnectCallback m_OnConnect = OnConnect;
		private static MrsKeyExchangeCallback m_OnKeyExchange = OnKeyExchange;
		private static MrsDisconnectCallback m_OnDisconnect = OnDisconnect;
		private static MrsErrorCallback m_OnError = OnError;
		private static MrsReadRecordCallback m_OnReadRecord = OnReadRecord;
        private static MrsReadCallback m_OnRead = OnRead;
		private static mrs.Connect.FallbackConnectCallback m_FallbackConnectCallback = OnFallbackConnectCallback;

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="args">コマンドライン</param>
		public void Initialize(string[] args)
		{
			MrsCmdParser parser = new MrsCmdParser('-');
			parser.Parse(args);

			m_connectionType = (MrsConnectionType)parser.GetNumeric<Int32>("connectionType", (Int32)MrsConnectionType.TCP);
			m_canKeyExchange = (parser.GetNumeric<Int32>("is_key_exchange", 1) != 0) ? true : false;
			m_canEncryptRecords = (parser.GetNumeric<Int32>("is_encrypt_records", 1) != 0) ? true : false;
            m_nRecordOptions = (ushort)((m_canEncryptRecords == true) ? MrsRecordOption.ON_CRYPT : MrsRecordOption.NONE);
            m_nWriteDataLen = parser.GetNumeric<UInt32>("write_data_len", 1024);
			m_nWriteCount = parser.GetNumeric<UInt32>("write_count", 10);
            m_nConnections = parser.GetNumeric<UInt32>("connections", 1);
            m_SleepMsec = parser.GetNumeric<UInt32>("sleep_msec", 1);

            m_server_addr = parser.GetString("server_addr", "127.0.0.1");
			m_server_port = parser.GetNumeric<ushort>("server_port", 22222);
			m_timeout_msec = parser.GetNumeric<UInt32>("timeout_msec", 5000);
            m_canValidRecord = (parser.GetNumeric<Int32>("is_valid_record", 1) != 0) ? true : false;
            m_ConnectionPath = parser.GetString("connection_path", "/");
			
			mrs.Connect.Request connect_request = new mrs.Connect.Request();
			connect_request.ConnectionType = MrsConnectionType.NONE;
			connect_request.Addr = m_server_addr;
			connect_request.Port = m_server_port;
			connect_request.TimeoutMsec = m_timeout_msec;

			switch (m_connectionType)
			{
			case MrsConnectionType.TCP:
			case MrsConnectionType.UDP:
			case MrsConnectionType.WS:
			case MrsConnectionType.WSS:
            case MrsConnectionType.TCP_SSL:
            case MrsConnectionType.MRU:
				{
					connect_request.ConnectionType = m_connectionType;
					g_Connect.AddRequest(connect_request);
				}
				break;

			default:
				{
					connect_request.ConnectionType = MrsConnectionType.TCP;
					g_Connect.AddRequest(connect_request);

					connect_request.ConnectionType = MrsConnectionType.WSS;
					connect_request.Port += 2;
					g_Connect.AddRequest(connect_request);

					connect_request.ConnectionType = MrsConnectionType.WS;
					connect_request.Port -= 1;
					g_Connect.AddRequest(connect_request);
				}
				break;
			}
			g_Connect.SetFallbackConnectCallback(m_FallbackConnectCallback);
		}

		/// <summary>
		/// 実行
		/// </summary>
		public void Run()
		{
			try
			{
				mrs_initialize();
				using (ExitSignal sig = new ExitSignal())
                {
					sig.SetSignal((obj, e) =>
					{
						m_bLoop = false;
						Thread.Sleep(10);
					});

                    uint connections = 0;
                    for(uint i = 0; i < m_nConnections; ++i)
                    {
                        MrsConnection client = g_Connect.FallbackConnect();
                        if (client == IntPtr.Zero)
                        {
                            MRS_LOG_ERR("mrs_connect[{0}]: {1}", i, mrs_get_error_string(mrs_get_last_error()));
                            break;
                        }
                        ++connections;
                    }
                    if (connections != m_nConnections) return;

                    while(m_bLoop)
                    {
                        mrs_update();
                        mrs_sleep(m_SleepMsec);
                    }
                }
			}
			catch (Exception e)
			{
				MRS_LOG_ERR(e.Message);
			}
			finally
			{
				mrs_finalize();
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
		/// <param name="disposed">廃棄フラグ</param>
		public void Dispose(bool disposed)
		{
			if (m_bDisposed == true) return;
			if (m_bDisposed != disposed)
			{
			}
			m_bDisposed = disposed;
		}

		/// <summary>
		/// フォールバック接続時に呼ばれる
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="request"></param>
		private static void OnFallbackConnectCallback(MrsConnection connection, mrs.Connect.Request request)
		{
			MRS_LOG_DEBUG( "OnFallbackConnectCallback connection_type={0} addr={1} port={2} timeout_msec={3}",
				(int)request.ConnectionType, request.Addr, request.Port, request.TimeoutMsec);

			mrs_set_connect_callback(connection, m_OnConnect);
			mrs_set_disconnect_callback(connection, m_OnDisconnect);
			mrs_set_error_callback(connection, m_OnError);
            if (m_canValidRecord)
            {
    			mrs_set_read_record_callback(connection, m_OnReadRecord);
            }
            else
            {
                mrs_set_read_callback(connection, m_OnRead);
            }
            mrs_connection_set_path(connection, m_ConnectionPath);
		}

		/// <summary>
		/// 鍵交換した時に呼ばれる
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="connection_data"></param>
		private static void OnKeyExchange(MrsConnection connection, IntPtr connection_data)
		{
			MRS_LOG_DEBUG("OnKeyExchange");
			WriteEchoAll(connection);
		}

		/// <summary>
		/// ソケット接続時に呼ばれる
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="connection_data"></param>
		private static void OnConnect(MrsConnection connection, IntPtr connection_data)
		{
			MRS_LOG_DEBUG( "OnConnect local_mrs_version=0x{0} remote_mrs_version=0x{1}",
				mrs_get_version(MRS_VERSION_KEY), mrs_connection_get_remote_version(connection, MRS_VERSION_KEY));

			if (m_canKeyExchange)
			{
				mrs_set_cipher(connection, mrs_cipher_create(MrsCipherType.ECDH));
				mrs_key_exchange(connection, m_OnKeyExchange);
			}
			else
			{
				WriteEchoAll(connection);
			}
		}

		/// <summary>
		/// ソケット切断時に呼ばれる
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="connection_data"></param>
		private static void OnDisconnect(MrsConnection connection, IntPtr connection_data)
		{
			MRS_LOG_DEBUG( "OnDisconnect local_mrs_version=0x{0} remote_mrs_version=0x{1}",
				mrs_get_version(MRS_VERSION_KEY), mrs_connection_get_remote_version(connection, MRS_VERSION_KEY));
		}

		/// <summary>
		/// ソケットにエラーが発生した時に呼ばれる
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="connection_data"></param>
		/// <param name="status"></param>
		private static void OnError(MrsConnection connection, IntPtr connection_data, MrsConnectionError status)
		{
			switch (status)
			{
			case MrsConnectionError.CONNECT_ERROR:
			case MrsConnectionError.CONNECT_TIMEOUT:
				{
					MrsConnection client = g_Connect.FallbackConnect(connection);
					if ( client != IntPtr.Zero ) return;
				}
				break;
			default: 
				break;
			}
	
			MRS_LOG_DEBUG( "OnError local_mrs_version=0x{0} remote_mrs_version=0x{1} status={2}",
				mrs_get_version(MRS_VERSION_KEY), mrs_connection_get_remote_version(connection, MRS_VERSION_KEY), mrs_get_connection_error_string(status));
		}

		/// <summary>
		/// レコード受信時に呼ばれる
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="connection_data"></param>
		/// <param name="seqnum"></param>
		/// <param name="options"></param>
		/// <param name="payload_type"></param>
		/// <param name="_payload"></param>
		/// <param name="payload_len"></param>
		private static void OnReadRecord(MrsConnection connection, IntPtr connection_data, UInt32 seqnum, UInt16 options, UInt16 payload_type, IntPtr _payload, UInt32 payload_len)
		{
			ParseRecord(connection, connection_data, seqnum, options, payload_type, _payload, payload_len);
		}

		/// <summary>
		/// バイナリデータ受信時に呼ばれる
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="connection_data"></param>
		/// <param name="_data"></param>
		/// <param name="data_len"></param>
        private static void OnRead(MrsConnection connection, IntPtr connection_data, IntPtr _data, UInt32 data_len)
        {
            ReadEcho(connection, _data, data_len);
        }

		/// <summary>
		/// レコードのパース
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="connection_data"></param>
		/// <param name="seqnum"></param>
		/// <param name="options"></param>
		/// <param name="payload_type"></param>
		/// <param name="_payload"></param>
		/// <param name="payload_len"></param>
		private static void ParseRecord(MrsConnection connection, IntPtr connection_data, UInt32 seqnum, UInt16 options, UInt16 payload_type, IntPtr _payload, UInt32 payload_len)
		{
			Mrs.MRS_LOG_DEBUG("ParseRecord seqnum=0x{0} options=0x{1:X2} payload={2:X}/{3}", seqnum, options, payload_type, payload_len);
			// MRS_PAYLOAD_TYPE_BEGIN - MRS_PAYLOAD_TYPE_ENDの範囲内で任意のIDを定義し、対応するアプリケーションコードを記述する
			switch (payload_type)
			{
			case 0x01:
				ReadEcho(connection, _payload, payload_len);
				break;
			default:
				break;
			}
		}

		/// <summary>
		/// エコーデータの読み込み
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="payload"></param>
		/// <param name="payload_len"></param>
		private static void ReadEcho(MrsConnection connection, IntPtr payload, UInt32 payload_len )
		{
			mrs.Time read_tm = new mrs.Time();
			read_tm.Set();

			byte[] read_data = new byte[payload_len];
			Marshal.Copy(payload, read_data, 0, (int)payload_len);

			mrs.Buffer buffer = new mrs.Buffer();
			buffer.Write(read_data);
            while(buffer.GetDataLen() > 0)
            {
			    mrs.Time write_tm = buffer.ReadTime();
			    MRS_LOG_DEBUG("ReadEcho data={0} data_len={1} diff_time={2}({3} - {4})",
                    ToString(buffer.GetData()), m_nWriteDataLen, (read_tm - write_tm).ToString(), read_tm.ToString(), write_tm.ToString());
                if (!buffer.Read(null, m_nWriteDataLen))
                {
                    MRS_LOG_ERR("Lost data. len={0} {1}", buffer.GetDataLen(), mrs.Utility.ToHex(buffer.GetData(), buffer.GetDataLen()));
                    m_bLoop = false;
                    break;
                }

			    ++m_nReadCount;
			    if (m_nWriteCount * m_nConnections <= m_nReadCount)
			    {
				    MRS_LOG_DEBUG("Since all records have been received, it is finished.");
				    m_bLoop = false;
			    }
            }
		}

		/// <summary>
		/// エコーデータを書き込み数分書き込む
		/// </summary>
		/// <param name="connection"></param>
		private static void WriteEchoAll(MrsConnection connection)
		{
			StringBuilder sb = new StringBuilder();
			switch (g_Connect.GetRequest().ConnectionType)
			{
			case MrsConnectionType.TCP:
				sb.Append("TCP ");
				break;
			case MrsConnectionType.UDP:
				sb.Append("UDP ");
				break;
			case MrsConnectionType.WS:
				sb.Append("WS ");
				break;
			case MrsConnectionType.WSS:
				sb.Append("WSS ");
				break;
            case MrsConnectionType.TCP_SSL:
                sb.Append("TCP_SSL");
                break;
            case MrsConnectionType.MRU:
                sb.Append("MRU");
                break;
			default:
                sb.Append("INVALID");
				break;
			}

			if (m_canKeyExchange && m_canEncryptRecords) 
				sb.Append("  CRYPT");
			else
				sb.Append("NOCRYPT");

			byte[] data = new byte[m_nWriteDataLen];
			for(uint i = 0; i < m_nWriteCount; ++i)
			{
				string str = String.Format("{0} {1}: {2}", sb.ToString(), connection, i + 1);
				byte[] conv = System.Text.Encoding.UTF8.GetBytes(str);
				System.Buffer.BlockCopy(conv, 0, data, 0, conv.Length);
				WriteEcho(connection, data, m_nWriteDataLen);
			}
		}

		/// <summary>
		/// エコーデータの書き込み
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="data"></param>
		private static void WriteEcho(MrsConnection connection, byte[] data, uint data_len)
		{
			mrs.Time write_tm = new mrs.Time();
			write_tm.Set();
			mrs.Buffer buffer = new mrs.Buffer();
			buffer.WriteTime(write_tm);
			buffer.Write(data, data_len);

            if (m_canValidRecord)
            {
    			mrs_write_record(connection, m_nRecordOptions, 0x01, buffer.GetData(), buffer.GetDataLen());
            }
            else
            {
                mrs_write(connection, buffer.GetData(), buffer.GetDataLen());
            }
		}
	}

	/// <summary>
	/// メインクラス
	/// </summary>
	public class MrsMain
	{
		/// <summary>
		/// メイン関数
		/// </summary>
		/// <param name="args">コマンドライン</param>
		public static void Main(string[] args)
		{
			using (EchoClient client = new EchoClient())
			{
				if (client != null)
				{
					client.Initialize(args);
					client.Run();
				}
			}
		}
	}
}
