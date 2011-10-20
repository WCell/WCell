#include "DetourNavMesh.Extern.h"

#include <string.h>
#include <iostream>

using namespace std;

typedef const unsigned short cushort;
typedef const unsigned char cuchr;
typedef const unsigned int cuint;
typedef const int cint;
typedef const float cfloat;

typedef void (__stdcall * NavMeshCreatedCallback)(
	const long long navMeshId, int w, int h, const float origin[3], const float tileW, const float tileH, const int maxTiles
		);

typedef void (__stdcall * TileCreatedCallback)(
	const long long navMeshId, cint x, cint y, cint nextX, cint nextY, unsigned int flags,
	cint vCount, cfloat *verts,																															// vertices
	cint pCount, cint pvCount, cint *pflinks, cushort *pVerts, cushort *pNeighbors, cushort *pFlags, cushort *pVCounts, cuchr *pAreas, cuchr *pTypes,	// polygons
	cint lCount, cuint *lRefs, cuint *lNextLinks, cuchr *lEdges, cuchr *lSides, cuchr *lBmins, cuchr *lBmaxs,											// links between tiles
	cint omCount, cfloat *omPos, cfloat *omRads, cushort *omPolys, cuchr *omFlags, cuchr *omSides														// Offmesh Connections
	);

typedef void (__stdcall * NavMeshDoneCallback)(
	const long long navMeshId
		);


NavMeshCreatedCallback navMeshCreatedCallback = 0;
TileCreatedCallback tileCreatedCallback = 0;
NavMeshDoneCallback navMeshDoneCallback = 0;


extern "C"  __declspec(dllexport) int __stdcall navmeshGetMaxVertsPerPolygon() {
	return DT_VERTS_PER_POLYGON;
}


extern "C"  __declspec(dllexport) void __stdcall navmeshSetNavMeshCreatedCallback(NavMeshCreatedCallback cb) {
	navMeshCreatedCallback = cb;
}

extern "C"  __declspec(dllexport) void __stdcall navmeshSetTileCreatedCallback(TileCreatedCallback cb) {
	tileCreatedCallback = cb;
}

extern "C"  __declspec(dllexport) void __stdcall navmeshSetNavMeshDoneCallback(NavMeshDoneCallback cb) {
	navMeshDoneCallback = cb;
}


void notifyNavMeshAdded(const dtNavMesh *mesh, const dtNavMeshParams *params) {
	if (navMeshCreatedCallback == 0) return;

	int w = 0, h = 0;
	for (int i = 0; i < mesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = mesh->getTile(i);
		if (!tile->header) continue;
		if (tile->header->x > w) {
			w = tile->header->x;
		}
		if (tile->header->y > h) {
			h = tile->header->y;
		}
	}

	navMeshCreatedCallback((long long)mesh, w+1, h+1, params->orig, params->tileWidth, params->tileHeight, params->maxTiles);

	for (int i = 0; i < mesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = mesh->getTile(i);
		if (!tile->header) continue;
		notifyTileAdded(mesh, tile);
	}

	navMeshDoneCallback((long long)mesh);
}



void notifyTileAdded(const dtNavMesh *navMesh, const dtMeshTile *tile) {
	if (tileCreatedCallback == 0) return;
	
	// next x & y
	int nextX, nextY;
	if (tile->next != 0) {
		nextX = tile->next->header->x;
		nextY = tile->next->header->y;
	}
	else {
		nextX = -1;
		nextY = -1;
	}

	// Polygons
	int polyCount = tile->header->polyCount;
	int pvCount =  polyCount * DT_VERTS_PER_POLYGON;
	int *pFirstlinks = new int[polyCount];
	unsigned short *pVerts = new unsigned short[pvCount];
	unsigned short *pNeighbors = new unsigned short[pvCount];
	unsigned short *pFlags = new unsigned short[polyCount];
	unsigned short *pVertCounts = new unsigned short[polyCount];
	unsigned char *pAreas = new unsigned char[polyCount];
	unsigned char *pTypes = new unsigned char[polyCount];
	for (int i = 0; i < polyCount; i++) {
		dtPoly const &poly = tile->polys[i];

		pFirstlinks[i] = poly.firstLink;
		memcpy(&pVerts[i * DT_VERTS_PER_POLYGON], poly.verts, DT_VERTS_PER_POLYGON * sizeof(unsigned short));
		memcpy(&pNeighbors[i * DT_VERTS_PER_POLYGON], poly.neis, DT_VERTS_PER_POLYGON * sizeof(unsigned short));
		pFlags[i] = poly.flags;
		pVertCounts[i] = poly.vertCount;
		pAreas[i] = poly.getArea();
		pTypes[i] = poly.getType();
	}

	// links
	int lCount = tile->header->maxLinkCount;
	unsigned int *lRefs = new unsigned int[lCount];
	unsigned int *lNextLinks = new unsigned int[lCount];
	unsigned char* lEdges = new unsigned char[lCount];
	unsigned char* lSides = new unsigned char[lCount];
	unsigned char* lBMins = new unsigned char[lCount];
	unsigned char* lBMaxs = new unsigned char[lCount];
	for (int i = 0; i < lCount; i++) {
		dtLink const &link = tile->links[i];
		
		lRefs[i] = link.ref;
		lNextLinks[i] = link.next;
		lEdges[i] = link.edge;
		lSides[i] = link.side;
		lBMins[i] = link.bmin;
		lBMaxs[i] = link.bmax;
	}

	// off mesh connections
	int omCount = tile->header->offMeshConCount;
	float *omPos = new float[6 * omCount];
	float *omRads = new float[omCount];
	unsigned short *omPolys = new unsigned short[omCount];
	unsigned char *omFlags = new unsigned char[omCount];
	unsigned char *omSides = new unsigned char[omCount];
	for (int i = 0; i < omCount; i++) {
		dtOffMeshConnection const &con = tile->offMeshCons[i];

		memcpy(&omPos[i * 6], con.pos, 6 * sizeof(float));
		omRads[i] = con.rad;
		omPolys[i] = con.poly;
		omFlags[i] = con.flags;
		omSides[i] = con.side;
	}

	//cout << "native pVerts: " << pVerts[0] << ", " << pVerts[DT_VERTS_PER_POLYGON] << endl;

	tileCreatedCallback(
		(long int)navMesh, tile->header->x, tile->header->y, nextX, nextY, (unsigned int)tile->flags,
		tile->header->vertCount*3, tile->verts,
		polyCount, pvCount, pFirstlinks, pVerts, pNeighbors, pFlags, pVertCounts, pAreas, pTypes,
		lCount, lRefs, lNextLinks, lEdges, lSides, lBMins, lBMaxs,
		omCount, omPos, omRads, omPolys, omFlags, omSides
		);
	
	delete [] pVerts;
	delete [] pNeighbors;
	delete [] pFlags;
	delete [] pVertCounts;
	delete [] pAreas;
	delete [] pTypes;
	
	delete [] lRefs;
	delete [] lNextLinks;
	delete [] lEdges;
	delete [] lSides;
	delete [] lBMins;
	delete [] lBMaxs;
	
	delete [] omPos;
	delete [] omRads;
	delete [] omPolys;
	delete [] omFlags;
	delete [] omSides;
}