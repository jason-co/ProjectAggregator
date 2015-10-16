using System.IO;

namespace EnvDTE.Helpers
{
    public class ProjectWrapper
    {
        private Project _project;

        public ProjectWrapper(Project project)
        {
            _project = project;
        }

        public string FullName { get { return _project.FullName; } }
    }
}
