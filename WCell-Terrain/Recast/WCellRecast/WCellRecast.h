#ifndef WCELLRECAST_H
#define WCELLRECAST_H

void (*BuildMeshCallback)(void* buildArg, int tileCount, const dtNavMeshParams* params);

extern "C" {
	 void buildMesh(void* buildArg, int vertCount, const float* verts, float minh, float maxh);

	 void 
}

#endif