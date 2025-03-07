﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace XamForms.Controls
{
    public partial class Calendar : ContentView
    {
        List<BoxView> rows;
        List<CalendarButton> buttons;
        List<Grid> MainCalendars;
        List<Label> TitleLabels;
        StackLayout MainView, ContentView;
        public static double GridSpace = 0;
        public event EventHandler<EventArgs> OnStartRenderCalendar, OnEndRenderCalendar;

        public Calendar()
        {
            TitleLeftArrow = new CalendarButton
            {
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.Transparent,
                FontSize = 24,
                Text = "❰",
                TextColor = Color.FromHex("#c82727")
            };
            TitleLabel = new Label
            {
                FontSize = 24,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Text = ""
            };
            TitleRightArrow = new CalendarButton
            {
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.Transparent,
                FontSize = 24,
                Text = "❱",
                TextColor = Color.FromHex("#c82727")
            };
            MonthNavigationLayout = new StackLayout
            {
                Padding = 0,
                VerticalOptions = LayoutOptions.Start,
                Orientation = StackOrientation.Horizontal,
                HeightRequest = Device.RuntimePlatform == Device.UWP ? 50 : 32,
                Children = { TitleLeftArrow, TitleLabel, TitleRightArrow }
            };
            ContentView = new StackLayout
            {
                Padding = 0,
                Orientation = StackOrientation.Vertical
            };
            MainView = new StackLayout
            {
                Padding = 0,
                Orientation = StackOrientation.Vertical,
                Children = { MonthNavigationLayout, ContentView }
            };


            TitleLeftArrow.Clicked += LeftArrowClickedEvent;
            TitleRightArrow.Clicked += RightArrowClickedEvent;
            dayLabels = new List<Label>(7);
            weekNumberLabels = new List<Label>(6);
            rows = new List<BoxView>(7);
            buttons = new List<CalendarButton>(42);
            MainCalendars = new List<Grid>(1);
            WeekNumbers = new List<Grid>(1);

            CalendarViewType = DateTypeEnum.Normal;
            YearsRow = 4;
            YearsColumn = 4;
        }

        public bool IsRendering { get { return Content == null; } }

        #region MinDate

        public static readonly BindableProperty MinDateProperty =
            BindableProperty.Create(nameof(MinDate), typeof(DateTime?), typeof(Calendar), null,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalendarChanges.MaxMin));

        /// <summary>
        /// Gets or sets the minimum date.
        /// </summary>
        /// <value>The minimum date.</value>
        public DateTime? MinDate
        {
            get { return (DateTime?)GetValue(MinDateProperty); }
            set { SetValue(MinDateProperty, value); ChangeCalendar(CalendarChanges.MaxMin); }
        }

        #endregion

        #region MaxDate

        public static readonly BindableProperty MaxDateProperty =
            BindableProperty.Create(nameof(MaxDate), typeof(DateTime?), typeof(Calendar), null,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalendarChanges.MaxMin));

        /// <summary>
        /// Gets or sets the max date.
        /// </summary>
        /// <value>The max date.</value>
        public DateTime? MaxDate
        {
            get { return (DateTime?)GetValue(MaxDateProperty); }
            set { SetValue(MaxDateProperty, value); }
        }

        #endregion

        #region StartDate

        public static readonly BindableProperty StartDateProperty =
            BindableProperty.Create(nameof(StartDate), typeof(DateTime), typeof(Calendar), DateTime.Now,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalendarChanges.StartDate));

        /// <summary>
        /// Gets or sets a date, to pick the month, the calendar is focused on
        /// </summary>
        /// <value>The start date.</value>
        public DateTime StartDate
        {
            get { return (DateTime)GetValue(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        #endregion

        #region StartDay

        public static readonly BindableProperty StartDayProperty =
            BindableProperty.Create(nameof(StartDate), typeof(DayOfWeek), typeof(Calendar), DayOfWeek.Sunday,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalendarChanges.StartDay));

        /// <summary>
        /// Gets or sets the day the calendar starts the week with.
        /// </summary>
        /// <value>The start day.</value>
        public DayOfWeek StartDay
        {
            get { return (DayOfWeek)GetValue(StartDayProperty); }
            set { SetValue(StartDayProperty, value); }
        }

        #endregion

        #region BorderWidth

        public static readonly BindableProperty BorderWidthProperty =
            BindableProperty.Create(nameof(BorderWidth), typeof(int), typeof(Calendar), 1,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeBorderWidth((int)newValue, (int)oldValue));

        protected void ChangeBorderWidth(int newValue, int oldValue)
        {
            if (newValue == oldValue) return;
            buttons.FindAll(b => !b.IsSelected && b.IsEnabled).ForEach(b => b.BorderWidth = newValue);
        }

        /// <summary>
        /// Gets or sets the border width of the calendar.
        /// </summary>
        /// <value>The width of the border.</value>
        public int BorderWidth
        {
            get { return (int)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        #endregion

        #region OuterBorderWidth

        public static readonly BindableProperty OuterBorderWidthProperty =
            BindableProperty.Create(nameof(OuterBorderWidth), typeof(int), typeof(Calendar), 0,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).MainCalendars.ForEach((obj) => obj.Padding = (int)newValue));

        /// <summary>
        /// Gets or sets the width of the whole calendar border.
        /// </summary>
        /// <value>The width of the outer border.</value>
        public int OuterBorderWidth
        {
            get { return (int)GetValue(OuterBorderWidthProperty); }
            set { SetValue(OuterBorderWidthProperty, value); }
        }

        #endregion

        #region BorderColor

        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(Calendar), Color.Transparent,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeBorderColor((Color)newValue, (Color)oldValue));

        protected void ChangeBorderColor(Color newValue, Color oldValue)
        {
            if (newValue == oldValue) return;
            MainCalendars.ForEach((obj) => obj.BackgroundColor = newValue);
            buttons.FindAll(b => b.IsEnabled && !b.IsSelected).ForEach(b => b.BorderColor = newValue);
        }

        /// <summary>
        /// Gets or sets the border color of the calendar.
        /// </summary>
        /// <value>The color of the border.</value>
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        #endregion

        #region CornerRadius

        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(Calendar), 0,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCornerRadius((int)newValue, (int)oldValue));

        protected void ChangeCornerRadius(int newValue, int oldValue)
        {
            if (newValue == oldValue) return;
            buttons.FindAll(b => !b.IsSelected && b.IsEnabled).ForEach(b => b.CornerRadius = newValue);
        }

        /// <summary>
        /// Gets or sets the border width of the calendar.
        /// </summary>
        /// <value>The width of the border.</value>
        public int CornerRadius
        {
            get { return (int)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        #endregion

        #region DatesBackgroundColor

        public static readonly BindableProperty DatesBackgroundColorProperty =
            BindableProperty.Create(nameof(DatesBackgroundColor), typeof(Color), typeof(Calendar), Color.Transparent,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesBackgroundColor((Color)newValue, (Color)oldValue));

        protected void ChangeDatesBackgroundColor(Color newValue, Color oldValue)
        {
            if (newValue == oldValue) return;
            if (DatesAlternateBackgroundColor.HasValue)
            {
                buttons.FindAll(b => b.IsEnabled && (!b.IsSelected || SelectedBackgroundColor != Color.Default) && b.Row % 2 != 0).ForEach(b => b.BackgroundColor = newValue);
            }
            else
            {
                buttons.FindAll(b => b.IsEnabled && (!b.IsSelected || SelectedBackgroundColor != Color.Default)).ForEach(b => b.BackgroundColor = newValue);
            }
        }

        /// <summary>
        /// Gets or sets the background color of the normal dates.
        /// </summary>
        /// <value>The color of the dates background.</value>
        public Color DatesBackgroundColor
        {
            get { return (Color)GetValue(DatesBackgroundColorProperty); }
            set { SetValue(DatesBackgroundColorProperty, value); }
        }

        #endregion

        #region DatesAlternateBackgroundColor

        public static readonly BindableProperty DatesAlternateBackgroundColorProperty =
            BindableProperty.Create(nameof(DatesAlternateBackgroundColor), typeof(Color?), typeof(Calendar), null,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesAlternateBackgroundColor((Color?)newValue, (Color?)oldValue));

        protected void ChangeDatesAlternateBackgroundColor(Color? newValue, Color? oldValue)
        {
            if (newValue == oldValue) return;
            if (newValue.HasValue)
            {
                buttons.FindAll(b => b.IsEnabled && (!b.IsSelected || SelectedBackgroundColor != Color.Default) && b.Row % 2 == 0).ForEach(b => b.BackgroundColor = newValue.Value);
            }
            else
            {
                buttons.FindAll(b => b.IsEnabled && (!b.IsSelected || SelectedBackgroundColor != Color.Default) && b.Row % 2 == 0).ForEach(b => b.BackgroundColor = DatesBackgroundColor);
            }
        }

        /// <summary>
        /// Gets or sets the background color of the normal dates.
        /// </summary>
        /// <value>The color of the dates background on the even rows.</value>
        public Color? DatesAlternateBackgroundColor
        {
            get { return (Color?)GetValue(DatesAlternateBackgroundColorProperty); }
            set { SetValue(DatesAlternateBackgroundColorProperty, value); }
        }

        #endregion

        #region DatesTextColor

        public static readonly BindableProperty DatesTextColorProperty =
            BindableProperty.Create(nameof(DatesTextColor), typeof(Color), typeof(Calendar), Color.Black,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesTextColor((Color)newValue, (Color)oldValue));

        protected void ChangeDatesTextColor(Color newValue, Color oldValue)
        {
            if (newValue == oldValue) return;
            buttons.FindAll(b => b.IsEnabled && (!b.IsSelected || SelectedTextColor != Color.Default) && !b.IsOutOfMonth).ForEach(b => b.TextColor = newValue);
        }

        /// <summary>
        /// Gets or sets the text color of the normal dates.
        /// </summary>
        /// <value>The color of the dates text.</value>
        public Color DatesTextColor
        {
            get { return (Color)GetValue(DatesTextColorProperty); }
            set { SetValue(DatesTextColorProperty, value); }
        }

        #endregion

        #region DatesFontAttributes

        public static readonly BindableProperty DatesFontAttributesProperty =
            BindableProperty.Create(nameof(DatesFontAttributes), typeof(FontAttributes), typeof(Calendar), FontAttributes.None,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesFontAttributes((FontAttributes)newValue, (FontAttributes)oldValue));

        protected void ChangeDatesFontAttributes(FontAttributes newValue, FontAttributes oldValue)
        {
            if (newValue == oldValue) return;
            buttons.FindAll(b => b.IsEnabled && (!b.IsSelected || SelectedTextColor != Color.Default) && !b.IsOutOfMonth).ForEach(b => b.FontAttributes = newValue);
        }

        /// <summary>
        /// Gets or sets the dates font attributes.
        /// </summary>
        /// <value>The dates font attributes.</value>
        public FontAttributes DatesFontAttributes
        {
            get { return (FontAttributes)GetValue(DatesFontAttributesProperty); }
            set { SetValue(DatesFontAttributesProperty, value); }
        }

        #endregion

        #region DatesFontSize

        public static readonly BindableProperty DatesFontSizeProperty =
            BindableProperty.Create(nameof(DatesFontSize), typeof(double), typeof(Calendar), 20.0,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesFontSize((double)newValue, (double)oldValue));

        protected void ChangeDatesFontSize(double newValue, double oldValue)
        {
            if (Math.Abs(newValue - oldValue) < 0.01) return;
            buttons?.FindAll(b => !b.IsSelected && b.IsEnabled).ForEach(b => b.FontSize = newValue);
        }

        /// <summary>
        /// Gets or sets the font size of the normal dates.
        /// </summary>
        /// <value>The size of the dates font.</value>
        public double DatesFontSize
        {
            get { return (double)GetValue(DatesFontSizeProperty); }
            set { SetValue(DatesFontSizeProperty, value); }
        }

        #endregion

        #region DatesFontFamily

        public static readonly BindableProperty DatesFontFamilyProperty =
                    BindableProperty.Create(nameof(DatesFontFamily), typeof(string), typeof(Calendar), default(string),
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeDatesFontFamily((string)newValue, (string)oldValue));

        protected void ChangeDatesFontFamily(string newValue, string oldValue)
        {
            if (newValue == oldValue) return;
            buttons?.FindAll(b => !b.IsSelected && b.IsEnabled).ForEach(b => b.FontFamily = newValue);
        }

        /// <summary>
        /// Gets or sets the font family of dates.
        /// </summary>
        public string DatesFontFamily
        {
            get { return GetValue(DatesFontFamilyProperty) as string; }
            set { SetValue(DatesFontFamilyProperty, value); }
        }

        #endregion

        #region ShowOutOfMonthDates

        public static readonly BindableProperty ShowOutOfMonthDatesProperty =
            BindableProperty.Create(nameof(ShowOutOfMonthDates), typeof(bool), typeof(Calendar), true,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalendarChanges.All));

        /// <summary>
        /// Gets or sets a the number of months to show
        /// </summary>
        /// <value>The start date.</value>
        public bool ShowOutOfMonthDates
        {
            get { return (bool)GetValue(ShowOutOfMonthDatesProperty); }
            set { SetValue(ShowOutOfMonthDatesProperty, value); }
        }

        #endregion

        #region ShowNumOfMonths

        public static readonly BindableProperty ShowNumOfMonthsProperty =
            BindableProperty.Create(nameof(ShowNumOfMonths), typeof(int), typeof(Calendar), 1,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalendarChanges.All));

        /// <summary>
        /// Gets or sets a the number of months to show
        /// </summary>
        /// <value>The start date.</value>
        public int ShowNumOfMonths
        {
            get { return (int)GetValue(ShowNumOfMonthsProperty); }
            set { SetValue(ShowNumOfMonthsProperty, value); }
        }

        #endregion

        #region ShowInBetweenMonthLabels

        public static readonly BindableProperty ShowInBetweenMonthLabelsProperty =
            BindableProperty.Create(nameof(ShowInBetweenMonthLabels), typeof(bool), typeof(Calendar), true,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeCalendar(CalendarChanges.All));

        /// <summary>
        /// Gets or sets a the number of months to show
        /// </summary>
        /// <value>The start date.</value>
        public bool ShowInBetweenMonthLabels
        {
            get { return (bool)GetValue(ShowInBetweenMonthLabelsProperty); }
            set { SetValue(ShowInBetweenMonthLabelsProperty, value); }
        }

        #endregion

        #region DateCommand

        public static readonly BindableProperty DateCommandProperty =
            BindableProperty.Create(nameof(DateCommand), typeof(ICommand), typeof(Calendar), null);

        /// <summary>
        /// Gets or sets the selected date command.
        /// </summary>
        /// <value>The date command.</value>
        public ICommand DateCommand
        {
            get { return (ICommand)GetValue(DateCommandProperty); }
            set { SetValue(DateCommandProperty, value); }
        }

        #endregion

        #region RowBackgroundColor

        public static readonly BindableProperty RowBackgroundColorProperty =
            BindableProperty.Create(nameof(RowBackgroundColor), typeof(Color), typeof(Calendar), Color.Transparent,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeRowBackgroundColor((Color)newValue, (Color)oldValue));

        protected void ChangeRowBackgroundColor(Color newValue, Color oldValue)
        {
            if (newValue == oldValue) return;
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if ((i + 1) % 2 != 0)
                {
                    row.Color = RowBackgroundColor;
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color of the normal dates.
        /// </summary>
        /// <value>The color of the dates background on the even rows.</value>
        public Color RowBackgroundColor
        {
            get { return (Color)GetValue(RowBackgroundColorProperty); }
            set { SetValue(RowBackgroundColorProperty, value); }
        }

        #endregion

        #region RowAlternateBackgroundColor

        public static readonly BindableProperty RowAlternateBackgroundColorProperty =
            BindableProperty.Create(nameof(RowAlternateBackgroundColor), typeof(Color), typeof(Calendar), Color.Transparent,
                                    propertyChanged: (bindable, oldValue, newValue) => (bindable as Calendar).ChangeRowAlternateBackgroundColor((Color)newValue, (Color)oldValue));

        protected void ChangeRowAlternateBackgroundColor(Color newValue, Color oldValue)
        {
            if (newValue == oldValue) return;
            for (var i = 0; i < rows.Count; i++)
            {
                if ((i + 1) % 2 == 0) continue;

                var row = rows[i];
                row.Color = newValue;
            }
        }

        /// <summary>
        /// Gets or sets the background color of the normal dates.
        /// </summary>
        /// <value>The color of the dates background on the even rows.</value>
        public Color RowAlternateBackgroundColor
        {
            get { return (Color)GetValue(RowAlternateBackgroundColorProperty); }
            set { SetValue(RowAlternateBackgroundColorProperty, value); }
        }

        #endregion

        public DateTime CalendarStartDate(DateTime date)
        {
            var start = date;
            var beginOfMonth = start.Day == 1;
            while (!beginOfMonth || start.DayOfWeek != StartDay)
            {
                start = start.AddDays(-1);
                beginOfMonth |= start.Day == 1;
            }
            return start;
        }

        #region Functions

        protected override void OnParentSet()
        {
            FillCalendarWindows();
            base.OnParentSet();
            ChangeCalendar(CalendarChanges.All);
        }

        protected Task FillCalendar()
        {
            return Task.Factory.StartNew(() =>
            {
                FillCalendarWindows();
            });
        }

        protected void FillCalendarWindows()
        {
            CreateWeeknumbers();
            CreateButtons();
            ShowHideElements();
        }

        protected void CreateWeeknumbers()
        {
            weekNumberLabels.Clear();
            WeekNumbers.Clear();
            if (!ShowNumberOfWeek) return;

            for (var i = 0; i < ShowNumOfMonths; i++)
            {
                var columDef = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                var rowDef = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
                var weekNumbers = new Grid { VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.Start, RowSpacing = 0, ColumnSpacing = 0, Padding = new Thickness(0, 0, 0, 0) };
                weekNumbers.ColumnDefinitions = new ColumnDefinitionCollection { columDef };
                weekNumbers.RowDefinitions = new RowDefinitionCollection { rowDef, rowDef, rowDef, rowDef, rowDef, rowDef };
                weekNumbers.WidthRequest = NumberOfWeekFontSize * (Device.RuntimePlatform == Device.iOS ? 1.5 : 2.5);

                for (int r = 0; r < 6; r++)
                {
                    weekNumberLabels.Add(new Label
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        TextColor = NumberOfWeekTextColor,
                        BackgroundColor = NumberOfWeekBackgroundColor,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = NumberOfWeekFontSize,
                        FontAttributes = NumberOfWeekFontAttributes,
                        FontFamily = NumberOfWeekFontFamily
                    });
                    weekNumbers.Children.Add(weekNumberLabels.Last(), 0, r);
                }
                WeekNumbers.Add(weekNumbers);
            }
        }

        protected void CreateButtons()
        {
            var columDef = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var rowDef = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            rows.Clear();
            buttons.Clear();
            MainCalendars.Clear();
            for (var i = 0; i < ShowNumOfMonths; i++)
            {
                var mainCalendar = new Grid { VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand, RowSpacing = GridSpace, ColumnSpacing = GridSpace, Padding = 1, BackgroundColor = Color.Transparent };
                mainCalendar.ColumnDefinitions = new ColumnDefinitionCollection { columDef, columDef, columDef, columDef, columDef, columDef, columDef };
                mainCalendar.RowDefinitions = new RowDefinitionCollection { rowDef, rowDef, rowDef, rowDef, rowDef, rowDef };

                for (int r = 0; r < 6; r++)
                {
                    var rowBG = RowBackgroundColor;
                    if ((r+1) % 2 == 0)
                    {
                        rowBG = RowAlternateBackgroundColor;
                    }

                    var boxView = new BoxView
                    {
                        Color = rowBG
                    };

                    rows.Add(boxView);

                    mainCalendar.Children.Add(boxView, 0, r);
                    Grid.SetColumnSpan(boxView, 7);

                    for (int c = 0; c < 7; c++)
                    {
                        buttons.Add(new CalendarButton
                        {
                            Row = r + 1,
                            Column = c + 1,
                            CornerRadius = CornerRadius,
                            BorderWidth = BorderWidth,
                            BorderColor = BorderColor,
                            FontSize = DatesFontSize,
                            BackgroundColor = DatesBackgroundColor,
                            TextColor = DatesTextColor,
                            FontAttributes = DatesFontAttributes,
                            FontFamily = DatesFontFamily,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.FillAndExpand
                        });

                        var b = buttons.Last();

                        if (DatesAlternateBackgroundColor.HasValue && b.Row % 2 == 0)
                        {
                            b.BackgroundColor = DatesAlternateBackgroundColor.Value;
                        }

                        b.Clicked += DateClickedEvent;
                        mainCalendar.Children.Add(b, c, r);
                    }

                }

                MainCalendars.Add(mainCalendar);
            }
        }

        public void ForceRedraw()
        {
            ChangeCalendar(CalendarChanges.All);
        }

        protected void ChangeCalendar(CalendarChanges changes)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                OnStartRenderCalendar?.Invoke(this, EventArgs.Empty);
                Content = null;
                if (changes.HasFlag(CalendarChanges.StartDate))
                {
                    TitleLabel.Text = StartDate.ToString(TitleLabelFormat);
                    if (TitleLabels != null)
                    {
                        var tls = StartDate.AddMonths(1);
                        foreach (var tl in TitleLabels)
                        {
                            (tl as Label).Text = tls.ToString(TitleLabelFormat);
                            tls = tls.AddMonths(1);
                        }
                    }
                }

                var start = CalendarStartDate(StartDate).Date;
                var beginOfMonth = false;
                var endOfMonth = false;
                var hideLastRow = false;
                for (int i = 0; i < buttons.Count; i++)
                {
                    endOfMonth |= beginOfMonth && start.Day == 1;
                    beginOfMonth |= start.Day == 1;

                    if (i < dayLabels.Count && WeekdaysShow && changes.HasFlag(CalendarChanges.StartDay))
                    {
                        if (WeekdaysFormat.ToLower() == "d1")
                        {
                            dayLabels[i].Text = start.ToString("ddd").Substring(0, 1);
                        }
                        else if (WeekdaysFormat.ToLower() == "d2")
                        {
                            dayLabels[i].Text = start.ToString("ddd").Substring(0, 2);
                        }
                        else
                        {
                            dayLabels[i].Text = start.ToString(WeekdaysFormat);
                        }
                    }

                    ChangeWeekNumbers(start, i);

                    if (changes.HasFlag(CalendarChanges.All))
                    {
                        buttons[i].Text = string.Format("{0}", start.Day);
                    }
                    else
                    {
                        buttons[i].TextWithoutMeasure = string.Format("{0}", start.Day);
                    }
                    buttons[i].Date = start;

                    buttons[i].IsOutOfMonth = !(beginOfMonth && !endOfMonth);
                    buttons[i].IsEnabled = ShowNumOfMonths == 1 || !buttons[i].IsOutOfMonth;

                    if (i == 35 && buttons[i].IsOutOfMonth)
                    {
                        // this means that we are on the bottom row and none of the dates 
                        // in it belong to the current month we should hide this row
                        hideLastRow = true;
                        rows.Last().IsVisible = false;
                    }

                    if (i >= 35 && hideLastRow)
                    {
                        buttons[i].IsVisible = false;
                    }
                    else
                    {
                        if (buttons[i].IsOutOfMonth)
                        {
                            buttons[i].IsVisible = ShowOutOfMonthDates;
                        }
                        else
                        {
                            buttons[i].IsVisible = true;
                        }
                        rows.Last().IsVisible = true;
                    }

                    SpecialDate sd = null;
                    if (SpecialDates != null)
                    {
                        sd = SpecialDates.FirstOrDefault(s => s.Date.Date == start.Date);
                    }

                    SetButtonNormal(buttons[i]);

                    if ((MinDate.HasValue && start < MinDate) || (MaxDate.HasValue && start > MaxDate) || (DisableAllDates && sd == null))
                    {
                        SetButtonDisabled(buttons[i]);
                    }
                    else if (buttons[i].IsEnabled && SelectedDates.Select(d => d.Date).Contains(start.Date))
                    {
                        SetButtonSelected(buttons[i], sd);
                    }
                    else if (sd != null)
                    {
                        SetButtonSpecial(buttons[i], sd);
                    }

                    start = start.AddDays(1);
                    if (i != 0 && (i + 1) % 42 == 0)
                    {
                        beginOfMonth = false;
                        endOfMonth = false;
                        start = CalendarStartDate(start);
                    }

                }
                if (DisableDatesLimitToMaxMinRange)
                {
                    TitleLeftArrow.IsEnabled = !(MinDate.HasValue && CalendarStartDate(StartDate).Date < MinDate);
                    TitleRightArrow.IsEnabled = !(MaxDate.HasValue && start > MaxDate);
                }
                Content = MainView;
                OnEndRenderCalendar?.Invoke(this, EventArgs.Empty);
            });
        }

        protected void SetButtonNormal(CalendarButton button)
        {
            button.BackgroundPattern = null;
            button.BackgroundImage = null;

            Device.BeginInvokeOnMainThread(() =>
            {
                button.IsEnabled = true;
                button.IsSelected = false;
                button.FontSize = DatesFontSize;
                button.BorderWidth = BorderWidth;
                button.BorderColor = BorderColor;
                button.CornerRadius = CornerRadius;
                button.FontFamily = button.IsOutOfMonth ? DatesFontFamilyOutsideMonth : DatesFontFamily;
                if (DatesAlternateBackgroundColor.HasValue && button.Row % 2 == 0)
                {
                    button.BackgroundColor = button.IsOutOfMonth ? DatesBackgroundColorOutsideMonth : DatesAlternateBackgroundColor.Value;
                }
                else
                {
                    button.BackgroundColor = button.IsOutOfMonth ? DatesBackgroundColorOutsideMonth : DatesBackgroundColor;
                }
                button.TextColor = button.IsOutOfMonth ? DatesTextColorOutsideMonth : DatesTextColor;
                button.FontAttributes = button.IsOutOfMonth ? DatesFontAttributesOutsideMonth : DatesFontAttributes;
                button.IsEnabled = ShowNumOfMonths == 1 || !button.IsOutOfMonth;
            });
        }

        protected void DateClickedEvent(object s, EventArgs a)
        {
            var selectedDate = (s as CalendarButton).Date;
            if (SelectedDate.HasValue && selectedDate.HasValue && SelectedDate.Value == selectedDate.Value)
            {
                ChangeSelectedDate(selectedDate);
                SelectedDate = null;
            }
            else
            {
                SelectedDate = selectedDate;
            }
        }

        #endregion

        public event EventHandler<DateTimeEventArgs> DateClicked;
    }
}

