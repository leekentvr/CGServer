ifeq ($(shell uname),Darwin)
MACOS_ARCH=x86_64
ARCH=arch -$(MACOS_ARCH)
GPP=$(ARCH) g++
MACVERFLAG=-mmacosx-version-min=12 #10.13
MACDLLFLAGS=-fembed-bitcode -arch $(MACOS_ARCH)
LIBDIR=./mrs_2.0.0/mrs/library/mrs/mac/10.14/enet_uv_openssl_1.1.1/
LIBFLAGS=$(LIBDIR)/libcrypto.a $(LIBDIR)/libenet.a   $(LIBDIR)/libmrs.a $(LIBDIR)/libssl.a $(LIBDIR)/libuv.a -lz -lldap -liconv -framework CoreFoundation -framework SystemConfiguration
CFLAGS_ADD=-I./mrs_2.0.0/mrs/include

else

OPENSSLDIR=openssl_bin/openssl_1.1.1/linux/ubuntu20
BORINGSSL=0
DLL=mrs3.so
ARCH=
GPP=g++
MACVERFLAG=
CFLAGS_ADD=-rdynamic -Icurl/ubuntu20/x86_64/curl/include
OUTLIB=libmrs3.a
OUTMOHLIB=libmoh.a
OPENSSLLIBS=$(OPENSSLDIR)/library/libssl.a $(OPENSSLDIR)/library/libcrypto.a  -ldl
LIBFLAGS=-lpthread -ldl curl/ubuntu20/x86_64/curl/lib/libcurl.a curl/ubuntu20/x86_64/libidn2/lib/libidn2.a curl/ubuntu20/x86_64/nghttp2/lib/libnghttp2.a -lz $(OPENSSLLIBS)
endif


CFLAGS=-std=c++14 -I. -g -O0 $(MACVERFLAG) $(CFLAGS_ADD) -Wall


all: echosv cgsv 

echosv: echosv.cpp
	$(GPP) -o echosv $(CFLAGS) echosv.cpp $(LIBFLAGS)
cgsv: cgsv.cpp
	$(GPP) -o cgsv $(CFLAGS) cgsv.cpp $(LIBFLAGS) 


