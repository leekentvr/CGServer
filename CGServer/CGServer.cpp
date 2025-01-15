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
#include<winsock2.h>                // For UDP / TCP
#include <Ws2tcpip.h>               // For UDP / TCP
#include <ws2tcpip.h>               // For inet_addr and other functions

#pragma comment(lib,"ws2_32.lib")   //Winsock Library

#define PORT 33333	//The port on which to listen for incoming data
//#define PORT 8844	//The port on which to listen for incoming data

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

const char* pkt = "Message to be sent\n";
sockaddr_in dest;

bool isServerShuttingDown = false;

SOCKET serverSocket, clientSocket;
#define BUFFER_SIZE 1024 //Max length of buffer

// Function to handle communication with the client
DWORD WINAPI ClientHandler(LPVOID lpParam) {
    SOCKET clientSocket = (SOCKET)lpParam;

    char buffer[BUFFER_SIZE];

    while (!isServerShuttingDown) {
        memset(buffer, 0, BUFFER_SIZE); // Clear the buffer
        int bytesReceived = recv(clientSocket, buffer, BUFFER_SIZE, 0);
        if (bytesReceived == SOCKET_ERROR) {
            printf(REDCOLOUR "Receive failed or sudden disconnect : %d\n" RESETCOLOUR, WSAGetLastError());
            break;
        }
        else if (bytesReceived == 0) {
            printf(REDCOLOUR "Client disconnected.\n" RESETCOLOUR);
            break;
        }

        // What is the recieved event
        int thisevent = (buffer[1] << 8) | buffer[0];
        // What is the total packet size
        int packetSendSize = (buffer[3] << 8) | buffer[2];

        // Create a new packet to broadcast
        std::vector<uint8_t> packetToTransmit;

        // Copy the good bytes bytes manually using a loop
        for (int i = 0; i < packetSendSize; ++i) {
            packetToTransmit.push_back(buffer[i]);
        }

        // Broadcast message to all clients
        EnterCriticalSection(&cs);
        for (int i = 0; i < clientCount; i++) {
            if (clientSockets[i] != clientSocket) { // Don't send back to the sender
                //send(clientSockets[i], buffer, bytesReceived, 0);
                send(clientSockets[i], reinterpret_cast<const char*>(buffer), packetSendSize, 0);
            }
        }
        LeaveCriticalSection(&cs);
    }

    // Remove the client socket from the list and close it
    EnterCriticalSection(&cs);
    for (int i = 0; i < clientCount; i++) {
        if (clientSockets[i] != clientSocket) {
            clientSockets[i] = clientSockets[--clientCount]; // Replace with last client
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

    printf("Start Accepting Connections\n");

    while (!isServerShuttingDown) {

        printf("Waiting for client connection...\n");

        printf("> ");
        clientSocket = accept(serverSocket, (struct sockaddr*)&clientAddr, &addrLen);
        if (clientSocket == INVALID_SOCKET) {
            //fprintf(stderr, "Accept failed: %d\n", WSAGetLastError());
            continue; // Continue accepting other clients
        }

        printf(GREENCOLOUR "Client connected!\n" RESETCOLOUR);

        // Add the client socket to the list
        EnterCriticalSection(&cs);
        if (clientCount < MAX_CLIENTS) {
            clientSockets[clientCount++] = clientSocket; // Add the new client
        }
        else {
            printf(REDCOLOUR "Max clients reached. Connection refused.\n" RESETCOLOUR);
            closesocket(clientSocket); // Reject connection
        }
        LeaveCriticalSection(&cs);

        // Create a thread to handle the client
        HANDLE threadHandle = CreateThread(NULL, 0, ClientHandler, (LPVOID)clientSocket, 0, NULL);
        if (threadHandle == NULL) {
            fprintf(stderr, "Failed to create thread: %d\n", GetLastError());
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

    printf(GREENCOLOUR "Start Thread \n" RESETCOLOUR);

    if (SENDJOINTSVIATCP) {
        WSADATA wsaData;
        SOCKET serverSocket;
        struct sockaddr_in serverAddr;

        // Initialize Winsock
        if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
            fprintf(stderr, "WSAStartup failed: %d\n", WSAGetLastError());
            return 1;
        }

        // Create a critical section for thread safety
        InitializeCriticalSection(&cs);

        // Create a socket
        serverSocket = socket(AF_INET, SOCK_STREAM, 0);
        if (serverSocket == INVALID_SOCKET) {
            fprintf(stderr, "Socket creation failed: %d\n", WSAGetLastError());
            WSACleanup();
            return 1;
        }

        // Define server address
        serverAddr.sin_family = AF_INET;
        serverAddr.sin_addr.s_addr = INADDR_ANY; // Listen on all interfaces
        serverAddr.sin_port = htons(PORT); // Port number

        // Bind the socket
        if (bind(serverSocket, (struct sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
            fprintf(stderr, "Bind failed: %d\n", WSAGetLastError());
            closesocket(serverSocket);
            WSACleanup();
            return 1;
        }

        // Start listening for incoming connections
        if (listen(serverSocket, SOMAXCONN) == SOCKET_ERROR) {
            fprintf(stderr, "Listen failed: %d\n", WSAGetLastError());
            closesocket(serverSocket);
            WSACleanup();
            return 1;
        }

        printf("Server is listening on port %d...\n", PORT);

        // Create a thread to accept incoming connections
        HANDLE acceptThread = CreateThread(NULL, 0, AcceptConnections, (LPVOID)serverSocket, 0, NULL);
        if (acceptThread == NULL) {
            fprintf(stderr, "Failed to create accept thread: %d\n", GetLastError());
            DeleteCriticalSection(&cs);
            closesocket(serverSocket);
            WSACleanup();
            return 1;
        }
    }

    char input[100]; // Buffer to store user input

    // Infinite loop to keep the program running until "quit" is entered
    while (1) {
        printf("> ");
        scanf_s("%99s", input); // Read input, limiting to 99 characters to prevent overflow    
        if (strcmp(input, "help") == 0) {
            printf("'quit' to exit program\n");
        }
        if (strcmp(input, "quit") == 0) {
            break;
        }
    }

    if (SENDJOINTSVIATCP) {
        // Close sockets and clean up
        isServerShuttingDown = true;

        printf("Exiting the program.\n");
        //DisconnectAllClients();

        //DeleteCriticalSection(&cs);
        closesocket(serverSocket);
        WSACleanup();
    }
}

