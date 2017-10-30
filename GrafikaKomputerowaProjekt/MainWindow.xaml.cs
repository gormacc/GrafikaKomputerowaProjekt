using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GrafikaKomputerowaProjekt.Restriction;

namespace GrafikaKomputerowaProjekt
{
    //notatki


    //notatki

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Verticle> _verticles = new List<Verticle>();
        private List<Verticle> _copyOfVerticles = new List<Verticle>();
        private readonly List<Line> _lines = new List<Line>();
        private readonly ContextMenu _verticleContextMenu = new ContextMenu();
        private readonly ContextMenu _firstVerticleContextMenu = new ContextMenu();
        private readonly ContextMenu _lineContextMenu = new ContextMenu();

        private int VerticleSize => Properties.Settings.Default.VerticleSize;
        private int LinePointSize => Properties.Settings.Default.LinePixelSize;
        private int RestrictionMargin => Properties.Settings.Default.RestrictionMargin;
        private int _verticleIndexer = 0;
        private Line _helpingLine;
        private Verticle _movingVerticle;
        private bool _isMovingVerticleSet = false;
        private bool _polygonMade = false;
        private int _movePolygonXPosition;
        private int _movePolygonYPosition;
        private Line _lineStillLengthBeingRestricted;
        private int _startCheckingRestrictionVerticleId = 0;
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeCanvas();
            InitializeVerticleContextMenu();
            InitializeFirstVerticleContextMenu();
            InitializeLineContextMenu();
        }

        private void InitializeCanvas()
        {
            EnableSettingVerticle();
            EnableDrawingHelpingLine();
        }

        #region Initializing Context Menus

        private void InitializeVerticleContextMenu()
        {
            MenuItem mi = new MenuItem { Header = "Usun wierzchołek" };
            mi.Click += DeleteVerticle;

            _verticleContextMenu.Items.Add(mi);
        }

        private void InitializeFirstVerticleContextMenu()
        {

            MenuItem endDrawingPolygonMenuItem = new MenuItem { Header = "Zakoncz rysowanie wielokata" };
            endDrawingPolygonMenuItem.Click += EndDrawingPolygon;

            _firstVerticleContextMenu.Items.Add(endDrawingPolygonMenuItem);
        }

        private void InitializeLineContextMenu()
        {
            MenuItem mi1 = new MenuItem { Header = "Dodaj wierzcholek" };
            mi1.Click += AddVerticleInTheMiddleOfLine;

            MenuItem mi2 = new MenuItem {Header = "Krawedz pozioma"};
            mi2.Click += AddHorizontalLineRestriction;

            MenuItem mi3 = new MenuItem { Header = "Krawedz pionowa" };
            mi3.Click += AddVerticalLineRestriction;

            MenuItem mi4 = new MenuItem { Header = "Stała długość" };
            mi4.Click += AddLengthStillRestriction;

            MenuItem mi5 = new MenuItem { Header = "Usuń restrykcje" };
            mi5.Click += RemoveRestriction;

            _lineContextMenu.Items.Add(mi1);
            _lineContextMenu.Items.Add(mi2);
            _lineContextMenu.Items.Add(mi3);
            _lineContextMenu.Items.Add(mi4);
            _lineContextMenu.Items.Add(mi5);
        }

        #endregion

        #region Enabling Disabling

        private void EnableDrawingHelpingLine()
        {
            canvas.MouseMove += LineHelper;
        }

        private void DisableDrawingHelpingLine()
        {
            canvas.MouseMove -= LineHelper;
        }

        private void EnableSettingVerticle()
        {
            canvas.MouseLeftButtonDown += SetVerticle;
        }

        private void DisableSettingVerticle()
        {
            canvas.MouseLeftButtonDown -= SetVerticle;
        }

        private void EnableMovingPolygon()
        {
            canvas.MouseLeftButtonDown += LeftButtonDownPolygon;
            canvas.MouseLeftButtonUp += LeftButtonUpPolygon;
        }

        private void DisableMovingPolygon()
        {
            canvas.MouseLeftButtonDown -= LeftButtonDownPolygon;
            canvas.MouseLeftButtonUp -= LeftButtonUpPolygon;
        }

        private void EnableMovingVerticles()
        {
            foreach (var ver in _verticles)
            {
                ver.Rectangle.MouseLeftButtonDown += LeftButtonDownVerticle;
                ver.Rectangle.MouseLeftButtonUp += LeftButtonUpVerticle;
            }
            canvas.MouseLeftButtonUp += LeftButtonUpVerticle;
        }

        private void DisableMovingVerticles()
        {
            foreach (var ver in _verticles)
            {
                ver.Rectangle.MouseLeftButtonDown -= LeftButtonDownVerticle;
                ver.Rectangle.MouseLeftButtonUp -= LeftButtonUpVerticle;
            }
            canvas.MouseLeftButtonUp -= LeftButtonUpVerticle;
        }

        #endregion

        #region Context Menu Clicks

        private bool CheckMenuItem(object menuItemToCheck, out Rectangle rc)
        {
            MenuItem mi = menuItemToCheck as MenuItem;
            if (mi != null)
            {
                rc = ((ContextMenu)mi.Parent).PlacementTarget as Rectangle;
                return true;
            }

            rc = new Rectangle();
            return false;
        }

        private void EndDrawingPolygon(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_verticles.Count < 3)
                return;

            _polygonMade = true;
            foreach (var verticle in _verticles)
            {
                verticle.Rectangle.ContextMenu = _verticleContextMenu;
            }
            ClearHelpingLine();

            Rectangle rc;
            if (CheckMenuItem(sender, out rc))
            {
                Verticle endVerticle = FindVerticeByRectangle(rc);
                Verticle lastVerticle = _verticles.LastOrDefault();
                _lines.Add(CreateLine(lastVerticle, endVerticle));

                DeleteTail(endVerticle.Id);
            }

            DisableSettingVerticle();
            DisableDrawingHelpingLine();
            EnableMovingVerticles();
            MovePolygonButton.IsEnabled = true;
        }

        private void DeleteTail(int endVerticleIndex)
        {
            List<Verticle> verticlesToDelete = new List<Verticle>();
            List<Line> linesToDelete = new List<Line>();

            foreach (var verticle in _verticles)
            {
                if (verticle.Id < endVerticleIndex)
                {
                    verticlesToDelete.Add(verticle);
                }
            }
            foreach (var line in _lines)
            {
                if (line.VerticleOneId < endVerticleIndex || line.VerticleTwoId < endVerticleIndex)
                {
                    linesToDelete.Add(line);
                }
            }

            foreach (var verticle in verticlesToDelete)
            {
                canvas.Children.Remove(verticle.Rectangle);
                _verticles.Remove(verticle);
            }
            foreach (var line in linesToDelete)
            {
                ClearLine(line);
                _lines.Remove(line);
            }
        }

        private void DeleteVerticle(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_polygonMade && _verticles.Count <= 3) return;

            Rectangle rc;
            if (CheckMenuItem(sender, out rc))
            {
                canvas.Children.Remove(rc);
                Verticle verticle = FindVerticeByRectangle(rc);

                DeleteLine(verticle);
                _verticles.Remove(verticle);
            }
        }

        private void DeleteLine(Verticle verticle)
        {
            int verticleId = verticle.Id;
            List<Line> linesToDelete = new List<Line>(_lines.Where(l => l.VerticleOneId == verticleId || l.VerticleTwoId == verticleId));

            foreach (var line in linesToDelete)
            {
                ClearLine(line);
                _lines.Remove(line);
            }

            if (linesToDelete.Count == 2)
            {
                AddLineAfterVerticleDeletion(linesToDelete[0], linesToDelete[1], verticleId);
            }
        }

        private void AddLineAfterVerticleDeletion(Line lineOne, Line lineTwo, int deletedVerticleId)
        {
            List<int> verticlesIds = new List<int>
            {
                lineOne.VerticleOneId == deletedVerticleId ? lineOne.VerticleTwoId : lineOne.VerticleOneId,
                lineTwo.VerticleOneId == deletedVerticleId ? lineTwo.VerticleTwoId : lineTwo.VerticleOneId
            };

            Verticle verticleOne = FindVerticleById(verticlesIds[0]);
            Verticle verticleTwo = FindVerticleById(verticlesIds[1]);

            _lines.Add(CreateLine(verticleOne, verticleTwo));
        }

        private void AddVerticleInTheMiddleOfLine(object sender, RoutedEventArgs routedEventArgs)
        {
            Rectangle rc;
            if (CheckMenuItem(sender, out rc))
            {
                Line line = FindLine(rc);
                Verticle verticleOne = FindVerticleById(line.VerticleOneId);
                Verticle verticleTwo = FindVerticleById(line.VerticleTwoId);

                ClearLine(line);
                _lines.Remove(line);

                int x = (verticleOne.X + verticleTwo.X) / 2;
                int y = (verticleOne.Y + verticleTwo.Y) / 2;

                Rectangle rectangle = SetPixel(x, y, VerticleSize, true);
                rectangle.MouseLeftButtonDown += LeftButtonDownVerticle;
                rectangle.MouseLeftButtonUp += LeftButtonUpVerticle;
                rectangle.ContextMenu = _verticleContextMenu;
                Verticle newVerticle = new Verticle(_verticleIndexer++, x, y, rectangle);
                _verticles.Add(newVerticle);

                _lines.Add(CreateLine(verticleOne, newVerticle));
                _lines.Add(CreateLine(newVerticle, verticleTwo));
            }
        }

        private void AddHorizontalLineRestriction(object sender, RoutedEventArgs routedEventArgs)
        {
            Rectangle rc;
            if (CheckMenuItem(sender, out rc))
            {
                Line line = FindLine(rc);
                line.Restriction = new HorizontalLineRestriction();

                if (!line.Restriction.CheckRestrictionAvailability(FindNearbyLine(line, true),
                    FindNearbyLine(line, false)))
                {
                    line.Restriction = new NoneRestriction();
                    return;
                }

                Verticle verticleOne = FindVerticleById(line.VerticleOneId);
                Verticle verticleTwo = FindVerticleById(line.VerticleTwoId);

                int y = (verticleOne.Y + verticleTwo.Y) / 2;
                verticleOne.Y = y;
                verticleTwo.Y = y;

                SetRestrictionPic(line, (verticleOne.X + verticleTwo.X) / 2, (verticleOne.Y + verticleTwo.Y) / 2);
                RedrawPolygon();
            }
        }

        private void AddVerticalLineRestriction(object sender, RoutedEventArgs routedEventArgs)
        {
            Rectangle rc;
            if (CheckMenuItem(sender, out rc))
            {
                Line line = FindLine(rc);
                line.Restriction = new VerticalLineRestriction();

                if (!line.Restriction.CheckRestrictionAvailability(FindNearbyLine(line, true),
                    FindNearbyLine(line, false)))
                {
                    line.Restriction = new NoneRestriction();
                    return;
                }

                Verticle verticleOne = FindVerticleById(line.VerticleOneId);
                Verticle verticleTwo = FindVerticleById(line.VerticleTwoId);

                int x = (verticleOne.X + verticleTwo.X) / 2;
                verticleOne.X = x;
                verticleTwo.X = x;

                SetRestrictionPic(line, (verticleOne.X + verticleTwo.X) / 2, (verticleOne.Y + verticleTwo.Y) / 2);
                RedrawPolygon();
            }
        }

        private void AddLengthStillRestriction(object sender, RoutedEventArgs routedEventArgs)
        {
            Rectangle rc;
            if (CheckMenuItem(sender, out rc))
            {
                Line line = FindLine(rc);
                line.Restriction = new LengthStillRestriction();

                LineLengthStackPanel.Visibility = Visibility.Visible;
                canvas.IsHitTestVisible = false;
                ClearCanvasButton.IsEnabled = false;
                MovePolygonButton.IsEnabled = false;

                Verticle verticleOne = FindVerticleById(line.VerticleOneId);
                Verticle verticleTwo = FindVerticleById(line.VerticleTwoId);

                _lineStillLengthBeingRestricted = line;
                SetRestrictionPic(line, (verticleOne.X + verticleTwo.X) / 2, (verticleOne.Y + verticleTwo.Y) / 2);
            }
        }

        private void SetRestrictionPic(Line actualLine, int middleX, int y)
        {
            Image image = actualLine.Restriction.GetRestrictionPic();
            actualLine.RestrictionPic = image;
            canvas.Children.Add(image);
            Canvas.SetLeft(image, middleX + RestrictionMargin);
            Canvas.SetTop(image, y + RestrictionMargin);
        }

        private void SetLengthOfRestrictedLine(object sender, RoutedEventArgs e)
        {
            int val;
            canvas.IsHitTestVisible = true;
            LineLengthStackPanel.Visibility = Visibility.Collapsed;
            ClearCanvasButton.IsEnabled = true;
            MovePolygonButton.IsEnabled = true;
            if (_lineStillLengthBeingRestricted == null) return;
            if (!int.TryParse(LineLengthTextBox.Text, out val)) return;
            Verticle verticleOne = FindVerticleById(_lineStillLengthBeingRestricted.VerticleOneId);
            Verticle verticleTwo = FindVerticleById(_lineStillLengthBeingRestricted.VerticleTwoId);
            ResetLengthRestrictedLine(verticleOne, verticleTwo, val);
            ((LengthStillRestriction)_lineStillLengthBeingRestricted.Restriction).LengthSet = val;
            RedrawPolygon();
        }

        #endregion

        #region Move Verticle

        private void LeftButtonDownVerticle(object sender, MouseButtonEventArgs e)
        {
            if (_isMovingVerticleSet)
            {
                _movingVerticle = null;
                _isMovingVerticleSet = false;
                return;
            }

            Rectangle rectangle = sender as Rectangle;
            if (rectangle != null)
            {
                _movingVerticle = FindVerticeByRectangle(rectangle);
                _isMovingVerticleSet = true;
                _copyOfVerticles = new List<Verticle>(_verticles);
                canvas.MouseMove += MoveVerticle;
            }
        }

        private void LeftButtonUpVerticle(object sender, MouseButtonEventArgs e)
        {
            canvas.MouseMove -= MoveVerticle;

            _movingVerticle = null;
            _isMovingVerticleSet = false;
        }

        private void MoveVerticle(object sender, MouseEventArgs e)
        {
            if (_isMovingVerticleSet)
            {
                //ustalenie pozycji myszki
                int x, y;
                GetMousePosition(sender, out x, out y);

                // usuwanie wierzchołka i linii

                int verticleId = _movingVerticle.Id;
                List<Line> lines = new List<Line>(_lines.Where(l => l.VerticleOneId == verticleId || l.VerticleTwoId == verticleId));

                foreach (var line in lines)
                {
                    if (line.Restriction.GetType() != typeof(NoneRestriction))
                    {
                        _startCheckingRestrictionVerticleId = verticleId;
                        _movingVerticle.SetNewCoordinates(x,y);
                        RecurencyCheckingRestriction(line, verticleId);
                        return;
                    }
                }

                canvas.Children.Remove(_movingVerticle.Rectangle);
                foreach (var line in lines)
                {
                    ClearLine(line);
                    line.Rectangles.Clear();
                }

                // -------------                

                // nowy punkt wierzchołka
                Rectangle newRectangle = SetPixel(x, y, VerticleSize, true);
                newRectangle.ContextMenu = _verticleContextMenu;
                newRectangle.MouseLeftButtonDown += LeftButtonDownVerticle;
                newRectangle.MouseLeftButtonUp += LeftButtonUpVerticle;
                _movingVerticle.SetNewRectangle(x, y, newRectangle);

                // --------------------

                // przerysowanie linii

                foreach (var line in lines)
                {
                    RedrawLine(line);
                    line.EnableClicking();
                }

                //--------------------------------
            }
        }

        private void RecurencyCheckingRestriction(Line line, int oppositeDirectionVerticle)
        {
            int directionVerticle = line.VerticleOneId == oppositeDirectionVerticle
                ? line.VerticleTwoId
                : line.VerticleOneId;

            Verticle verticleMoved = FindVerticleByIdInCopy(oppositeDirectionVerticle);
            Verticle secondVerticle = FindVerticleByIdInCopy(directionVerticle);
            line.Restriction.ReorganizeLine(verticleMoved, secondVerticle);

            if (directionVerticle == _startCheckingRestrictionVerticleId)
            {
                _verticles = _copyOfVerticles;
                RedrawPolygon();
                return;
            }

            Line nextLine = FindNearbyLine(line, line.VerticleOneId == directionVerticle);
            RecurencyCheckingRestriction(nextLine, directionVerticle);
        }

        #endregion

        #region Move Polygon

        private void AllowMovingPolygon(object sender, RoutedEventArgs e)
        {
            DisableMovingVerticles();
            EnableMovingPolygon();

            foreach (var verticle in _verticles)
            {
                verticle.Rectangle.MouseLeftButtonUp += LeftButtonUpPolygon;
            }
            foreach (var line in _lines)
            {
                foreach (var rectangle in line.Rectangles)
                {
                    rectangle.MouseLeftButtonUp += LeftButtonUpPolygon;
                }
            }
        }

        private void ForbidMovingPolygon(object sender, RoutedEventArgs e)
        {
            EnableMovingVerticles();
            DisableMovingPolygon();

            foreach (var verticle in _verticles)
            {
                verticle.Rectangle.MouseLeftButtonUp -= LeftButtonUpPolygon;
            }
            foreach (var line in _lines)
            {
                foreach (var rectangle in line.Rectangles)
                {
                    rectangle.MouseLeftButtonUp -= LeftButtonUpPolygon;
                }
            }
        }

        private void LeftButtonDownPolygon(object sender, MouseButtonEventArgs e)
        {
            int x, y;
            GetMousePosition(sender, out x, out y);
            _movePolygonXPosition = x;
            _movePolygonYPosition = y;

            canvas.MouseMove += MovePolygon;
        }

        private void LeftButtonUpPolygon(object sender, MouseButtonEventArgs e)
        {
            canvas.MouseMove -= MovePolygon;
        }

        private void MovePolygon(object sender, MouseEventArgs e)
        {
            if (_verticles.Count == 0) return;

            int x, y;
            GetMousePosition(sender, out x, out y);

            canvas.Children.Clear();

            int dx = _movePolygonXPosition - x;
            int dy = _movePolygonYPosition - y;

            _movePolygonXPosition = x;
            _movePolygonYPosition = y;

            foreach (var verticle in _verticles)
            {
                verticle.X -= dx;
                verticle.Y -= dy;
                verticle.Rectangle = SetPixel(verticle.X, verticle.Y, VerticleSize, true);
            }

            foreach (var line in _lines)
            {
                RedrawLine(line);
                line.EnableClicking();
            }

        }

        #endregion

        #region Drawing

        private void SetVerticle(object sender, MouseButtonEventArgs e)
        {
            int x, y;
            GetMousePosition(sender, out x, out y);

            Rectangle rectangle = SetPixel(x, y, VerticleSize, true);
            rectangle.ContextMenu = _firstVerticleContextMenu;
            _verticles.Add(new Verticle(_verticleIndexer++, x, y, rectangle));

            if (_verticles.Count >= 2)
            {
                int index = _verticles.Count - 2;
                _lines.Add(CreateLine(_verticles[index], _verticles[index + 1]));
            }
        }

        private Rectangle SetPixel(int x, int y, int size, bool hitTestVisible = false)
        {
            Rectangle rectangle = new Rectangle() { Width = size, Height = size, Fill = Brushes.Black };
            rectangle.IsHitTestVisible = hitTestVisible;

            canvas.Children.Add(rectangle);
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            
            return rectangle;
        }

        private List<Rectangle> DrawLine(Verticle v1, Verticle v2)
        {
            int x1 = v1.X;
            int x2 = v2.X;
            int y1 = v1.Y;
            int y2 = v2.Y;

            List<Rectangle> listOfRectangles = new List<Rectangle>();

            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;
            // ustalenie kierunku rysowania
            if (x1 < x2)
            {
                xi = 1;
                dx = x2 - x1;
            }
            else
            {
                xi = -1;
                dx = x1 - x2;
            }
            // ustalenie kierunku rysowania
            if (y1 < y2)
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }

            // pierwszy piksel
            listOfRectangles.Add(SetPixel(x, y, LinePointSize));
            Rectangle rectangle;

            // oś wiodąca OX
            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                // pętla po kolejnych x
                while (x != x2)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                    }
                    rectangle = SetPixel(x, y, LinePointSize);
                    rectangle.ContextMenu = _lineContextMenu;
                    listOfRectangles.Add(rectangle);
                }
            }
            // oś wiodąca OY
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;
                // pętla po kolejnych y
                while (y != y2)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }
                    rectangle = SetPixel(x, y, LinePointSize);
                    rectangle.ContextMenu = _lineContextMenu;
                    listOfRectangles.Add(rectangle);
                }
            }
            return listOfRectangles;
        }

        private void ResetLengthRestrictedLine(Verticle v1, Verticle v2, int length) // współrzędne v2 są zmieniane
        {
            int x1 = v1.X;
            int x2 = v2.X;
            int y1 = v1.Y;
            int y2 = v2.Y;

            int counter = 0;

            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;
            // ustalenie kierunku rysowania
            if (x1 < x2)
            {
                xi = 1;
                dx = x2 - x1;
            }
            else
            {
                xi = -1;
                dx = x1 - x2;
            }
            // ustalenie kierunku rysowania
            if (y1 < y2)
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }

            // pierwszy piksel
            counter++;

            // oś wiodąca OX
            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                // pętla po kolejnych x
                while (counter++ != length)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                    }
                }
            }
            // oś wiodąca OY
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;
                // pętla po kolejnych y
                while (counter++ != length)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }
                }
            }

            //ustawienie x i y
            v2.SetNewCoordinates(x,y);
        }

        private void RedrawLine(Line line)
        {
            Verticle verticleOne = FindVerticleById(line.VerticleOneId);
            Verticle verticleTwo = FindVerticleById(line.VerticleTwoId);
            line.Rectangles = DrawLine(verticleOne, verticleTwo);
            SetRestrictionPic(line, (verticleOne.X + verticleTwo.X) / 2, (verticleOne.Y + verticleTwo.Y) / 2);
        }

        private Line CreateLine(Verticle v1, Verticle v2)
        {
            Line line = new Line(v1.Id, v2.Id, DrawLine(v1, v2));
            line.EnableClicking();
            return line;
        }

        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            _verticles.Clear();
            _lines.Clear();
            _verticleIndexer = 0;            
            DisableMovingVerticles();
            DisableMovingPolygon();
            if (_polygonMade)
            {
                EnableSettingVerticle();
                EnableDrawingHelpingLine();
            }           
            _helpingLine = null;
            _polygonMade = false;
            MovePolygonButton.IsEnabled = false;
        }

        private void LineHelper(object sender, MouseEventArgs e)
        {
            if (_verticles.Count == 0) return;

            ClearHelpingLine();

            int x, y;
            GetMousePosition(sender, out x, out y);

            Verticle extraVerticle = new Verticle(int.MaxValue, x, y);
            Verticle lastVerticle = _verticles.LastOrDefault();
            _helpingLine = new Line(-1, -1, DrawLine(extraVerticle, lastVerticle));
        }

        private void ClearHelpingLine()
        {
            if (_helpingLine != null)
            {
                ClearLine(_helpingLine);
            }
        }

        private void ClearLine(Line line)
        {
            foreach (var rectangle in line.Rectangles)
            {
                canvas.Children.Remove(rectangle);
            }
            canvas.Children.Remove(line.RestrictionPic);
        }

        private void RedrawPolygon()
        {
            canvas.Children.Clear();

            foreach (var verticle in _verticles)
            {
                verticle.Rectangle = SetPixel(verticle.X, verticle.Y, VerticleSize, true);
                verticle.Rectangle.ContextMenu = _verticleContextMenu;
            }
            EnableMovingVerticles();

            foreach (var line in _lines)
            {
                RedrawLine(line);
                line.EnableClicking();
            }
        }

        #endregion

        #region Find Fuctions

        private Line FindNearbyLine(Line actualLine, bool nearV1)
        {
            if (nearV1)
            {
                return _lines.FirstOrDefault(
                    l => (l.VerticleOneId == actualLine.VerticleOneId && l.VerticleTwoId != actualLine.VerticleTwoId) ||
                         (l.VerticleTwoId == actualLine.VerticleOneId && l.VerticleOneId != actualLine.VerticleTwoId));
            }
            else
            {
                return _lines.FirstOrDefault(
                    l => (l.VerticleOneId == actualLine.VerticleTwoId && l.VerticleTwoId != actualLine.VerticleOneId) ||
                         (l.VerticleTwoId == actualLine.VerticleTwoId && l.VerticleOneId != actualLine.VerticleOneId));
            }
        }



        private void RemoveRestriction(object sender, RoutedEventArgs routedEventArgs)
        {
            Rectangle rc;
            if (CheckMenuItem(sender, out rc))
            {
                Line line = FindLine(rc);
                line.Restriction = new NoneRestriction();
                canvas.Children.Remove(line.RestrictionPic);
                line.RestrictionPic = new Image();
            }
        }

        private Line FindLine(Rectangle rectangle)
        {
            foreach (var line in _lines)
            {
                foreach (var rect in line.Rectangles)
                {
                    if (Equals(rectangle, rect))
                    {
                        return line;
                    }
                }
            }
            return _lines.FirstOrDefault();
        }

        private Verticle FindVerticleById(int id)
        {
            return _verticles.FirstOrDefault(v => v.Id == id);
        }

        private Verticle FindVerticeByRectangle(Rectangle rectangle)
        {
            return _verticles.FirstOrDefault(v => Equals(v.Rectangle, rectangle));
        }

        private Verticle FindVerticleByIdInCopy(int id)
        {
            return _copyOfVerticles.FirstOrDefault(v => v.Id == id);
        }

        private void GetMousePosition(object mouse, out int x, out int y)
        {
            Point p = Mouse.GetPosition((Canvas)mouse);
            x = (int)p.X;
            y = (int)p.Y;
        }

        #endregion

    }
}
