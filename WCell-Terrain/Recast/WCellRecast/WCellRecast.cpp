#ifndef WCELLRECAST_CPP
#define WCELLRECAST_CPP

#include <stdio.h>
#include <stdlib.h>
#include "SampleInterfaces.h"
#include "InputGeom.h"
#include "Sample_TileMesh.h"

void buildMesh(int vertCount, const float* verts, float minh, float maxh) 
{
	Sample_TileMesh mesh;
	InputGeom geom;
	BuildContext ctx;

	// Create input mesh
	// Note: The area parameter is in fact just a color for rendering
	geom.addConvexVolume(verts, vertCount, minh, maxh, 100);

	// set input mesh
	mesh.handleMeshChanged(&geom);

	// build nav-mesh
	mesh.handleBuild();
}

#endif