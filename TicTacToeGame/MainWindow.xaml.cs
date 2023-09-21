using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace TicTacToeGame
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<Player, ImageSource> imageSources = new()
        {
            {Player.X, new BitmapImage(new Uri("pack://application:,,,/DomainThings/X15.png")) },
            {Player.O, new BitmapImage(new Uri("pack://application:,,,/DomainThings/O15.png"))}
        };

        private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> animations = new()
        {
            {Player.X, new ObjectAnimationUsingKeyFrames() },
            { Player.O, new ObjectAnimationUsingKeyFrames() },
        };

        private readonly DoubleAnimation fadeOutAnimation = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(.5),
            From = 1,
            To = 0,
        };

        private readonly DoubleAnimation fadeInAnimation = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(.5),
            From = 0,
            To = 1,
        };

        private readonly Image[,] imageControls = new Image[3,3];
        private readonly GameSituation gameSituation = new GameSituation();
        public MainWindow()
        {
            InitializeComponent();
            SetUpTheGamePlace();
            SetUpAnimations();
            gameSituation.MoveMade += WhenMoveMade;
            gameSituation.GameEnded += WhenGameEnded;
            gameSituation.GameRestarted += WhenGameRestarted;
        }

        private void SetUpTheGamePlace()
        {
            for(int row = 0;row < 3; row++)
            {
                for(int col = 0;col < 3;col++)
                {
                    Image imageControl = new Image();
                    GameGrid.Children.Add(imageControl);
                    imageControls[row, col] = imageControl;
                }
            }
        }
        private void WhenMoveMade(int row, int col)
        {
            Player player = gameSituation.GameGrid[row, col];
            imageControls[row, col].BeginAnimation(Image.SourceProperty, animations[player]);
            PlayerImage.Source = imageSources[gameSituation.CurrentlyPlayer];
        }

        private void SetUpAnimations()
        {
            animations[Player.X].Duration = TimeSpan.FromSeconds(.25);
            animations[Player.O].Duration = TimeSpan.FromSeconds(.25);

            for(int i = 0; i< 16; i++)
            {
                Uri xUri = new Uri($"pack://application:,,,/DomainThings/X{i}.png");
                BitmapImage xImg= new BitmapImage(xUri);
                DiscreteObjectKeyFrame xKeyFrame = new DiscreteObjectKeyFrame(xImg);
                animations[Player.X].KeyFrames.Add(xKeyFrame);

                Uri oUri = new Uri($"pack://application:,,,/DomainThings/O{i}.png");
                BitmapImage oImg = new BitmapImage(oUri);
                DiscreteObjectKeyFrame oKeyFrame = new DiscreteObjectKeyFrame(oImg);
                animations[Player.O].KeyFrames.Add(oKeyFrame);
            }
        }

        private async Task FadeOut(UIElement uIElement)
        {
            uIElement.BeginAnimation(OpacityProperty, fadeOutAnimation);
            await Task.Delay(fadeOutAnimation.Duration.TimeSpan);
            uIElement.Visibility = Visibility.Hidden;
        }

        private async Task FadeIn(UIElement uIElement)
        {
            uIElement.Visibility = Visibility.Visible;
            uIElement.BeginAnimation(OpacityProperty, fadeInAnimation);
            await Task.Delay(fadeInAnimation.Duration.TimeSpan);
        }
        private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
        {
            await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas));
            ResultText.Text = text;
            WinnerImage.Source = winnerImage;
            await FadeIn(EndScreen);
        }

        private async Task TransitionToGameScreen()
        {
            await FadeOut(EndScreen);
            Line.Visibility = Visibility.Hidden;
            await Task.WhenAll(FadeIn(TurnPanel), FadeIn(GameCanvas));
        }
        private (Point, Point) FindLinePoints(WinInformation winInformation)
        {
            double squareSize = GameGrid.Width / 3;
            double margin = squareSize / 2;

            if (winInformation.Type == WinTypeHowTheWinnerWon.Row)
            {
                double y = winInformation.Number * squareSize + margin;
                return (new Point(0,y), new Point(GameGrid.Width,y));
            }
            if (winInformation.Type == WinTypeHowTheWinnerWon.Column)
            {
                double x = winInformation.Number * squareSize + margin;
                return (new Point(x, 0), new Point(x, GameGrid.Height));
            }
            if (winInformation.Type == WinTypeHowTheWinnerWon.MainDiagonal)
            {
                return (new Point(0,0), new Point(GameGrid.Width, GameGrid.Height));
            }
            return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height));
        }

        private async Task ShowLine(WinInformation winInformation)
        {
            (Point start, Point end) = FindLinePoints(winInformation);

            Line.X1 = start.X;
            Line.Y1 = start.Y;

            DoubleAnimation x2Animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(.25),
                From = start.X,
                To = end.X,
            };
            DoubleAnimation y2Animation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(.25),
                From = start.Y,
                To = end.Y,
            };

            Line.Visibility = Visibility.Visible;
            Line.BeginAnimation(Line.X2Property, x2Animation);
            Line.BeginAnimation(Line.Y2Property, y2Animation);
            await Task.Delay(x2Animation.Duration.TimeSpan);
        }
        private async void WhenGameEnded(ResultOfTheGame resultOfTheGame)
        {
            if (resultOfTheGame.Winner == Player.None)
            {
                await TransitionToEndScreen("Draw", null);
            }
            else
            {
                await ShowLine(resultOfTheGame.WinInformation);
                await Task.Delay(1000);
                await TransitionToEndScreen("Winner: ", imageSources[resultOfTheGame.Winner]);
            }
        }
        private async void WhenGameRestarted()
        {
            for(int row  = 0; row < 3; row ++)
            {
                for(int col = 0; col < 3; col++)
                {
                    imageControls[row, col].BeginAnimation(Image.SourceProperty, null);
                    imageControls[row, col].Source = null;
                }
            }
            PlayerImage.Source = imageSources[gameSituation.CurrentlyPlayer];
            await TransitionToGameScreen();
        }
        private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double squareSize = GameGrid.Width / 3;
            Point clickPosition = e.GetPosition(GameGrid);
            int row = (int)(clickPosition.Y / squareSize);
            int col = (int)(clickPosition.X / squareSize);
            gameSituation.MakeMove(row, col);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(gameSituation.IsGameOver)
            {
                gameSituation.NewGameReset();
            }
        }
    }
}
