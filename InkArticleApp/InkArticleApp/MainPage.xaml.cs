namespace InkArticleApp
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Windows.Devices.Input;
  using Windows.Foundation;
  using Windows.UI.Input.Inking;
  using Windows.UI.Input.Inking.Core;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;

  public sealed partial class MainPage : Page
  {
    enum Mode
    {
      Freeform = 0,
      SnapX = 1,
      SnapY = 2
    }
    public MainPage()
    {
      this.InitializeComponent();
      this.currentScaleFactor = 1.0m;
      this.currentMode = Mode.Freeform;
      this.UpdateModeText();
      this.Loaded += this.OnLoaded;

      this.currentGridSize = BASE_GRID_SIZE;
      this.graphPaper.GridSize = this.currentGridSize;
    }
    /// <summary>
    /// event handler for MainPage.Loaded
    /// </summary>
    void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
      // Added a member variable of type CoreWetStrokeUpdateSource called 'wetUpdateSource'
      this.wetUpdateSource = CoreWetStrokeUpdateSource.Create(this.inkCanvas.InkPresenter);
      this.wetUpdateSource.WetStrokeStarting += OnStrokeStarting;
      this.wetUpdateSource.WetStrokeContinuing += OnStrokeContinuing;
    }
    void OnStrokeStarting(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
    {
      // as the stroke is starting, reset our member variables which store
      // which X or Y point we want to snap to.
      this.snapX = this.snapY = null;

      // I am assuming that we do get a first ink point.
      InkPoint firstPoint = args.NewInkPoints.First();

      // now decide whether we need to set up a snap point for the X value or
      // one for the Y value.
      if (this.currentMode == Mode.SnapX)
      {
        this.snapX = this.NearestGridSizeMultiple(firstPoint.Position.X);
      }
      else if (this.currentMode == Mode.SnapY)
      {
        this.snapY = this.NearestGridSizeMultiple(firstPoint.Position.Y);
      }
      this.SnapPoints(args.NewInkPoints);
    }
    double? snapX;
    double? snapY;


    void SnapPoints(IList<InkPoint> newInkPoints)
    {
      // do we need to do any snapping?
      if (this.currentMode != Mode.Freeform)
      {
        for (int i = 0; i < newInkPoints.Count; i++)
        {
          if (this.snapX.HasValue)
          {
            // replace this point with the same point but with the X value snapped.
            newInkPoints[i] = new InkPoint(
              new Point(this.snapX.Value, newInkPoints[i].Position.Y),
              newInkPoints[i].Pressure);
          }
          else if (this.snapY.HasValue)
          {
            // replace this point with the same point but with the Y value snapped.
            newInkPoints[i] = new InkPoint(
              new Point(newInkPoints[i].Position.X, this.snapY.Value),
              newInkPoints[i].Pressure);
          }
        }
      }
    }

    double NearestGridSizeMultiple(double value)
    {
      // Note. I have added a new member variable 'currentGridSize' which I keep
      // in sync with the GridSize of the GraphPaperUserControl.
      // This is because this code runs on a non-UI thread so it cannot simply
      // call into that property on the user control.

      var divisor = value / this.currentGridSize;
      var fractional = divisor - Math.Floor(divisor);

      if (fractional >= 0.5)
      {
        divisor = Math.Ceiling(divisor);
      }
      else
      {
        divisor = Math.Floor(divisor);
      }
      return (divisor * this.currentGridSize);
    }
    int currentGridSize;


    void OnStrokeContinuing(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
    {
      this.SnapPoints(args.NewInkPoints);
    }

    void UpdateModeText() => this.txtMode.Text = this.currentMode.ToString();

    void OnInkCanvasManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
      var newScaleFactor = (decimal)e.Delta.Scale * this.currentScaleFactor;

      if ((newScaleFactor <= MAX_SCALE_FACTOR) && (newScaleFactor >= MIN_SCALE_FACTOR))
      {
        this.currentScaleFactor = newScaleFactor;

        var newGridSize = (int)(this.currentScaleFactor * BASE_GRID_SIZE);

        if (newGridSize != this.graphPaper.GridSize)
        {
          this.graphPaper.GridSize = newGridSize;
          this.currentGridSize = newGridSize;
        }
      }
    }
    void OnInkCanvasTapped(object sender, TappedRoutedEventArgs e)
    {
      if (e.PointerDeviceType == PointerDeviceType.Touch)
      {
        // Apologies for doing such a horrible thing to an enum.
        this.currentMode =
          (Mode)((((int)this.currentMode) + 1) % ((int)Mode.SnapY + 1));

        this.UpdateModeText();
      }
    }
    CoreWetStrokeUpdateSource wetUpdateSource;
    Mode currentMode;
    decimal currentScaleFactor;
    static readonly decimal MAX_SCALE_FACTOR = 8.0m;
    static readonly decimal MIN_SCALE_FACTOR = 0.5m;
    static readonly int BASE_GRID_SIZE = 20;
  }
}

/*

*/
