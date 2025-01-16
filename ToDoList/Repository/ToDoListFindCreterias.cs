namespace LifePlanner.Repository
{
    public class ToDoListFindCreterias : PagingCreterias
    {
        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = DateTime.Now;
        public IEnumerable<Guid> Ids { get; set; } = Enumerable.Empty<Guid>();
        public string Name { get; set; } = string.Empty;
        /**
         * expression-bodied property
         */
        public static ToDoListFindCreterias Empty => new ToDoListFindCreterias() { };
    }
}

