#include "mrs.hpp"
#include <stdio.h>
#include <vector>
#include <algorithm>

// Global variable to manage client connections
static std::vector<MrsConnection> g_clients;

void on_connect(MrsConnection connection) {
    fprintf(stderr, "Client connected: %p\n", connection);
}

void on_disconnect(MrsConnection connection, void* connection_data) {
    fprintf(stderr, "Client disconnected: %p\n", connection);
    
    // Remove disconnected client from the list
    auto it = std::find(g_clients.begin(), g_clients.end(), connection);
    if (it != g_clients.end()) {
        g_clients.erase(it);
        fprintf(stderr, "Client removed from list. Current connections: %zu\n", g_clients.size());
    }
}

void on_error(MrsConnection connection, void* connection_data, MrsConnectionError status) {
    fprintf(stderr, "Error occurred: %s\n", mrs_get_error_string(mrs_get_last_error()));
    
    // Remove client with error from the list
    if (connection) {
        auto it = std::find(g_clients.begin(), g_clients.end(), connection);
        if (it != g_clients.end()) {
            g_clients.erase(it);
            fprintf(stderr, "Client removed from list due to error. Current connections: %zu\n", g_clients.size());
        }
    }
}

// Callback function when receiving a record from client
void on_read_record(MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len) {
    fprintf(stderr, "Record received: seqnum=%u, options=0x%02X, payload_type=0x%02X, payload_len=%u\n", 
            seqnum, options, payload_type, payload_len);

    switch (payload_type) {
    case 0x01:
        fprintf(stderr, "Type 0x01 record received: '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);
        
        // Broadcast to all clients
        for (auto client : g_clients) {
            // Don't send back to the source client
            if (client != connection) {
                mrs_write_record(client, options, payload_type, payload, payload_len);
            }
        }
        break;
    default:
        break;
    }
}

// Callback function to handle new client connections
void on_new_connection(MrsServer server, void* server_data, MrsConnection client) {
    fprintf(stderr, "New client connection: %p\n", client);
    
    // Set callback functions
    mrs_set_disconnect_callback(client, on_disconnect);
    mrs_set_error_callback(client, on_error);
    mrs_set_read_record_callback(client, on_read_record);
    
    // Add to client list
    g_clients.push_back(client);
    fprintf(stderr, "Client added to list. Current connections: %zu\n", g_clients.size());
    
    on_connect(client);
}

int main(int argc, char** argv) {
    // Initialize MRS library
    mrs_initialize();

    // Create TCP server
    MrsServer tcp_server = NULL;
    tcp_server = mrs_server_create(MRS_CONNECTION_TYPE_TCP, "0.0.0.0", 22222, 10); // Listen address and port
    if (!tcp_server) {
        fprintf(stderr, "TCP server creation error: %s\n", mrs_get_error_string(mrs_get_last_error()));
        return 1;
    }

    // Set callback functions
    mrs_server_set_new_connection_callback(tcp_server, on_new_connection);
    mrs_set_error_callback(tcp_server, on_error);
    fprintf(stderr, "Broadcast server starting...\n");

    // Main loop
    bool done = false;
    while (!done) {
        mrs_update();
        mrs_sleep(1); // 1ms sleep (about 1000 loops per second)
    };
    
    // Cleanup
    mrs_close(tcp_server);
    mrs_finalize();
    return 0;
}
