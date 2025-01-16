using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;

//var builder = WebApplication.CreateBuilder(args);

//// swagger
///**
// * Khi bạn gọi AddEndpointsApiExplorer, ASP.NET Core sẽ quét tất cả các endpoint Minimal API đã được ánh xạ (như MapGet, MapPost, ...) trong ứng dụng.
// * Các endpoint này sẽ được đăng ký vào metadata của Swagger, và sau đó Swagger UI sẽ hiển thị chúng dưới dạng tài liệu API.
// */
//builder.Services.AddEndpointsApiExplorer();
///**
// * Tạo tài liệu OpenAPI tự động: Phân tích các controller, action, và Minimal APIs trong ứng dụng để tạo tài liệu OpenAPI (trước đây gọi là Swagger Specification).
// * Hỗ trợ Swagger UI: Kết hợp với middleware UseSwagger và UseSwaggerUI, nó cung cấp một giao diện tương tác để thử nghiệm API.
// * Tùy chỉnh tài liệu API: Cung cấp các tùy chọn để bổ sung thông tin, tùy chỉnh tài liệu (metadata), thêm xác thực (authentication), hoặc định nghĩa các phần mở rộng.
//*/
//builder.Services.AddSwaggerGen();
//// end swagger

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger(options =>
//    {
//        //options.SerializeAsV2 = true; // Nếu dùng version Swagger JSon v2 else default v3
//    }); // Kích hoạt middleware để sinh ra tài liệu Swagger (OpenAPI) cho ứng dụng.
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("swagger/v1/swagger.json", "v1");
//        options.RoutePrefix = string.Empty;
//    }); // Kích hoạt middleware để hiển thị giao diện người dùng Swagger UI, cho phép tương tác với các API endpoints thông qua trình duyệt.
//}

//app.MapGet("/", () => "Hello World!");

//app.Run();

/**
 * Unit of work
 * Là 1 design pattern &&&& thường dùng repository pattern (allow we phân tách code --truy xuất vs database && business logic)
 * Là 1 object lưu lại các thao tác với database ( example: read, write ... with database) => instead of we gửi trực tiếp tới database 
 * thì UoW là 1 lớp sẽ:
 *  + save all thao tác
 *  + gửi all thao tác đó to database
 * Mỗi lần thao tác đến database (get, ngắt kết nối, connect),cũng như thao tác nội bộ database nó sẽ tốn 1 số chi phí nên thay vì gửi nhiều câu lệnh riêng lẻ chúng ta
 * gom lại và gửi 1 lần (đồng thời chúng ta có thể use transaction if insert, nhiều nơi với cùng 1 transaction)
 */


/**
 * Repository
 * -- Thông thường lấy data from các object đã xử lý trong bộ nhớ rồi we create các câu lệnh SQL và send to csdl (trộn lẫn process business logic & làm việc với csdl) --
 * 
 * -- code business logic || code CSDL (trộn lẫn, đôi khi complex)
 * -- cách work giữa object in memory &&&& cách work vs 1 CSDL SQL là khác nhau 
 *     ( viết code CRUD edit 1 property of 1 object in memory || work collection in memory ### when work CSDL create SQL 
 *      + thì nó mang tính dữ liệu hơn là hướng đối tượng
 *      + cách nhìn vào các row, line ### object in memory
 *      + quy tắc càng phức tạp thì viết c# easy hơn
 *     )
 * Allow we create ra 1 lớp trung gian giữa code làm việc vs domain và code làm việc với CSDL
 * 
 * -- Domain chỉ vấn đề, object liên quan tới nghiệp vụ of bài toán => Những entity object liên quan called is domain entity
 * -- Trong 1 phần mềm chúng ta có nhiều object khác nữa như (object SQL connection)
 * -- Thường những object thuộc domain là đối tượng bất biến (người gửi, người nhận) (trừ khi bài toán thay đổi) và những cái object còn lại thường xuyên change như chuyển 1 dạng CSDL này sang # thì object chịu trách nhiệm 
 * connect CSDL thay đổi
 * 
 * 
 * Client dùng 1 Repository "kho dữ liệu" -> no need interest đằng sau reposity how do work (CRUD data)
 * 

 * 
 * Nhờ có repository mà code chương trình chúng ta độc lập hoàn toàn với việc quản lý CSDL bên dưới trong CSDL
 * Khi làm với business logic chúng ra chỉ làm việc với object in bộ nhớ không phải nghĩ làm sao để chuyển nó về các string SQL
 * => bảo trì dễ dàng (tuy lần đầu phải viết nhiều code but chường trình lớn dần lên)
 * => viết unit test cho business logic 1 cách rõ ràng (k cần phải setup database bên dưới)
 * 
 */



namespace LifePlanner
{
    internal class Program
    {
        static void Main(string[] args) // k cần khởi tạo object vẫn can access - tạo ra 1 lần duy nhất được lưu trong memory (xuyên suốt chương trình)
        {
            //IConfigurationRoot config = new ConfigurationBuilder() // nơi define sources configure (JSON, evironment, command line, ...)
            //    .AddJsonFile("appSettings.json", optional: true) // flag = true "if file not exists k gây ra lỗi"
            //    .AddJsonFile("connectionStrings.json", optional: true)
            //    .Build(); // create object ConfigurationBuilder
            /**
             * Server=localhost;Database=ToDoList;Trusted_Connection=True;TrustServerCertificate=True;
             * Server: IP máy chủ
             * Encrypt=True;: Kích hoạt kết nối được mã hóa.
             * ------------------- Trusted_Connection ------------------------------------------------------------------------------------------------------------------
             *  True:   Sử dụng Windows Authentication để kết nối, tức là SQL Server sẽ dựa vào tài khoản Windows của người dùng đang chạy ứng dụng để xác thực.
             *          Ví dụ: Nếu ứng dụng đang chạy với tài khoản User01, SQL Server sẽ xác thực bằng tài khoản đó.
             *          ################################################################################################
             *  False || không sử dụng:
             *          Ứng dụng sẽ cần cung cấp tài khoản SQL Server cụ thể (SQL Authentication) trong chuỗi kết nối bằng cách thêm User ID và Password.
             *          Ví dụ: Server=.;Database=YourDatabase;User ID=sa;Password=yourpassword;
             * ======================================================================================================================================================
             * 
             * ------------------- TrustServerCertificate ------------------------------------------------------------------------------------------------------------------
             *  True:   Allow application pass -> k cần xác thực tính hợp lệ của chứng chỉ SSL/TLS được cung cấp bởi SQL Server
             *          =>>> "Kết nối vẫn mã hóa but k kiểm tra chứng chỉ có hợp lệ không" <<<=
             *          **** use trong môi trường kiểm thử thôi nhé (vì k bảo mật) ***
             *          ################################################################################################
             *  False || không sử dụng:
             *          Ứng dụng sẽ cần cung cấp tài khoản SQL Server cụ thể (SQL Authentication) trong chuỗi kết nối bằng cách thêm User ID và Password.
             *          Ví dụ: Server=.;Database=YourDatabase;User ID=sa;Password=yourpassword;
             * ======================================================================================================================================================
             */
            //string connectionString = config.GetConnectionString("ToDoListDatabase") ?? string.Empty; // null-coalescing [kouə'les] operator (toán tử hợp nhất null) -- check null && default null


            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure((context, app) =>
                    {
                        // Config middleware
                        ConfigureMiddleware(app, context.HostingEnvironment);
                    });
                });

        private static void ConfigureServices(IServiceCollection services)
        {
            // swagger
            services.AddEndpointsApiExplorer();
            // Add services for controlles
            services.AddControllers();
            // Thêm Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Duc Tran API",
                    Version = "v1",
                    Description = "An example API with SQL Server and Swagger",
                });
            });
            
        }

        private static void ConfigureMiddleware(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger(options =>
                {
                    //options.SerializeAsV2 = true; // Nếu dùng version Swagger JSon v2 else default v3
                }); // Kích hoạt middleware để sinh ra tài liệu Swagger (OpenAPI) cho ứng dụng.
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty; // show swagger UI tại đường dẫn gốc http://localhost:5000
                }); // Kích hoạt middleware để hiển thị giao diện người dùng Swagger UI, cho phép tương tác với các API endpoints thông qua trình duyệt.
                app.UseDeveloperExceptionPage(); // Hiển thị thông tin chi tiết lỗi khi phát triển
            }
            else
            {
                app.UseHsts(); // Thêm middleware HSTS (HTTP Strict Transport Security)
            }

            //app.UseHttpsRedirection(); // Chuyển hướng HTTP sang HTTPS
            //app.UseStaticFiles();      // Phục vụ các tệp tĩnh (CSS, JS, hình ảnh, v.v.)
            //app.UseAuthorization();    // Thêm middleware phân quyền

            // Pipeline middleware
            app.UseRouting(); // Xác định hệ thống định tuyến
            // Định nghĩa các endpoint
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Kích hoạt endpoint cho API controllers
                // Bạn có thể thêm các endpoint khác nếu cần
            });
        }
    }
}