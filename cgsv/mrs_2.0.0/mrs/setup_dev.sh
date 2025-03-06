#!/bin/bash

if [ "$(uname)" = "Darwin" ]; then
    if [ "$(which brew)" != "" ]; then
        brew install cmake
    else
        echo "Unsupported package manager"
    fi
elif [ "$(which yum 2>/dev/null)" != "" ]; then
    sudo yum install -y cmake gcc gcc-c++
    sudo yum install -y zlib-devel
    sudo yum install -y gdb
elif [ "$(which apt 2>/dev/null)" != "" ]; then
    sudo apt install -y cmake g++
    sudo apt install -y zlib1g-dev
    sudo apt install -y gdb
else
    echo "Unsupported package manager"
fi

