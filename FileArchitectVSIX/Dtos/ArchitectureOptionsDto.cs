using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectVSIX.Dtos
{
    public class ArchitectureOptionsDto
    {
        public bool UseRepository { get; set; }
        public bool UseCQRS { get; set; }
        public bool UseUnitOfWork { get; set; }
        public bool UseAutoMapper { get; set; }
        public bool UserTestingProject { get; set; }
        public bool UserSqlServer { get; set; }
        public bool UserPostgreSQL { get; set; }
        public bool UserMongoDB { get; set; }
    }
}
