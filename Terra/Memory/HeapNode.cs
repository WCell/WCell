namespace Terra.Memory
{
    internal class HeapNode
    {
        public float Importance;
        public ILabelled Object;

        public HeapNode ()
        {
            Object = null;
            Importance = 0.0f;
        }

        public HeapNode(ILabelled obj, float importance)
        {
            Object = obj;
            Importance = importance;
        }

        public HeapNode(HeapNode node)
        {
            Object = node.Object;
            Importance = node.Importance;
        }
    }
}