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
    class ParsingDetailsResultsFragment : Android.Support.V4.App.Fragment
    {
        string title, text, latex;
        public ParsingDetailsResultsFragment(string title, string text, string latex)
        {
            this.title = title;
            this.text = text == null ? "" : text;
            this.latex = latex == null ? "" : latex;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.pager_parsingresults, container, false);
            view.FindViewById<TextView>(Resource.Id.titleTextView).Text = title;

            TextInputEditText textField = view.FindViewById<TextInputEditText>(Resource.Id.textTextField);
            textField.Text = text;
            textField.SetOnTouchListener(new EditTextTochListener(textField));

            TextInputEditText latexField = view.FindViewById<TextInputEditText>(Resource.Id.latexTextField);
            latexField.Text = latex;
            latexField.SetOnTouchListener(new EditTextTochListener(latexField));

            view.FindViewById<ImageButton>(Resource.Id.textCopyContent).Click += delegate
            {
                if (textField.Text != "")
                {
                    ClipboardManager clipboard = (ClipboardManager)view.Context.GetSystemService(Context.ClipboardService);
                    ClipData clip = ClipData.NewPlainText("text", textField.Text);
                    clipboard.PrimaryClip = clip;
                    Toast.MakeText(view.Context, "Текст скопирован в буфер обмена", ToastLength.Short).Show();
                }
            };
            view.FindViewById<ImageButton>(Resource.Id.textSendContent).Click += delegate
            {
                if (textField.Text != "")
                {
                    Intent shareIntent = new Intent();
                    shareIntent.SetAction(Intent.ActionSend);
                    shareIntent.PutExtra(Intent.ExtraSubject, "Devenir parsing result");
                    shareIntent.PutExtra(Intent.ExtraText, textField.Text);
                    shareIntent.SetType("text/plain");
                    StartActivity(shareIntent);
                }
            };

            view.FindViewById<ImageButton>(Resource.Id.latexCopyContent).Click += delegate
            {
                if (latexField.Text != "")
                {
                    ClipboardManager clipboard = (ClipboardManager)view.Context.GetSystemService(Context.ClipboardService);
                    ClipData clip = ClipData.NewPlainText("LaTeX", latexField.Text);
                    clipboard.PrimaryClip = clip;
                    Toast.MakeText(view.Context, "Текст скопирован в буфер обмена", ToastLength.Short).Show();
                }
            };
            view.FindViewById<ImageButton>(Resource.Id.latexSendContent).Click += delegate
            {
                if (latexField.Text != "")
                {
                    Intent shareIntent = new Intent();
                    shareIntent.SetAction(Intent.ActionSend);
                    shareIntent.PutExtra(Intent.ExtraSubject, "Devenir parsing result");
                    shareIntent.PutExtra(Intent.ExtraText, latexField.Text);
                    shareIntent.SetType("text/plain");
                    StartActivity(shareIntent);
                }
            };



            return view;
        }

        public class EditTextTochListener : Java.Lang.Object, View.IOnTouchListener
        {
            TextInputEditText editText;
            public EditTextTochListener(TextInputEditText editText)
            {
                this.editText = editText;
            }
            public bool OnTouch(View v, MotionEvent e)
            {
                if (editText.HasFocus)
                {
                    v.Parent.RequestDisallowInterceptTouchEvent(true);
                    switch (e.Action & MotionEventActions.Mask)
                    {
                        case MotionEventActions.Scroll:
                            v.Parent.RequestDisallowInterceptTouchEvent(false);
                            return true;
                    }
                }
                return false;
            }

        }

    }
}