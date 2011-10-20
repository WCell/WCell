#pragma once
#ifndef DETOURNAVMESH_EXTERN_H
#define DETOURNAVMESH_EXTERN_H

#include "DetourNavMesh.h"

void notifyNavMeshAdded(const dtNavMesh *mesh, const dtNavMeshParams *prms);

void notifyTileAdded(const dtNavMesh *navMesh, const dtMeshTile *tile);

// cannot notify after every tile, since some tile information is not gonna be present
// at that time
//void notifyTileAdded(dtNavMesh *navMesh, dtMeshTile *tile);

#endif