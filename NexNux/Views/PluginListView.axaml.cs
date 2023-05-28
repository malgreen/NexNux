using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using NexNux.Models.Gamebryo;
using NexNux.ViewModels;

namespace NexNux.Views;

public partial class PluginListView : UserControl
{
    public PluginListView()
    {
        InitializeComponent();
        SetupGridHandlers();
        _isDragging = false;
    }

    Point? _dragStartPoint;
    private bool _isDragging;

    private void SetupGridHandlers()
    {
        this.GetControl<DataGrid>("PluginsGrid").AddHandler(DragDrop.DropEvent, DataGrid_Drop);
        this.GetControl<DataGrid>("PluginsGrid").AddHandler(PointerMovedEvent, DataGrid_PointerMoved);
        this.GetControl<DataGrid>("PluginsGrid").AddHandler(PointerReleasedEvent, DataGrid_PointerReleased);
        this.GetControl<DataGrid>("PluginsGrid").CellPointerPressed += DataGridCell_PointerPressed;
        this.GetControl<DataGrid>("PluginsGrid").AddHandler(DragDrop.DragOverEvent, DataGrid_DragOver);
        this.GetControl<DataGrid>("PluginsGrid").AddHandler(PointerEnteredEvent, DataGrid_PointerEnter);
    }
    
    private void DataGrid_PointerEnter(object? sender, PointerEventArgs e)
    {
        // Workaround for the 'DragLeave' event not being fired, so the drop indicator line is only removed
        // after moving the mouse back into the datagrid, after dropping outside of it
        // This is broken on Linux
        ClearDropPoint();
        _dragStartPoint = null;
        _isDragging = false;
    }

    private void DataGrid_DragOver(object? sender, DragEventArgs e)
    {
        ClearDropPoint();
        DataGridRow? targetRow = ((Control)e.Source!).GetSelfAndVisualAncestors()
                                                        .OfType<DataGridRow>()
                                                        .FirstOrDefault();
        ShowDropPoint(e.Data.Get("DragSource") as DataGridRow, targetRow);
    }

    private void DataGridCell_PointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        // It is split into this, so that the dragging doesn't start as soon as one presses a row
        if (!e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _dragStartPoint = e.PointerPressedEventArgs.GetPosition(null);
    }

    private void DataGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragStartPoint = null;
        _isDragging = false;
        ClearDropPoint();
    }

    private void DataGrid_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragStartPoint == null) return;
        if (_isDragging) return;
        if (sender is DataGrid)
        {
            Point mousePosition = e.GetPosition(null);
            Vector positionDiff = _dragStartPoint.Value - mousePosition;
            
            bool draggedEnough = Math.Abs(positionDiff.X) > 4 || Math.Abs(positionDiff.Y) > 4;

            if (draggedEnough && !_isDragging)
            {
                // Get the dragged row
                DataGridRow? sourceRow = ((Control)e.Source!).GetSelfAndVisualAncestors()
                                                            .OfType<DataGridRow>()
                                                            .FirstOrDefault();
                if (sourceRow == null) return;

                // Hacky workaround for getting the data, could be done via
                GamebryoPlugin? dropData = this.GetControl<DataGrid>("PluginsGrid").SelectedItem as GamebryoPlugin;
                if (dropData == null) return;

                // Actually do the interactivity part
                DataObject dataObject = new DataObject();
                dataObject.Set("DraggedPlugin", dropData);
                dataObject.Set("SourceIndex", sourceRow.GetIndex());
                dataObject.Set("DragSource", sourceRow);
                
                DragDrop.DoDragDrop(e, dataObject, DragDropEffects.Move);
                _isDragging = true;
            }
        }
    }

    private void DataGrid_Drop(object? sender, DragEventArgs e)
    {
        if(sender is DataGrid && DataContext is PluginListViewModel plvm)
        {
            _dragStartPoint = null;
            _isDragging = false;

            // Retrieve we put into the DataModel
            GamebryoPlugin? draggedPlugin = e.Data.Get("DraggedPlugin") as GamebryoPlugin;
            if (draggedPlugin == null) return;

            // Get targetRow by the drop position
            DataGridRow? targetRow = ((Control)e.Source!).GetSelfAndVisualAncestors()
                                                            .OfType<DataGridRow>()
                                                            .FirstOrDefault();

            int sourceIndex = (int)(e.Data.Get("SourceIndex") ?? throw new InvalidOperationException());
            int targetIndex = plvm.VisiblePlugins.Count - 1; // If dragged to the empty part of the DataGrid, it should just add it underneath

            // The actual drop/movement operation - this should be converted to command/interaction for MVVM
            if (targetRow != null)
            { 
                targetIndex = targetRow.GetIndex();
            }

            // Indexes used to move items in the VM
            if (sourceIndex == targetIndex) return;
            plvm.ReorderPlugin(sourceIndex, targetIndex);

            // This is necessary for some reason, maybe because DataGrid cells are recycled?
            this.GetControl<DataGrid>("PluginsGrid").Items = null;
            this.GetControl<DataGrid>("PluginsGrid").Items = plvm.VisiblePlugins;

            // Remove the line that indicates drop point
            ClearDropPoint();
        }
    }
    
    private void ShowDropPoint(DataGridRow? sourceRow, DataGridRow? targetRow)
    {
        if (sourceRow == null || targetRow == null) return;

        Point startPoint;
        Point endPoint;

        // When moving row 'down', it moves them to the index after the target
        if (targetRow.GetIndex() > sourceRow.GetIndex())
        {
            double startPointY = targetRow.Bounds.BottomLeft.Y + targetRow.Bounds.Height;
            double endPointY = targetRow.Bounds.BottomRight.Y + targetRow.Bounds.Height;

            startPoint = new Point(targetRow.Bounds.BottomLeft.X, startPointY);
            endPoint = new Point(targetRow.Bounds.BottomRight.X, endPointY);
        }
        // When moving rows 'up', its moves them to the index before the target
        else
        {
            double startPointY = targetRow.Bounds.TopLeft.Y + targetRow.Bounds.Height;
            double endPointY = targetRow.Bounds.TopRight.Y + targetRow.Bounds.Height;

            startPoint = new Point(targetRow.Bounds.TopLeft.X, startPointY);
            endPoint = new Point(targetRow.Bounds.TopRight.X, endPointY);
        }        

        Line line = new Line
        {
            Tag = "tempLine",
            StartPoint = startPoint,
            EndPoint = endPoint,
            Stroke = Brushes.Gray,
            StrokeThickness = 1,
            IsEnabled = false // So we can't drop on the actual indicator
        };
        this.GetControl<Canvas>("PluginsGridCanvas").Children.Add(line);
    }

    private void ClearDropPoint()
    {
        if (this.GetControl<Canvas>("PluginsGridCanvas").Children.Count <= 1) return;

        Line? existingLine = this.GetControl<Canvas>("PluginsGridCanvas").Children.OfType<Line>().FirstOrDefault();
        if (existingLine == null) return;
        this.GetControl<Canvas>("PluginsGridCanvas").Children.Remove(existingLine);
    }
}
