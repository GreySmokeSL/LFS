using System;
using System.ComponentModel;
using Domain.Enums;
using System.Windows;
using DevExpress.Mvvm;
using System.IO;
using System.Linq;
using Core.Helper;
using System.Collections.Generic;
using BLL.Helper;
using Domain.Models;

namespace LFSApp.ViewModels
{
    public class ViewDataViewModel : BaseViewModel<ViewModelBehavior>
    {
        public const int PageNumberMinValue = 1;
        public const int PageLineCountDefaultValue = 1000;

        public ViewDataViewModel() : base(ViewModelBehavior.View_Data)
        {
        }

        public override BaseDataModel<ViewModelBehavior> GetDataModel()
        {
            return new ViewDataModel(TargetFile);
        }

        public string PreviewContent
        {
            get { return (string)GetValue(PreviewContentProperty); }
            set { SetValue(PreviewContentProperty, value); }
        }

        public static readonly DependencyProperty PreviewContentProperty =
            DependencyProperty.Register("PreviewContent", typeof(string), typeof(ViewDataViewModel), new PropertyMetadata(default));

        public int PageLineCount
        {
            get { return (int)GetValue(PageLineCountProperty); }
            set { SetValue(PageLineCountProperty, value); }
        }

        public static readonly DependencyProperty PageLineCountProperty =
            DependencyProperty.Register("PageLineCount", typeof(int), typeof(ViewDataViewModel), new UIPropertyMetadata(PageLineCountDefaultValue, PageLineCountChangedCallback));

        private static void PageLineCountChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ViewDataViewModel vdv)
                vdv.UpdatePreviewContent(true);
        }

        public string TargetFile
        {
            get { return (string)GetValue(TargetFileProperty); }
            set { SetValue(TargetFileProperty, value); }
        }

        public static readonly DependencyProperty TargetFileProperty =
            DependencyProperty.Register("TargetFile", typeof(string), typeof(ViewDataViewModel), new UIPropertyMetadata(default, TargetFileChangedCallback));

        private static void TargetFileChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ViewDataViewModel vdv)
                vdv.UpdatePreviewContent(true);
        }

        public int PageNumber
        {
            get { return (int)GetValue(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public static readonly DependencyProperty PageNumberProperty =
            DependencyProperty.Register("PageNumber", typeof(int), typeof(ViewDataViewModel), new UIPropertyMetadata(PageNumberMinValue, PageNumberChangedCallback));

        private static void PageNumberChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ViewDataViewModel vdv)
                vdv.UpdatePreviewContent();
        }

        private bool _updateInProgress = false;
        private void UpdatePreviewContent(bool resetPageNumber = false)
        {
            if(_updateInProgress) return;

            try
            {
                _updateInProgress = true;

                var validationResult = new List<string>();
                if (this.Parent != null)
                    validationResult.AddRange(this.Parent.GetErrors());
                validationResult.AddRange(this.GetDataModel()?.Validate());

                if (!validationResult.IsEmpty())
                    ShowValidationResult(validationResult);
                else
                {
                    if (resetPageNumber)
                        PageNumber = PageNumberMinValue;

                    PreviewContent = File.ReadLines(TargetFile)
                        .Skip(PageLineCount * (PageNumber - 1))
                        .Take(PageLineCount)
                        .Join(Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                PreviewContent = $"Unexpected error on content processing: {ex.Message}, source {TargetFile}";
            }
            finally
            {
                _updateInProgress = false;
            }
        }
    }
}