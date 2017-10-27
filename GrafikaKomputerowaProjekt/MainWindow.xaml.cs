using System;
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

    //Context Menu , usuwanie wierzchołków ???? 
    // poruszanie wielokąta słabo działa, trzeba wykrywać czy kliknięto w środku
    //notatki

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Verticle> _verticles = new List<Verticle>();
        private readonly List<Line> _lines = new List<Line>();
        private readonly ContextMenu _verticleContextMenu = new ContextMenu();
        private readonly ContextMenu _firstVerticleContextMenu = new ContextMenu();
        private readonly ContextMenu _lineContextMenu = new ContextMenu();
        private int _verticleIndexer = 0;
        private const int VerticleSize = 10;
        private const int LinePointSize = 4;
        private const int LineMarginOfError = 10;
        private Line _helpingLine;
        private Verticle _movingVerticle;
        private bool _isMovingVerticleSet = false;
        private bool _polygonMade = false;
        private int _movePolygonXPosition;
        private int _movePolygonYPosition;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCanvas();
            InitializeVerticleContextMenu();
            InitializeFirstVerticleContextMenu();
            InitializeLineContextMenu();
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

            _lineContextMenu.Items.Add(mi1);
            _lineContextMenu.Items.Add(mi2);
            _lineContextMenu.Items.Add(mi3);
            _lineContextMenu.Items.Add(mi4);
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
            canvas.MouseMove += MoveVerticle;
        }

        private void DisableMovingVerticles()
        {
            foreach (var ver in _verticles)
            {
                ver.Rectangle.MouseLeftButtonDown -= LeftButtonDownVerticle;
                ver.Rectangle.MouseLeftButtonUp -= LeftButtonUpVerticle;
            }
            canvas.MouseMove -= MoveVerticle;
        }

        #endregion

        private void InitializeCanvas()
        {
            EnableSettingVerticle();
            EnableDrawingHelpingLine();
        }

        private void EndDrawingPolygon(object sender, RoutedEventArgs routedEventArgs)
        {
            if(_verticles.Count < 3)
                return;

            _polygonMade = true;
            foreach (var verticle in _verticles)
            {
                verticle.Rectangle.ContextMenu = _verticleContextMenu;
            }
            ClearHelpingLine();



            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                Rectangle rc = ((ContextMenu)mi.Parent).PlacementTarget as Rectangle;

                Verticle endVerticle = _verticles.FirstOrDefault(v => Equals(v.Rectangle, rc));
                Verticle lastVerticle = _verticles.LastOrDefault();

                _lines.Add(CreateLine(lastVerticle, endVerticle));

                DeleteTail(endVerticle.Id);
            }

            DisableSettingVerticle();
            DisableDrawingHelpingLine();
            EnableMovingVerticles();
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
            if(_polygonMade && _verticles.Count <= 3) return;

            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                Rectangle rc = ((ContextMenu)mi.Parent).PlacementTarget as Rectangle;
                canvas.Children.Remove(rc);

                Verticle verticle = _verticles.FirstOrDefault(v => Equals(v.Rectangle, rc));

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

            Verticle verticleOne = _verticles.FirstOrDefault(v => v.Id == verticlesIds[0]);
            Verticle verticleTwo = _verticles.FirstOrDefault(v => v.Id == verticlesIds[1]);

            _lines.Add(CreateLine(verticleOne, verticleTwo)); 
        }

        private void SetVerticle(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition((Canvas)sender);
            int x = (int) p.X;
            int y = (int) p.Y;

            Rectangle rectangle = SetPixel(x, y, VerticleSize, true);

            rectangle.ContextMenu = _firstVerticleContextMenu;

            _verticles.Add(new Verticle(_verticleIndexer++, x, y, rectangle));

            if (_verticles.Count >= 2)
            {
                int index = _verticles.Count - 2;
                _lines.Add(CreateLine(_verticles[index], _verticles[index + 1])); 
            }
        }

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
                _movingVerticle = _verticles.FirstOrDefault(v => Equals(v.Rectangle, rectangle));

                _isMovingVerticleSet = true;
            }
        }

        private void LeftButtonUpVerticle(object sender, MouseButtonEventArgs e)
        {
            _movingVerticle = null;
            _isMovingVerticleSet = false;
        }

        private void MoveVerticle(object sender, MouseEventArgs e)
        {
            if (_isMovingVerticleSet)
            {
                // usuwanie wierzchołka i linii

                canvas.Children.Remove(_movingVerticle.Rectangle);

                int verticleId = _movingVerticle.Id;

                List<Line> lines = new List<Line>(_lines.Where(l => l.VerticleOneId == verticleId || l.VerticleTwoId == verticleId));

                foreach (var line in lines)
                {
                    ClearLine(line);
                    line.Rectangles.Clear();
                }

                // -------------

                // nowy punkt wierzchołka
                Point p = Mouse.GetPosition((Canvas)sender);
                int x = (int)p.X;
                int y = (int)p.Y;

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

        private void LeftButtonDownPolygon(object sender, MouseButtonEventArgs e) // dodać sprawdzanie czy kliknięto w środek wielokąta
        {
            Point p = Mouse.GetPosition((Canvas)sender);
            _movePolygonXPosition = (int) p.X;
            _movePolygonYPosition = (int) p.Y;

            canvas.MouseMove += MovePolygon;
        }

        private void LeftButtonUpPolygon(object sender, MouseButtonEventArgs e)
        {
            canvas.MouseMove -= MovePolygon;
        }

        private void MovePolygon(object sender, MouseEventArgs e)
        {
            if(_verticles.Count == 0) return;

            Point p = Mouse.GetPosition((Canvas)sender);
            int x = (int)p.X;
            int y = (int)p.Y;

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
            }

        }

        private void RedrawLine(Line line)
        {
            Verticle verticleOne = _verticles.FirstOrDefault(v => v.Id == line.VerticleOneId);
            Verticle verticleTwo = _verticles.FirstOrDefault(v => v.Id == line.VerticleTwoId);
            line.Rectangles = DrawLine(verticleOne, verticleTwo);
        }

        private Rectangle SetPixel(int x, int y, int size, bool hitTestVisible = false)
        {
            Rectangle rectangle = new Rectangle() { Width = size, Height = size, Fill = Brushes.Black };

            canvas.Children.Add(rectangle);
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            rectangle.IsHitTestVisible = hitTestVisible;
            return rectangle;
        }

        private Line CreateLine(Verticle v1, Verticle v2)
        {
            Line line = new Line(v1.Id, v2.Id, DrawLine(v1, v2));
            line.EnableClicking();
            return line;
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
            listOfRectangles.Add(SetPixel(x,y,LinePointSize));
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


        private void ClearCanvasButton(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            _verticles.Clear();
            _lines.Clear();
            _verticleIndexer = 0;
            EnableSettingVerticle();
            DisableMovingVerticles();
            //DisableMovingPolygon();
            EnableDrawingHelpingLine();
            _helpingLine = null;
            _polygonMade = false;            
        }

        private void LineHelper(object sender, MouseEventArgs e)
        {
            if(_verticles.Count == 0) return;

            ClearHelpingLine();

            Point p = Mouse.GetPosition((Canvas)sender);
            int x = (int)p.X;
            int y = (int)p.Y;

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
        }

        // zaznaczanie linii

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

        private void AddVerticleInTheMiddleOfLine(object sender, RoutedEventArgs routedEventArgs)
        {
            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                Rectangle rc = ((ContextMenu)mi.Parent).PlacementTarget as Rectangle;

                Line line = FindLine(rc);

                Verticle verticleOne = _verticles.FirstOrDefault(v => v.Id == line.VerticleOneId);
                Verticle verticleTwo = _verticles.FirstOrDefault(v => v.Id == line.VerticleTwoId);

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
            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                Rectangle rc = ((ContextMenu)mi.Parent).PlacementTarget as Rectangle;
                Line line = FindLine(rc);

                line.Restriction = new HorizontalLineRestriction();

                if (!line.Restriction.CheckRestrictionAvailability(FindNearbyLine(line, true),
                    FindNearbyLine(line, false)))
                {
                    line.Restriction = new NoneRestriction();
                    return;
                }

                Verticle verticleOne = _verticles.FirstOrDefault(v => v.Id == line.VerticleOneId);
                Verticle verticleTwo = _verticles.FirstOrDefault(v => v.Id == line.VerticleTwoId);

                int y = (verticleOne.Y + verticleTwo.Y) / 2;
                verticleOne.Y = y;
                verticleTwo.Y = y;

                RedrawPolygon();

            }
        }

        private void AddVerticalLineRestriction(object sender, RoutedEventArgs routedEventArgs)
        {
            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                Rectangle rc = ((ContextMenu)mi.Parent).PlacementTarget as Rectangle;
                Line line = FindLine(rc);

                line.Restriction = new VerticalLineRestriction();

                if (!line.Restriction.CheckRestrictionAvailability(FindNearbyLine(line, true),
                    FindNearbyLine(line, false)))
                {
                    line.Restriction = new NoneRestriction();
                    return;
                }

                
                Verticle verticleOne = _verticles.FirstOrDefault(v => v.Id == line.VerticleOneId);
                Verticle verticleTwo = _verticles.FirstOrDefault(v => v.Id == line.VerticleTwoId);

                int x = (verticleOne.X + verticleTwo.X) / 2;
                verticleOne.X = x;
                verticleTwo.X = x;

                RedrawPolygon();
            }
        }

        private void AddLengthStillRestriction(object sender, RoutedEventArgs routedEventArgs)
        {
            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                Rectangle rc = ((ContextMenu)mi.Parent).PlacementTarget as Rectangle;
                Line line = FindLine(rc);

                line.Restriction = new LengthStillRestriction();



                RedrawPolygon();
            }
        }

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

        
    }
}
