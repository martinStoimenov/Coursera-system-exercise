using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CourseraSystemExam
{
    internal class Program
    {
        public static Services Services;
        public static CourseraContext courseraDbContext;

        static void Main(string[] args)
        {
            Extensions.RegisterServices();

            Console.WriteLine("Please enter the minimum credit for course...    e.g. 30");
            var minCredit = 0;
            int.TryParse(Console.ReadLine(), out minCredit);

            Console.WriteLine("Please enter the start date for a course...    e.g. 2019/08/20");
            var startDateCredit = DateTime.MinValue;
            DateTime.TryParse(Console.ReadLine(), out startDateCredit);

            Console.WriteLine("Please enter the end date for a course...   e.g. 2019/08/20");
            var endDateCredit = DateTime.MinValue;
            DateTime.TryParse(Console.ReadLine(), out endDateCredit);

            Console.WriteLine("Please enter the path to save the reports...");
            var pathToSaveReports = Console.ReadLine();

            GenerateCourseraReport(minCredit, startDateCredit,endDateCredit, pathToSaveReports);

            Console.ReadLine();
        }

        static async Task GenerateCourseraReport(int minCredit, DateTime startDateCredit, DateTime endDateCredit, string pathToSaveReports)
        {
            //var res = await Services.GetAllStudents<StudentViewModel>();
            //var res = await Services.GetAllInstructors<InstructorViewModel>();
            //var res = await Services.GetAllCourses<CourseViewModel>();
            
            var res = await Services.GetAllCoursesStudentsRelations<StudentsCoursesModel>();

            var output = res.Where(x => x.Course.Credit >= minCredit || (x.CompletionDate >= startDateCredit && x.CompletionDate <= endDateCredit)).OrderBy(x=>x.StudentPinNavigation.FirstName);

            Console.WriteLine();
            Console.WriteLine();

            foreach (var item in output)
            {
                Console.WriteLine($"Student name : {item.StudentPinNavigation.FirstName} {item.StudentPinNavigation.LastName}");
                Console.WriteLine($"Course name : {item.Course.Name} with {item.Course.Credit} credits completed on {item.CompletionDate?.ToString("d")}");
                Console.WriteLine($"Instructor name : {item.Course.Instructor.FirstName} {item.Course.Instructor.LastName}.");
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------");
            }
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