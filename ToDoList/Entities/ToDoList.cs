using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LifePlanner.Entities
{
    public class ToDoList
    {
        public required Guid Id { get; set; }
        [MaxLength(255)]
        public required string Name { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        /**
         * C1: Fluent API
         * protected override void OnModelCreating(ModelBuilder modelBuilder)
         * {
         *      modelBuilder.Entity<Event>()
         *      .Property(e => e.EventDate)
         *      .HasColumnType("smalldatetime");
         *  }
         *  C2: below => Data Annotations
         *  
         *  Note: SQL Server smalldatetime: Phạm vi giá trị: Từ 01/01/1900 đến 06/06/2079. (4byte)
         *        C# DateTime               Phạm vi giá trị: 01/01/0001 đến 31/12/9999.
         */
        [Column (TypeName="smalldatetime")]
        public DateTime StartDate {get;set;}
        [Column (TypeName="smalldatetime")]
        public DateTime EndDate { get; set; }

        /**
         * Tinyint: Nguyên, k dấu 0 -> 255
         */
        public byte Order { get; set; }

        /**
         * Real: Kiểu số thực gần đúng (appoximate floating point) 4byte
         */
        public float Duration { get; set; }
        /**
         * nvarchar(1000) 1000 ký tự Unicode
         */
        [MaxLength(1000)]
        public string? Experience { get; set; }
    }
}

