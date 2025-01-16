namespace LifePlanner.Repository
{
    public class PagingCreterias
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = int.MaxValue;
    }
}

