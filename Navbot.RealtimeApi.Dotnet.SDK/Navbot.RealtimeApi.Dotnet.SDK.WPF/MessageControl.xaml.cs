using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

/// <summary>
/// Interaction logic for MessageControl.xaml
/// </summary>
public partial class MessageControl : UserControl
{
    public static readonly DependencyProperty SourceImageProperty = DependencyProperty.Register(
        nameof(SourceImage), typeof(ImageSource), typeof(MessageControl), new PropertyMetadata(null));

    public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register(
        nameof(ContentText), typeof(string), typeof(MessageControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsLeftAlignedProperty = DependencyProperty.Register(
        nameof(IsLeftAligned), typeof(bool), typeof(MessageControl), new PropertyMetadata(false));

    public static readonly DependencyProperty IsRightAlignedProperty = DependencyProperty.Register(
        nameof(IsRightAligned), typeof(bool), typeof(MessageControl), new PropertyMetadata(false));

    public static readonly DependencyProperty ContentAlignmentProperty = DependencyProperty.Register(
        nameof(ContentAlignment), typeof(HorizontalAlignment), typeof(MessageControl), new PropertyMetadata(HorizontalAlignment.Left));

    public ImageSource SourceImage
    {
        get => (ImageSource)GetValue(SourceImageProperty);
        set => SetValue(SourceImageProperty, value);
    }

    public string ContentText
    {
        get => (string)GetValue(ContentTextProperty);
        set => SetValue(ContentTextProperty, value);
    }

    public bool IsLeftAligned
    {
        get => (bool)GetValue(IsLeftAlignedProperty);
        set => SetValue(IsLeftAlignedProperty, value);
    }

    public bool IsRightAligned
    {
        get => (bool)GetValue(IsRightAlignedProperty);
        set => SetValue(IsRightAlignedProperty, value);
    }

    public HorizontalAlignment ContentAlignment
    {
        get => (HorizontalAlignment)GetValue(ContentAlignmentProperty);
        set => SetValue(ContentAlignmentProperty, value);
    }

    public MessageControl()
    {
        var grid = new Grid { Margin = new Thickness(10) };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var leftImage = new Image
        {
            Width = 40,
            Height = 40,
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Top
        };
        leftImage.SetBinding(Image.SourceProperty, new System.Windows.Data.Binding(nameof(SourceImage)) { Source = this });

        var contentTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        contentTextBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(ContentText)) { Source = this });

        var textBlockContainer = new Border
        {
            Background = new SolidColorBrush(Colors.LightGray),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10),
            MaxWidth = 400,
            Margin = new Thickness(5),
            Child = contentTextBlock
        };

        var rightImage = new Image
        {
            Width = 40,
            Height = 40,
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Top
        };

        grid.Children.Add(leftImage);
        grid.Children.Add(textBlockContainer);
        grid.Children.Add(rightImage);
        Grid.SetColumn(leftImage, 0);
        Grid.SetColumn(textBlockContainer, 1);
        Grid.SetColumn(rightImage, 2);

        Content = grid;
    }


}
