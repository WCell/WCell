using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.DB;
using WCell.Util.Variables;
using WCell.Util.Conversion;
using System.Text;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Content
{
	/// <summary>
	/// TODO: Make it simple to add content from outside
	/// </summary>
	[GlobalMgr]
	public static class ContentMgr
	{
		public static readonly List<Assembly> AdditionalAssemblies = new List<Assembly>();
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Determines how to interact with invalid content data.
		/// </summary>
		[Variable("ContentErrorResponse")]
		public static ErrorResponse ErrorResponse = ErrorResponse.None;

		/// <summary>
		/// Causes an error to be thrown if certain data is not present when requested
		/// </summary>
		[NotVariable]
		public static bool ForceDataPresence = false;
		//ErrorResponse.Warn;

		public static bool EnableCaching = true;

		private static string s_implementationFolder, s_implementationRoot;
		private static LightDBDefinitionSet s_definitions;
		private static Dictionary<Type, LightDBMapper> s_mappersByType;

		/// <summary>
		/// The name of the ContentProvider, which is also the name of the folder within the Content/Impl/ folder,
		/// in which to find the Table-definitions.
		/// </summary>
		public static string ContentProviderName = "UDB";

		private const string CacheFileSuffix = ".cache";

		public static string ImplementationRoot
		{
			get { return s_implementationRoot; }
		}

		public static string ImplementationFolder
		{
			get { return s_implementationFolder; }
		}

		public static LightDBDefinitionSet Definitions
		{
			get { return s_definitions; }
		}

		public static LightDBMapper GetMapper<T>() where T : IDataHolder
		{
			return GetMapper(typeof(T));
		}

		public static LightDBMapper GetMapper(Type t)
		{
			EnsureInitialized();
			LightDBMapper mapper;
			if (!s_mappersByType.TryGetValue(t, out mapper))
			{
				throw new Exception(string.Format(
					"DataHolder Type \"{0}\" was not registered - Make sure that it's XML definition was defined and associated correctly. " +
					"If the Type is not in the Core, call ContentHandler.Initialize(Assembly) on its Assembly first.", t.FullName));
			}
			return mapper;
		}

		public static Dictionary<object, IDataHolder> GetObjectMap<T>() where T : IDataHolder
		{
			return GetMapper<T>().GetObjectMap<T>();
		}

		#region Events
		/// <summary>
		/// Reports incorrect data, that is not Database-provider dependent.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="args"></param>
		public static void OnInvalidClientData(string msg, params object[] args)
		{
			OnInvalidClientData(string.Format(msg, args));
		}

		/// <summary>
		/// Reports incorrect data, that is not Database-provider dependent.
		/// </summary>
		public static void OnInvalidClientData(string msg)
		{
			switch (ErrorResponse)
			{
				case ErrorResponse.Exception:
					{
						throw new ContentException("Error encountered when loading Content: " + msg);
					}
				case ErrorResponse.Warn:
					{
						log.Warn(msg);
						break;
					}
				default:
					break;
			}
		}

		/// <summary>
		/// Reports incorrect data, caused by the Database-provider.
		/// </summary>
		public static void OnInvalidDBData(string msg, params object[] args)
		{
			OnInvalidDBData(string.Format(msg, args));
		}

		/// <summary>
		/// Reports incorrect data, caused by the Database-provider.
		/// </summary>
		public static void OnInvalidDBData(string msg)
		{
			switch (ErrorResponse)
			{
				case ErrorResponse.Exception:
					{
						throw new ContentException("Error encountered when loading Content: " + msg);
					}
				case ErrorResponse.Warn:
					{
						log.Warn("<" + ContentProviderName + ">" + msg);
						break;
					}
				default:
					break;
			}
		}
		#endregion

		public static void EnsureInitialized()
		{
			if (s_mappersByType == null)
			{
				InitializeDefault();
			}
		}

		#region Init
		private static bool inited;

		[Initialization]
		[DependentInitialization(typeof(RealmContentDBMgr))]
		public static void Initialize()
		{
			InitializeAndLoad(typeof(ContentMgr).Assembly);

			RealmServer.InitMgr.SignalGlobalMgrReady(typeof(ContentMgr));
		}

		public static void InitializeDefault()
		{
			RealmContentDBMgr.Initialize();
			InitializeAndLoad(typeof(ContentMgr).Assembly);
		}

		public static void InitializeAndLoad(Assembly asm)
		{
			if (!inited)
			{
				Converters.Provider = new NHibernateConverterProvider();
				Initialize(asm);
				Load();
				inited = true;
			}
		}

		public static void Initialize(Assembly asm)
		{
			s_implementationRoot = Path.Combine(RealmServerConfiguration.ContentDir, "Impl");
			s_implementationFolder = Path.Combine(s_implementationRoot, ContentProviderName);

			var defs = DataHolderMgr.CreateDataHolderDefinitionArray(asm);
			LightDBMgr.InvalidDataHandler = OnInvalidDBData;

			s_definitions = new LightDBDefinitionSet(defs);
		}
		#endregion

		#region Check
		/// <summary>
		/// Checks the validity of all Table definitions
		/// </summary>
		/// <returns>The amount of invalid columns.</returns>
		public static int Check(Action<string> feedbackCallback)
		{
			EnsureInitialized();
			var count = 0;
			foreach (var mapper in s_mappersByType.Values)
			{
				foreach (var table in mapper.Mapping.TableDefinitions)
				{
					var fields = table.ColumnDefinitions;
					foreach (var field in fields)
					{
						if (!field.IsEmpty)
						{
							try
							{ //TODO: Remove the need for this crap
								using ( //var reader = 
									mapper.Wrapper.Query(string.Format("select {0} from {1} {2}", field.ColumnName, table.Name," LIMIT 1")))
								{
								}
							}
							catch (Exception e)
							{
								feedbackCallback(
									string.Format("Invalid column \"{0}\" in table \"{1}\": {2}", field, table.Name,
											  e.GetAllMessages().ToString("\n\t")));
								count++;
							}
						}
					}
				}
			}
			return count;
		}
		#endregion

		#region Loading
		public static void Load()
		{
			s_definitions.Clear();
			var tableFile = Path.Combine(s_implementationFolder, "Tables.xml");
			var dataDefDir = Path.Combine(s_implementationFolder, "Data");

			s_definitions.LoadTableDefinitions(tableFile);

			CheckVersion();

			s_definitions.LoadDataHolderDefinitions(dataDefDir);

			if (!RealmContentDBMgr.Initialized) //TODO: Should this end up being content database?
			{
				throw new InvalidOperationException("Content Database must be initialized.");
			}

			s_mappersByType = CreateMappersByType();
		}

		public static void CheckVersion() //TODO: Re-implement check
		{
			var dbVersion = s_definitions.DBVersionLocation;
			if (dbVersion != null && dbVersion.IsValid)
			{
				string versionStr;
				try
				{
					versionStr = dbVersion.MaxVersion.ToString(); // (new NHibernateDbWrapper()).GetDatabaseVersion(dbVersion.Table, dbVersion.Column);
				}
				catch (Exception e)
				{
					throw new ContentException(e, "Unable to validate version of database content - Required " + dbVersion);
				}

				//var minVersion = dbVersion.MinVersion;
				//var maxVersion = dbVersion.MaxVersion;
				//float version;

				//if (!float.TryParse(versionStr, out version))
				//{
				//    throw new ContentException(string.Format("Unable to read version from Database due to an invalid format: " + versionStr));
				//}

				//if (version < minVersion || version > maxVersion)
				//{
				//    throw new ContentException(string.Format("Supplied database's version is {0}, while content provider only supports versions {1} through {2}",
				//            versionStr, minVersion, maxVersion));
				//}
			}
		}

		public static Dictionary<Type, LightDBMapper> CreateMappersByType()
		{
			var map = new Dictionary<Type, LightDBMapper>();
			foreach (var mapping in s_definitions.Mappings)
			{
				var mapper = new LightDBMapper(mapping, new NHibernateDbWrapper());
				foreach (var def in mapping.DataHolderDefinitions)
				{
					map.Add(def.Type, mapper);
				}
			}
			return map;
		}

		/// <summary>
		/// Ensures that the DataHolder of the given type and those that are connected with it, are loaded.
		/// 
		/// </summary>
		public static bool Load<T>() where T : IDataHolder
		{
			return Load<T>(false);
		}

		/// <summary>
		/// Ensures that the DataHolder of the given type and those that are connected with it, are loaded.
		/// 
		/// </summary>
		/// <param name="force">Whether to re-load if already loaded.</param>
		public static bool Load<T>(bool force) where T : IDataHolder
		{
			EnsureInitialized();

			var mapper = GetMapper<T>();

			if (force || !mapper.Fetched)
			{
				if (force && mapper.IsCached())
				{
					mapper.FlushCache();
				}
				Load(mapper);
				return true;
			}
			return false;
		}

		public static void Load(LightDBMapper mapper)
		{
			Load(mapper, true);
		}

		public static void Load(LightDBMapper mapper, bool failException)
		{
			try
			{
				if (EnableCaching && mapper.SupportsCaching)
				{
					if (mapper.LoadCache())
					{
						// loaded cache successfully
						return;
					}
				}

				//Utility.Measure("Loading content from DB - " + mapper + "", 1, () => {
				mapper.Fetch();
				//});

				if (EnableCaching && mapper.SupportsCaching)
				{
					log.Info("Saving cache for: " + mapper.Mapping.DataHolderDefinitions.ToString(", "));
					mapper.SaveCache();
				}
			}
			catch (Exception e)
			{
				if (failException)
				{
					throw new ContentException(e, "Unable to load entries using \"{0}\"", mapper);
				}
				// LogUtil.ErrorException(e, "Unable to load entries using \"{0}\"", mapper);
			}
		}

		/// <summary>
		/// Fetches all content of all registered DataHolders.
		/// </summary>
		public static void FetchAll()
		{
			EnsureInitialized();

			foreach (var mapper in s_mappersByType.Values)
			{
				mapper.Fetch();
			}
		}
		#endregion

		#region Editing
		/// <summary>
		/// Updates changes to the Object in the underlying Database.
        /// FlushCommit() needs to be called to persist the operation.
		/// </summary>
		public static void CommitUpdate(this IDataHolder obj)
		{
			var mapper = GetMapper(obj.GetType());
			mapper.Update(obj);
		}

		/// <summary>
		/// Inserts the Object into the underlying Database.
        /// FlushCommit() needs to be called to persist the operation.
		/// </summary>
		public static void CommitInsert(this IDataHolder obj)
		{
			var mapper = GetMapper(obj.GetType());
			mapper.Insert(obj);
		}

		/// <summary>
		/// Deletes the Object from the underlying Database.
        /// FlushCommit() needs to be called to persist the operation.
		/// </summary>
		public static void CommitDelete(this IDataHolder obj)
		{
			var mapper = GetMapper(obj.GetType());
			mapper.Delete(obj);
		}

		/// <summary>
		/// Updates changes to the Object in the underlying Database.
		/// </summary>
		public static void CommitUpdateAndFlush(this IDataHolder obj)
		{
			obj.CommitUpdate();
			FlushCommit(obj.GetType());
		}

		/// <summary>
		/// Inserts the Object into the underlying Database.
		/// FlushCommit() needs to be called to persist the operation.
		/// </summary>
		public static void CommitInsertAndFlush(this IDataHolder obj)
		{
			obj.CommitInsert();
			FlushCommit(obj.GetType());
		}

		/// <summary>
		/// Deletes the Object from the underlying Database.
		/// FlushCommit() needs to be called to persist the operation.
		/// </summary>
		public static void CommitDeleteAndFlush(this IDataHolder obj)
		{
			obj.CommitDelete();
			FlushCommit(obj.GetType());
		}

		/// <summary>
		/// Ignore all changes before last FlushCommit() (will not change the Object's state).
		/// </summary>
		public static void IgnoreUnflushedChanges<T>() where T : IDataHolder
		{
			var mapper = GetMapper(typeof(T));
			mapper.IgnoreUnflushedChanges();
		}

		/// <summary>
		/// Flush all commited changes to the underlying Database.
		/// FlushCommit() needs to be called to persist the operation.
		/// Will be executed in the global IO context.
		/// </summary>
		public static void FlushCommit<T>() where T : IDataHolder
		{
			var mapper = GetMapper(typeof(T));
			RealmServer.IOQueue.ExecuteInContext(() => mapper.Flush());
		}

		/// <summary>
		/// Flush all commited changes to the underlying Database.
		/// FlushCommit() needs to be called to persist the operation.
		/// Will be executed in the global IO context.
		/// </summary>
		public static void FlushCommit(Type t)
		{
			var mapper = GetMapper(t);
			RealmServer.IOQueue.ExecuteInContext(() => mapper.Flush());
		}
		#endregion

		#region Caching
		public static void SaveCache(this LightDBMapper mapper)
		{
			// save cache after loading
			var file = GetCacheFilename(mapper);
			try
			{
				mapper.SaveCache(file);
			}
			catch (Exception e)
			{
				File.Delete(file);
				LogUtil.ErrorException(e, "Failed to save cache to file: " + file);
			}
		}

		public static bool IsCached(this LightDBMapper mapper)
		{
			var file = GetCacheFilename(mapper);
			return File.Exists(file);
		}

		public static void FlushCache(this LightDBMapper mapper)
		{
			var file = GetCacheFilename(mapper);
			if (File.Exists(file))
			{
				File.Delete(file);
			}
		}

		private static bool LoadCache(this LightDBMapper mapper)
		{
			var file = GetCacheFilename(mapper);
			if (mapper.IsCached())
			{
				try
				{
					// Utility.Measure("Loading contnt from Cache - " + mapper, 1, () => {
					if (mapper.LoadCache(file))
					{
						// loaded cache successfully
						return true;
					}
					log.Warn("Cache signature in file \"{0}\" is out of date.", file);
					log.Warn("Reloading content from Database...");
					//});
					//return;
				}
				catch (Exception ex)
				{
					if (ex is EndOfStreamException)
					{
						log.Warn("Cache signature in file \"{0}\" is out of date.", file);
					}
					else
					{
						LogUtil.ErrorException(ex, "Unable to load cache from \"" + file + "\".");
					}
					log.Warn("Reloading content from Database...");
				}
			}
			return false;
		}

		public static string GetCacheFilename(this LightDBMapper mapper)
		{
			var str = new StringBuilder(mapper.Mapping.DataHolderDefinitions.Length * 12);
			foreach (var def in mapper.Mapping.DataHolderDefinitions)
			{
				str.Append(def.Name);
			}
			str.Append(CacheFileSuffix);
			return RealmServerConfiguration.Instance.GetCacheFile(str.ToString());
		}

		/// <summary>
		/// Deletes all cache-files
		/// </summary>
		public static void PurgeCache()
		{
			foreach (var file in Directory.GetFiles(RealmServerConfiguration.Instance.CacheDir))
			{
				if (file.EndsWith(CacheFileSuffix))
				{
					File.Delete(file);
				}
			}
		}
		#endregion

		public static void SaveDefaultStubs()
		{
			SaveStubs(typeof(ContentMgr).Assembly);
		}

		public static void SaveStubs(Assembly asm)
		{
			EnsureInitialized();

			LightDBMgr.SaveAllStubs(Path.Combine(s_implementationRoot, ".stubs"),
									DataHolderMgr.CreateDataHolderDefinitionArray(asm));
		}
	}
}