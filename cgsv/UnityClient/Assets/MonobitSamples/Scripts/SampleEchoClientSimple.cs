using UnityEngine;
using System;
using System.Runtime.InteropServices;

using MrsServer = System.IntPtr;
using MrsConnection = System.IntPtr;
using MrsCipher = System.IntPtr;

public class SampleEchoClientSimple : Mrs {
    // Connection settings
    private string m_Host = "127.0.0.1";
    private int m_Port = 22222;
    private int m_TimeoutMsec = 5000;
    private MrsConnection m_Connection;
    private bool m_IsRunning = false;
    private int m_MessagesSent = 0;
    private int m_MaxMessages = 10;
    private float m_MessageInterval = 1.0f;
    private float m_Timer = 0.0f;
    
    // Static instance reference (to access non-static members from callback functions)
    private static SampleEchoClientSimple s_Instance;

    void Awake() {
        gameObject.AddComponent<mrs.ScreenLogger>();
        s_Instance = this;
    }

    void Start() {
        // MRS initialization is automatically done in the parent class
    }

    void OnGUI() {
        if (!m_IsRunning) {
            GUILayout.BeginVertical(GUILayout.Width(300));
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Server Address:", GUILayout.Width(120));
            m_Host = GUILayout.TextField(m_Host, GUILayout.Width(150));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Port:", GUILayout.Width(120));
            string portStr = GUILayout.TextField(m_Port.ToString(), GUILayout.Width(150));
            int.TryParse(portStr, out m_Port);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Start Echo Client", GUILayout.Width(300), GUILayout.Height(50))) {
                m_IsRunning = true;
                StartEchoClient();
            }
            
            GUILayout.EndVertical();
        } else {
            GUILayout.Label($"Connected to server: {m_Host}:{m_Port}");
            GUILayout.Label($"Messages sent: {m_MessagesSent}/{m_MaxMessages}");
            
            if (GUILayout.Button("Back", GUILayout.Width(300), GUILayout.Height(50))) {
                if (m_Connection != IntPtr.Zero) {
                    mrs_close(m_Connection);
                    m_Connection = IntPtr.Zero;
                }
                mrs.Utility.LoadScene("SampleMain");
            }
        }
    }

    void StartEchoClient() {
        Debug.Log($"Connecting to server: {m_Host}:{m_Port}...");
        
        // Create TCP client connection
        m_Connection = mrs_connect(MrsConnectionType.TCP, m_Host, (UInt16)m_Port, (UInt32)m_TimeoutMsec);
        if (m_Connection == IntPtr.Zero) {
            Debug.LogError($"Connection error: {mrs_get_error_string(mrs_get_last_error())}");
            m_IsRunning = false;
            return;
        }
        
        // Set callback functions
        mrs_set_connect_callback(m_Connection, OnConnect);
        mrs_set_disconnect_callback(m_Connection, OnDisconnect);
        mrs_set_error_callback(m_Connection, OnError);
        mrs_set_read_record_callback(m_Connection, OnReadRecord);
        
        Debug.Log("Connection process started. Waiting for server response...");
    }

    void Update() {
        mrs_update();
        if (m_IsRunning && m_Connection != IntPtr.Zero) {
            // Send messages periodically
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
        
        // Send message to server
        bool success = mrs_write_record(m_Connection, 0, 0x01, messageBytes, (UInt32)messageBytes.Length);
        Debug.Log($"Message sent: {(success ? "Success" : "Failed")} {message}");
        m_MessagesSent++;
    }

    // Callback function when connection is ready
    [AOT.MonoPInvokeCallback(typeof(MrsConnectCallback))]
    private static void OnConnect(MrsConnection connection, IntPtr connection_data) {
        Debug.Log("Connected to server");
    }

    // Callback function on disconnect
    [AOT.MonoPInvokeCallback(typeof(MrsDisconnectCallback))]
    private static void OnDisconnect(MrsConnection connection, IntPtr connection_data) {
        Debug.Log("Disconnected from server");
    }

    // Callback function on any errors
    [AOT.MonoPInvokeCallback(typeof(MrsErrorCallback))]
    private static void OnError(MrsConnection connection, IntPtr connection_data, MrsConnectionError status) {
        Debug.LogError($"Error occurred: {mrs_get_connection_error_string(status)}");
    }

    // Callback function when receiving a record
    [AOT.MonoPInvokeCallback(typeof(MrsReadRecordCallback))]
    private static void OnReadRecord(MrsConnection connection, IntPtr connection_data, UInt32 seqnum, UInt16 options, UInt16 payload_type, IntPtr _payload, UInt32 payload_len) {
        Debug.Log($"Record received: seqnum={seqnum} options=0x{options:X2} payload_type=0x{payload_type:X2} payload_len={payload_len}");
        
        // Convert IntPtr to byte array
        byte[] payload = new byte[payload_len];
        Marshal.Copy(_payload, payload, 0, (int)payload_len);
        
        // Display the payload if it's text
        if (payload_type == 0x01) {
            string response = System.Text.Encoding.UTF8.GetString(payload, 0, (int)payload_len);
            Debug.Log($"Response from server: {response}");
        }
    }

    void OnDestroy() {
        if (m_Connection != IntPtr.Zero) {
            mrs_close(m_Connection);
            m_Connection = IntPtr.Zero;
        }
    }
}
