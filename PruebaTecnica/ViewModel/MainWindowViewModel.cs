using Microsoft.Win32;

using MvvmHelpers.Commands;

using PropertyChanged;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PruebaTecnica.ViewModel {

    [AddINotifyPropertyChangedInterface]
    public class MainWindowViewModel {

        #region Properties

        public string? programPath { get; set; }

        public string? res { get; set; }

        public string? dirr { get; set; }

        #endregion

        #region Commands
        public Command OpenFileommand { get; set; }
        public AsyncCommand<string> CalculateCommand { get; set; }

        #endregion

        #region TextBoxes

        private string? _SustantivoText;
        [AlsoNotifyFor(nameof(SustantivoTextIsValid))]
        public string SustantivoText {
            get => _SustantivoText!;
            set {
                _SustantivoText = new string(value.Where(char.IsDigit).ToArray());
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

        #region validation

        public bool SustantivoTextIsValid => ValidateTextBox(_SustantivoText!);
        public bool OtroTextIsValid => ValidateTextBox(_VerboText!);

        #endregion

        #region Constructor

        public MainWindowViewModel() {

            // Esto e nadamas para probar
            dirr = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            var filePath = Path.Combine(dirr, "input.txt");

            programPath = filePath;

            OpenFileommand = new Command(OpenFileAction);
            CalculateCommand = new AsyncCommand<string>(CalculateAction);

        }

        private async Task CalculateAction(string num) {
            int[] intcodeProgram = Array.Empty<int>();

            switch (num) {
                case "1":
                    if (!string.IsNullOrEmpty(programPath)
                    && !string.IsNullOrEmpty(SustantivoText)
                    && !string.IsNullOrEmpty(VerboText)) {
                        try {
                            intcodeProgram = ReadIntcodeProgramFromFile(programPath);

                            ModifyIntcodeProgram(intcodeProgram, int.Parse(SustantivoText), int.Parse(VerboText));

                            int result = ExecuteIntcodeProgram(intcodeProgram);

                            res = $"Sustantivo: {SustantivoText} \nVerbo {VerboText}\nResultado: {result}";

                            string fileName = "respuesta1.txt";
                            string filePath = Path.Combine(dirr!, fileName);
                            using StreamWriter writer = new(filePath);
                            await writer.WriteAsync(res);
                            MessageBox.Show($"Archivo creado en: {filePath}");

                        } catch (IOException ex) {
                            throw new Exception($"Error al leer el archivo: {ex.Message}");
                        } catch (InvalidOperationException ex) {
                        }
                    } else {
                        MessageBox.Show("Por favor, especifique la ruta del programa IntCode, el valor de sustantivo y el valor de verbo.");
                    }
                    break;


                case "2":
                    if (!string.IsNullOrEmpty(programPath) && !string.IsNullOrEmpty(res)) {
                        try {
                            const int MAX_VALUE = 999;
                            const int DESIRED_OUTPUT = 19690720;

                            // Leer el programa Intcode desde el archivo
                            intcodeProgram = ReadIntcodeProgramFromFile(programPath);

                            bool resultFound = false;

                            // Buscar los valores de entrada que produzcan el resultado deseado
                            await Task.Run(() => {
                                Parallel.For(0, MAX_VALUE + 1, (sustantivo) => {
                                    for (int verbo = 0; verbo <= MAX_VALUE; verbo++) {
                                        int[] modifiedProgram = (int[])intcodeProgram.Clone();
                                        ModifyIntcodeProgram(modifiedProgram, sustantivo, verbo);
                                        int result = ExecuteIntcodeProgram(modifiedProgram);

                                        if (result == DESIRED_OUTPUT) {
                                            SustantivoText = $"{sustantivo}";
                                            VerboText = $"{verbo}";
                                            resultFound = true;
                                            return;
                                        }
                                    }
                                });
                            });

                            // Mostrar los valores de entrada que producen el resultado deseado
                            if (resultFound) {
                                string fileName = "respuesta2.txt";
                                string filePath = Path.Combine(dirr!, fileName);
                                using StreamWriter writer = new(filePath);
                                await writer.WriteAsync($"Sustantivo: {SustantivoText}\nVerbo: {VerboText}");
                            } else {
                                MessageBox.Show("No se encontró ningún resultado.");
                            }
                        } catch (IOException ex) {
                            MessageBox.Show($"Error al leer el archivo: {ex.Message}");
                        } catch (InvalidOperationException ex) {
                            MessageBox.Show($"Error al ejecutar el programa: {ex.Message}");
                        } catch (Exception ex) {
                            MessageBox.Show($"Se produjo un error inesperado: {ex.Message}");
                        }
                    } else {
                        MessageBox.Show("Por favor, especifique la ruta del programa IntCode.");
                    }
                    break;
                default:
                    break;
            }
        }




        #endregion

        private void OpenFileAction() {

            var openFileDialog = new OpenFileDialog {
                Filter = "Text files (*.txt)|*.txt"
            };

            if (openFileDialog.ShowDialog() == true) {
                programPath = openFileDialog.FileName;
            }

            // Part 1

            Part1();

            // Part 2

            Part2();

        }

        static int ExecuteIntcodeProgram(int[] intcodeProgram) {

            for (int i = 0; i < intcodeProgram.Length; i += 4) {
                int opcode = intcodeProgram[i];
                switch (opcode) {
                    case 1:
                        int input1Position = intcodeProgram[i + 1];
                        int input2Position = intcodeProgram[i + 2];
                        int outputPosition = intcodeProgram[i + 3];
                        intcodeProgram[outputPosition] = intcodeProgram[input1Position] + intcodeProgram[input2Position];
                        break;
                    case 2:
                        input1Position = intcodeProgram[i + 1];
                        input2Position = intcodeProgram[i + 2];
                        outputPosition = intcodeProgram[i + 3];
                        intcodeProgram[outputPosition] = intcodeProgram[input1Position] * intcodeProgram[input2Position];
                        break;
                    case 99:
                        return intcodeProgram[0];
                    default:
                        throw new ArgumentException($"Invalid opcode {opcode} at position {i}");
                }
            }
            throw new ArgumentException("No halt instruction found.");
        }


        private static void ModifyIntcodeProgram(int[] intcodeProgram, int noun, int verb) {
            if (noun >= 0 && noun < intcodeProgram.Length) {
                intcodeProgram[1] = noun;
            }
            if (verb >= 0 && verb < intcodeProgram.Length) {
                intcodeProgram[2] = verb;
            }
        }

        static int[] ReadIntcodeProgramFromFile(string filename) {
            string input = File.ReadAllText(filename);
            string[] split = input.Split(',');
            int[] intcodeProgram = Array.ConvertAll(split, int.Parse);
            return intcodeProgram;
        }

        // This will restrict the user of punting only numbers
        private static bool ValidateTextBox(string value) {
            return string.IsNullOrEmpty(value) || value.All(char.IsDigit);
        }

        private void Part2() {
            const int DESIRED_OUTPUT = 19690720;

            int[] intcodeProgram = ReadIntcodeProgramFromFile(programPath!);

            Parallel.For(0, 99, noun => {
                for (int verb = 0; verb <= 99; verb++) {
                    int[] modifiedProgram = (int[])intcodeProgram.Clone();
                    ModifyIntcodeProgram(modifiedProgram, noun, verb);
                    int result = ExecuteIntcodeProgram(modifiedProgram);
                    if (result == DESIRED_OUTPUT) {
                        MessageBox.Show($"Sustantivo: {noun}, verbo: {verb}");
                        return;
                    }
                }
            });

            MessageBox.Show("Desired output not found.");
        }

        private void Part1() {

            var intcodeProgram = ReadIntcodeProgramFromFile(programPath!);

            ModifyIntcodeProgram(intcodeProgram, 12, 2);

            int result = ExecuteIntcodeProgram(intcodeProgram);

            MessageBox.Show("The value at position 0 after executing the Intcode program is: " + result);
        }

    }
}
