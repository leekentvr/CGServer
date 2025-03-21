#include "mrs.hpp"
#include <stdio.h>
#include <vector>
#include <algorithm>
#include <sstream>

// Define a class to encapsulate MrsConnection and additional data
class ClientInfo {
public:
    MrsConnection connection;
    std::string type;
    std::string localroom;
    std::vector<std::string> subscriptions;

    ClientInfo(MrsConnection conn) : connection(conn) {}

    // Equality operator to compare ClientInfo objects based on connection
    bool operator==(const MrsConnection& conn) const {
        return connection == conn;
    }
};

class RoomInfo {
public:
    std::string roomName;

    RoomInfo(std::string roomNameInternal) : roomName(roomNameInternal) {}
     
    // Equality operator to compare ClientInfo objects based on connection
    bool operator==(const std::string& roomNameInternal) const {
        return roomName == roomNameInternal;
    }
};

// Global variable to manage client connections
static std::vector<ClientInfo> g_all_clients;

void on_disconnect(MrsConnection connection, void* connection_data) {
    fprintf(stderr, "Client disconnected: %p\n", connection);
    
    // Remove disconnected client from all clients
    auto it = std::find(g_all_clients.begin(), g_all_clients.end(), connection);
    if (it != g_all_clients.end()) {
        g_all_clients.erase(it);
        fprintf(stderr, "Client removed from general list. Current connections: %zu\n", g_all_clients.size());
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
    }
}

ClientInfo* find_client(MrsConnection connection) {
	auto it = std::find(g_all_clients.begin(), g_all_clients.end(), connection);
	if (it != g_all_clients.end()) {
		return &(*it);
	}
	return nullptr;
}

// Callback function when receiving a record from client
void on_read_record(MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len) {
    fprintf(stderr, "Record received: seqnum=%u, options=0x%02X, payload_type=0x%02X, payload_len=%u\n",
        seqnum, options, payload_type, payload_len);

    switch (payload_type) {
    case 0x00: { // NewSkeleton = 0
        // Forward skeletons to all clients except the source client
        ClientInfo* thisClient = find_client(connection);
        for (const auto& client : g_all_clients) {
            if (client.connection != connection) {
                if (std::find(client.subscriptions.begin(), client.subscriptions.end(), thisClient->localroom) != client.subscriptions.end()) {
                    mrs_write_record(client.connection, options, payload_type, payload, payload_len);
                }
            }
        }
        break;
    }
    case 0x01: { // HMDPosition = 1
        // Forward HMD positions to all AreaManagers

        break;
    }
    case 0x02: { // SimpleString = 2
        // Forward strings to everyone
        fprintf(stderr, "Type 0x02 record received: '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);

        // Broadcast to all clients
        for (const auto& client : g_all_clients) {
            mrs_write_record(client.connection, options, payload_type, payload, payload_len);
        }
        break;
    }
    case 0x03: { // IdentifySelfToServer = 3
        fprintf(stderr, "Type 0x03 record received: '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);

        // Convert payload to std::string for easier manipulation
        std::string payload_str((const char*)payload, payload_len);

        // Split the payload_str by comma
		//"HMD,Unknown"  Example payload
        std::stringstream ss(payload_str);
        std::string type, localroom;
        if (std::getline(ss, type, ',') && std::getline(ss, localroom, ',')) {
            for (auto& client : g_all_clients) {
                if (client.connection == connection) {
                    client.type = type;
					// If the localroom is unknown, do not update for clientInfo
                    if (localroom != "Unknown") {
                        client.localroom = localroom;
                        fprintf(stderr, "Client type updated: '%s', local room updated: '%s'\n", type.c_str(), localroom.c_str());
					}
					else { // Room == Unknown
                        if (type == "AreaManager") {
                            fprintf(stderr, "\033[1;31mError: AreaManager must have a local room.\033[0m\n");
                        }
                    }
                    break;
                }
            }
        }
        else {
            // Handle unknown identification
            fprintf(stderr, "Received unknown identification: '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);
        }
        break;
    }
    case 0x04: { // NewRoomHost = 4
        // Forward to all clients and area managers
        for (const auto& client : g_all_clients) {
            mrs_write_record(client.connection, options, payload_type, payload, payload_len);
        }
        break;
    }
    case 0x05: { // RequestRoomList 
        std::string availableRooms;

        for (const auto& client : g_all_clients) {
			if (client.type == "AreaManager") {
                // Each AreaManager will have an assigned local room. 
                // No two should be the same
				availableRooms += client.localroom + ",";
			}
        }
        if (availableRooms.size() > 0) {
            // Send CSV string of available rooms back to requester
            mrs_write_record(connection, options, payload_type, static_cast<const void*>(availableRooms.c_str()), availableRooms.size());
        }

        break;
    }
    case 0x06: { // SubscribeToRoom Subscribe to new room
        fprintf(stderr, "Type 0x06 record received: '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);
        // Convert payload to std::string for easier manipulation
        std::string payload_str((const char*)payload, payload_len);

            // Add payload string to client subscriptions list
            for (auto& client : g_all_clients) {
                if (client.connection == connection) {
                    if (std::find(client.subscriptions.begin(), client.subscriptions.end(), payload_str) == client.subscriptions.end()) {
                        client.subscriptions.push_back(payload_str);
                        fprintf(stderr, "Client subscribed to room '%s'\n", payload_str.c_str());
                    } else {
                        fprintf(stderr, "Client already subscribed to room '%s'\n", payload_str.c_str());
                    }

                    // Print out all subscriptions
                    fprintf(stderr, "Current subscriptions: ");
                    for (const auto& subscription : client.subscriptions) {
                        fprintf(stderr, "%s ", subscription.c_str());
                    }
                    fprintf(stderr, "\n");
                    break;
                }
            }
        break;
    }

    case 0x07: { // UnsubscribeFromRoom
        fprintf(stderr, "Type 0x07 record received: '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);
        // Convert payload to std::string for easier manipulation
        std::string payload_str((const char*)payload, payload_len);

        // Remove payload string from client subscriptions list
        for (auto& client : g_all_clients) {
            if (client.connection == connection) {
                auto it = std::find(client.subscriptions.begin(), client.subscriptions.end(), payload_str);
                if (it != client.subscriptions.end()) {
                    client.subscriptions.erase(it);
                    fprintf(stderr, "Client unsubscribed from room '%s'\n", payload_str.c_str());
                }
                break;
            }
        }
        break;
    }
    default:
        fprintf(stderr, "Unknown payload type: 0x%02X\n", payload_type);
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
    g_all_clients.emplace_back(client); // Add with empty room
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
