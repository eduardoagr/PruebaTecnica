using Microsoft.Win32;

using MvvmHelpers.Commands;

using PropertyChanged;

using System.IO;
using System.Linq;

namespace PruebaTecnica.ViewModel {

    [AddINotifyPropertyChangedInterface]
    public class MainWindowViewModel {

        public string? FilePath { get; set; }

        public Command OpenFileommand { get; set; }

        #region TextBoxes

        private string? _sustantivoText;
        [AlsoNotifyFor(nameof(SustantivoTextIsValid))]
        public string SustantivoText {
            get => _sustantivoText!;
            set {
                _sustantivoText = new string(value.Where(char.IsDigit).ToArray());
            }
        }

        private string? _VerboText;
        [AlsoNotifyFor(nameof(OtroTextIsValid))]
        public string VerboText {
            get => _VerboText!;
            set {
                _VerboText = new string(value.Where(char.IsDigit).ToArray());
            }
        }

        #endregion

        public bool SustantivoTextIsValid => ValidateTextBox(_sustantivoText!);
        public bool OtroTextIsValid => ValidateTextBox(_VerboText!);


        public MainWindowViewModel() {

            var dir = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            var filePath = Path.Combine(dir, "input.txt");

            FilePath = filePath;

            OpenFileommand = new Command(OpenFileAction);

        }

        private void OpenFileAction() {

            var openFileDialog = new OpenFileDialog {
                Filter = "Text files (*.txt)|*.txt"
            };

            if (openFileDialog.ShowDialog() == true) {
                FilePath = openFileDialog.FileName;
            }
        }

        private static bool ValidateTextBox(string value) {
            return string.IsNullOrEmpty(value) || value.All(char.IsDigit);
        }
    }
}
