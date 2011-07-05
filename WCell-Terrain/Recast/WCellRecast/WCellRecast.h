#ifndef WCELLRECAST_H
#define WCELLRECAST_H

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include "Recast.h"
#include "InputGeom.h"
#include "DetourNavMesh.h"
#include "DetourNavMeshQuery.h"
#include "DetourNavMeshBuilder.h"

extern "C" {

	typedef void (__cdecl *BuildMeshCallback)(
		int userId, 
		int vertComponentCount, 
		int polyCount,
		const float* verts, 

		int totalPolyIndexCount,				// count of all vertex indices in all polys
		const unsigned char* polyIndexCounts,
		const unsigned int* polyVerts,
		const unsigned int* polyNeighbors,
		const unsigned short* polyFlags,
		const unsigned char* polyAreasAndTypes
		);

	__declspec( dllexport ) int __cdecl buildMeshFromFile(
		int userId, 
		const char* inputFilename, 
		const char* navmeshFilename, 
		BuildMeshCallback callback);

	//__declspec( dllexport ) void buildMesh(int userId, int vertCount, const float* verts, float minh, float maxh, BuildMeshCallback callback);


	// copied from Sample_TileMesh.cpp

	static const int NAVMESHSET_MAGIC = 'M'<<24 | 'S'<<16 | 'E'<<8 | 'T'; //'MSET';
	static const int NAVMESHSET_VERSION = 1;

	struct NavMeshSetHeader
	{
		int magic;
		int version;
		int numTiles;
		dtNavMeshParams params;
	};

	struct NavMeshTileHeader
	{
		dtTileRef tileRef;
		int dataSize;
	};

	void saveMesh(const char* filename, const dtNavMesh* mesh);

	dtNavMesh* loadMesh(const char* path);
}

class WCellBuildContext;

dtNavMesh* buildMesh(InputGeom* geom, WCellBuildContext* ctx);

unsigned char* buildTileMesh(const int tx, const int ty, 
	const float* bmin, const float* bmax, int& dataSize,
	InputGeom* geom,
	rcConfig& cfg,
	rcContext* ctx);



// ################################################################################################
// some stuff we had to copy&paste to make things work

#ifdef __GNUC__
#include <stdint.h>
typedef int64_t TimeVal;
#else
typedef __int64 TimeVal;
#endif

class WCellBuildContext : public rcContext
{
	TimeVal m_startTime[RC_MAX_TIMERS];
	int m_accTime[RC_MAX_TIMERS];

	static const int MAX_MESSAGES = 1000;
	const char* m_messages[MAX_MESSAGES];
	int m_messageCount;
	static const int TEXT_POOL_SIZE = 8000;
	char m_textPool[TEXT_POOL_SIZE];
	int m_textPoolSize;

public:
	WCellBuildContext() :
	  m_messageCount(0),
		  m_textPoolSize(0)
	  {
		  resetTimers();
	  }

	  virtual ~WCellBuildContext()
	  {
	  }

	  /// Dumps the log to stdout.
	  void dumpLog(const char* format, ...) {}
	  /// Returns number of log messages.
	  int getLogCount() const {}
	  /// Returns log message text.
	  const char* getLogText(const int i) const { return 0; }

protected:	
	/// Virtual functions for custom implementations.
	///@{
	virtual void doResetLog() {}
	virtual void doLog(const rcLogCategory /*category*/, const char* /*msg*/, const int /*len*/) {}
	virtual void doResetTimers() {}
	virtual void doStartTimer(const rcTimerLabel /*label*/) {}
	virtual void doStopTimer(const rcTimerLabel /*label*/) {}
	virtual int doGetAccumulatedTime(const rcTimerLabel /*label*/) const { return 0; }
	///@}
};


inline unsigned int nextPow2(unsigned int v)
{
	v--;
	v |= v >> 1;
	v |= v >> 2;
	v |= v >> 4;
	v |= v >> 8;
	v |= v >> 16;
	v++;
	return v;
}

inline unsigned int ilog2(unsigned int v)
{
	unsigned int r;
	unsigned int shift;
	r = (v > 0xffff) << 4; v >>= r;
	shift = (v > 0xff) << 3; v >>= shift; r |= shift;
	shift = (v > 0xf) << 2; v >>= shift; r |= shift;
	shift = (v > 0x3) << 1; v >>= shift; r |= shift;
	r |= (v >> 1);
	return r;
}

#endif