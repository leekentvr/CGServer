// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#include <array>
#include <iostream>
#include <map>
#include <vector>
#include <thread>
#include <string>
#include <stdio.h>                  // For string handling
#include <stdlib.h>                 // For string handling
#include <string.h>                 // For string handling
#include <fstream>                  // For file operations
#include <chrono>                   // For timestamps
#include <winsock2.h>                // For UDP / TCP
#include <Ws2tcpip.h>               // For UDP / TCP
#include <mutex>
#include <condition_variable>
#include <queue> 

using namespace std;

#pragma comment(lib,"ws2_32.lib")   //Winsock Library

#define PORT 33333	//The port on which to listen for incoming data 

//#define PORT 8844	// Use when local server

#define MAX_CLIENTS 12
SOCKET clientSockets[MAX_CLIENTS]; // Array to hold client sockets

int clientCount = 0; // Current number of connected clients
CRITICAL_SECTION cs; // Critical section for thread safety

#define REDCOLOUR   "\x1B[31m"
#define GREENCOLOUR   "\x1B[32m"
#define YELLOWCOLOUR   "\x1B[33m"
#define MAGENTACOLOUR   "\x1B[35m"

#define RESETCOLOUR "\x1B[0m"

// Toggle functions
bool SENDJOINTSVIATCP = true;       // Send joints via TCP
bool RECORDTIMESTAMPS = false;           // logs timestamps to outputFile

const char* pkt = "\nMessage to be sent";
sockaddr_in dest;

bool isServerShuttingDown = false;

SOCKET serverSocket, clientSocket;
//#define BUFFER_SIZE 1024 //Max length of buffer
#define BUFFER_SIZE 2048 //Max length of buffer

std::map<SOCKET, std::string> clientNames; // Map of client names

std::queue<std::pair<SOCKET, std::vector<char>>> packetQueue; // Ensure this line ends with a semicolon
std::mutex queueMutex;
std::condition_variable queueCondition;


void EnqueuePacket(SOCKET clientSocket, const char* buffer, int length) {
    std::lock_guard<std::mutex> lock(queueMutex);
    packetQueue.push({ clientSocket, std::vector<char>(buffer, buffer + length) });
	//printf("\nEnqueued %d bytes", length);
    queueCondition.notify_one();
}

std::pair<SOCKET, std::vector<char>> DequeuePacket() {
    std::unique_lock<std::mutex> lock(queueMutex);
    queueCondition.wait(lock, [] { return !packetQueue.empty(); });
    auto packet = packetQueue.front();
    packetQueue.pop();
	//printf("\nDequeued %d bytes", packet.second.size());
    return packet;
}

DWORD WINAPI PacketProcessor(LPVOID lpParam) {
    printf(GREENCOLOUR "\nPacket Processor Thread Started" RESETCOLOUR);
    while (!isServerShuttingDown) {
        auto packet = DequeuePacket();
        SOCKET clientSocket = packet.first;
        std::vector<char> buffer = packet.second;
        int bytesReceived = buffer.size();

        // What is the received event
        int thisevent = (buffer[1] << 8) | buffer[0];
        // What is the total packet size
        int packetStringLength = (buffer[3] << 8) | buffer[2];


        // Validate packetStringLength
        if (packetStringLength <= 0 || packetStringLength - 1 > BUFFER_SIZE - 4) {
            printf(REDCOLOUR "skip %d" RESETCOLOUR, thisevent);
            continue;
        }

        // To assign client name
        std::string clientName = clientNames[clientSocket];

        //printf("%s", buffer.data());
        // Convert buffer to string, ignoring first 5 bytes
        std::string receivedData(buffer.data() + 4, packetStringLength - 1);

        int newRoomID = (buffer[5] << 8) | buffer[4];

        switch (thisevent) {
        case 0: // EVENT 0: New Skeleton. Send to all HMDs
            printf("0");
            break;
        case 1: // EVENT 1: HMD Position goes to everyone
            printf("1");
            break;
        case 2: // EVENT 2: Simple Debug String, goes to everyone
            printf("\n2");
            printf("%s: %s\n", clientName.c_str(), receivedData.c_str());
            break;
        case 3: // EVENT 3: IdentifySelfToServer
            clientNames[clientSocket] = clientName + clientNames[clientSocket]; // Assign the name to the client
            printf("\n3,IdentifySelfToServer: %s: %s.\n", clientNames[clientSocket].c_str(), receivedData.c_str());
            break;
        case 4: // EVENT 4: NewRoom
            printf("\n4,NewRoom: %s: %d.\n", clientName.c_str(), newRoomID);
            break;
        default:
            break;
        }

        bool SendToSelf = false;

        // Broadcast message to all clients
        EnterCriticalSection(&cs);
        for (int i = 0; i < clientCount; i++) {
            if (clientSockets[i] != clientSocket || SendToSelf == true) { // Don't send back to the sender
                // If the client is not an Areamanager (or is a HMDevent which goes to all AM) then send it
                if (clientNames[clientSocket].find("AreaManager") == std::string::npos || thisevent == 1) {
                    send(clientSockets[i], buffer.data(), bytesReceived, 0);
                }
				else {
					printf("\nSkipping %s", clientNames[clientSocket].c_str());
				}
            }
        }
        LeaveCriticalSection(&cs);
    }

    printf(REDCOLOUR "\nProcessing thread stopped, this should not happen" RESETCOLOUR);
    return 0;
}

DWORD WINAPI ClientHandler(LPVOID lpParam) {
    SOCKET clientSocket = (SOCKET)lpParam;
    std::string clientName = clientNames[clientSocket];
    char buffer[BUFFER_SIZE];

    while (!isServerShuttingDown) {
        memset(buffer, 0, BUFFER_SIZE);
        int bytesReceived = recv(clientSocket, buffer, BUFFER_SIZE, 0);
        if (bytesReceived == SOCKET_ERROR) {
            printf(REDCOLOUR "\nReceive failed or sudden disconnect : %d" RESETCOLOUR, WSAGetLastError());
            break;
        }
        else if (bytesReceived == 0) {
            printf(REDCOLOUR "\nClient disconnected." RESETCOLOUR);
            break;
        }
        else if (bytesReceived > BUFFER_SIZE) {
            printf(REDCOLOUR "\nPacket size exceeds buffer limit." RESETCOLOUR);
            continue;
        }
        else {
            //printf("\nReceived %d bytes", bytesReceived);
            EnqueuePacket(clientSocket, buffer, bytesReceived);
        }
    }

    EnterCriticalSection(&cs);
    for (int i = 0; i < clientCount; i++) {
        if (clientSockets[i] == clientSocket) {
            clientSockets[i] = clientSockets[--clientCount];
            clientNames.erase(clientSocket);
            break;
        }
    }
    LeaveCriticalSection(&cs);

    closesocket(clientSocket);
    return 0;
}


// Function to accept incoming connections in a separate thread
DWORD WINAPI AcceptConnections(LPVOID lpParam) {
    SOCKET serverSocket = (SOCKET)lpParam;
    SOCKET clientSocket;
    struct sockaddr_in clientAddr;
    int addrLen = sizeof(clientAddr);

    printf("\nStart Accepting Connections");

    while (!isServerShuttingDown) {

        printf("\nWaiting for client connection...");

        clientSocket = accept(serverSocket, (struct sockaddr*)&clientAddr, &addrLen);
        if (clientSocket == INVALID_SOCKET) {
            continue; // Continue accepting other clients
        }

        printf(GREENCOLOUR "\nClient connected!" RESETCOLOUR);

        // Assign a name to the client
        std::string clientName = std::to_string(clientCount + 1);

        // Add the client socket to the list
        EnterCriticalSection(&cs);
        if (clientCount < MAX_CLIENTS) {
            clientSockets[clientCount++] = clientSocket; // Add the new client
            clientNames[clientSocket] = clientName; // Assign the name to the client
        }
        else {
            printf(REDCOLOUR "\nMax clients reached. Connection refused." RESETCOLOUR);
            closesocket(clientSocket); // Reject connection
        }
        LeaveCriticalSection(&cs);

        // Create a thread to handle the client
        HANDLE threadHandle = CreateThread(NULL, 0, ClientHandler, (LPVOID)clientSocket, 0, NULL);
        if (threadHandle == NULL) {
            fprintf(stderr, "\nFailed to create thread: %d", GetLastError());
            closesocket(clientSocket); // Close the socket if thread creation failed
        }
        else {
            CloseHandle(threadHandle); // Close the thread handle in the main thread
        }
    }
    return 0;
}

void DisconnectAllClients() 
{
    isServerShuttingDown = true;

    EnterCriticalSection(&cs);
    for (int i = 0; i < clientCount; i++) {
        shutdown(clientSockets[i], SD_BOTH); // Gracefully shutdown the connection
        closesocket(clientSockets[i]);
    }
    clientCount = 0;
    LeaveCriticalSection(&cs);
}

int main()
{
    SOCKET socketToTransmit = NULL;

    printf(GREENCOLOUR "Start App" RESETCOLOUR);

    if (SENDJOINTSVIATCP) {
        WSADATA wsaData;
        SOCKET serverSocket;
        struct sockaddr_in serverAddr;

        // Initialize Winsock
        if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
            fprintf(stderr, "\nWSAStartup failed: %d", WSAGetLastError());
            return 1;
        }

        // Create a critical section for thread safety
        InitializeCriticalSection(&cs);

        // Create a socket
        serverSocket = socket(AF_INET, SOCK_STREAM, 0);
        if (serverSocket == INVALID_SOCKET) {
            fprintf(stderr, "\nSocket creation failed: %d", WSAGetLastError());
            WSACleanup();
            return 1;
        }

        // Define server address
        serverAddr.sin_family = AF_INET;
        serverAddr.sin_addr.s_addr = INADDR_ANY; // Listen on all interfaces
        serverAddr.sin_port = htons(PORT); // Port number

        // Bind the socket
        if (bind(serverSocket, (struct sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
            fprintf(stderr, "\nBind failed: %d", WSAGetLastError());
            closesocket(serverSocket);
            WSACleanup();
            return 1;
        }

        // Start listening for incoming connections
        if (listen(serverSocket, SOMAXCONN) == SOCKET_ERROR) {
            fprintf(stderr, "\nListen failed: %d", WSAGetLastError());
            closesocket(serverSocket);
            WSACleanup();
            return 1;
        }

        printf("\nServer is listening on port %d...", PORT);

        // Create a thread to accept incoming connections
        HANDLE acceptThread = CreateThread(NULL, 0, AcceptConnections, (LPVOID)serverSocket, 0, NULL);
        if (acceptThread == NULL) {
            fprintf(stderr, "\nFailed to create accept thread: %d", GetLastError());
            DeleteCriticalSection(&cs);
            closesocket(serverSocket);
            WSACleanup();
            return 1;
        }

        // Create a thread to process packets
        HANDLE packetProcessorThread = CreateThread(NULL, 0, PacketProcessor, NULL, 0, NULL);
        if (packetProcessorThread == NULL) {
            fprintf(stderr, "\nFailed to create packet processor thread: %d", GetLastError());
            DeleteCriticalSection(&cs);
            closesocket(serverSocket);
            WSACleanup();
            return 1;
        }

    }

    // Infinite loop to keep the program running until "quit" is entered
    while (1) {
    }

    if (SENDJOINTSVIATCP) {
        // Close sockets and clean up
        isServerShuttingDown = true;

        printf("\nExiting the program.");
        DisconnectAllClients();

        //DeleteCriticalSection(&cs);
        closesocket(serverSocket);
        WSACleanup();
    }
}

