#!/bin/bash

case "$(uname)" in
*MINGW*)
    CWD=$(pwd -W)
    ;;
*)
    CWD=${PWD}
    ;;
esac

MRS_ROOT_DIR=${CWD}/../../..
MRS_INCLUDE_DIR=${MRS_ROOT_DIR}/include
if [ -z "${MRS_DEPS_PROJECT_NAME}" ]; then
    MRS_DEPS_PROJECT_NAME=enet_uv_openssl_1.1.1
fi

if [ -z "${MRS_PLATFORM_TYPE}" ]; then
    echo "Invalid MRS_PLATFORM_TYPE:"
    echo "linux"
    echo "mac"
    echo "windows"
    exit 1
fi

if [ -z "${MRS_PLATFORM_VERSION}" ]; then
    echo "Invalid MRS_PLATFORM_VERSION:"
    case "${MRS_PLATFORM_TYPE}" in
    windows)
        ls -1 ${MRS_ROOT_DIR}/library/mrs/${MRS_PLATFORM_TYPE}/${MRS_DEPS_PROJECT_NAME}
        ;;
    *)
        ls -1 ${MRS_ROOT_DIR}/library/mrs/${MRS_PLATFORM_TYPE}
        ;;
    esac
    exit 1
fi

case "${MRS_PLATFORM_TYPE}" in
windows)
    case "${MRS_PLATFORM_VERSION}" in
    2012)
        cmake_generator="Visual Studio 11 2012"
        ;;
    2013)
        cmake_generator="Visual Studio 12 2013"
        ;;
    2015)
        cmake_generator="Visual Studio 14 2015"
        ;;
    2017)
        cmake_generator="Visual Studio 15 2017"
        ;;
    *)
        echo "Invalid msvs_version: ${msvs_version}"
        exit 1
        ;;
    esac
    
    if [ ! -d windows ]; then
        mkdir windows
    fi
    pushd windows
        MRS_LIBRARY_DIR=${MRS_ROOT_DIR}/library/mrs/${MRS_PLATFORM_TYPE}/${MRS_DEPS_PROJECT_NAME}/${MRS_PLATFORM_VERSION}
        msvs_runtimes=$(ls ${MRS_LIBRARY_DIR})
        for msvs_runtime in ${msvs_runtimes[@]}; do
            msvs_archs=$(ls ${MRS_LIBRARY_DIR}/${msvs_runtime})
            for msvs_arch in ${msvs_archs[@]}; do
                msvs_configurations=$(ls ${MRS_LIBRARY_DIR}/${msvs_runtime}/${msvs_arch})
                for msvs_configuration in ${msvs_configurations[@]}; do
                    mrs_library_dir=${MRS_LIBRARY_DIR}/${msvs_runtime}/${msvs_arch}/${msvs_configuration}
                    
                    build_dir=${MRS_PLATFORM_VERSION}_${msvs_runtime}_${msvs_arch}_${msvs_configuration}
                    if [ -d "${build_dir}" ]; then
                        rm -fr ${build_dir}
                    fi
                    mkdir -p ${build_dir}
                    pushd ${build_dir}
                        cmake.exe -G "${cmake_generator}" -A ${msvs_arch} -DCMAKE_CONFIGURATION_TYPES=${msvs_configuration} -DMSVS_VERSION=${MRS_PLATFORM_VERSION} -DMSVS_RUNTIME=${msvs_runtime} -DMRS_INCLUDE_DIR=${MRS_INCLUDE_DIR} -DMRS_LIBRARY_DIR=${mrs_library_dir} -DMRS_PLATFORM_TYPE=${MRS_PLATFORM_TYPE} ../../..
                    popd
                done
            done
        done
    popd
    ;;
*)
    MRS_LIBRARY_DIR=${MRS_ROOT_DIR}/library/mrs/${MRS_PLATFORM_TYPE}/${MRS_PLATFORM_VERSION}/${MRS_DEPS_PROJECT_NAME}
    cmake -DMRS_INCLUDE_DIR=${MRS_INCLUDE_DIR} -DMRS_LIBRARY_DIR=${MRS_LIBRARY_DIR} -DMRS_PLATFORM_TYPE=${MRS_PLATFORM_TYPE} ..
    ;;
esac
