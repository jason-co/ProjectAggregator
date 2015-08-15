using System.IO;

namespace EnvDTE.Helpers
{
    public class ProjectWrapper
    {
        private readonly string _projectPath;
        private Project _project;

        public ProjectWrapper(Project project)
        {
            _project = project;
        }

        public ProjectWrapper(string projectPath, VisualStudioVersion visualStudioVersion)
        {
            var dte = EnvDTEFactory.Create(visualStudioVersion);
            _projectPath = Path.GetFileName(projectPath);
            _project = dte.Solution.AddFromFile(projectPath);
        }

        public Project Project { get { return _project; } }
        public string FileName { get { return _project.FileName; } }
        public string FullName { get { return _project.FullName; } }
        public string Name { get { return _project.Name; } }
        public string ProjectName { get { return _projectPath; } }
    }
}
