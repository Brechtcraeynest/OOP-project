using Project_ict;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading; // Voeg toe als timer
using System.IO;



namespace ProjectICT
{
    public partial class MainWindow : Window
    {
        // Maak een nieuwe dispatchertimer aan die gametimer noemt en een nieuwe serialPort van de library
        DispatcherTimer gameTimer = new DispatcherTimer();
        SerialPort serialPort = new SerialPort();
        private List<ScoreOpslaan> scores = new List<ScoreOpslaan>();

        // Maak Rectangles aan die als hitboxen werken voor de elementen in het project
        Rect playerHitBox;
        Rect grondHitBox;
        Rect obstakelHitBox;
        Rect coinHitbox;
        Rect coinHitbox2;

        //Maak een nieuw random object aan
        Random rand = new Random();

        // Maak score en coins aan van zelfgemaakte klasses
        Score score = new Score();
        Coins coins = new Coins();

        ImageBrush achtergrondImage = new ImageBrush();
        ImageBrush playerImage = new ImageBrush();
        ImageBrush obstakelImage = new ImageBrush();
        ImageBrush coinImage = new ImageBrush();

        // Maak alle variabelen aan die in het project gebruikt worden
        bool springen;
        int snelheid = 30;
        int zwaartekracht = 50;
        bool gameover = false;
        int levens = 3;
        int coinpos1 = 500;
        int coinpos2 = 650;
        int bewegenNaarRechts = 12;
        int bewegenAchtergrond = 3;
        double spriteInt = 0;

        //integer array die zorgt dat het opstakel een random hoogte heeft
        int[] obstakelPositie = { 320, 310, 300, 305, 315 };

        public MainWindow()
        {
            InitializeComponent();
            SchrijfTxtInList();
            //Voeg een extra optie toe aan de combobox
            cbxPortName.Items.Add("None");

            //Zet de compoorts erin
            foreach (string s in SerialPort.GetPortNames())
                cbxPortName.Items.Add(s);

            //Zet de focus op mycanvas, anders werkt springen niet
            myCanvas.Focus();
            //Zet het gameEngine event op de game timer tick
            gameTimer.Tick += gameEngine;
            // laat de game timer elke 20ms tikken
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);

            achtergrondImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\background.gif"));
            // Voeg de achtergrond toe
            rectAchtergrond.Fill = achtergrondImage;
            rectAchtergrond2.Fill = achtergrondImage;

            obstakelImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\obstakel.png"));
            rectVoorbeeldObstakel.Fill = obstakelImage;

            coinImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\coin.gif"));
            rectVoorbeeldCoin.Fill = coinImage;

            playerImage.ImageSource = new BitmapImage(new Uri("E:\\Downloads\\Afgewerkt Project ICT - Brecht Craeynest (1)\\Project ict\\Project ict\\Afbeeldingen\\Run (1)2.png"));
            rectVoorbeeldPlayer.Fill = playerImage;
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            // Als het game over is en enter word ingeduwt, e is een parameter die de key aangeeft
            if (e.Key == Key.Enter && gameover)
            {
                // start het spel
                StartGame();
            }
        }

        private void Canvas_KeyUp(object sender, KeyEventArgs e)
        {
            // Als spatie is ingedrukt en springen is true en de y positie is boven 260
            if (e.Key == Key.Space && !springen && Canvas.GetTop(rectPlayer) > 260)
            {
                // zet de integers op de juiste waarden
                springen = true;
                snelheid = 15;
                zwaartekracht = -12;
                playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Jump (4).png"));
            }
        }

        private void cbxPortName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (serialPort != null)
            {
                //zorg dat de serial port bruikbaar is en start hem
                if (serialPort.IsOpen)
                    serialPort.Close();

                if (cbxPortName.SelectedItem.ToString() != "None")
                {
                    serialPort.PortName = cbxPortName.SelectedItem.ToString();
                    serialPort.Open();
                }
                else
                {
                    MessageBox.Show("Kies een COM-poort.", "Fout",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Startgame_Click(object sender, RoutedEventArgs e)
        {
            if ((serialPort != null) && (serialPort.IsOpen))
            {
                //check of er een poort is geselecteerd en start het spel
                gbxTutorial.Visibility = Visibility.Hidden;
                gbxPoort.Visibility = Visibility.Hidden;
                gbxStart.Visibility = Visibility.Hidden;
                lblcontrols.Visibility = Visibility.Hidden;
                StartGame();
            }
            else
            { MessageBox.Show($"Fout met het selecteren van een COM-poort"); }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Zorg dat alles afgesloten wordt als het programma dicht gaat
            serialPort.WriteLine("Daaaag");
            Thread.Sleep(200);
            serialPort.WriteLine("");
            serialPort.Dispose();
        }

        private void StartGame()
        {
            //zorg dat de juist elementen van het spel zichtbaar zijn
            grpbxStartspel.Visibility = Visibility.Visible;

            //Zet gameover als onzichtbaar  
            lblGameOver.Visibility = Visibility.Hidden;

            Canvas.SetLeft(rectAchtergrond, 0); // Zet de eerste achtergrond op een afstand van 0
            Canvas.SetLeft(rectAchtergrond2, 1262); // Zet de 2de achtergrond op een afstand van 1262

            Canvas.SetLeft(rectPlayer, 110); //idem
            Canvas.SetTop(rectPlayer, 140);

            Canvas.SetLeft(rectObstakel, 950);
            Canvas.SetTop(rectObstakel, 310);

            Canvas.SetLeft(rectCoin, 500);
            Canvas.SetTop(rectCoin, 320);

            Canvas.SetLeft(rectCoin2, 650);
            Canvas.SetTop(rectCoin2, 320);

            loopImage(1);

            obstakelImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\obstakel.png"));
            rectObstakel.Fill = obstakelImage;

            coinImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\coin.gif"));
            rectCoin.Fill = coinImage;
            rectCoin2.Fill = coinImage;

            // Zorg dat alle bools en variabelen gereset zijn
            springen = false;
            gameover = false;
            levens = 3;
            score.Resetten();
            coins.Resetten();
            bewegenNaarRechts = 12;

            // Zet de score en cointaantal klaar
            lblScoreText.Content = $"Score: {score.Tonen()}";
            lblCoinsText.Content = $"Coins: {coins.Tonen()}";
            lblLevens.Content = $"Levens: {levens}";
            serialPort.WriteLine($" S:{score.Tonen()} C:{coins.Tonen()} L:{levens}");
            // Start de game timer 
            gameTimer.Start();
        }

        private void loopImage(double i)
        {
            switch (i)
            {

                case 1:
                    playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Run (1)2.png"));
                    break;
                case 2:
                    playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Run (2)2.png"));
                    break;
                case 3:
                    playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Run (3)2.png"));
                    break;
                case 4:
                    playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Run (4)2.png"));
                    break;
                case 5:
                    playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Run (5)2.png"));
                    break;
                case 6:
                    playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Run (6)2.png"));
                    break;
                case 7:
                    playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Run (7)2.png"));
                    break;
                case 8:
                    playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Run (8)2.png"));
                    break;

            }
            rectPlayer.Fill = playerImage;
        }

        private void gameEngine(object sender, EventArgs e)
        {
            // beweeg het karakter naar beneden met de zwaartekracht integer
            Canvas.SetTop(rectPlayer, Canvas.GetTop(rectPlayer) + zwaartekracht);
            // Zorg dat de achtergronden met 3 pixels verplaatsen elke tick
            Canvas.SetLeft(rectAchtergrond, Canvas.GetLeft(rectAchtergrond) - bewegenAchtergrond);
            Canvas.SetLeft(rectAchtergrond2, Canvas.GetLeft(rectAchtergrond2) - bewegenAchtergrond);
            // Zorg dat de rectangles met 12 pixels verplaatsen elke tick
            Canvas.SetLeft(rectObstakel, Canvas.GetLeft(rectObstakel) - bewegenNaarRechts);
            Canvas.SetLeft(rectCoin, Canvas.GetLeft(rectCoin) - bewegenNaarRechts);
            Canvas.SetLeft(rectCoin2, Canvas.GetLeft(rectCoin2) - bewegenNaarRechts);
            // Link score text met score integer
            lblScoreText.Content = $"Score: {score.Tonen()}";
            lblCoinsText.Content = $"Coins: {coins.Tonen()}";

            // Zorg dat de hitboxen overeenkomen met de wpf elementen
            playerHitBox = new Rect(Canvas.GetLeft(rectPlayer), Canvas.GetTop(rectPlayer), rectPlayer.Width, rectPlayer.Height);
            grondHitBox = new Rect(Canvas.GetLeft(rectGrond), Canvas.GetTop(rectGrond), rectGrond.Width, rectGrond.Height);
            obstakelHitBox = new Rect(Canvas.GetLeft(rectObstakel), Canvas.GetTop(rectObstakel), rectObstakel.Width, rectObstakel.Height);
            coinHitbox = new Rect(Canvas.GetLeft(rectCoin), Canvas.GetTop(rectCoin), rectCoin.Width, rectCoin.Height);
            coinHitbox2 = new Rect(Canvas.GetLeft(rectCoin2), Canvas.GetTop(rectCoin2), rectCoin2.Width, rectCoin2.Height);

            if (playerHitBox.IntersectsWith(grondHitBox))
            {
                //Als de rectPlayer op de grond is zet je zwaartekracht op 0
                zwaartekracht = 0;
                // Zet de rectPlayer op de grond
                Canvas.SetTop(rectPlayer, Canvas.GetTop(rectGrond) - rectPlayer.Height);
                springen = false;

                spriteInt += .5;
                // Als de sprite boven 8 gaat resetten naar 1
                if (spriteInt > 8)
                {
                    spriteInt = 1;
                }
                // Zet de values in loopimage
                loopImage(spriteInt);

            }

            if (playerHitBox.IntersectsWith(obstakelHitBox))
            {
                // Zet gameover als true en stop de gametimer
                if (levens <= 1 && rectObstakel.Visibility == Visibility.Visible)
                {
                    gameover = true;
                    gameTimer.Stop();
                }
                else if (rectObstakel.Visibility == Visibility.Visible)
                {
                    rectObstakel.Visibility = Visibility.Hidden;
                    levens = levens - 1;
                    lblLevens.Content = $"Levens: {levens}";
                    serialPort.WriteLine($" S:{score.Tonen()} C:{coins.Tonen()} L:{levens}");
                }
            }

            if (coinHitbox.IntersectsWith(obstakelHitBox) && rectCoin.Visibility == Visibility.Visible && rectObstakel.Visibility == Visibility.Visible)
            {
                //Zorg dat de coins nooit in een obstakel zitten
                Canvas.SetLeft(rectCoin, coinpos1 + 30);
            }

            if (coinHitbox2.IntersectsWith(obstakelHitBox) && rectCoin2.Visibility == Visibility.Visible && rectObstakel.Visibility == Visibility.Visible)
            {
                //Zorg dat de coins nooit in een obstakel zitten
                Canvas.SetLeft(rectCoin, coinpos2 + 30);
            }

            if (playerHitBox.IntersectsWith(coinHitbox) && rectCoin.Visibility == Visibility.Visible)
            {
                // Voeg toe aan het cointotaal, zet de rectCoin invisible en de variabele als false
                coins.Toevoegen();
                rectCoin.Visibility = Visibility.Hidden;
                serialPort.WriteLine($" S:{score.Tonen()} C:{coins.Tonen()} L:{levens}");
            }

            if (playerHitBox.IntersectsWith(coinHitbox2) && rectCoin2.Visibility == Visibility.Visible)
            {
                // Voeg toe aan het cointotaal, zet de rectCoin invisible en de variabele als false
                coins.Toevoegen();
                rectCoin2.Visibility = Visibility.Hidden;
                serialPort.WriteLine($" S:{score.Tonen()} C:{coins.Tonen()} L:{levens}");
            }

            if (springen)
            {
                // Zet zwaartekracht op -9 zodat de rectPlayer omhoog gaat
                zwaartekracht = -9;
                // zet de snelheid wat lager
                snelheid--;
            }

            else
            {
                // Zet anders zwaartekracht op 12
                zwaartekracht = 12;
            }

            if (snelheid < 0)
            {
                springen = false;
            }

            if (Canvas.GetLeft(rectAchtergrond) < -1262)
            {
                // Als de eerste achtergrond volledig doorlopen is, zet hem achter de 2de
                Canvas.SetLeft(rectAchtergrond, Canvas.GetLeft(rectAchtergrond2) + rectAchtergrond2.Width);
            }

            if (Canvas.GetLeft(rectAchtergrond2) < -1262)
            {
                // Hetzelfde voor de 2de achtergrond
                Canvas.SetLeft(rectAchtergrond2, Canvas.GetLeft(rectAchtergrond) + rectAchtergrond.Width);
            }

            if (Canvas.GetLeft(rectObstakel) < -50)
            {
                Canvas.SetLeft(rectObstakel, 950);
                // Kies een random positie zodat het opstakel hoger of lager is 
                Canvas.SetTop(rectObstakel, obstakelPositie[rand.Next(0, obstakelPositie.Length)]);
                // Voeg 1 toe aan de score
                score.Toevoegen();
                serialPort.WriteLine($" S:{score.Tonen()} C:{coins.Tonen()} L:{levens}");
                rectObstakel.Visibility = Visibility.Visible;
            }

            if (Canvas.GetLeft(rectCoin) < -40)
            {
                // Zet de rectCoin op de juiste positie en maak ze visible

                Canvas.SetLeft(rectCoin, coinpos1);
                rectCoin.Visibility = Visibility.Visible;
                coinpos1 = 500;
            }

            if (Canvas.GetLeft(rectCoin2) < -40)
            {
                // Zet de rectCoin op de juiste positie en maak ze visible
                Canvas.SetLeft(rectCoin2, coinpos2);
                rectCoin.Visibility = Visibility.Visible;
                coinpos2 = 650;
            }

            if (Convert.ToInt32(score.Tonen()) % 2 == 0)
            {
                bewegenNaarRechts = 12 + (Convert.ToInt32(score.Tonen()) / 2);
            }

            if (gameover)
            {
                // Zet een zwart kader rond het opstakel voor de duidelijkheid
                rectObstakel.Stroke = Brushes.Black;
                rectObstakel.StrokeThickness = 1;

                // Idem met de rectPlayer
                rectPlayer.Stroke = Brushes.Red;
                rectPlayer.StrokeThickness = 1;
                // Zorg voor een game over scherm
                lblLevens.Content = "DUW OP ENTER OM TE HERSTARTEN";
                serialPort.WriteLine($"GAME OVER");
                lblGameOver.Content = $"GAME OVER";
                lblGameOver.Visibility = Visibility.Visible;

                playerImage.ImageSource = new BitmapImage(new Uri("C:\\Users\\brech\\source\\repos\\Project ict\\Project ict\\Afbeeldingen\\Dead (10).png"));
                rectPlayer.Fill = playerImage;
            }

            else
            {
                // Als er geen gameover is zet je alles af
                rectPlayer.StrokeThickness = 0;
                rectObstakel.StrokeThickness = 0;
                lblGameOver.Visibility = Visibility.Hidden;
            }
        }

        private void btnHighscores_Click(object sender, RoutedEventArgs e)
        {
            grdHighscore.Visibility = Visibility.Visible;
        }

        private void btnTerugStartscherm_Click(object sender, RoutedEventArgs e)
        {
            grdHighscore.Visibility = Visibility.Hidden;
        }

        private void btnScoreToevoegen_Click(object sender, RoutedEventArgs e)
        {
            string name = txtbxNaam.Text;
            int score = 0;
            if (!string.IsNullOrEmpty(name) && int.TryParse(txtbxScore.Text, out score))
            {
                // Create a new ScoreOpslaan object and add it to the scores list
                ScoreOpslaan scoreOpslaan = new ScoreOpslaan(name, score);
                scores.Add(scoreOpslaan);

                // Sort scores by score in descending order
                scores = scores.OrderByDescending(x => x.Score).ToList();

                // Update the scores list on the UI
                UpdateList();

                // Save scores to file
                SlaOpInTxt();

                // Clear text boxes
                txtbxNaam.Clear();
                txtbxScore.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a valid name and score.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SchrijfTxtInList()
        {
            try
            {
                string[] lines = File.ReadAllLines("scores.txt");
                scores = new List<ScoreOpslaan>();
                foreach (string line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int score))
                    {
                        ScoreOpslaan ScoreOpslaan = new ScoreOpslaan(parts[0].Trim(), score);
                        scores.Add(ScoreOpslaan);
                    }
                }
                scores = scores.OrderByDescending(x => x.Score).ToList(); // Sorteer scores 
                UpdateList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden van scores: {ex.Message}");
            }
        }

        private void SlaOpInTxt()
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (ScoreOpslaan ScoreOpslaan in scores)
                {
                    string line = $"{ScoreOpslaan.Name}: {ScoreOpslaan.Score}";
                    lines.Add(line);
                }
                File.WriteAllLines("scores.txt", lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan van scores: {ex.Message}");
            }
        }

        private void UpdateList()
        {
            scoresList.Items.Clear();
            foreach (ScoreOpslaan scoreOpslaan in scores)
            {
                scoresList.Items.Add($"{scoreOpslaan.Name}: {scoreOpslaan.Score}");
            }

            if (scores.Count > 0)
            {
                ScoreOpslaan topScore = scores[0];
                lblTopScore.Content = "Top score: " + topScore.Score.ToString() + " door " + topScore.Name;
            }
            else
            {
                lblTopScore.Content = "";
            }
        }
    }
}