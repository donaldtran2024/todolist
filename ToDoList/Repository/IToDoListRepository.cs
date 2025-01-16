using LifePlanner.Entities;

namespace LifePlanner.Repository
{
    internal interface IToDoListRepository
    {
        ToDoList? FindById(Guid id);
        IEnumerable<ToDoList> Find(ToDoListFindCreterias toDoListFindCreterias, ToDoListSortBy sortBy = ToDoListSortBy.NameAscending);
        ToDoList? Add(ToDoList toDoList);
        int DeleteAll();
        int Update(ToDoList toDoList);
    }
}

