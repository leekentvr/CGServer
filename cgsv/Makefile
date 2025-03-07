ifeq ($(shell uname),Darwin)
MACOS_ARCH=x86_64
ARCH=arch -$(MACOS_ARCH)
GPP=$(ARCH) g++
MACVERFLAG=-mmacosx-version-min=12 #10.13
MACDLLFLAGS=-fembed-bitcode -arch $(MACOS_ARCH)
LIBDIR=./mrs_2.0.0/mrs/library/mrs/mac/10.14/enet_uv_openssl_1.1.1/
LIBFLAGS=$(LIBDIR)/libcrypto.a $(LIBDIR)/libenet.a   $(LIBDIR)/libmrs.a $(LIBDIR)/libssl.a $(LIBDIR)/libuv.a -lz -lldap -liconv -framework CoreFoundation -framework SystemConfiguration


else

DLL=mrs3.so
ARCH=
GPP=g++
MACVERFLAG=
CFLAGS_ADD=-rdynamic 
LIBDIR=./mrs_2.0.0/mrs/library/mrs/linux/ubuntu16_5.4.0/enet_uv_openssl_1.1.1/
LIBFLAGS=$(LIBDIR)/libmrs.a $(LIBDIR)/libenet.a $(LIBDIR)/libssl.a $(LIBDIR)/libcrypto.a $(LIBDIR)/libuv.a -lpthread -ldl -lz $(OPENSSLLIBS)
endif


CFLAGS=-std=c++14 -I. -I./mrs_2.0.0/mrs/include -g -O0 $(MACVERFLAG) $(CFLAGS_ADD) -Wall


all: echosv cgsv echocl echo_server echo_client

echosv: echosv.cpp
	$(GPP) -o echosv $(CFLAGS) echosv.cpp $(LIBFLAGS)
cgsv: cgsv.cpp
	$(GPP) -o cgsv $(CFLAGS) cgsv.cpp $(LIBFLAGS) 
echocl: echocl.cpp
	$(GPP) -o echocl $(CFLAGS) echocl.cpp $(LIBFLAGS)

echo_server: echo_server.cpp
	$(GPP) -o echo_server $(CFLAGS) echo_server.cpp $(LIBFLAGS)

echo_client: echo_client.cpp
	$(GPP) -o echo_client $(CFLAGS) echo_client.cpp $(LIBFLAGS)



