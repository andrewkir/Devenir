using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

namespace DevenirProject
{
    class InnerParsingResultsFragment : Android.Support.V4.App.Fragment
    {
        string title, text, latex;
        public InnerParsingResultsFragment(string title, string text, string latex)
        {
            this.title = title;
            this.text = text == null ? "" : text;
            this.latex = latex == null ? "" : latex;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.pager_parsingresults, container, false);
            view.FindViewById<TextView>(Resource.Id.titleTextView).Text = title;

            TextInputLayout textField = view.FindViewById<TextInputLayout>(Resource.Id.textTextField);
            textField.EditText.Text = text;

            TextInputLayout latexField = view.FindViewById<TextInputLayout>(Resource.Id.latexTextField);
            latexField.EditText.Text = latex;

            view.FindViewById<ImageButton>(Resource.Id.textCopyContent).Click += delegate
            {
                if (text != "")
                {
                    ClipboardManager clipboard = (ClipboardManager)view.Context.GetSystemService(Context.ClipboardService);
                    ClipData clip = ClipData.NewPlainText("text", text);
                    clipboard.PrimaryClip = clip;
                    Toast.MakeText(view.Context, "Текст скопирован в буфер обмена", ToastLength.Short).Show();
                }
            };
            view.FindViewById<ImageButton>(Resource.Id.textSendContent).Click += delegate
            {
                if (text != "")
                {
                    Intent shareIntent = new Intent();
                    shareIntent.SetAction(Intent.ActionSend);
                    shareIntent.PutExtra(Intent.ExtraSubject, "Devenir parsing result");
                    shareIntent.PutExtra(Intent.ExtraText, text);
                    shareIntent.SetType("text/plain");
                    StartActivity(shareIntent);
                }
            };
            
            view.FindViewById<ImageButton>(Resource.Id.latexCopyContent).Click += delegate
            {
                if (latex != "")
                {
                    ClipboardManager clipboard = (ClipboardManager)view.Context.GetSystemService(Context.ClipboardService);
                    ClipData clip = ClipData.NewPlainText("LaTeX", latex);
                    clipboard.PrimaryClip = clip;
                    Toast.MakeText(view.Context, "Текст скопирован в буфер обмена", ToastLength.Short).Show();
                }
            };
            view.FindViewById<ImageButton>(Resource.Id.latexSendContent).Click += delegate
            {
                if (latex != "")
                {
                    Intent shareIntent = new Intent();
                    shareIntent.SetAction(Intent.ActionSend);
                    shareIntent.PutExtra(Intent.ExtraSubject, "Devenir parsing result");
                    shareIntent.PutExtra(Intent.ExtraText, text);
                    shareIntent.SetType("text/plain");
                    StartActivity(shareIntent);
                }
            };

            return view;
        }
    }
}