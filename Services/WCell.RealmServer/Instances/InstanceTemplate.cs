using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
    public delegate BaseInstance InstanceCreator();

    public class InstanceTemplate
    {
        private readonly MapTemplate m_MapTemplate;
        public InstanceCreator Creator;

        public InstanceTemplate(MapTemplate template)
        {
            m_MapTemplate = template;
        }

        public MapTemplate MapTemplate
        {
            get { return m_MapTemplate; }
        }

        internal BaseInstance Create()
        {
            if (Creator != null)
            {
                return Creator();
            }
            return null;
        }
    }
}