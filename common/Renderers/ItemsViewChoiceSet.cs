﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using AdaptiveCards.ObjectModel.WinUI3;
using AdaptiveCards.Rendering.WinUI3;
using CommunityToolkit.WinUI.Controls;
using DevHome.Common.DevHomeAdaptiveCards.CardModels;
using DevHome.Common.DevHomeAdaptiveCards.InputValues;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace DevHome.Common.Renderers;

public enum DevHomeChoiceSetKind
{
    Unknown,
    ItemsViewChoiceSet,
}

/// <summary>
/// Renders a list of expanded choice set items as an ItemsView
/// </summary>
public class ItemsViewChoiceSet : IAdaptiveElementRenderer
{
    /// <summary>
    /// The DataTemplate to be used for the ItemsView. This will be used to allow consumers to specify how the items
    /// look within the ItemsView.
    /// </summary>
    private readonly DataTemplate _itemsTemplate;

    /// <summary>
    /// The items to be displayed in the ItemsView. The items are objects which will be used as the Content
    /// of the ListViewItems we add to the ItemsView. The datatemplate should use Binding to display values from
    /// these objects.
    /// </summary>
    /// <remarks>
    /// The order of the items in this list will should be the same order as the items in the choice set.
    /// </remarks>
    private readonly List<object> _originalItems;

    public ItemsView ChoiceSetItemsView { get; private set; } = new();

    public List<ListViewItem> ListViewItemsForItemsView { get; private set; } = new();

    public ItemsViewChoiceSet(DataTemplate itemsTemplate, List<object> objectList)
    {
        _itemsTemplate = itemsTemplate;
        _originalItems = objectList;

        // set the template for the items view.
        ChoiceSetItemsView.ItemTemplate = _itemsTemplate;
    }

    public UIElement Render(IAdaptiveCardElement element, AdaptiveRenderContext context, AdaptiveRenderArgs renderArgs)
    {
        // As we add more types of choice sets, we can add more cases here.
        if (element is DevHomeAdaptiveSettingsCardItemsViewChoiceSet settingsCardChoiceSet)
        {
            return GetItemsViewElement(settingsCardChoiceSet, context, renderArgs);
        }

        // Use default render for all other cases.
        var renderer = new AdaptiveChoiceSetInputRenderer();
        return renderer.Render(element, context, renderArgs);
    }

    private ItemsView GetItemsViewElement(DevHomeAdaptiveSettingsCardItemsViewChoiceSet settingsCardChoiceSet, AdaptiveRenderContext context, AdaptiveRenderArgs renderArgs)
    {
        // Check if the choice set is multi-select, and if it is make sure the ItemsView is set to allow multiple selection.
        if (settingsCardChoiceSet.IsMultiSelect)
        {
            ChoiceSetItemsView.SelectionMode = ItemsViewSelectionMode.Multiple;
        }

        // Go through all the items in the choice set and make an item for each one.
        for (var i = 0; i < settingsCardChoiceSet.SettingsCards.Count; i++)
        {
            var communityToolKitCard = new SettingsCard();
            var devHomeAdaptiveSettingsCard = settingsCardChoiceSet.SettingsCards[i];
            communityToolKitCard.Description = devHomeAdaptiveSettingsCard.Description;
            communityToolKitCard.Header = devHomeAdaptiveSettingsCard.Header;
            communityToolKitCard.HeaderIcon = ConvertBase64StringToImageSource(devHomeAdaptiveSettingsCard.HeaderIcon);

            ListViewItemsForItemsView.Add(new ListViewItem() { Content = communityToolKitCard });
        }

        // Set upp the ItemsSource for the ItemsView and add the input value to the context.
        // the input value is used to get the current index of the items view in relation
        // to the item in the choice set.
        ChoiceSetItemsView.ItemsSource = ListViewItemsForItemsView;
        context.AddInputValue(new ItemsViewInputValue(settingsCardChoiceSet, ChoiceSetItemsView), renderArgs);

        // Return the ItemsView.
        return ChoiceSetItemsView;
    }

    // convert base64 string to image that can be used in a imageIcon control
    public static ImageIcon ConvertBase64StringToImageSource(string base64String)
    {
        var bytes = Convert.FromBase64String(base64String);
        var bitmapImage = new BitmapImage();

        using (var stream = new InMemoryRandomAccessStream())
        {
            using (var writer = new DataWriter(stream))
            {
                writer.WriteBytes(bytes);
                writer.StoreAsync().GetAwaiter().GetResult();
                writer.FlushAsync().GetAwaiter().GetResult();
                writer.DetachStream();
            }

            stream.Seek(0);
            bitmapImage.SetSource(stream);
        }

        var icon = new ImageIcon() { Source = bitmapImage };
        return icon;
    }
}