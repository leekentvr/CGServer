#include "mrs.hpp"
#include <stdio.h>
#include <string.h>
#include <iostream>

// Callback function on connection is ready.
void on_connect(MrsConnection connection, void* connection_data) {
    fprintf(stderr, "Connected to server\n");
}

// Callback function on disconnect.
void on_disconnect(MrsConnection connection, void* connection_data) {
    fprintf(stderr, "Disconnected from server\n");
}

// Callback function on any errors.
void on_error(MrsConnection connection, void* connection_data, MrsConnectionError status) {
    fprintf(stderr, "Error occurred: %s\n", mrs_get_connection_error_string(status));
}

// Callback function when receiving a record.
void on_read_record(MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len) {
    fprintf(stderr, "Record received: seqnum=%u options=0x%02X payload_type=0x%02X payload_len=%u\n", 
            seqnum, options, payload_type, payload_len);
    
    // Display the payload if it's text
    if (payload_type == 0x01) {
        fprintf(stdout, "Response from server: %.*s\n", payload_len, (const char*)payload);
    }
}


int main(int argc, char** argv) {
    const char* host = "127.0.0.1";
    int port = 5333;

    if (argc > 1) host = argv[1];
    if (argc > 2) port = atoi(argv[2]);
    
    mrs_initialize();
    
    // Create TCP client connection
    MrsConnection connection = mrs_connect(MRS_CONNECTION_TYPE_TCP, host, port, 5000); // timeout 5000msec
    if (!connection) {
        fprintf(stderr, "Connection error: %s\n", mrs_get_error_string(mrs_get_last_error()));
        mrs_finalize();
        return 1;
    }
    
    // Set callback functions
    mrs_set_connect_callback(connection, on_connect);
    mrs_set_disconnect_callback(connection, on_disconnect);
    mrs_set_error_callback(connection, on_error);
    mrs_set_read_record_callback(connection, on_read_record);
    
    fprintf(stderr, "Connecting to %s:%d...\n", host, port);
    
    // Main loop
    const char* message = "hello";
    bool running = true;
    int loop_count = 0;
    int messages_sent = 0;
    
    fprintf(stderr, "Sending 'hello' message every 1000 loops (approximately every second)...\n");
    
    while (running && messages_sent < 10) { // Exit after sending 10 messages
        mrs_update();
        
        // Send message every 1000 loops
        if (loop_count >= 1000) {
            loop_count = 0;
            
            // Check if connection is established
            if (mrs_connection_is_connected(connection)) {
                // Send message to server
                int ret=mrs_write_record(connection, 0, 0x01, message, 6); // Length of "hello" is 5, but including NULL terminator it's 6 bytes
                fprintf(stderr, "Message sent: xx:%d %s\n", ret, message);
                messages_sent++;
            } else {
                fprintf(stderr, "Connection not established, cannot send message\n");
            }
        }

        mrs_sleep(1); // 1ms sleep
        loop_count++;
    }

    mrs_close(connection);
    mrs_finalize();
    
    return 0;
}
