using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using static CourseraSystemExam.Services;

namespace CourseraSystemExam
{
    internal class Program
    {
        public static IConfiguration? iConfiguration;
        public static Services Services;
        public static CourseraContext courseraDbContext;

        static void Main(string[] args)
        {
            Extensions.RegisterServices();

            // TODO:

            //var minCredit = Console.ReadLine();
            //var startDateCredit = Console.ReadLine();
            //var endDateCredit = Console.ReadLine();
            //var pathToSaveReports = Console.ReadLine();

            ShowCourseraReport();

            Console.WriteLine("Hello, World!");
            Console.ReadLine();
        }


        static async Task ShowCourseraReport()
        {
            //var res = await Services.GetAllStudents<StudentViewModel>();
            //var res = await Services.GetAllInstructors<InstructorViewModel>();
            //var res = await Services.GetAllCourses<CourseViewModel>();
            
            var res = await Services.GetAllCoursesStudentsRelations<StudentsCoursesModel>();

            // the time wasn't enough for me but i have all the data that's needed for the CSV and HTML files in the res variable

            var a = 0;
        }

    }

    public static class Common
    {
        private static readonly string CSVreportFileNameWithExtension = "report.csv";
        private static readonly string HTMLreportFileNameWithExtension = "report.html";
    }

    public static class Extensions
    {
        public static void RegisterServices()
        {
            var services = new ServiceCollection();

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddSingleton<Services>();
            services.AddDbContext<CourseraContext>(options => options.UseSqlServer("Server=.;Database=coursera;Integrated Security=true;Trusted_Connection=true;;TrustServerCertificate=True"));

            var serviceProvider = services.BuildServiceProvider();
            Program.Services = serviceProvider.GetService<Services>();
            Program.courseraDbContext = serviceProvider.GetService<CourseraContext>();

            RegisterAutomapper();
        }

        private static void RegisterAutomapper()
        {
            AutoMapperConfig.RegisterMappings(typeof(StudentViewModel).GetTypeInfo().Assembly);
        }
    }

    public class Services
    {
        private readonly IRepository<Student> studentRepo;
        private readonly IRepository<Course> courseRepo;
        private readonly IRepository<Instructor> instructorRepo;
        private readonly IRepository<StudentsCoursesXref> studentsCoursesRepo;

        public Services(IRepository<Student> studentRepo,
            IRepository<Course> courseRepo, 
            IRepository<Instructor> instructorRepo, 
            IRepository<StudentsCoursesXref> studentsCoursesRepo
            )
        {
            this.studentRepo = studentRepo;
            this.courseRepo = courseRepo;
            this.instructorRepo = instructorRepo;
            this.studentsCoursesRepo = studentsCoursesRepo;
        }

        public async Task<IEnumerable<T>> GetAllStudents<T>() => await studentRepo.All().Include(x=>x.StudentsCoursesXrefs).ThenInclude(x=>x.Course).To<T>().ToListAsync();
        public async Task<IEnumerable<T>> GetAllInstructors<T>() => await instructorRepo.All().Include(x => x.Courses).ThenInclude(x => x.StudentsCoursesXrefs).ThenInclude(x=>x.StudentPinNavigation).To<T>().ToListAsync();
        public async Task<IEnumerable<T>> GetAllCourses<T>() => await courseRepo.All().Include(x => x.StudentsCoursesXrefs).ThenInclude(x => x.StudentPinNavigation).To<T>().ToListAsync();
        public async Task<IEnumerable<T>> GetAllCoursesStudentsRelations<T>() => await studentsCoursesRepo.All().To<T>().ToListAsync();
    }

    public class StudentViewModel : IMapFrom<Student>
    {
        public string Pin { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime TimeCreated { get; set; }

        public IEnumerable<StudentsCoursesModel> Courses { get; set; }
    }

    public class CourseViewModel : IMapFrom<Course>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int InstructorId { get; set; }

        public byte TotalTime { get; set; }

        public byte Credit { get; set; }

        public DateTime TimeCreated { get; set; }

        public InstructorViewModel Instructor { get; set; }

        public IEnumerable<StudentViewModel> Students { get; set; }
    }

    public class InstructorViewModel : IMapFrom<Instructor>
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime TimeCreated { get; set; }

        public IEnumerable<CourseViewModel> Courses { get; set; }
    }

    public class StudentsCoursesModel : IMapFrom<StudentsCoursesXref>
    {
        public string StudentPin { get; set; }

        public int CourseId { get; set; }

        public DateTime? CompletionDate { get; set; }

        public CourseViewModel Course { get; set; }

        public StudentViewModel StudentPinNavigation { get; set; }
    }






    //static void GetAppSettingsFile()
    //{
    //    var builder = new ConfigurationBuilder()
    //        .SetBasePath(Directory.GetCurrentDirectory())
    //        .AddJsonFile("appsettings.json",
    //            optional: false, reloadOnChange: true);
    //    iConfiguration = builder.Build();
    //}
}