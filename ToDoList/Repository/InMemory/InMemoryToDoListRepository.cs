using LifePlanner.Entities;

namespace LifePlanner.Repository.InMemory
{
    internal class InMemoryToDoListRepository : IToDoListRepository
    {
        private readonly List<ToDoList> toDoLists = new();

        public ToDoList? Add(ToDoList toDoList)
        {
            toDoLists.Add(toDoList);
            return toDoList;
        }

        public int DeleteAll()
        {
            int count = toDoLists.Count;
            toDoLists.Clear();
            return count;
        }

        public IEnumerable<ToDoList> Find(ToDoListFindCreterias creterias, ToDoListSortBy sortBy = ToDoListSortBy.NameAscending)
        {
            var query = from o in toDoLists select o;

            if (creterias.Ids.Any())
            {
                query = query.Where(p => creterias.Ids.Contains(p.Id));
            }

            if (!string.IsNullOrEmpty(creterias.Name))
            {
                query = query.Where(p => p.Name.Contains(creterias.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (creterias.StartDate.HasValue)
            {
                query = query.Where(p => p.StartDate >= creterias.StartDate.Value);
            }

            if (creterias.EndDate.HasValue)
            {
                query = query.Where(p => p.EndDate >= creterias.EndDate.Value);
            }

            if(creterias.Take > 0 && creterias.Take != int.MaxValue)
            {
                query = query.Take(creterias.Take);
            }
            return query;
        }

        public ToDoList? FindById(Guid id)
        {
            return toDoLists.Where(p => p.Id == id).FirstOrDefault();
        }

        public int Update(ToDoList toDoList)
        {
            var p = toDoLists.Where(p => p.Id == toDoList.Id).FirstOrDefault();

            if(p != null)
            {
                toDoLists.Remove(p);
                toDoLists.Add(toDoList);
                return 1;
            }
            return 0;
        }
    }
}

