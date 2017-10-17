using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GrafikaKomputerowaProjekt
{
    //notatki

    //Context Menu , usuwanie wierzchołków ???? 
    //popraw draw line (rozdziel na tworzenie obiektu linii , oraz tworzenie listy rectanglów

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
        private int _verticleIndexer = 0;
        private const int VerticleSize = 10;
        private const int LinePointSize = 4;
        private Line _helpingLine;
        private Verticle _movingVerticle;
        private bool _isMovingVerticleSet = false;
        private bool _polygonMade = false;
        private int _movePolygonXPosition;
        private int _movePolygonYPosition;

        public MainWindow()
        {
            InitializeComponent();
            InitializeVerticleContextMenu();
            InitializeFirstVerticleContextMenu();
        }

        private void InitializeVerticleContextMenu()
        {
            MenuItem mi = new MenuItem {Header = "Usun wierzchołek"};
            mi.Click += DeleteVerticle;

            _verticleContextMenu.Items.Add(mi);
        }

        private void InitializeFirstVerticleContextMenu()
        {
            MenuItem deleteVerticleMenuItem = new MenuItem {Header = "Usun wierzchołek"};
            deleteVerticleMenuItem.Click += DeleteVerticle;

            MenuItem endDrawingPolygonMenuItem = new MenuItem {Header = "Zakoncz rysowanie wielokata"};
            endDrawingPolygonMenuItem.Click += EndDrawingPolygon;

            _firstVerticleContextMenu.Items.Add(deleteVerticleMenuItem);
            _firstVerticleContextMenu.Items.Add(endDrawingPolygonMenuItem);
        }

        private void EndDrawingPolygon(object sender, RoutedEventArgs routedEventArgs)
        {
            if(_verticles.Count < 3)
                return;

            _polygonMade = true;

            canvas.MouseLeftButtonDown -= SetVerticle;
            canvas.MouseMove -= LineHelper;
            canvas.MouseLeftButtonDown += LeftButtonDownPolygon;
            canvas.MouseLeftButtonUp += LeftButtonUpPolygon;
            foreach (var ver in _verticles)
            {
                ver.Rectangle.MouseLeftButtonDown += LeftButtonDownVerticle;
                ver.Rectangle.MouseLeftButtonUp += LeftButtonUpVerticle;
            }
            _verticles.FirstOrDefault(v => v.Id == 0).Rectangle.ContextMenu = _verticleContextMenu;

            ClearHelpingLine();

            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                Rectangle rc = ((ContextMenu)mi.Parent).PlacementTarget as Rectangle;

                Verticle endVerticle = _verticles.FirstOrDefault(v => Equals(v.Rectangle, rc));
                Verticle lastVerticle = _verticles.LastOrDefault();

                _lines.Add(DrawLine(lastVerticle, endVerticle)); 
            }

            canvas.MouseMove += MoveVerticle;
        }

        private void DeleteVerticle(object sender, RoutedEventArgs routedEventArgs) 
        {
            if(_polygonMade && _verticles.Count < 3) return;

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
                foreach (var rectangle in line.rectangles)
                {
                    canvas.Children.Remove(rectangle);
                }
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

            _lines.Add(DrawLine(verticleOne, verticleTwo)); 
        }

        private void SetVerticle(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition((Canvas)sender);
            int x = (int) p.X;
            int y = (int) p.Y;

            Rectangle rectangle = SetPixel(x, y, VerticleSize, true);

            rectangle.ContextMenu = _verticleIndexer == 0 ? _firstVerticleContextMenu : _verticleContextMenu;

            _verticles.Add(new Verticle(_verticleIndexer++, x, y, rectangle));

            if (_verticles.Count >= 2)
            {
                int index = _verticles.Count - 2;
                _lines.Add(DrawLine(_verticles[index], _verticles[index + 1])); 
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

                List<Line> linesToDelete = new List<Line>(_lines.Where(l => l.VerticleOneId == verticleId || l.VerticleTwoId == verticleId));

                List<int> verticlesIds = new List<int>();

                foreach (var line in linesToDelete)
                {
                    foreach (var rectangle in line.rectangles)
                    {
                        canvas.Children.Remove(rectangle);
                    }
                    verticlesIds.Add(line.VerticleOneId == verticleId ? line.VerticleTwoId : line.VerticleOneId);
                    _lines.Remove(line);
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

                foreach (var verId in verticlesIds)
                {
                    Verticle ver = _verticles.FirstOrDefault(v => v.Id == verId);
                    _lines.Add(DrawLine(ver, _movingVerticle));
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

            _lines.Clear();

            for (int i = 0; i < _verticles.Count - 1; i++)
            {
                _lines.Add(DrawLine(_verticles[i], _verticles[i+1]));
            }
            _lines.Add(DrawLine(_verticles[_verticles.Count - 1], _verticles[0]));

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

        private Line DrawLine(Verticle v1, Verticle v2)
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
                    listOfRectangles.Add(SetPixel(x, y, LinePointSize));
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
                    listOfRectangles.Add(SetPixel(x, y, LinePointSize));
                }
            }
            return new Line(v1.Id, v2.Id, listOfRectangles);
        }


        private void ClearCanvasButton(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            _verticles.Clear();
            _lines.Clear();
            _verticleIndexer = 0;
            canvas.MouseLeftButtonDown += SetVerticle;
            canvas.MouseLeftButtonDown -= LeftButtonDownPolygon;
            canvas.MouseLeftButtonUp -= LeftButtonUpPolygon;
            canvas.MouseMove += LineHelper;
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

            _helpingLine = DrawLine(lastVerticle, extraVerticle);
        }

        private void ClearHelpingLine()
        {
            if (_helpingLine != null)
            {
                foreach (var rectangle in _helpingLine.rectangles)
                {
                    canvas.Children.Remove(rectangle);
                }
                _lines.Remove(_helpingLine);
            }
        }

        //private bool IsPointInPolygon(Verticle x, Verticle y, Verticle z)
        //{
        //    var det = x.X * y.Y + y.X * z.Y + z.X * x.Y - z.X * y.Y - x.X * z.Y - y.X * x.Y;
        //    if (det != 0)
        //        return false;

        //    return (Math.Min(x.X, y.X) <= z.X) && (z.X <= Math.Max(x.X, y.X)) && (Math.Min(x.Y, y.Y) <= z.Y) && (z.Y <= Math.Max(x.Y, y.Y));
        //}
    }
}
