#include "mrs.hpp"
#include <stdio.h>
#include <vector>
#include <algorithm>

// Global variable to manage client connections
static std::vector<MrsConnection> g_all_clients;

// Global variable for all AreaManagers / Clients
static std::vector<MrsConnection> g_clients_areamanagers;
static std::vector<MrsConnection> g_clients_viewers;

// Define a class to encapsulate MrsConnection and additional data
class ClientInfo {
public:
    MrsConnection connection;
    std::string room;

    ClientInfo(MrsConnection conn, const std::string& roomName) : connection(conn), room(roomName) {}

    // Equality operator to compare ClientInfo objects based on connection
    bool operator==(const MrsConnection& conn) const {
        return connection == conn;
    }
};

void on_disconnect(MrsConnection connection, void* connection_data) {
    fprintf(stderr, "Client disconnected: %p\n", connection);
    
    // Remove disconnected client from all clients
    auto it = std::find(g_all_clients.begin(), g_all_clients.end(), connection);
    if (it != g_all_clients.end()) {
        g_all_clients.erase(it);
        fprintf(stderr, "Client removed from general list. Current connections: %zu\n", g_all_clients.size());
    }

    // Remove disconnected client from the area managers
    auto it = std::find(g_clients_areamanagers.begin(), g_clients_areamanagers.end(), connection);
    if (it != g_clients_areamanagers.end()) {
        g_clients_areamanagers.erase(it);
        fprintf(stderr, "Area Manager removed from list. Current area manager connections: %zu\n", g_clients_areamanagers.size());
    }

    // Remove disconnected client from all viewers
    auto it = std::find(g_clients_viewers.begin(), g_clients_viewers.end(), connection);
    if (it != g_clients_viewers.end()) {
        g_clients_viewers.erase(it);
        fprintf(stderr, "Viewer removed from list. Current viewer connections: %zu\n", g_clients_viewers.size());
    }
}

void on_error(MrsConnection connection, void* connection_data, MrsConnectionError status) {
    fprintf(stderr, "Error occurred: %s\n", mrs_get_error_string(mrs_get_last_error()));
    
    // Remove client with error from the list
    if (connection) {
        auto it = std::find(g_all_clients.begin(), g_all_clients.end(), connection);
        if (it != g_all_clients.end()) {
            g_all_clients.erase(it);
            fprintf(stderr, "Client removed from list due to error. Current connections: %zu\n", g_all_clients.size());
        }

        // Remove disconnected client from the area managers
        auto it = std::find(g_clients_areamanagers.begin(), g_clients_areamanagers.end(), connection);
        if (it != g_clients_areamanagers.end()) {
            g_clients_areamanagers.erase(it);
            fprintf(stderr, "Area Manager removed from list due to error. Current area manager connections: %zu\n", g_clients_areamanagers.size());
        }

        // Remove disconnected client from all viewers
        auto it = std::find(g_clients_viewers.begin(), g_clients_viewers.end(), connection);
        if (it != g_clients_viewers.end()) {
            g_clients_viewers.erase(it);
            fprintf(stderr, "Viewer removed from list due to error. Current viewer connections: %zu\n", g_clients_viewers.size());
        }
    }
}

// Callback function when receiving a record from client
void on_read_record(MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len) {
    fprintf(stderr, "Record received: seqnum=%u, options=0x%02X, payload_type=0x%02X, payload_len=%u\n", 
            seqnum, options, payload_type, payload_len);

    switch (payload_type) {
        case 0x00: { // NewSkeleton = 0
            // Forward skeletons to all clients. Not to Area Managers.
            for (auto client : g_all_clients) {
                // Don't send back to the source client
                if (client != connection) {
                    mrs_write_record(client, options, payload_type, payload, payload_len);
                }
            }
            break;
        }
        case 0x01: { // HMDPosition = 1
            // Forward HMD positions to all AreaManagers
            for (auto client : g_clients_areamanagers) {
                if (client != connection) {
                    mrs_write_record(client, options, payload_type, payload, payload_len);
                }
            }
            break;
        }
        case 0x02: { // SimpleString = 2
            // Forward strings to everyone
            fprintf(stderr, "Type 0x02 record received: '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);

            // Broadcast to all clients
            for (auto client : g_all_clients) {
                mrs_write_record(client, options, payload_type, payload, payload_len);
            }
            break;
        }
        case 0x03: { // IdentifySelfToServer = 3
            fprintf(stderr, "Type 0x03 record received: '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);

            // Convert payload to std::string for easier manipulation
            std::string payload_str((const char*)payload, payload_len);

            // Check if the payload contains specific substrings
            if (payload_str.find("AreaManager") != std::string::npos) {
                // Add to area managers list if not already present
                if (std::find(g_clients_areamanagers.begin(), g_clients_areamanagers.end(), connection) == g_clients_areamanagers.end()) {
                    g_clients_areamanagers.push_back(connection);
                    fprintf(stderr, "Client added to Area Managers list. Current area manager connections: %zu\n", g_clients_areamanagers.size());
                }
            } else if (payload_str.find("Viewer") != std::string::npos) {
                // Add to viewers list if not already present
                if (std::find(g_clients_viewers.begin(), g_clients_viewers.end(), connection) == g_clients_viewers.end()) {
                    g_clients_viewers.push_back(connection);
                    fprintf(stderr, "Client added to Viewers list. Current viewer connections: %zu\n", g_clients_viewers.size());
                }
            } else {
                // Handle unknown identification
                fprintf(stderr, "Received unknown identification: '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);
            }
            break;
        }
        case 0x04: { // NewRoomHost = 4
            // Forward to all clients and area managers.
            for (auto client : g_all_clients) {
                mrs_write_record(client, options, payload_type, payload, payload_len);
            }
            for (auto client : g_clients_areamanagers) {
                mrs_write_record(client, options, payload_type, payload, payload_len);
            }
            break;
        }
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
    g_all_clients.push_back(client);
    fprintf(stderr, "Client added to list. Current connections: %zu\n", g_all_clients.size());
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
