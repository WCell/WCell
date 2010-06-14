How to get this to work with Recast:

1. Check out the contents from http://code.google.com/p/recastcuda/ into a folder called "Recast" on the same level as the folder that contains the Terrain*.sln file
2. Make sure your App.config contains the right directory that contains extracted map and object files
2b. Extract the WORLD\\MAPS\\ directory tree into that folder
	(eg, if the folder is called "f", then you want a directory "f\World\Maps\" that contains all the map and object files)
3. Start the RecastRunner
-> Done