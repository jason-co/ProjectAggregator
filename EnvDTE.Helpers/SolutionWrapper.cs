using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EnvDTE.Helpers
{
    public class SolutionWrapper
    {
        #region fields

        private readonly string[] _projectExtensions = { ".csproj", ".vbproj" };

        private readonly IList<ProjectWrapper> _projectWrappers;

        private readonly DTE _dte;
        private readonly string _solutionName;
        private readonly VisualStudioVersion _visualStudioVersion;

        #endregion

        #region constructor

        public SolutionWrapper(string solutionName, VisualStudioVersion visualStudioVersion)
        {
            _solutionName = solutionName;
            _visualStudioVersion = visualStudioVersion;
            _projectWrappers = new List<ProjectWrapper>();
            _dte = EnvDTEFactory.Create(visualStudioVersion);
        }

        #endregion

        #region public methods

        public async Task<bool> AggregateProjects(string rootPath, int iterations = 15)
        {
            var missingProjects = GetProjectsMissingFromSolution(rootPath).ToArray();
            if (missingProjects.Any())
            {
                await OpenSolution();

                for (int i = 0; i < iterations; i++)
                {
                    //Console.WriteLine("\n************\nIteration{0}\n************\n", i + 1);
                    AddProjects(missingProjects);
                }

                await SaveAsync();

                await CloseAsync();

                return true;
            }

            return false;
        }

        #endregion

        #region Collecting Projects

        private IEnumerable<FileInfo> GetProjectsMissingFromSolution(string rootPath)
        {
            var projectsInSolution = GetProjectNamesInSolution();
            var projectsInDirectory = GetProjectFilesInDirectory(rootPath);

            foreach (var projectFile in projectsInDirectory)
            {
                if (!projectsInSolution.Any(p => p.Contains(projectFile.Name)))
                {
                    yield return projectFile;
                }
            }
        }

        private IList<string> GetProjectNamesInSolution()
        {
            var projects = new List<string>();
            if (File.Exists(_solutionName))
            {
                var filter = "proj\"";
                using (var file = new StreamReader(_solutionName))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var split = line.Split(new[] { "\"" }, StringSplitOptions.RemoveEmptyEntries);
                            var projectFile = split.FirstOrDefault(s =>
                                s.IndexOf("proj", StringComparison.OrdinalIgnoreCase) >= 0
                                && s.IndexOf("project", StringComparison.OrdinalIgnoreCase) < 0);
                            var projectName = projectFile.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                            projects.Add(projectName);
                        }
                    }
                }

            }

            return projects;
        }

        private FileInfo[] GetProjectFilesInDirectory(string rootPath)
        {
            var directoryInfo = new DirectoryInfo(rootPath);

            return directoryInfo.EnumerateFiles().Where(f => _projectExtensions.Contains(f.Extension)).ToArray();
        }

        #endregion

        #region Project Wrappers Creation

        private void CreateProjectWrappers()
        {
            var projs = _dte.Solution.Projects.Cast<Project>().ToArray();

            foreach (var project in projs)
            {
                CreateProjectWrapper(project);
            }
        }

        private void CreateProjectWrapper(Project project)
        {
            if (project != null)
            {
                if (project.Kind == Constants.vsProjectKindSolutionItems
                    || project.Kind == Constants.vsProjectKindMisc)
                {
                    foreach (var projectItem in project.ProjectItems.Cast<ProjectItem>())
                    {
                        CreateProjectWrapper(projectItem.SubProject);
                    }
                }
                else
                {
                    var wrapper = new ProjectWrapper(project);
                    _projectWrappers.Add(wrapper);
                }
            }
        }

        #endregion

        #region Add Projects

        private void AddProjects(FileInfo[] missingProjects)
        {
            foreach (var project in missingProjects)
            {
                bool succeeded = true;
                try
                {
                    if (_projectWrappers.All(p => p.FullName != project.FullName))
                    {
                        AddProjectFromFile(project.FullName);
                    }
                }
                catch (Exception)
                {
                    succeeded = false;
                }
                finally
                {
                    if (succeeded)
                    {
                        Console.WriteLine(project.Name);
                    }
                }
            }
        }

        private void AddProjectFromFile(string fileName)
        {
            var project = _dte.Solution.AddFromFile(fileName, false);
            var wrapper = new ProjectWrapper(project);
            _projectWrappers.Add(wrapper);
        }

        #endregion

        #region Solution Open/Save/Close

        private async Task OpenSolution()
        {
            _dte.Solution.Open(_solutionName);

            await AttemptTo(CreateProjectWrappers);
        }

        private async Task SaveAsync()
        {
            await AttemptTo(() => SaveInternal(_solutionName));
        }

        private void SaveInternal(string filePath)
        {
            _dte.Solution.SaveAs(filePath);
        }

        private async Task CloseAsync()
        {
            await AttemptTo(CloseInternal);
        }

        private void CloseInternal()
        {
            _dte.Solution.Close();
            _dte.Quit();
        }

        #endregion

        #region Helpers

        private async Task AttemptTo(Action act, int attemptsLeft = 10)
        {
            try
            {
                if (attemptsLeft >= 0)
                {
                    act();
                }
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(3000);
                Task.Run(() => AttemptTo(act, attemptsLeft - 1)).Wait();
            }
        }

        #endregion
    }
}
