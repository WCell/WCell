using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Tests.AI
{
	class AIActionStub : AIAction
	{
		public bool Started = false;
		public int UpdateCount = 0;
		public bool Executing = false;

		public AIActionStub(Unit owner) : base(owner)
		{
			
		}

		public override void Start()
		{
			Started = true;
		}

		public override void Update()
		{
			UpdateCount++;
		}

		public override void Stop()
		{
			Started = false;
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.Background; }
		}
	}
}