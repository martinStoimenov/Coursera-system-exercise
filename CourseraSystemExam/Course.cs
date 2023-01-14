using System;
using System.Collections.Generic;

namespace CourseraSystemExam;

public partial class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int InstructorId { get; set; }

    public byte TotalTime { get; set; }

    public byte Credit { get; set; }

    public DateTime TimeCreated { get; set; }

    public virtual Instructor Instructor { get; set; } = null!;

    public virtual ICollection<StudentsCoursesXref> StudentsCoursesXrefs { get; } = new List<StudentsCoursesXref>();
}
