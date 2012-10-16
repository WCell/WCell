#ifndef WCELLRECAST_H
#define WCELLRECAST_H

#include <queue>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <iostream>
#include "Recast.h"
#include "InputGeom.h"
#include "DetourNavMesh.h"
#include "DetourNavMeshQuery.h"
#include "DetourNavMeshBuilder.h"
#include "boost/thread.hpp"

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
		BuildMeshCallback callback,
		int numCores
		);

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

dtNavMesh* buildMesh(InputGeom* geom, WCellBuildContext* wcellContext, int numCores);

unsigned char* buildTileMesh(const int tx, const int ty, 
	const float* bmin, const float* bmax, int& dataSize,
	InputGeom* geom,
	rcConfig cfg,
	rcContext* ctx
	);



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

template <typename TData>
class MonitorQueue : boost::noncopyable
{
public:
	void Push(const TData& data)
	{
		boost::mutex::scoped_lock lock(monitorMutex);
		//std::cout << "Pushing data to stack: " << &data << "\n";
		queue.push(data);
		itemAvailable.notify_one();
	}

	TData PopWait()
	{
		boost::mutex::scoped_lock lock(monitorMutex);
		if (queue.empty())
		{
			itemAvailable.wait(lock);
		}

		TData temp = queue.front();
		queue.pop();
		return temp;
	}

private:
	std::queue<TData> queue;

	boost::mutex monitorMutex;
	boost::condition_variable itemAvailable;
};

class TileDispatcher : boost::noncopyable
{
public:
	int maxWidth, maxHeight;
	
	void Reset()
	{
		nextWidth = 0;
		maxWidth = 0;
		nextHeight = 0;
		maxHeight = 0;
	}

	void GetNextNeededTile(int &x, int &y)
	{
		boost::mutex::scoped_lock lock(dispatchMutex);
		if (nextWidth >= maxWidth)
		{
			nextWidth = 0;
			nextHeight++;
		}

		if (nextHeight >= maxHeight)
		{
			x = -1; y = -1;
			return;
		}

		//std::cout << "NextTile: " << nextWidth << ", " << nextHeight << "\n";

		x = nextWidth++;
		y = nextHeight;
	}

	TileDispatcher()
	{
		Reset();
	}

private:
	boost::mutex dispatchMutex;
	int nextWidth, nextHeight;
};

#define INVALID_TILE -47
struct TileData
{
	int X, Y, DataSize;
	unsigned char* Data;
};

MonitorQueue<TileData> tileQueue;
TileDispatcher dispatcher;
struct QuadrantTiler
{
	InputGeom* geom;
	rcConfig cfg;
	rcContext ctx;

	void operator()()
	{
		const float* bmin = geom->getMeshBoundsMin();
		const float* bmax = geom->getMeshBoundsMax();
		const float tcs = cfg.tileSize*cfg.cs;

		int x = 0; 
		int y = 0;
		while (x != -1)
		{
			dispatcher.GetNextNeededTile(x, y);
			float tileBmin[3], tileBmax[3];
			tileBmin[0] = bmin[0] + x*tcs;
			tileBmin[1] = bmin[1];
			tileBmin[2] = bmin[2] + y*tcs;
			
			tileBmax[0] = bmin[0] + (x+1)*tcs;
			tileBmax[1] = bmax[1];
			tileBmax[2] = bmin[2] + (y+1)*tcs;
			
			int dataSize = 0;
			unsigned char* data = buildTileMesh(x, y, tileBmin, tileBmax, dataSize, geom, cfg, &ctx);
			if (data)
			{
				TileData newData;
				newData.X = x;
				newData.Y = y;
				newData.DataSize = dataSize;
				newData.Data = data;

				//std::cout << "\tNew TileData: {X = " << newData.X << ", Y = " << newData.Y << ", DataSize = " << newData.DataSize << "}\n";
				tileQueue.Push(newData);
			}
		}

		TileData doneData;
		doneData.X = INVALID_TILE;
		doneData.Y = INVALID_TILE;
		doneData.DataSize = 0;
		doneData.Data = (unsigned char*)0;

		tileQueue.Push(doneData);
	}
};

struct TileAdder
{
	dtNavMesh* mesh;
	int numThreads;

	void operator()()
	{
		int numThreadsCompleted = 0;
		std::cout << "Working";
		while(numThreadsCompleted < numThreads)
		{
			//std::cout << "**Waiting for tile data...**\n";
			TileData data = tileQueue.PopWait();
			//std::cout << "**TileData found: ";
			//Give the user some progress indicator in case of large data sets
			std::cout << "\r" << data.X << ", " << data.Y << "    ";
			if (data.X == INVALID_TILE && data.Y == INVALID_TILE)
			{
				//std::cout << "Invalid Data, one of the threads has completed**\n";
				numThreadsCompleted++;
				continue;
			}

			//std::cout << "Valid data, adding to mesh**\n";

			// Remove any previous data (navmesh owns and deletes the data).
			mesh->removeTile(mesh->getTileRefAt(data.X, data.Y, 0), 0, 0);

			// Let the navmesh own the data
			dtStatus status = mesh->addTile(data.Data, data.DataSize, DT_TILE_FREE_DATA, 0, 0);
			if (dtStatusFailed(status))
				dtFree(data.Data);
		}
		std::cout << std::endl;
		std::cout << "All quadrants finished" << std::endl;
	}
};

#endif