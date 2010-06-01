/*************************************************************************
 *
 *   file			: SingleLinkNode.cs
 *   copyright		: (C) 2004  Julian M Bucknall 
 *   last changed	: $LastChangedDate: 2008-11-25 11:16:45 +0100 (ti, 25 nov 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 686 $
 *
 *   Written/rights held by Julian M Bucknall (boyet.com)
 *   http://www.boyet.com/Articles/LockfreeStack.html
 *   http://www.boyet.com/Articles/LockfreeQueue.html
 *   
 *   Modified by WCell
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Collections
{
	internal class SingleLinkNode<T>
	{
		public SingleLinkNode<T> Next;
		public T Item;
	}
}
