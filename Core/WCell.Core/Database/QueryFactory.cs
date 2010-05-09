using System;
using Castle.ActiveRecord;

namespace WCell.Core.Database
{
	public static class QueryFactory
	{
		public static AsyncQuery CreateResultQuery<T, V>(Func<V> querier)
			where V : ActiveRecordBase
		{
			return new AsyncQueryProc<T, V>(querier);
		}

		public static AsyncQuery CreateResultQuery<T, V>(Func<V> querier, Action<T, V> callback)
			where V : ActiveRecordBase
		{
			return new AsyncQueryProc<T, V>(querier, callback);
		}

		public static AsyncQuery CreateResultQuery<T, V>(Func<V> querier, Action<T, V> callback, T parameter)
			where V : ActiveRecordBase
		{
			return new AsyncQueryProc<T, V>(querier, callback, parameter);
		}

		public static AsyncQuery CreateNonResultQuery(Action querier)
		{
			return new AsyncNonQueryProc(querier);
		}

		public static AsyncQuery CreateNonResultQuery(Action querier, Action callback)
		{
			return new AsyncNonQueryProc(querier, callback);
		}

		public static AsyncQuery CreateNonResultQuery<T>(Action querier)
		{
			return new AsyncNonQueryGenericProc<T>(querier);
		}

		public static AsyncQuery CreateNonResultQuery<T>(Action querier, Action<T> callback)
		{
			return new AsyncNonQueryGenericProc<T>(querier, callback);
		}

		public static AsyncQuery CreateNonResultQuery<T>(Action querier, Action<T> callback, T parameter)
		{
			return new AsyncNonQueryGenericProc<T>(querier, callback, parameter);
		}

		public static AsyncQuery CreateNonResultQuery<T, T2>(Action querier)
		{
			return new AsyncNonQueryGenericProc<T, T2>(querier);
		}

		public static AsyncQuery CreateNonResultQuery<T, T2>(Action querier, Action<T, T2> callback)
		{
			return new AsyncNonQueryGenericProc<T, T2>(querier, callback);
		}

		public static AsyncQuery CreateNonResultQuery<T, T2>(Action querier, Action<T, T2> callback, 
																T parameterOne, T2 parameterTwo)
		{
			return new AsyncNonQueryGenericProc<T, T2>(querier, callback, parameterOne, parameterTwo);
		}
	}
}
