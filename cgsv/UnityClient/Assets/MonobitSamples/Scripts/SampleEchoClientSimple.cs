using UnityEngine;
using System;
using System.Runtime.InteropServices;

using MrsServer = System.IntPtr;
using MrsConnection = System.IntPtr;
using MrsCipher = System.IntPtr;

public class SampleEchoClientSimple : Mrs {
    // 接続設定
    private string m_Host = "127.0.0.1";
    private int m_Port = 22222;
    private int m_TimeoutMsec = 5000;
    private MrsConnection m_Connection;
    private bool m_IsRunning = false;
    private int m_MessagesSent = 0;
    private int m_MaxMessages = 10;
    private float m_MessageInterval = 1.0f;
    private float m_Timer = 0.0f;
    
    // 静的インスタンス参照（コールバック関数から非静的メンバーにアクセスするため）
    private static SampleEchoClientSimple s_Instance;

    void Awake() {
        gameObject.AddComponent<mrs.ScreenLogger>();
        s_Instance = this;
    }

    void Start() {
        // MRSの初期化は親クラスで自動的に行われます
    }

    void OnGUI() {
        if (!m_IsRunning) {
            GUILayout.BeginVertical(GUILayout.Width(300));
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("サーバーアドレス:", GUILayout.Width(120));
            m_Host = GUILayout.TextField(m_Host, GUILayout.Width(150));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("ポート:", GUILayout.Width(120));
            string portStr = GUILayout.TextField(m_Port.ToString(), GUILayout.Width(150));
            int.TryParse(portStr, out m_Port);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("エコークライアント開始", GUILayout.Width(300), GUILayout.Height(50))) {
                m_IsRunning = true;
                StartEchoClient();
            }
            
            GUILayout.EndVertical();
        } else {
            GUILayout.Label($"サーバーに接続中: {m_Host}:{m_Port}");
            GUILayout.Label($"送信メッセージ数: {m_MessagesSent}/{m_MaxMessages}");
            
            if (GUILayout.Button("戻る", GUILayout.Width(300), GUILayout.Height(50))) {
                if (m_Connection != IntPtr.Zero) {
                    mrs_close(m_Connection);
                    m_Connection = IntPtr.Zero;
                }
                mrs.Utility.LoadScene("SampleMain");
            }
        }
    }

    void StartEchoClient() {
        Debug.Log($"サーバーに接続中: {m_Host}:{m_Port}...");
        
        // TCPクライアント接続を作成
        m_Connection = mrs_connect(MrsConnectionType.TCP, m_Host, (UInt16)m_Port, (UInt32)m_TimeoutMsec);
        if (m_Connection == IntPtr.Zero) {
            Debug.LogError($"接続エラー: {mrs_get_error_string(mrs_get_last_error())}");
            m_IsRunning = false;
            return;
        }
        
        // コールバック関数を設定
        mrs_set_connect_callback(m_Connection, OnConnect);
        mrs_set_disconnect_callback(m_Connection, OnDisconnect);
        mrs_set_error_callback(m_Connection, OnError);
        mrs_set_read_record_callback(m_Connection, OnReadRecord);
        
        Debug.Log("接続処理を開始しました。サーバーからの応答を待っています...");
    }

    void Update() {
        mrs_update();
        if (m_IsRunning && m_Connection != IntPtr.Zero) {
            // 定期的にメッセージを送信
            if (m_MessagesSent < m_MaxMessages && mrs_connection_is_connected(m_Connection)) {
                m_Timer += Time.deltaTime;
                if (m_Timer >= m_MessageInterval) {
                    m_Timer = 0;
                    SendMessage();
                }
            }
        }
    }

    void SendMessage() {
        string message = "hello";
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        
        // サーバーにメッセージを送信
        bool success = mrs_write_record(m_Connection, 0, 0x01, messageBytes, (UInt32)messageBytes.Length);
        Debug.Log($"メッセージ送信: {(success ? "成功" : "失敗")} {message}");
        m_MessagesSent++;
    }

    // 接続準備完了時のコールバック関数
    [AOT.MonoPInvokeCallback(typeof(MrsConnectCallback))]
    private static void OnConnect(MrsConnection connection, IntPtr connection_data) {
        Debug.Log("サーバーに接続しました");
    }

    // 切断時のコールバック関数
    [AOT.MonoPInvokeCallback(typeof(MrsDisconnectCallback))]
    private static void OnDisconnect(MrsConnection connection, IntPtr connection_data) {
        Debug.Log("サーバーから切断されました");
    }

    // エラー発生時のコールバック関数
    [AOT.MonoPInvokeCallback(typeof(MrsErrorCallback))]
    private static void OnError(MrsConnection connection, IntPtr connection_data, MrsConnectionError status) {
        Debug.LogError($"エラーが発生しました: {mrs_get_connection_error_string(status)}");
    }

    // レコード受信時のコールバック関数
    [AOT.MonoPInvokeCallback(typeof(MrsReadRecordCallback))]
    private static void OnReadRecord(MrsConnection connection, IntPtr connection_data, UInt32 seqnum, UInt16 options, UInt16 payload_type, IntPtr _payload, UInt32 payload_len) {
        Debug.Log($"レコード受信: seqnum={seqnum} options=0x{options:X2} payload_type=0x{payload_type:X2} payload_len={payload_len}");
        
        // IntPtrからバイト配列に変換
        byte[] payload = new byte[payload_len];
        Marshal.Copy(_payload, payload, 0, (int)payload_len);
        
        // ペイロードがテキストの場合は表示
        if (payload_type == 0x01) {
            string response = System.Text.Encoding.UTF8.GetString(payload, 0, (int)payload_len);
            Debug.Log($"サーバーからの応答: {response}");
        }
    }

    void OnDestroy() {
        if (m_Connection != IntPtr.Zero) {
            mrs_close(m_Connection);
            m_Connection = IntPtr.Zero;
        }
    }
}
