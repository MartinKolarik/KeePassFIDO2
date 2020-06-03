#include <iostream>
#include <windows.h>
#include <filesystem>

extern "C" {
#include "fido.h"
}

// error codes
enum {
	ERR_ARGC = 1,
	ERR_PROCESS_HANDLE,
	ERR_QUERY_PROCESS_NAME,
	ERR_PROCESS_NAME,
	ERR_READ_MEMORY,
	ERR_READ_MEMORY_SIZE,
	ERR_READ_MEMORY_PLACEHOLDER,
	ERR_UNKNOWN_OPERATION,
	ERR_NO_DEVICE,
	ERR_MULTIPLE_DEVICES,
	ERR_FIDO_OP_ERROR,
	ERR_WRITE_MEMORY,
	ERR_PIN_TOO_LONG,
	ERR_ICON_LENGTH,
};

// We do not do anything with client data, so sample data
// from libfido2 are used for all operations.
static const unsigned char clientDataHash[32] = {
	0xf9, 0x64, 0x57, 0xe7, 0x2d, 0x97, 0xf6, 0xbb,
	0xdd, 0xd7, 0xfb, 0x06, 0x37, 0x62, 0xea, 0x26,
	0x20, 0x44, 0x8e, 0x69, 0x7c, 0x03, 0xf2, 0x31,
	0x2f, 0x99, 0xdc, 0xaf, 0x3e, 0x8a, 0x91, 0x6b,
};

static const char rpId[] = "KeePassFIDO2";
static const char rpName[] = "KeePassFIDO2";

static const byte userId1[] = "KeePassFIDO2UserId1";
static const byte userId2[] = "KeePassFIDO2UserId2";
static const char userName[] = "KeePassFIDO2UserName";

// The value that we expect at the beginning and at the end of the shared data structure.
// This helps to ensure that a malicious process can't use this module for a privilege escalation and overwrite memory in another process.
const BYTE DATA_PLACEHOLDER[] = "....KeePassFIDO2-placeholder....";

/**
 * First, perform security checks:
 *  - `processHandle` must point to KeePass.exe process,
 *  - `structurePointer` must point to a memory block that starts and ends with the expected value.
 *
 *  After verification, read data at `structurePointer` from `processHandle`.
 *
 * @param processHandle
 * @param structurePointer
 * @param dataRead
 * @param dataReadSize
 * @return zero on success, otherwise one of the errors codes defined at the top of the file
 */
int verifyAndReadData(HANDLE processHandle, LPVOID structurePointer, BYTE dataRead[], SIZE_T dataReadSize) {
	DWORD processPathBufferSize = 1024;
	CHAR processPathBuffer[1024];

	if (!QueryFullProcessImageNameA(processHandle, 0, processPathBuffer, &processPathBufferSize)) {
		return ERR_QUERY_PROCESS_NAME;
	}

	// process name must be KeePass.exe
	if (std::filesystem::path(processPathBuffer).filename() != "KeePass.exe") {
		return ERR_PROCESS_NAME;
	}

	SIZE_T nBytesRead = 0;
	BOOL success;

	success = ReadProcessMemory(
		processHandle,
		structurePointer,
		dataRead,
		dataReadSize,
		&nBytesRead
	);

	if (!success) {
		return ERR_READ_MEMORY;
	}

	if (nBytesRead != dataReadSize) {
		return ERR_READ_MEMORY_SIZE;
	}

	// data must start and end with the expected value
	if (memcmp(dataRead, DATA_PLACEHOLDER, 32) || memcmp(dataRead + 224, DATA_PLACEHOLDER, 32)) {
		return ERR_READ_MEMORY_PLACEHOLDER;
	}

	return 0;
}

/**
 * Write data to memory at `structurePointer` in a process identified by `processHandle`.
 * No security checks here as this should be only used after `verifyAndReadData()`
 *
 * @param processHandle
 * @param structurePointer
 * @param data
 * @param dataSize
 * @return zero on success
 */
int writeData (HANDLE processHandle, LPVOID structurePointer, BYTE data[], SIZE_T dataSize) {
	SIZE_T nBytesWritten = 0;
	BOOL success;

	success = WriteProcessMemory(
		processHandle,
		structurePointer,
		data,
		dataSize,
		&nBytesWritten
	);

	if (!success || nBytesWritten != dataSize) {
		return ERR_WRITE_MEMORY;
	}

	return 0;
}

/**
 * Discover and return an external fido device.
 *
 * @param device
 * @return zero on success
 */
int getFidoDevice(fido_dev_t *&device) {
	size_t deviceInfoListSize = 2;
	fido_dev_info_t *deviceInfoList = fido_dev_info_new(deviceInfoListSize);
	size_t deviceInfoListCount = 0;

	fido_dev_info_manifest(deviceInfoList, deviceInfoListSize, &deviceInfoListCount);

	if (deviceInfoListCount == 0) {
		return ERR_NO_DEVICE;
	} else if (deviceInfoListCount > 1) {
		// To work correctly when multiple devices are connected at the same time, we would need
		// an UI to select which device should be used, and that would require a two-way communication
		// with the C# module. Keeping it simple for now with support for only one device.
		return ERR_MULTIPLE_DEVICES;
	}

	device = fido_dev_new();

	if (fido_dev_open(device, fido_dev_info_path(deviceInfoList)) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	return 0;
}

/**
 * Create a new credential on the authenticator.
 *
 * @param device
 * @param userId
 * @param userIdLength
 * @param pin
 * @param data
 * @return zero on success
 */
int makeCredential(fido_dev_t *device, const byte userId[], size_t userIdLength, char pin[], byte data[]) {
	fido_cred_t *credential = fido_cred_new();

	// based on libfido2 docs the COSE_ES256 type has best device support:
	// https://developers.yubico.com/libfido2/Manuals/fido_cred_set_type.html
	if (fido_cred_set_type(credential, COSE_ES256) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_cred_set_clientdata_hash(credential, clientDataHash, sizeof(clientDataHash)) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_cred_set_rp(credential, rpId, rpName) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	// 6 (prefix) + 44 (base64 key length) + 1 (null) = 51 bytes
	char icon[51] = "data:,";
	memcpy(icon + 6, data + 104, 44); // 104 = offset where the key starts
	icon[50] = NULL;

	if (fido_cred_set_user(credential, userId, userIdLength, userName, userName, icon) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_cred_set_rk(credential, FIDO_OPT_TRUE) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_cred_set_uv(credential, FIDO_OPT_TRUE) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_cred_set_extensions(credential, 0) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_dev_make_cred(device, credential, pin) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	return 0;
}

/**
 * The main method that handles creating new credentials.
 *
 * @param processHandle
 * @param dataPointer
 * @return zero on success
 */
int create(HANDLE processHandle, LPVOID dataPointer) {
	SIZE_T dataSize = 256;
	BYTE data[256];
	int r;

	if ((r = verifyAndReadData(processHandle, dataPointer, data, dataSize)) != 0) {
		return r;
	}

	fido_init(0);
	fido_dev_t *device = fido_dev_new();

	if ((r = getFidoDevice(device)) != 0) {
		return r;
	}

	size_t pinLength = data[33];
	char pin[64];

	if (pinLength > 63) {
		return ERR_PIN_TOO_LONG;
	}

	memcpy(pin, &data[40], pinLength);
	pin[pinLength] = NULL;

	// The credential metadata are not returned when there's only one credential,
	// so we need to make two of them to ensure we can later read the data stored in the icon field.
	if ((r = makeCredential(device, userId1, sizeof(userId1), pin, data)) != 0) {
		memset(pin, 0, sizeof(pin));
		return r;
	}

	if ((r = makeCredential(device, userId2, sizeof(userId1), pin, data)) != 0) {
		memset(pin, 0, sizeof(pin));
		return r;
	}

	memset(pin, 0, sizeof(pin));
	return 0;
}

/**
 * The main method that handles creating retrieving data from existing credentials.
 *
 * @param processHandle
 * @param dataPointer
 * @return
 */
int get (HANDLE processHandle, LPVOID dataPointer) {
	SIZE_T dataSize = 256;
	BYTE data[256];
	int r;

	if ((r = verifyAndReadData(processHandle, dataPointer, data, dataSize)) != 0) {
		return r;
	}

	fido_init(0);
	fido_dev_t *device = fido_dev_new();

	if ((r = getFidoDevice(device)) != 0) {
		return r;
	}

	fido_assert_t *assert = fido_assert_new();

	if (fido_assert_set_clientdata_hash(assert, clientDataHash, sizeof(clientDataHash)) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_assert_set_rp(assert, rpId) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_assert_set_extensions(assert, 0) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_assert_set_up(assert, FIDO_OPT_TRUE) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	if (fido_assert_set_uv(assert, FIDO_OPT_TRUE) != FIDO_OK) {
		return ERR_FIDO_OP_ERROR;
	}

	size_t pinLength = data[33];
	char pin[64];

	if (pinLength > 63) {
		return ERR_PIN_TOO_LONG;
	}

	memcpy(pin, &data[40], pinLength);
	pin[pinLength] = NULL;

	if (fido_dev_get_assert(device, assert, pin) != FIDO_OK) {
		memset(pin, 0, sizeof(pin));
		return ERR_FIDO_OP_ERROR;
	}

	memset(pin, 0, sizeof(pin));
	auto icon = fido_assert_user_icon(assert, 1);

	if (strlen(icon) != 50) {
		return ERR_ICON_LENGTH;
	}

	if ((r = writeData(processHandle, (byte *) dataPointer + 32, (BYTE *) icon + 6, 44)) != 0) {
		return r;
	}

	return 0;
}

int main(int argc, char *argv[]) {
	// argv[1] = caller PID
	// argv[2] = operation name
	// argv[3] = data structure pointer
	if (argc != 4) {
		return ERR_ARGC;
	}

	auto pid = strtoul(argv[1], nullptr, 10);
	auto operation = argv[2];
	auto dataPointer = reinterpret_cast<LPVOID>(strtoull(argv[3], nullptr, 10));
	auto processHandle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);

	if (!processHandle) {
		return ERR_PROCESS_HANDLE;
	}

	if (!strcmp(operation, "create")) {
		return create(processHandle, dataPointer);
	} else if (!strcmp(operation, "get")) {
		return get(processHandle, dataPointer);
	}

	return ERR_UNKNOWN_OPERATION;
}
