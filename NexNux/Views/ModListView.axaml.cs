using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Collections.Generic;
using NexNux.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using NexNux.ViewModels;
using Avalonia.VisualTree;
using System.Linq;
using Avalonia.LogicalTree;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.Diagnostics;
using System.Reactive;
using Avalonia.Controls.ApplicationLifetimes;

namespace NexNux.Views;

public partial class ModListView : ReactiveWindow<ModListViewModel>
{
    Point? _dragStartPoint;
    public ModListView()
    {
        InitializeComponent();
        this.DataContextChanged += ModListView_DataContextChanged;
        this.WhenActivated(d => d(ViewModel!.ShowModInstallDialog.RegisterHandler(DoShowModInstallDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowModUninstallDialog.RegisterHandler(DoShowModUninstallDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowErrorDialog.RegisterHandler(DoShowErrorDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowModExistsDialog.RegisterHandler(DoShowModExistsDialogAsync)));
        this.WhenActivated(d => d(ViewModel!.ShowGameList.RegisterHandler(DoShowGameList)));
        SetupGridHandlers();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SetupGridHandlers()
    {
        this.GetControl<DataGrid>("GridMods").AddHandler(DragDrop.DropEvent, DataGrid_Drop);
        this.GetControl<DataGrid>("GridMods").AddHandler(PointerMovedEvent, DataGrid_PointerMoved);
        this.GetControl<DataGrid>("GridMods").AddHandler(PointerReleasedEvent, DataGrid_PointerReleased);
        this.GetControl<DataGrid>("GridMods").CellPointerPressed += DataGridCell_PointerPressed;
        this.GetControl<DataGrid>("GridMods").AddHandler(DragDrop.DragOverEvent, DataGrid_DragOver);
        this.GetControl<DataGrid>("GridMods").AddHandler(PointerEnterEvent, DataGrid_PointerEnter);
    }

    private void DataGrid_PointerEnter(object? sender, PointerEventArgs e)
    {
        // Workaround for the 'DragLeave' event not being fired, so the drop indicator line is only removed
        // after moving the mouse back into the datagrid, after dropping outside of it
        ClearDropPoint();
        _dragStartPoint = null;
    }

    private void DataGrid_DragOver(object? sender, DragEventArgs e)
    {
        ClearDropPoint();
        DataGridRow? targetRow = ((IControl)e.Source).GetSelfAndVisualAncestors()
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
    }

    private void DataGrid_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragStartPoint == null) return; // Basically '!IsDragging', it's modified in the PointerPressed and PointerReleased

        if (sender is DataGrid dataGrid)
        {
            Point mousePosition = e.GetPosition(null);
            Vector positionDiff = _dragStartPoint.Value - mousePosition;

            bool draggedEnoughX = Math.Abs(positionDiff.X) > 4;
            bool draggedEnoughY = Math.Abs(positionDiff.Y) > 4;

            if (draggedEnoughX || draggedEnoughY)
            {
                // Get the dragged row
                DataGridRow? sourceRow = ((IControl)e.Source).GetSelfAndVisualAncestors()
                                                            .OfType<DataGridRow>()
                                                            .FirstOrDefault();
                if (sourceRow == null) return;

                // Hacky workaround for getting the data, could be done via
                Mod? dropData = this.GetControl<DataGrid>("GridMods").SelectedItem as Mod;
                if (dropData == null) return;

                // Actually do the interactivity part
                DataObject dataObject = new DataObject();
                dataObject.Set("DraggedMod", dropData);
                dataObject.Set("SourceIndex", sourceRow.GetIndex());
                dataObject.Set("DragSource", sourceRow);
                
                DragDrop.DoDragDrop(e, dataObject, DragDropEffects.Move);
            }
        }
    }

    private void DataGrid_Drop(object? sender, DragEventArgs e)
    {
        if(sender is DataGrid dataGrid && DataContext is ModListViewModel mlvm)
        {
            _dragStartPoint = null;

            // Retrieve we put into the DataModel
            Mod? draggedMod = e.Data.Get("DraggedMod") as Mod;
            if (draggedMod == null) return;

            // Get targetRow by the drop position
            DataGridRow? targetRow = ((IControl)e.Source).GetSelfAndVisualAncestors()
                                                            .OfType<DataGridRow>()
                                                            .FirstOrDefault();

            int sourceIndex = (int)e.Data.Get("SourceIndex");
            int targetIndex = mlvm.VisibleMods.Count - 1; // If dragged to the empty part of the DataGrid, it should just add it underneath

            // The actual drop/movement operation - this should be converted to command/interaction for MVVM
            if (targetRow != null)
            {
                targetIndex = targetRow.GetIndex();
            }

            // Indexes used to move items in the VM
            if (sourceIndex == targetIndex) return;
            mlvm.VisibleMods.Move(sourceIndex, targetIndex);

            // This is necessary for some reason, maybe because DataGrid cells are recycled?
            this.GetControl<DataGrid>("GridMods").Items = null; 
            this.GetControl<DataGrid>("GridMods").Items = mlvm.VisibleMods;

            // Remove the line that indicates drop point
            ClearDropPoint();
        }
    }

    private void ModListView_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ModListViewModel modListViewModel)
        {
            Title = "NexNux - " + modListViewModel.CurrentGame;
        }
    }

    private async Task DoShowErrorDialogAsync(InteractionContext<string, bool> interactionContext)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Error!", interactionContext.Input, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Warning);
        await messageBox.ShowDialog(this);
        interactionContext.SetOutput(true);
    }

    private async Task DoShowModInstallDialogAsync(InteractionContext<ModConfigViewModel, Mod?> interactionContext)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            AllowMultiple = false,
            Title = "Choose mod archive",
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter()
                {
                    Extensions = new List<string> {"zip", "rar", "7z", "tar", "gzip"}
                }
            }
        };

        string[]? result = await openFileDialog.ShowAsync(this);
        if (result == null)
        {
            interactionContext.SetOutput(null);
            return;
        }

        ModConfigView dialog = new ModConfigView();
        interactionContext.Input.UpdateModArchive(string.Join("", result));
        dialog.DataContext = interactionContext.Input;

        Mod? mod = await dialog.ShowDialog<Mod>(this);
        interactionContext.SetOutput(mod);
    }
    
    private async Task DoShowModUninstallDialogAsync(InteractionContext<Mod, bool> interactionContext)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(
            $"Uninstalling {interactionContext.Input}, are you sure?",
            $"This will also delete the files for \"{interactionContext.Input}\" from your system.", // currently this is a lie
            ButtonEnum.OkCancel,
            MessageBox.Avalonia.Enums.Icon.Warning
        );
        var result = await messageBox.Show(this);
        interactionContext.SetOutput(result == ButtonResult.Ok);
    }

    private async Task DoShowModExistsDialogAsync(InteractionContext<Mod, bool> interactionContext)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(
            "Mod already exists",
            $"Mod \"{interactionContext.Input}\" already exists, continuing will merge the two while overriding existing files.", // currently this is a lie
            ButtonEnum.OkCancel,
            MessageBox.Avalonia.Enums.Icon.Info
        );
        var result = await messageBox.Show(this);
        interactionContext.SetOutput(result == ButtonResult.Ok);
    }

    private void ShowDropPoint(DataGridRow sourceRow, DataGridRow targetRow)
    {
        if (sourceRow == null || targetRow == null) return;

        Point startPoint = new Point(0, 0);
        Point endPoint = new Point(0, 0);

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

        Line line = new Line()
        {
            Tag = "tempLine",
            StartPoint = startPoint,
            EndPoint = endPoint,
            Stroke = Brushes.Gray,
            StrokeThickness = 1
        };
        line.IsEnabled = false; // So we can't drop on the actual indicator
        this.GetControl<Canvas>("GridModsCanvas").Children.Add(line);
    }

    private void ClearDropPoint()
    {
        if (this.GetControl<Canvas>("GridModsCanvas").Children.Count <= 1) return;

        Line existingLine = this.GetControl<Canvas>("GridModsCanvas").Children.OfType<Line>().First();
        if (existingLine != null)
        {
            this.GetControl<Canvas>("GridModsCanvas").Children.Remove(existingLine);
        }
    }

    private async Task DoShowGameList(InteractionContext<Unit, Unit> interactionContext)
    {
        GameListViewModel viewModel = new GameListViewModel();
        GameListView gameListView = new GameListView();
        gameListView.DataContext = viewModel;
        gameListView.Show();
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.MainWindow = gameListView;
        }
        this.Close();
    }
}