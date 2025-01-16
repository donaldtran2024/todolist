using System.Text;
using LifePlanner.Entities;
using Microsoft.Data.SqlClient;

namespace LifePlanner.Repository.SqlServer
{
    public class SqlServerToDoListRepository : IToDoListRepository
    {
        private const string INSERT_COMMAND = "INSERT INTO ToDoLists VALUES (@ToDoListName, @ToDoListDescription, @ToDoListStartDate, @ToDoListEndDate, @ToDoListOrder, @ToDoListDuration, @ToDoListExperience)";
        private const string UPDATE_COMMAND = "UPDATE ToDoLists Set ToDoListName = @ToDoListName, ToDoListDescription = @ToDoListDescription, ToDoListStartDate = @ToDoListStartDate, ToDoListEndDate = @ToDoListEndDate, ToDoListOrder = @ToDoListOrder, ToDoListDuration = @ToDoListDuration, ToDoListExperience = @ToDoListExperience";
        private const string FIND_BY_ID_QUERY = "SELECT ToDoListName, ToDoListDescription, ToDoListStartDate, ToDoListEndDate, ToDoListOrder, ToDoListDuration, ToDoListExperience FROM ToDoLists WHERE ToDoListId = @ToDoListId";
        private const string SELECT = "SELECT ";
        private const string FIND_ALL = "ToDoListId, ToDoListName, ToDoListDescription, ToDoListStartDate, ToDoListEndDate, ToDoListOrder, ToDoListDuration, ToDoListExperience FROM ToDoLists WHERE (1 = 1)";
        private const string DELETE_ALL = "DELETE FROM ToDoLists";

        private readonly SqlConnection connection;
        private readonly SqlTransaction? transaction;

        public SqlServerToDoListRepository(SqlConnection connection, SqlTransaction? transaction)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.transaction = transaction;
        }

        public ToDoList? Add(ToDoList toDoList)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = INSERT_COMMAND;
            if(transaction != null)
            {
                cmd.Transaction = transaction;
            }

            cmd.Parameters.Add(new SqlParameter("@ToDoListId", System.Data.SqlDbType.UniqueIdentifier)).Value = toDoList.Id;
            cmd.Parameters.Add(new SqlParameter("@ToDoListName", System.Data.SqlDbType.NVarChar, 255)).Value = toDoList.Name;
            cmd.Parameters.Add(new SqlParameter("@ToDoListDescription", System.Data.SqlDbType.NVarChar, 1000)).Value = toDoList.Description;
            cmd.Parameters.Add(new SqlParameter("@ToDoListStartDate", System.Data.SqlDbType.SmallDateTime)).Value = toDoList.StartDate;
            cmd.Parameters.Add(new SqlParameter("@ToDoListEndDate", System.Data.SqlDbType.SmallDateTime)).Value = toDoList.EndDate;
            cmd.Parameters.Add(new SqlParameter("@ToDoListOrder", System.Data.SqlDbType.TinyInt)).Value = toDoList.Order;
            cmd.Parameters.Add(new SqlParameter("@ToDoListDuration", System.Data.SqlDbType.Float)).Value = toDoList.Duration;
            cmd.Parameters.Add(new SqlParameter("@ToDoListExperience", System.Data.SqlDbType.NVarChar, 1000)).Value = toDoList.Experience;

            if(cmd.ExecuteNonQuery() > 0)
            {
                return toDoList;
            }
            else
            {
                return null;
            }
        }

        public int DeleteAll()
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = DELETE_ALL;
            if(transaction != null)
            {
                cmd.Transaction = transaction;
            }

            return cmd.ExecuteNonQuery(); // return number row
        }

        public IEnumerable<ToDoList> Find(ToDoListFindCreterias creterias, ToDoListSortBy sortBy = ToDoListSortBy.NameAscending)
        {
            var cmd = connection.CreateCommand();
            if(transaction != null)
            {
                cmd.Transaction = transaction;
            }

            var sql = new StringBuilder(SELECT);
            if(creterias.Take > 0)
            {
                sql.Append("TOP ");
                sql.Append(creterias.Take);
                sql.Append(' ');
            }

            sql.Append(FIND_ALL);

            if(creterias.Ids.Any())
            {
                sql.Append(" AND ToDoListId IN (");
                sql.Append(string.Join(',', creterias.Ids.Select(id => $"'${id}'")));
                sql.Append(')');
            }

            if (!creterias.StartDate.HasValue || !creterias.EndDate.HasValue)
            {
                throw new Exception("Start date or end date is null.");
            }
            else if (creterias.StartDate.Value > creterias.EndDate.Value)
            {
                throw new Exception("Start date is earlier than or equal to end date.");
            }

            if(creterias.StartDate.HasValue)
            {
                sql.Append(" AND ToDoListStartDate >=");
                sql.Append(creterias.StartDate);
                
            }

            if(creterias.EndDate.HasValue)
            {
                sql.Append(" AND ToDoListEndDate <=");
                sql.Append(creterias.EndDate);
            }

            if (sortBy == ToDoListSortBy.NameAscending)
            {
                sql.Append(" ORDER BY ToDoListName");
            }
            else if (sortBy == ToDoListSortBy.NameDescending)
            {
                sql.Append(" ORDER BY ToDoListStartDate DESC");
            }
            else if (sortBy == ToDoListSortBy.StartDateAscending)
            {
                sql.Append(" ORDER BY ToDoListStartDate");
            }
            else if (sortBy == ToDoListSortBy.StartDateDescending)
            {
                sql.Append(" ORDER BY ToDoListStartDate DESC");
            }
            else if (sortBy == ToDoListSortBy.EndDateAscending)
            {
                sql.Append(" ORDER BY ToDoListEndate");
            }
            else if (sortBy == ToDoListSortBy.EndateDescending)
            {
                sql.Append(" ORDER BY ToDoListEndate DESC");
            }
            else if (sortBy == ToDoListSortBy.OrderByAscending)
            {
                sql.Append(" ORDER BY ToDoListOrder");
            }
            else
            {
                sql.Append(" ORDER BY ToDoListOrder DESC");
            }

            if(creterias.Skip > 0)
            {
                sql.Append(" OFFSET ");
                sql.Append(creterias.Skip);
                sql.Append(" ROWS");
            }

            cmd.CommandText = sql.ToString();
            using var reader = cmd.ExecuteReader(); // return object
            var toDoList = new List<ToDoList>();

            if(reader != null)
            {
                while(reader.Read())
                {
                    toDoList.Add(new ToDoList()
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        StartDate = reader.GetDateTime(3),
                        EndDate = reader.GetDateTime(4),
                        Order = reader.GetByte(5),
                        Duration = reader.GetFloat(6),
                        Experience = reader.IsDBNull(7) ? null : reader.GetString(7)
                    });
                }
            }

            return toDoList;
        }

        public ToDoList? FindById(Guid id)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = FIND_BY_ID_QUERY;

            if(transaction != null)
            {
                cmd.Transaction = transaction;
            }

            cmd.Parameters.Add(new SqlParameter("@ToDoListId", System.Data.SqlDbType.UniqueIdentifier)).Value = id;

            using var reader = cmd.ExecuteReader();
            if(reader != null && reader.Read())
            {
                return new ToDoList()
                {
                    Id = id,
                    Name = reader.GetString(0),
                    Description = reader.IsDBNull(1) ? null : reader.GetString(1),
                    StartDate = reader.GetDateTime(2),
                    EndDate = reader.GetDateTime(3),
                    Order = reader.GetByte(4),
                    Duration = reader.GetFloat(5),
                    Experience = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
            }
            else
            {
                return null;
            }
        }

        public int Update(ToDoList toDoList)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = UPDATE_COMMAND;

            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }

            cmd.Parameters.Add(new SqlParameter("@ToDoListId", System.Data.SqlDbType.UniqueIdentifier)).Value = toDoList.Id;
            cmd.Parameters.Add(new SqlParameter("@ToDoListName", System.Data.SqlDbType.NVarChar, 255)).Value = toDoList.Name;
            cmd.Parameters.Add(new SqlParameter("@ToDoListDescription", System.Data.SqlDbType.NVarChar, 1000)).Value = toDoList.Description;
            cmd.Parameters.Add(new SqlParameter("@ToDoListStartDate", System.Data.SqlDbType.SmallDateTime)).Value = toDoList.StartDate;
            cmd.Parameters.Add(new SqlParameter("@ToDoListEndDate", System.Data.SqlDbType.SmallDateTime)).Value = toDoList.EndDate;
            cmd.Parameters.Add(new SqlParameter("@ToDoListOrder", System.Data.SqlDbType.TinyInt)).Value = toDoList.Order;
            cmd.Parameters.Add(new SqlParameter("@ToDoListDuration", System.Data.SqlDbType.Float)).Value = toDoList.Duration;
            cmd.Parameters.Add(new SqlParameter("@ToDoListExperience", System.Data.SqlDbType.NVarChar, 1000)).Value = toDoList.Experience;

            return cmd.ExecuteNonQuery();
        }
    }
}
