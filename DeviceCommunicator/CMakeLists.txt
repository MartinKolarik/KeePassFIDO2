cmake_minimum_required(VERSION 3.10)
project(DeviceCommunicator)
set(CMAKE_CXX_STANDARD 17)

link_directories(include)
include_directories(include)

add_executable(DeviceCommunicator src/main.cpp)
target_link_libraries(DeviceCommunicator fido2 cbor crypto-45)

add_custom_command(
	TARGET ${PROJECT_NAME} POST_BUILD
	COMMAND ${CMAKE_COMMAND} -E copy
	${CMAKE_CURRENT_SOURCE_DIR}/include/cbor.dll ${CMAKE_CURRENT_SOURCE_DIR}/include/crypto-45.dll ${CMAKE_CURRENT_SOURCE_DIR}/include/fido2.dll
	${CMAKE_CURRENT_BINARY_DIR}/
)

add_custom_command(
	TARGET ${PROJECT_NAME} POST_BUILD
	COMMAND ${CMAKE_COMMAND} -E copy
	${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}.exe ${CMAKE_CURRENT_BINARY_DIR}/cbor.dll ${CMAKE_CURRENT_BINARY_DIR}/crypto-45.dll ${CMAKE_CURRENT_BINARY_DIR}/fido2.dll
	${CMAKE_CURRENT_SOURCE_DIR}/../KeePassPlugin/bin/Debug/
)

add_custom_command(
	TARGET ${PROJECT_NAME} POST_BUILD
	COMMAND ${CMAKE_COMMAND} -E copy
	${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}.exe ${CMAKE_CURRENT_BINARY_DIR}/cbor.dll ${CMAKE_CURRENT_BINARY_DIR}/crypto-45.dll ${CMAKE_CURRENT_BINARY_DIR}/fido2.dll
	${CMAKE_CURRENT_SOURCE_DIR}/../KeePassPlugin/bin/Release/
)
