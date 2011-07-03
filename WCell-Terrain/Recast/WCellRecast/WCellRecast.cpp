#ifndef WCELLRECAST_CPP
#define WCELLRECAST_CPP

#include "WCellRecast.h"
#include <math.h>
#include <assert.h>

#define CleanupAfterBuild() { \
	if (mesh != 0) { dtFreeNavMesh(mesh); } \
	}

int __cdecl buildMeshFromFile(int userId, const char* inputFilename, const char* navmeshFilename, BuildMeshCallback callback) 
{
	BuildContext ctx;
	dtNavMesh* mesh = NULL;

	if (navmeshFilename) {
		// load navmesh from file
		mesh = loadMesh(navmeshFilename);
	}
	
	if (!mesh) { 
		InputGeom geom;
		dtNavMeshQuery* navMeshQuery;

		// no navmesh has been loaded
		// Load input mesh
		if (!geom.loadMesh(&ctx, inputFilename)) {
			return 0;
		}

		// build nav-mesh
		mesh = buildMesh(&geom, &ctx);
		if (!mesh) {
			// building failed
			return 0;
		}

		if (navmeshFilename) {
			// save to file
			saveMesh(navmeshFilename, mesh);
		}
	}

	//int tileCount = 0;
	int vertCount = 0;
	int polyCount = 0;
	int totalPolyVertCount = 0;

	float* allVerts;
	unsigned char* polyVertCounts;
	unsigned int* polyVerts;
	unsigned int* polyNeighbors;
	unsigned short* polyFlags;
	unsigned char* polyAreasAndTypes;

	// count sizes and offsets
	unsigned int* tilePolyIndexOffsets = new unsigned int[mesh->getMaxTiles()];

	for (int i = 0; i < mesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = ((const dtNavMesh*)mesh)->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;

		//tileCount++;
		tilePolyIndexOffsets[i] = polyCount;

		polyCount += tile->header->polyCount;
		vertCount += tile->header->vertCount;

		for (int j = 0; j < tile->header->polyCount; j++) {
			totalPolyVertCount += tile->polys[j].vertCount;
		}
	}

	// allocate arrays
	allVerts = new float[vertCount * 3];
	polyVertCounts = new unsigned char[polyCount];
	polyVerts = new unsigned int[totalPolyVertCount];
	polyNeighbors = new unsigned int[totalPolyVertCount];
	polyFlags = new unsigned short[polyCount];
	polyAreasAndTypes = new unsigned char[polyCount];

	int p = 0;
	int v = 0;
	int polyIndexOffset = 0;


	// copy data
	// iterate tiles
	for (int i = 0; i < mesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = ((const dtNavMesh*)mesh)->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;

		memcpy(&allVerts[v], tile->verts, tile->header->vertCount * sizeof(float) * 3);

		// iterate polygons
		for (int j = 0; j < tile->header->polyCount; j++) {
			int absolutePolyIndex = p+j;
			dtPoly& poly = tile->polys[j];

			polyVertCounts[absolutePolyIndex] = poly.vertCount;

			// iterate polygon vertices and edges
			for (int k = 0; k < poly.vertCount; k++) {
				int absolutePolyVertIndex = polyIndexOffset + k;

				polyVerts[absolutePolyVertIndex] = p + poly.verts[k];

				assert(p == tilePolyIndexOffsets[i]);

				// find absolute index of neighbor poly
				unsigned short neiRef = poly.neis[k];
				unsigned int idx = (unsigned int)-1;
				if (neiRef & DT_EXT_LINK)
				{
					// Tile border edge -> find linked polygon
					for (unsigned int l = poly.firstLink; l != DT_NULL_LINK; l = tile->links[l].next)
					{
						const dtLink* link = &tile->links[l];
						if (link->ref != 0 && link->edge == k)
						{
							// lookup linked neighbor tile and poly
							unsigned int salt, neigTile, neiPoly;
							mesh->decodePolyId(link->ref, salt, neigTile, neiPoly);

							// absolute poly index
							idx = tilePolyIndexOffsets[neigTile] + neiPoly;
						}
					}
				}
				else if (neiRef)
				{
					// Tile-internal edge
					idx = (unsigned int)(neiRef - 1);
				}

				assert((int)idx == -1 || idx < polyCount);

				polyNeighbors[absolutePolyVertIndex] = idx;
			}
			polyIndexOffset += poly.vertCount;
		}

		v += tile->header->vertCount * 3;
		p += tile->header->polyCount;
	}


	callback(
		userId, 
		vertCount * 3, 
		polyCount,
		allVerts, 
		totalPolyVertCount, 
		polyVertCounts, 
		polyVerts, 
		polyNeighbors, 
		polyFlags,
		polyAreasAndTypes
		);
	
	delete[] allVerts;
	delete[] polyVertCounts;
	delete[] polyVerts;
	delete[] polyNeighbors;
	delete[] polyFlags;
	delete[] polyAreasAndTypes;
	delete[] tilePolyIndexOffsets;
	CleanupAfterBuild();

	return 1;
}

dtNavMesh* buildMesh(InputGeom* geom, BuildContext* ctx)
{
	dtNavMesh* mesh;

	if (!geom || !geom->getMesh())
	{
		CleanupAfterBuild();
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: No vertices and triangles.");
		return false;
	}
	
	mesh = dtAllocNavMesh();
	if (!mesh)
	{
		CleanupAfterBuild();
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not allocate navmesh.");
		return false;
	}

	// setup some default parameters
	rcConfig cfg;

	memset(&cfg, 0, sizeof(rcConfig));
	
	//cfg.cs = 0.6;							// cell size is a sort of resolution -> the bigger the faster
	cfg.cs = 0.7f;
	cfg.ch = 0.27f;							// cell height -> distance from mesh to ground, if too low, recast will not build essential parts of the mesh for some reason
	cfg.walkableSlopeAngle = 50;			// this does not magically add anything, if set very high
	cfg.walkableClimb = 1.5f;				// how high the agent can climb in one step
	cfg.walkableHeight = 0.5f;				// minimum space to ceiling
	cfg.walkableRadius = 0.5f;				// minimum distance to objects
	cfg.tileSize = (1600/3.0f) / 8;			// 8x8 navmesh tiles per actual tile
	cfg.maxEdgeLen = 20.0f / cfg.cs;
	cfg.maxSimplificationError = 1.3f;
	cfg.minRegionArea = (int)rcSqr(8);		// Note: area = size*size
	cfg.mergeRegionArea = (int)rcSqr(8);	// Note: area = size*size
	cfg.maxVertsPerPoly = 6;
	cfg.detailSampleDist = cfg.cs * 9;
	cfg.detailSampleMaxError = cfg.ch * 1.0f;

	// default calculations - for some reason not included in basic recast
	const float* bmin = geom->getMeshBoundsMin();
	const float* bmax = geom->getMeshBoundsMax();

	char text[64];
	int gw = 0, gh = 0;
	rcCalcGridSize(bmin, bmax, cfg.cs, &gw, &gh);
	const int ts = (int)cfg.tileSize;
	const int tw = (gw + ts-1) / ts;
	const int th = (gh + ts-1) / ts;

	// Max tiles and max polys affect how the tile IDs are caculated.
	// There are 22 bits available for identifying a tile and a polygon.
	int tileBits = rcMin((int)ilog2(nextPow2(tw*th)), 14);
	if (tileBits > 14) tileBits = 14;
	int polyBits = 22 - tileBits;
	int maxTiles = 1 << tileBits;
	int maxPolysPerTile = 1 << polyBits;

	dtNavMeshParams params;
	rcVcopy(params.orig, geom->getMeshBoundsMin());
	params.tileWidth = cfg.tileSize * cfg.cs;
	params.tileHeight = cfg.tileSize * cfg.cs;
	params.maxTiles = maxTiles;
	params.maxPolys = maxPolysPerTile;
	
	dtStatus status;
	
	status = mesh->init(&params);
	if (dtStatusFailed(status))
	{
		CleanupAfterBuild();
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init navmesh.");
		return false;
	}
	
	/*
	dtNavMeshQuery* navQuery = dtAllocNavMeshQuery();
	status = navQuery->init(mesh, 2048);
	if (dtStatusFailed(status))
	{
		CleanupAfterBuild();
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init Detour navmesh query");
		return false;
	}*/
	
	// start building
	const float tcs = cfg.tileSize*cfg.cs;

	ctx->startTimer(RC_TIMER_TEMP);

	for (int y = 0; y < th; ++y)
	{
		for (int x = 0; x < tw; ++x)
		{
			float tileBmin[3], tileBmax[3];
			tileBmin[0] = bmin[0] + x*tcs;
			tileBmin[1] = bmin[1];
			tileBmin[2] = bmin[2] + y*tcs;
			
			tileBmax[0] = bmin[0] + (x+1)*tcs;
			tileBmax[1] = bmax[1];
			tileBmax[2] = bmin[2] + (y+1)*tcs;
			
			int dataSize = 0;
			unsigned char* data = buildTileMesh(x, y, tileBmin, tileBmax, dataSize, geom, cfg, ctx);
			if (data)
			{
				// Remove any previous data (navmesh owns and deletes the data).
				mesh->removeTile(mesh->getTileRefAt(x,y,0),0,0);

				// Let the navmesh own the data.
				dtStatus status = mesh->addTile(data,dataSize,DT_TILE_FREE_DATA,0,0);
				if (dtStatusFailed(status))
					dtFree(data);
			}
		}
	}
	
	// Start the build process.	
	ctx->stopTimer(RC_TIMER_TEMP);

	return mesh;
}


#define CleanupAfterTileBuild() { \
	if (m_solid == 0) delete m_solid; \
	if (m_triareas == 0) delete[] m_triareas; \
	if (m_chf == 0) delete m_chf; \
	if (m_cset == 0) delete m_cset; \
	if (m_pmesh == 0) delete m_pmesh; \
	if (m_dmesh == 0) delete m_dmesh; \
}

unsigned char* buildTileMesh(const int tx, const int ty, 
	const float* bmin, const float* bmax, int& dataSize,
	InputGeom* geom,
	rcConfig& cfg,
	rcContext* ctx)
{
	const float* verts = geom->getMesh()->getVerts();
	const int nverts = geom->getMesh()->getVertCount();
	const int ntris = geom->getMesh()->getTriCount();
	const rcChunkyTriMesh* chunkyMesh = geom->getChunkyMesh();
		
	// fix some configuration values
	cfg.walkableHeight = (int)ceilf(cfg.walkableHeight / cfg.ch);
	cfg.walkableClimb = (int)floorf(cfg.walkableClimb / cfg.ch);
	cfg.walkableRadius = (int)ceilf(cfg.walkableRadius / cfg.cs);
	cfg.borderSize = cfg.walkableRadius + 3;				// Reserve enough padding.
	cfg.width = cfg.tileSize + cfg.borderSize*2;
	cfg.height = cfg.tileSize + cfg.borderSize*2;
	
	rcVcopy(cfg.bmin, bmin);
	rcVcopy(cfg.bmax, bmax);
	cfg.bmin[0] -= cfg.borderSize*cfg.cs;
	cfg.bmin[2] -= cfg.borderSize*cfg.cs;
	cfg.bmax[0] += cfg.borderSize*cfg.cs;
	cfg.bmax[2] += cfg.borderSize*cfg.cs;
	
	// Reset build times gathering.
	ctx->resetTimers();
	
	// Start the build process.
	ctx->startTimer(RC_TIMER_TOTAL);
	
	ctx->log(RC_LOG_PROGRESS, "Building navigation:");
	ctx->log(RC_LOG_PROGRESS, " - %d x %d cells", cfg.width, cfg.height);
	ctx->log(RC_LOG_PROGRESS, " - %.1fK verts, %.1fK tris", nverts/1000.0f, ntris/1000.0f);
	
	// all involved objects
	rcHeightfield* m_solid = 0;
	unsigned char* m_triareas = 0;
	rcCompactHeightfield* m_chf = 0;
	rcContourSet* m_cset = 0;
	rcPolyMesh* m_pmesh = 0;
	rcPolyMeshDetail* m_dmesh = 0;

	
	// Allocate voxel heightfield where we rasterize our input data to.
	m_solid = rcAllocHeightfield();
	if (!m_solid)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'solid'.");
		return 0;
	}
	if (!rcCreateHeightfield(ctx, *m_solid, cfg.width, cfg.height, cfg.bmin, cfg.bmax, cfg.cs, cfg.ch))
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create solid heightfield.");
		return 0;
	}
	
	// Allocate array that can hold triangle flags.
	// If you have multiple meshes you need to process, allocate
	// and array which can hold the max number of triangles you need to process.
	m_triareas = new unsigned char[chunkyMesh->maxTrisPerChunk];
	if (!m_triareas)
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'm_triareas' (%d).", chunkyMesh->maxTrisPerChunk);
		return 0;
	}
	
	float tbmin[2], tbmax[2];
	tbmin[0] = cfg.bmin[0];
	tbmin[1] = cfg.bmin[2];
	tbmax[0] = cfg.bmax[0];
	tbmax[1] = cfg.bmax[2];
	int cid[512];// TODO: Make grow when returning too many items.
	const int ncid = rcGetChunksOverlappingRect(chunkyMesh, tbmin, tbmax, cid, 512);
	if (!ncid) {
		CleanupAfterTileBuild();
		return 0;
	}
	
	int m_tileTriCount = 0;
	
	for (int i = 0; i < ncid; ++i)
	{
		const rcChunkyTriMeshNode& node = chunkyMesh->nodes[cid[i]];
		const int* tris = &chunkyMesh->tris[node.i*3];
		const int ntris = node.n;
		
		m_tileTriCount += ntris;
		
		memset(m_triareas, 0, ntris*sizeof(unsigned char));
		rcMarkWalkableTriangles(ctx, cfg.walkableSlopeAngle,
								verts, nverts, tris, ntris, m_triareas);
		
		rcRasterizeTriangles(ctx, verts, nverts, tris, m_triareas, ntris, *m_solid, cfg.walkableClimb);
	}
	
	// Once all geometry is rasterized, we do initial pass of filtering to
	// remove unwanted overhangs caused by the conservative rasterization
	// as well as filter spans where the character cannot possibly stand.

	// Domi edit: Do not filter any triangles
#ifndef DOMI_EDIT
	rcFilterLowHangingWalkableObstacles(ctx, cfg.walkableClimb, *m_solid);
	rcFilterLedgeSpans(ctx, cfg.walkableHeight, cfg.walkableClimb, *m_solid);
	rcFilterWalkableLowHeightSpans(ctx, cfg.walkableHeight, *m_solid);
#endif
	
	// Compact the heightfield so that it is faster to handle from now on.
	// This will result more cache coherent data as well as the neighbours
	// between walkable cells will be calculated.
	m_chf = rcAllocCompactHeightfield();
	if (!m_chf)
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'chf'.");
		return 0;
	}
	if (!rcBuildCompactHeightfield(ctx, cfg.walkableHeight, cfg.walkableClimb, *m_solid, *m_chf))
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build compact data.");
		return 0;
	}

	// Erode the walkable area by agent radius.
	if (!rcErodeWalkableArea(ctx, cfg.walkableRadius, *m_chf))
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not erode.");
		return 0;
	}

	// (Optional) Mark areas.
	const ConvexVolume* vols = geom->getConvexVolumes();
	for (int i  = 0; i < geom->getConvexVolumeCount(); ++i)
		rcMarkConvexPolyArea(ctx, vols[i].verts, vols[i].nverts, vols[i].hmin, vols[i].hmax, (unsigned char)vols[i].area, *m_chf);
	
	if (0)		// m_monotonePartitioning
	{
		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildRegionsMonotone(ctx, *m_chf, cfg.borderSize, cfg.minRegionArea, cfg.mergeRegionArea))
		{
			CleanupAfterTileBuild();
			ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build regions.");
			return 0;
		}
	}
	else
	{
		// Prepare for region partitioning, by calculating distance field along the walkable surface.
		if (!rcBuildDistanceField(ctx, *m_chf))
		{
			CleanupAfterTileBuild();
			ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build distance field.");
			return 0;
		}
		
		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildRegions(ctx, *m_chf, cfg.borderSize, cfg.minRegionArea, cfg.mergeRegionArea))
		{
			CleanupAfterTileBuild();
			ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build regions.");
			return 0;
		}
	}
 	
	// Create contours.
	m_cset = rcAllocContourSet();
	if (!m_cset)
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'cset'.");
		return 0;
	}
	if (!rcBuildContours(ctx, *m_chf, cfg.maxSimplificationError, cfg.maxEdgeLen, *m_cset))
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create contours.");
		return 0;
	}
	
	if (m_cset->nconts == 0)
	{
		CleanupAfterTileBuild();
		return 0;
	}
	
	// Build polygon navmesh from the contours.
	m_pmesh = rcAllocPolyMesh();
	if (!m_pmesh)
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'pmesh'.");
		return 0;
	}
	if (!rcBuildPolyMesh(ctx, *m_cset, cfg.maxVertsPerPoly, *m_pmesh))
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not triangulate contours.");
		return 0;
	}
	
	// Build detail mesh.
	m_dmesh = rcAllocPolyMeshDetail();
	if (!m_dmesh)
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'dmesh'.");
		return 0;
	}
	
	if (!rcBuildPolyMeshDetail(ctx, *m_pmesh, *m_chf,
							   cfg.detailSampleDist, cfg.detailSampleMaxError,
							   *m_dmesh))
	{
		CleanupAfterTileBuild();
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could build polymesh detail.");
		return 0;
	}
	
	unsigned char* navData = 0;
	int navDataSize = 0;
	if (cfg.maxVertsPerPoly <= DT_VERTS_PER_POLYGON)
	{
		if (m_pmesh->nverts >= 0xffff)
		{
			CleanupAfterTileBuild();

			// The vertex indices are ushorts, and cannot point to more than 0xffff vertices.
			ctx->log(RC_LOG_ERROR, "Too many vertices per tile %d (max: %d).", m_pmesh->nverts, 0xffff);
			return false;
		}
		
		// Update poly flags from areas.
		for (int i = 0; i < m_pmesh->npolys; ++i)
		{
			if (m_pmesh->areas[i] == RC_WALKABLE_AREA)
				m_pmesh->areas[i] = SAMPLE_POLYAREA_GROUND;
			
			if (m_pmesh->areas[i] == SAMPLE_POLYAREA_GROUND ||
				m_pmesh->areas[i] == SAMPLE_POLYAREA_GRASS ||
				m_pmesh->areas[i] == SAMPLE_POLYAREA_ROAD)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK;
			}
			else if (m_pmesh->areas[i] == SAMPLE_POLYAREA_WATER)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_SWIM;
			}
			else if (m_pmesh->areas[i] == SAMPLE_POLYAREA_DOOR)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK | SAMPLE_POLYFLAGS_DOOR;
			}
		}
		
		dtNavMeshCreateParams params;
		memset(&params, 0, sizeof(params));
		params.verts = m_pmesh->verts;
		params.vertCount = m_pmesh->nverts;
		params.polys = m_pmesh->polys;
		params.polyAreas = m_pmesh->areas;
		params.polyFlags = m_pmesh->flags;
		params.polyCount = m_pmesh->npolys;
		params.nvp = m_pmesh->nvp;
		params.detailMeshes = m_dmesh->meshes;
		params.detailVerts = m_dmesh->verts;
		params.detailVertsCount = m_dmesh->nverts;
		params.detailTris = m_dmesh->tris;
		params.detailTriCount = m_dmesh->ntris;
		params.offMeshConVerts = geom->getOffMeshConnectionVerts();
		params.offMeshConRad = geom->getOffMeshConnectionRads();
		params.offMeshConDir = geom->getOffMeshConnectionDirs();
		params.offMeshConAreas = geom->getOffMeshConnectionAreas();
		params.offMeshConFlags = geom->getOffMeshConnectionFlags();
		params.offMeshConUserID = geom->getOffMeshConnectionId();
		params.offMeshConCount = geom->getOffMeshConnectionCount();
		params.walkableHeight = cfg.walkableHeight;
		params.walkableRadius = cfg.walkableRadius;
		params.walkableClimb = cfg.walkableClimb;
		params.tileX = tx;
		params.tileY = ty;
		params.tileLayer = 0;
		rcVcopy(params.bmin, m_pmesh->bmin);
		rcVcopy(params.bmax, m_pmesh->bmax);
		params.cs = cfg.cs;
		params.ch = cfg.ch;
		params.buildBvTree = true;
		
		if (!dtCreateNavMeshData(&params, &navData, &navDataSize))
		{
			CleanupAfterTileBuild();
			ctx->log(RC_LOG_ERROR, "Could not build Detour navmesh.");
			return 0;
		}		
	}
	ctx->stopTimer(RC_TIMER_TOTAL);
	
	// Show performance stats.
	duLogBuildTimes(*ctx, ctx->getAccumulatedTime(RC_TIMER_TOTAL));
	ctx->log(RC_LOG_PROGRESS, ">> Polymesh: %d vertices  %d polygons", m_pmesh->nverts, m_pmesh->npolys);

	dataSize = navDataSize;
	
	CleanupAfterTileBuild();
	return navData;
}


void saveMesh(const char* path, const dtNavMesh* mesh)
{
	if (!mesh) return;

	FILE* fp = fopen(path, "wb");
	if (!fp)
		return;

	// Store header.
	NavMeshSetHeader header;
	header.magic = NAVMESHSET_MAGIC;
	header.version = NAVMESHSET_VERSION;
	header.numTiles = 0;
	for (int i = 0; i < mesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = mesh->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;
		header.numTiles++;
	}
	memcpy(&header.params, mesh->getParams(), sizeof(dtNavMeshParams));
	fwrite(&header, sizeof(NavMeshSetHeader), 1, fp);

	// Store tiles.
	for (int i = 0; i < mesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = mesh->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;

		NavMeshTileHeader tileHeader;
		tileHeader.tileRef = mesh->getTileRef(tile);
		tileHeader.dataSize = tile->dataSize;
		fwrite(&tileHeader, sizeof(tileHeader), 1, fp);

		fwrite(tile->data, tile->dataSize, 1, fp);
	}

	fclose(fp);
}

dtNavMesh* loadMesh(const char* path)
{
	FILE* fp = fopen(path, "rb");
	if (!fp) return 0;

	// Read header.
	NavMeshSetHeader header;
	fread(&header, sizeof(NavMeshSetHeader), 1, fp);
	if (header.magic != NAVMESHSET_MAGIC)
	{
		fclose(fp);
		return 0;
	}
	if (header.version != NAVMESHSET_VERSION)
	{
		fclose(fp);
		return 0;
	}

	dtNavMesh* mesh = dtAllocNavMesh();
	if (!mesh)
	{
		fclose(fp);
		return 0;
	}
	dtStatus status = mesh->init(&header.params);
	if (dtStatusFailed(status))
	{
		fclose(fp);
		return 0;
	}

	// Read tiles.
	for (int i = 0; i < header.numTiles; ++i)
	{
		NavMeshTileHeader tileHeader;
		fread(&tileHeader, sizeof(tileHeader), 1, fp);
		if (!tileHeader.tileRef || !tileHeader.dataSize)
			break;

		unsigned char* data = (unsigned char*)dtAlloc(tileHeader.dataSize, DT_ALLOC_PERM);
		if (!data) break;
		memset(data, 0, tileHeader.dataSize);
		fread(data, tileHeader.dataSize, 1, fp);

		mesh->addTile(data, tileHeader.dataSize, DT_TILE_FREE_DATA, tileHeader.tileRef, 0);
	}

	fclose(fp);

	return mesh;
}

//void buildMesh(void* buildArg, int vertCount, const float* verts, float minh, float maxh, BuildMeshCallback* callback) 
//{
//	//InputGeom geom;
//	//BuildContext ctx;
//
//	//// Create input mesh
//	// ..
//
//	//// set input mesh
//	//sampler.handleMeshChanged(&geom);
//
//	//// build nav-mesh
//	//sampler.handleBuild();
//
//	//mesh = sampler.getNavMesh();
//	//navMeshQuery = sampler.getNavMeshQuery();
//
//	////navMeshQuery->
//}

#endif