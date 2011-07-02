#ifndef WCELLRECAST_CPP
#define WCELLRECAST_CPP

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include "SampleInterfaces.h"
#include "InputGeom.h"
#include "Sample_TileMesh.h"


Sample_TileMesh sampler;
dtNavMesh* mesh;
dtNavMeshQuery* navMeshQuery;

void buildMesh(void* buildArg, int vertCount, const float* verts, float minh, float maxh) 
{
	InputGeom geom;
	BuildContext ctx;

	// Create input mesh
	// Note: The area parameter is in fact just a color for rendering
	geom.addConvexVolume(verts, vertCount, minh, maxh, 100);

	// set input mesh
	sampler.handleMeshChanged(&geom);

	// build nav-mesh
	sampler.handleBuild();

	mesh = sampler.getNavMesh();
	navMeshQuery = sampler.getNavMeshQuery();

	navMeshQuery->
}

#endif