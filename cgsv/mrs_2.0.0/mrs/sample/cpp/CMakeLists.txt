cmake_minimum_required(VERSION 2.8)

project(mrs_sample)

include(${CMAKE_CURRENT_LIST_DIR}/../../mrs_link.cmake)

add_executable(base_loop ${CMAKE_CURRENT_LIST_DIR}/source/base_loop.cpp)
target_link_libraries(base_loop ${MRS_LINK_LIBRARIES})

add_executable(log ${CMAKE_CURRENT_LIST_DIR}/source/log.cpp)
target_link_libraries(log ${MRS_LINK_LIBRARIES})

add_executable(echo_client ${CMAKE_CURRENT_LIST_DIR}/source/echo_client.cpp)
target_link_libraries(echo_client ${MRS_LINK_LIBRARIES})

add_executable(echo_server ${CMAKE_CURRENT_LIST_DIR}/source/echo_server.cpp)
target_link_libraries(echo_server ${MRS_LINK_LIBRARIES})

add_executable(base_server ${CMAKE_CURRENT_LIST_DIR}/source/base_server.cpp)
target_link_libraries(base_server ${MRS_LINK_LIBRARIES})
