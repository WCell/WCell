using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cell.Core
{
	public interface IPooledObject
	{
		void Cleanup();
	}
}