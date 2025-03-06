#!/bin/bash

if [ "$(uname)" = "Darwin" ]; then
    if [ "$(which brew)" != "" ]; then
        brew install ruby
        brew install curl
        brew install lsof
    else
        echo "Unsupported package manager"
    fi
elif [ "$(which yum 2>/dev/null)" != "" ]; then
    sudo yum install -y ruby
    sudo yum install -y curl
    sudo yum install -y lsof
elif [ "$(which apt 2>/dev/null)" != "" ]; then
    sudo apt install -y ruby
    sudo apt install -y curl
    sudo apt install -y lsof
else
    echo "Unsupported package manager"
fi

