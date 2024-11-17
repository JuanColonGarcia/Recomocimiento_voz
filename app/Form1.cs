using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace REcoSample
{
    public partial class Form1 : Form
    {
        private System.Speech.Recognition.SpeechRecognitionEngine _recognizer =
           new SpeechRecognitionEngine();
        private SpeechSynthesizer synth = new SpeechSynthesizer();

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            synth.Speak("Bienvenido al diseño de interfaces avanzadas . Inicializando la Aplicación");

            Grammar grammar = CreateGrammarBuilderRGBSemantics2(null);
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.UnloadAllGrammars();
            // Nivel de confianza del reconocimiento 70%
            _recognizer.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 60);
            grammar.Enabled = true;
            _recognizer.LoadGrammar(grammar);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(_recognizer_SpeechRecognized);
            //reconocimiento asíncrono y múltiples veces
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            InicializarRutasImagenes();

            synth.Speak("Aplicación preparada para reconocer su voz, por favor, diga una figura geométrica y un color");
        }

        public enum Figura
        {
            Circulo,
            Cuadrado,
            Triangulo,
            Rectangulo,
            Hexagono
        }

        private Dictionary<Figura, string> figuraImagenes;
        private void InicializarRutasImagenes()
        {
            string carpetaFiguras = Path.Combine(Application.StartupPath, "figuras");
            figuraImagenes = new Dictionary<Figura, string>
           {

            { Figura.Circulo, Path.Combine(carpetaFiguras, "circulo.png") },
            { Figura.Cuadrado, Path.Combine(carpetaFiguras, "cuadrado.png") },
            { Figura.Triangulo, Path.Combine(carpetaFiguras, "triangulo.png") },
            { Figura.Rectangulo, Path.Combine(carpetaFiguras, "rectangulo.png") },
            { Figura.Hexagono, Path.Combine(carpetaFiguras, "hexagono.png") }

            };
        }


        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Obtenemos un diccionario con los elementos semánticos
            SemanticValue semantics = e.Result.Semantics;

            string rawText = e.Result.Text;

            if (!semantics.ContainsKey("rgb") || !semantics.ContainsKey("figura"))
            {
                this.label1.Text = "No info provided.";
                return;
            }
            else
            {
                this.label1.Text = rawText;

                // Convertir el valor semántico de la figura a un string y luego a un enum 'Figura'
                string figuraReconocidaStr = semantics["figura"].Value.ToString();
                if (Enum.TryParse(figuraReconocidaStr, out Figura figuraReconocida))
                {
                    // Ahora 'figuraReconocida' es del tipo 'Figura'
                    this.BackColor = Color.FromArgb((int)semantics["rgb"].Value);
                    Update();

                    // Mostrar la imagen asociada a la figura reconocida
                    if (figuraImagenes.TryGetValue(figuraReconocida, out string rutaImagen))
                    {
                        pictureBox1.Image = Image.FromFile(rutaImagen); // Mostrar imagen en pictureBox1
                    }
                    else
                    {
                        MessageBox.Show($"No hay imagen disponible para la figura: {figuraReconocida}");
                    }
                }
                else
                {
                    MessageBox.Show("Figura no válida.");
                }
            }
        }


        private Grammar CreateGrammarBuilderRGBSemantics2(params int[] info)
        {
            //synth.Speak("Creando ahora la gramática");
            Choices figureChoice = new Choices();

            // FIGURA
            figureChoice.Add(new SemanticResultValue("Circulo", "Circulo"));
            figureChoice.Add(new SemanticResultValue("Cuadrado", "Cuadrado"));
            figureChoice.Add(new SemanticResultValue("Triangulo", "Triangulo"));
            figureChoice.Add(new SemanticResultValue("Rectangulo", "Rectangulo"));
            figureChoice.Add(new SemanticResultValue("Hexagono", "Hexagono"));

            SemanticResultKey figuraResultKey = new SemanticResultKey("figura", figureChoice);
            GrammarBuilder figuras = new GrammarBuilder(figuraResultKey);

            // COLOR

            Choices colorChoice = new Choices();

            SemanticResultValue choiceResultValue =
                    new SemanticResultValue("Rojo", Color.FromName("Red").ToArgb());
            GrammarBuilder resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);

            choiceResultValue =
                   new SemanticResultValue("Azul", Color.FromName("Blue").ToArgb());
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);

            choiceResultValue =
                   new SemanticResultValue("Verde", Color.FromName("Green").ToArgb());
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);

            choiceResultValue =
                   new SemanticResultValue("Rosa", Color.FromName("pink").ToArgb());
            resultValueBuilder = new GrammarBuilder(choiceResultValue);
            colorChoice.Add(resultValueBuilder);

            SemanticResultKey choiceResultKey = new SemanticResultKey("rgb", colorChoice);
            GrammarBuilder colores = new GrammarBuilder(choiceResultKey);


            GrammarBuilder poner = "Poner";
            GrammarBuilder cambiar = "Cambiar";

            Choices dos_alternativas = new Choices(poner, cambiar);
            GrammarBuilder frase = new GrammarBuilder(dos_alternativas);
            frase.Append(figuras);
            frase.Append(colores);
            Grammar grammar = new Grammar(frase);
            grammar.Name = "Poner/Cambiar";

            // EJEMPLO: PONER CUADRADO VERDE
            //Grammar grammar = new Grammar("so.xml.txt");

            return grammar;



        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}