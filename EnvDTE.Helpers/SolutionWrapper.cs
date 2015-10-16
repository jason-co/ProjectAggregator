using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core;

namespace EnvDTE.Helpers
{
    public class SolutionWrapper
    {
        #region fields

        private const string SolutionBusyMessage = "(RPC_E_SERVERCALL_RETRYLATER)";
        private const int SolutionWaitTimeInMS = 3000;

        private readonly string[] _projectExtensions = { ".csproj", ".vbproj" };

        private readonly IList<ProjectWrapper> _projectWrappers;

        private readonly DTE _dte;
        private readonly string _solutionName;
        private readonly ILogger _logger;

        private FileInfo[] _missingProjects;

        #endregion

        #region constructor

        public SolutionWrapper(string solutionName, VisualStudioVersion visualStudioVersion, ILogger logger)
        {
            _solutionName = solutionName;
            _logger = logger;
            _projectWrappers = new List<ProjectWrapper>();
            _dte = EnvDTEFactory.Create(visualStudioVersion);
        }

        #endregion

        #region public properties

        public FileInfo[] MissingProjects { get { return _missingProjects; } }

        #endregion

        #region public methods

        public async Task<bool> AggregateProjects(string rootPath, int iterations = 15)
        {
            _missingProjects = GetProjectsMissingFromSolution(rootPath).ToArray();

            if (_missingProjects.Any())
            {
                await OpenSolution();

                _logger.Log("Attempting to add all projects in {0} attempts", iterations);
                for (int i = 0; i < iterations && _missingProjects.Any(); i++)
                {
                    _logger.Log("************ Attempt {0} ************", i + 1);

                    _logger.Log("Number of projects missing from the solution: {0}", _missingProjects.Count());

                    AddProjects(_missingProjects);

                    await AttemptTo(() =>
                    {
                        _missingProjects = _missingProjects.Where(p => _projectWrappers.All(pw => pw.FullName != p.FullName)).ToArray();
                    });
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
                            var projectFile = split.FirstOrDefault(s => s.EndsWith("proj", StringComparison.OrdinalIgnoreCase));
                            var projectName = projectFile.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                            projects.Add(projectName);
                        }
                    }
                }

                _logger.Log("Number of projects currently in the solution: {0}", projects.Count());
            }

            return projects;
        }

        private FileInfo[] GetProjectFilesInDirectory(string rootPath)
        {
            var directoryInfo = new DirectoryInfo(rootPath);

            return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => _projectExtensions.Contains(f.Extension)).ToArray();
        }

        #endregion

        #region Add Projects

        private void AddProjects(FileInfo[] missingProjects)
        {
            foreach (var project in missingProjects)
            {
                try
                {
                    AttemptTo(() =>
                    {
                        if (_projectWrappers.All(p => p.FullName != project.FullName))
                        {
                            AddProjectFromFile(project.FullName);
                            _logger.Log("\tProject Added: {0}", project.Name);
                        }
                    }, 3).Wait();
                }
                catch (Exception)
                {
                    _logger.Log("Skipping adding this project for now");
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
            if (!File.Exists(_solutionName))
            {
                _logger.Log("Since solution does not exist, creating dummy placeholder");
                await SaveAsync();
            }
            else
            {
                _logger.Log("Solution already exists");
            }

            _dte.Solution.Open(_solutionName);

            await AttemptTo(CreateProjectWrappers);
        }

        private async Task SaveAsync()
        {
            _logger.Log("Saving Solution");
            await AttemptTo(() =>
            {
                _dte.Solution.SaveAs(_solutionName);
            });
        }

        public async Task CloseAsync()
        {
            _logger.Log("Closing Solution");
            await AttemptTo(() =>
            {
                _dte.Solution.Close();
                _dte.Quit();
            });
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
            catch (Exception ex)
            {
                if (ex.Message.Contains(SolutionBusyMessage))
                {
                    _logger.Log("Solution busy... Waiting {0} seconds", SolutionWaitTimeInMS / 1000);
                }
                System.Threading.Thread.Sleep(SolutionWaitTimeInMS);
                Task.Run(() => AttemptTo(act, attemptsLeft - 1)).Wait();
            }
        }

        #endregion
    }
}
