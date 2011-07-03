#include <Windows.h>
#include <strsafe.h>

#include <string>
#include <cstdlib>
#include <iostream>

using namespace std;

void ErrorExit(LPTSTR lpszFunction) 
{ 
	// Retrieve the system error message for the last-error code

	LPVOID lpMsgBuf;
	LPVOID lpDisplayBuf;
	DWORD dw = GetLastError(); 

	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | 
		FORMAT_MESSAGE_FROM_SYSTEM |
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		dw,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR) &lpMsgBuf,
		0, NULL );

	// Display the error message and exit the process

	lpDisplayBuf = (LPVOID)LocalAlloc(LMEM_ZEROINIT, 
		(lstrlen((LPCTSTR)lpMsgBuf) + lstrlen((LPCTSTR)lpszFunction) + 40) * sizeof(TCHAR)); 
	StringCchPrintf((LPTSTR)lpDisplayBuf, 
		LocalSize(lpDisplayBuf) / sizeof(TCHAR),
		TEXT("%s (#%d): %s"), 
		lpszFunction, dw, lpMsgBuf); 
	MessageBox(NULL, (LPCTSTR)lpDisplayBuf, TEXT("Error"), MB_OK); 

	LocalFree(lpMsgBuf);
	LocalFree(lpDisplayBuf);
	ExitProcess(dw); 
}

// Functor types
typedef void (* EmptyVoidCallback)();
typedef bool (* EmptyBoolCallback)();

typedef bool (* PtrBoolCallback)(void *);
typedef void (__stdcall * PtrVoidCallback)(void *);

typedef void (* genMeshCallback)(void *geom, float *vertices, int vcount, int *triangles, int tcount, const char* name);
typedef void (* meshGenAddCallback)(const char* name, PtrVoidCallback cb);

HINSTANCE dllHandle;

FARPROC getProc(LPCSTR name) {
	FARPROC functor = GetProcAddress(HMODULE(dllHandle), name);
	if (functor == NULL) {
		wchar_t wname[255];
		int len = strlen(name);
		len = mbtowc(wname, name, len);	// odd stuff
		if (len == -1) {
			perror("Could not convert name");
			ErrorExit(L"Failed to find procedure");
		}
		else {
			wname[len] = 0;
			basic_string<TCHAR> str =  basic_string<TCHAR>(L"Failed to find procedure \"") + wname + L"\" in library";
			ErrorExit(LPTSTR(str.c_str()));
		}
	}
	return functor;
}

#define CallExtern(name) EmptyVoidCallback(getProc(name))

/**
* Some test data
*/
const int vcount = 10;
const int tcount = 7;
float verts[] = {
	32.471557617, 31.175949097, 3.788104773,
	31.950048447, 31.175949097, 2.338263273,
	31.688222885, 31.175949097, 0.819886208,
	34.222843170, 31.175949097, 6.309397697,
	33.236907959, 31.175949097, 5.125359535,
	45.539302826, 31.175949097, 7.342514992,
	39.693061829, 31.175949097, 8.885383606,
	36.730842590, 31.175949097, 8.079663277,
	35.399402618, 31.175949097, 7.304241180,
	38.176704407, 31.175949097, 8.612103462
};
int triangles[] = {
	1, 2, 3,
	4, 5, 1,
	6, 7, 4,
	7, 8, 4,
	8, 9, 4,
	7, 10, 8,
	4, 1, 3
};

void __stdcall stubGen(void * geom) {
	genMeshCallback(getProc("genMesh"))(geom, verts, vcount, triangles, tcount, "[Test]");
}

void _init() {
	//meshGenAddCallback(getProc("meshGenAdd"))("[Test]", stubGen);
}

extern "C" {
	typedef void (*BuildMeshCallback)(
		int userId, 
		int vertComponentCount, 
		int polyCount, 
		float* verts, 

		int totalPolyIndexCount,				// count of all vertex indices in all polys
		unsigned char* polyIndexCounts,
		unsigned int* polyVerts,
		unsigned int* polyNeighbors,
		unsigned short* polyFlags,
		unsigned char* polyAreasAndTypes);


	typedef unsigned char (*buildMeshFromFileCb)(
			int userId,
			const char* inputFilename, 
			const char* navmeshFilename, 
			BuildMeshCallback callback);

	void onMesh(
		int userId, 
		int vertComponentCount, 
		int polyCount, 
		float* verts, 

		int totalPolyIndexCount,				// count of all vertex indices in all polys
		unsigned char* polyIndexCounts,
		unsigned int* polyVerts,
		unsigned int* polyNeighbors,
		unsigned short* polyFlags,
		unsigned char* polyAreasAndTypes);

	typedef void (*mainFunc)(int argc, char** args);
}

int main(int argc, char** argv) {
	dllHandle = LoadLibrary(L"Recast.dll");
	if (dllHandle == NULL) {
		ErrorExit(L"Failed to load library");
	}

	_init();

	// void buildMeshFromFile(int userId, const char* inputFilename, const char* navmeshFilename, BuildMeshCallback callback) 
	buildMeshFromFileCb cb = (buildMeshFromFileCb)getProc("buildMeshFromFile");
	cb(123, "../../../Run/Content/Maps/RecastInput/EasternKingdoms_tile_49_36.obj", "../../../Run/Content/Maps/RecastNavMeshes/EasternKingdoms_tile_49_36.nav", onMesh);

	mainFunc runit = (mainFunc)getProc("runit");
	runit(0, 0);

	FreeLibrary(dllHandle);

	return 0;
}
void onMesh(
	int userId, 
	int vertComponentCount, 
	int polyCount, 
	float* verts, 

	int totalPolyIndexCount,				// count of all vertex indices in all polys
	unsigned char* polyIndexCounts,
	unsigned int* polyVerts,
	unsigned int* polyNeighbors,
	unsigned short* polyFlags,
	unsigned char* polyAreasAndTypes) {
		float vertts[15];

		memcpy(vertts, verts, sizeof(float) * 15);
}