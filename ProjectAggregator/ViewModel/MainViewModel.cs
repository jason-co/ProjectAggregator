using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Core;
using Core.Commands;
using EnvDTE.Helpers;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace ProjectAggregator.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region private members

        private const string SolutionExtension = ".sln";

        private readonly ILogger _logger;
        private readonly IList<FileInfo> _missingProjects; 

        private string _solutionFileName;
        private string _rootFolderPath;
        private bool _isAggregating;
        private bool _displayMissingProjects;

        private SolutionWrapper _solutionWrapper;
        private VisualStudioVersion _selectedVisualStudioVersion;

        private RelayCommand _openSolutionCommand;
        private RelayCommand _createSolutionCommand;
        private RelayCommand _selectFolderCommand;
        private RelayCommand _aggregateCommand;
        private RelayCommand _viewMissingProjectsCommand;
        private RelayCommand _closeMissingProjectsCommand;

        #endregion

        #region constructor

        public MainViewModel(ILogger logger)
        {
            VisualStudioVersions = Enum.GetValues(typeof(VisualStudioVersion)).Cast<VisualStudioVersion>().ToArray();
            _selectedVisualStudioVersion = VisualStudioVersion.VisualStudio2015;
            _logger = logger;
            _missingProjects = new ObservableCollection<FileInfo>();
        }

        #endregion

        #region properties

        public ILogger Logger { get { return _logger; } }
        public IList<FileInfo> MissingProjects {  get { return _missingProjects; } }

        public string SolutionFileName
        {
            get { return _solutionFileName; }
            set { SetProperty(ref _solutionFileName, value); }
        }

        public string RootFolderPath
        {
            get { return _rootFolderPath; }
            set { SetProperty(ref _rootFolderPath, value); }
        }

        public bool IsAggregating
        {
            get { return _isAggregating; }
            set { SetProperty(ref _isAggregating, value); }
        }

        public VisualStudioVersion[] VisualStudioVersions { get; }

        public VisualStudioVersion SelectedVisualStudioVersion
        {
            get { return _selectedVisualStudioVersion; }
            set { SetProperty(ref _selectedVisualStudioVersion, value); }
        }

        public bool DisplayMissingProjects
        {
            get { return _displayMissingProjects; }
            set { SetProperty(ref _displayMissingProjects, value); }
        }

        #endregion

        #region ICommands

        public ICommand OpenSolutionCommand => RelayCommand.CreateCommand(ref _openSolutionCommand, OpenSolutionExecute, OpenSolutionCanExecute);
        public ICommand CreateSolutionCommand => RelayCommand.CreateCommand(ref _createSolutionCommand, CreateSolutionExecute, CreateSolutionCanExecute);
        public ICommand SelectFolderCommand => RelayCommand.CreateCommand(ref _selectFolderCommand, SelectFolderExecute, SelectFolderCanExecute);
        public ICommand AggregateCommand => RelayCommand.CreateCommand(ref _aggregateCommand, AggregateExecute, AggregateCanExecute);
        public ICommand ViewMissingProjectsCommand => RelayCommand.CreateCommand(ref _viewMissingProjectsCommand, ViewMissingProjectsExecute, ViewMissingProjectsCanExecute);
        public ICommand CloseMissingProjectsCommand => RelayCommand.CreateCommand(ref _closeMissingProjectsCommand, CloseMissingProjectsExecute);


        private void OpenSolutionExecute()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = String.Format("Solution files (*{0})|*{0}", SolutionExtension);

            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                SolutionFileName = fileDialog.FileName;
            }
        }

        private bool OpenSolutionCanExecute()
        {
            return !_isAggregating;
        }

        private void CreateSolutionExecute()
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = SolutionExtension;
            saveDialog.Filter = String.Format("Solution files (*{0})|*{0}", SolutionExtension);

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                SolutionFileName = saveDialog.FileName;
            }
        }

        private bool CreateSolutionCanExecute()
        {
            return !_isAggregating;
        }

        private void SelectFolderExecute()
        {
            var folderDialog = new FolderBrowserDialog();

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                RootFolderPath = folderDialog.SelectedPath;
            }
        }
        private bool SelectFolderCanExecute()
        {
            return !_isAggregating;
        }

        private bool AggregateCanExecute()
        {
            return !String.IsNullOrEmpty(_solutionFileName) && !String.IsNullOrEmpty(_rootFolderPath) && !IsAggregating;
        }

        private void AggregateExecute()
        {
            IsAggregating = true;

            Task.Run(() => AggregateAsync());
        }

        private void ViewMissingProjectsExecute()
        {
            _missingProjects.Clear();
            if (_solutionWrapper != null)
            {
                foreach (var project in _solutionWrapper.MissingProjects)
                {
                    _missingProjects.Add(project);
                }
            }
            DisplayMissingProjects = true;
        }

        private bool ViewMissingProjectsCanExecute()
        {
            return !IsAggregating;
        }

        private void CloseMissingProjectsExecute()
        {
            DisplayMissingProjects = false;
        }

        #endregion

        #region private methods

        private async Task AggregateAsync()
        {
            try
            {
                _logger.Log(String.Empty);
                _logger.Log("**************************************************");
                _logger.Log("     Starting the Project Aggregation process     ");
                _logger.Log("**************************************************");
                _logger.Log(String.Empty);

                _logger.Log("Solution: {0}", _solutionFileName);
                _logger.Log("Root Folder: {0}", _rootFolderPath);

                _solutionWrapper = new SolutionWrapper(_solutionFileName, _selectedVisualStudioVersion, _logger);

                await _solutionWrapper.AggregateProjects(_rootFolderPath);
            }
            catch (Exception)
            {
                _logger.Log("~~~~~~ An error occurred while attempting to aggregate all the projects ~~~~~~~");

                if (_solutionWrapper != null)
                {
                    await _solutionWrapper.CloseAsync();
                }
            }
            finally
            {
                BeginUpdateUI(() =>
                {

                    IsAggregating = false;

                    _logger.Log(String.Empty);
                    _logger.Log("**************************************************");
                    _logger.Log("     Project Aggretation process is finished     ");
                    _logger.Log("**************************************************");
                    _logger.Log(String.Empty);
                });
            }
        }

        #endregion
    }
}
