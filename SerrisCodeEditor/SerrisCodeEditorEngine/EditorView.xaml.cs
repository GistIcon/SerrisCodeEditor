﻿using Microsoft.Toolkit.Uwp.Helpers;
using SerrisCodeEditorEngine.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SerrisCodeEditorEngine
{
    public sealed partial class EditorView : UserControl
    {
        private WebView editor_view;

        public EditorView()
        {
            InitializeComponent();
        }




        /*======================
         ------PARAMETERS-------
        =======================*/




        public string Code
        {
            set
            {
                IsLoading(true);

                if(Initialized)
                {
                    SetCode(value);
                    Languages.GetActualLanguage(CodeLanguage.ToLower(), editor_view);
                }
                else
                {
                    EditorLoaded += (e, f) =>
                    {
                        SetCode(value);
                        Languages.GetActualLanguage(CodeLanguage.ToLower(), editor_view);
                    };
                }

            }
        }
        public static readonly DependencyProperty CodeProperty = DependencyProperty.Register("Code", typeof(string), typeof(EditorView), null);



        public string MonacoModelID
        {
            get { return (string)GetValue(MonacoModelIDProperty); }
            set { SetValue(MonacoModelIDProperty, value); }
        }

        public static readonly DependencyProperty MonacoModelIDProperty = DependencyProperty.Register("MonacoModelID", typeof(string), typeof(EditorView), null);

        public int CursorPositionRow
        {
            get { return (int)GetValue(CursorPositionRowProperty); }
            set { SetValue(CursorPositionRowProperty, value); }
        }
        public static readonly DependencyProperty CursorPositionRowProperty = DependencyProperty.Register("CursorPositionRow", typeof(int), typeof(EditorView), null);

        public int CursorPositionColumn
        {
            get { return (int)GetValue(CursorPositionColumnProperty); }
            set { SetValue(CursorPositionColumnProperty, value); }
        }
        public static readonly DependencyProperty CursorPositionColumnProperty = DependencyProperty.Register("CursorPositionColumn", typeof(int), typeof(EditorView), null);

        public string CodeLanguage
        {
            get { return (string)GetValue(CodeLanguageProperty); }
            set { SetValue(CodeLanguageProperty, value); }
        }
        public static readonly DependencyProperty CodeLanguageProperty = DependencyProperty.Register("CodeLanguage", typeof(string), typeof(EditorView), null);

        public Brush BackgroundWait
        {
            get { return (Brush)GetValue(BackgroundWaitProperty); }
            set { SetValue(BackgroundWaitProperty, value); }
        }
        public static readonly DependencyProperty BackgroundWaitProperty = DependencyProperty.Register("BackgroundWait", typeof(Brush), typeof(EditorView), null);

        public Brush ForegroundWait
        {
            get { return (Brush)GetValue(ForegroundWaitProperty); }
            set { SetValue(ForegroundWaitProperty, value); }
        }
        public static readonly DependencyProperty ForegroundWaitProperty = DependencyProperty.Register("ForegroundWait", typeof(Brush), typeof(EditorView), null);

        public bool isReadOnly
        {
            get { return (bool)GetValue(isReadOnlyProperty); }
            set { SetValue(isReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty isReadOnlyProperty = DependencyProperty.Register("isReadOnly", typeof(bool), typeof(EditorView), null);

        private bool Initialized = false, isWindowsPhone = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");




        /*=============================
         ------PRIVATE FUNCTIONS-------
        ===============================*/




        //Convert C# String to JS String (HttpUtility doesn't exist in WinRT) - this function has been fund here: http://joonhachu.blogspot.fr/2010/01/c-javascript-encoder.html
        private static string JavaScriptEncode(string s)
        {
            if (s == null || s.Length == 0)
            {
                return string.Empty;
            }
            char c;
            int i;
            int len = s.Length;
            var sb = new StringBuilder(len + 4);


            for (i = 0; i < len; ++i)
            {
                c = s[i];
                switch (c)
                {
                    case '\\':
                    case '"':
                    case '>':
                    case '\'':
                        sb.Append('\\');
                        sb.Append(c);
                        break;

                    case '\b':
                        sb.Append("\\b");
                        break;

                    case '\t':
                        sb.Append("\\t");
                        break;

                    case '\n':
                        sb.Append("\\n");
                        break;

                    case '\f':
                        sb.Append("\\f");
                        break;

                    case '\r':
                        sb.Append("\\r");
                        break;

                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }


        private void InitializeEditor()
        {
            editor_view.Navigate(new Uri("ms-appx-web:///SerrisCodeEditorEngine/Pages/editor.html"));
            editor_view.LoadCompleted += (a, b) => { };
        }

        private void IsLoading(bool enabled)
        {
            if (enabled)
            {
                LoadingScreen.Visibility = Visibility.Visible;
                loading_ring.IsActive = true;
            }
            else
            {
                LoadingScreen.Visibility = Visibility.Collapsed;
                loading_ring.IsActive = false;
            }
        }

        private async void SetCode(string code)
        {
            if (Initialized)
            {
                //await editor_view.InvokeScriptAsync("eval", new string[] { @"editor.setValue('" + JavaScriptEncode(code) + "');" });
                await editor_view.InvokeScriptAsync("eval", new string[] { @"var createNewModel = true; if(selectedID.length != 0) { modelsList[selectedIDIndex].model = editor.getModel(); modelsList[selectedIDIndex].state = editor.saveViewState(); } for(var i = 0; i <= (modelsList.length - 1); i++) { if(modelsList[i].id.includes('" + MonacoModelID + "')) { editor.setModel(modelsList[i].model); editor.restoreViewState(modelsList[i].state); createNewModel = false; selectedID = '" + MonacoModelID + "'; selectedIDIndex = i; editor.focus(); break; } } if(createNewModel) { modelsList.push({ id: '" + MonacoModelID + "', model: monaco.editor.createModel('" + JavaScriptEncode(code) + "'), state: null }); editor.setModel(modelsList[modelsList.length - 1].model); selectedID = '" + MonacoModelID + "'; selectedIDIndex =  modelsList.length - 1; /*sceelibs.consoleManager.log('New model: " + MonacoModelID + "');*/ editor.focus(); }" });

                if (isReadOnly)
                {
                    string[] set_read = { @"editor.updateOptions({ readOnly: true});" };
                    await editor_view.InvokeScriptAsync("eval", set_read);
                }
                else
                {
                    string[] set_read = { @"editor.updateOptions({ readOnly: false});" };
                    await editor_view.InvokeScriptAsync("eval", set_read);
                }

                SetCursorPosition(new PositionSCEE { column = CursorPositionColumn, row = CursorPositionRow });

                /*if (!isWindowsPhone)
                {
                    string[] desktop_string = { @"editor.setOptions({ enableBasicAutocompletion: true, enableLiveAutocompletion: true, enableSnippets: true }); document.getElementById('editor').style.fontSize = '14px'; document.getElementById('tab-button').style.display = 'none';" };
                    await editor_view.InvokeScriptAsync("eval", desktop_string);
                }
                else
                {
                    string[] mobile_string = { @"document.getElementById('editor').style.fontSize = '18px'; document.getElementById('tab-button').style.display = 'block';" };
                    await editor_view.InvokeScriptAsync("eval", mobile_string);
                }*/

                IsLoading(false);
            }
        }

        public async Task<string> GetCode()
        {
            if (Initialized)
            {
                return await editor_view.InvokeScriptAsync("eval", new string[] { @"editor.getValue()" });
            }
            else
            {
                return "";
            }
        }

        private void Editor_view_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            editor_view.AddWebAllowedObject("sceelibs", new SCEELibs.SCEELibs());
        }

        private void editor_view_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e) { InitializeEditor(); }




        /*=====================
         ------FUNCTIONS-------
        =======================*/




        ///<summary>
        ///Enable or not auto completation in the editor - ex: component_name.EnableAutoCompletation(true);
        ///</summary>
        public async void EnableAutoCompletation(bool enable)
        {
            if (Initialized)
            {
                if (enable)
                {
                    string[] set_code = { "editor.setOptions({ enableBasicAutocompletion: true, enableLiveAutocompletion: true, enableSnippets: true });" };
                    await editor_view.InvokeScriptAsync("eval", set_code);
                }
                else
                {
                    string[] set_code = { "editor.setOptions({ enableBasicAutocompletion: false, enableLiveAutocompletion: false, enableSnippets: false });" };
                    await editor_view.InvokeScriptAsync("eval", set_code);
                }
            }
        }

        ///<summary>
        ///Force to update the language in the editor - ex: component_name.ForceUpdateLanguage();
        ///</summary>
        public void ForceUpdateLanguage()
        {
            if (Initialized)
            {
                Languages.GetActualLanguage(CodeLanguage.ToLower(), editor_view);
            }
        }

        ///<summary>
        ///Set the code on the document, but he didn't clear undo/redo history - ex: component_name.SetCodeButNoClearHistory("Toothless say hi !");
        ///</summary>
        public async void SetCodeButNoClearHistory(string setcode)
        {
            if (Initialized)
            {
                string[] set_code = { $"editor.setValue('{JavaScriptEncode(setcode)}', -1)" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Insert code at the cursor - ex: component_name.InsertCodeAtCursor("Toothless say hi !");
        ///</summary>
        public async void InsertCodeAtCursor(string setcode)
        {
            if (Initialized)
            {
                string[] set_code = { $"editor.insert('{JavaScriptEncode(setcode)}')" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Get the lines count of the document - ex: int test = await component_name.GetLinesCount();
        ///</summary>
        public async Task<int> GetLinesCount()
        {
            if (Initialized)
            {
                string[] set_code = { " '' + editor.getModel().getLineCount();" };
                return int.Parse(await editor_view.InvokeScriptAsync("eval", set_code));
            }
            return 0;
        }

        ///<summary>
        ///Set the cursor position to the last line on the document - ex: component_name.GoToLastLine();
        ///</summary>
        public async void GoToLastLine()
        {
            if (Initialized)
            {
                string[] set_code = { "editor.setPosition(new monaco.Position(editor.getModel().getLineCount(), 1));" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Clear the editor and the undo/redo history of the document - ex: component_name.ClearEditor();
        ///</summary>
        public async void ClearEditor()
        {
            if (Initialized)
            {
                string[] set_code = { "editor.setValue('');" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Get text range of the document - ex: string test = await component_name.GetTextRange();
        ///</summary>
        public async Task<string> GetTextRange(RangeSCEE range)
        {
            if (Initialized)
            {
                string[] set_code = { $"var Range = require('ace/range').Range; editor.getTextRange(new Range({range.from_column}, {range.from_row}, {range.to_column}, {range.to_row}))" };
                return await editor_view.InvokeScriptAsync("eval", set_code);
            }
            return null;
        }

        ///<summary>
        ///Replace text range on the document - ex: component_name.ReplaceTextRange();
        ///</summary>
        public async void ReplaceTextRange(RangeSCEE range, string replace)
        {
            if (Initialized)
            {
                string[] set_code = { $"var Range = require('ace/range').Range; editor.replace(new Range({range.from_column}, {range.from_row}, {range.to_column}, {range.to_row}), '{JavaScriptEncode(replace)}')" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Get selected text of the document - ex: string test = await component_name.GetSelectedText();
        ///</summary>
        public async Task<string> GetSelectedText()
        {
            if (Initialized)
            {
                string[] set_code = { "editor.getSelectedText()" };
                return await editor_view.InvokeScriptAsync("eval", set_code);
            }
            return null;
        }

        ///<summary>
        ///Replace actual selected text of the document - ex: component_name.ReplaceTextSelection("toothless have replaced this selection !");
        ///</summary>
        public async void ReplaceTextSelection(string replacement)
        {
            if (Initialized)
            {
                string[] set_code = { $"editor.session.replace(editor.selection.getRange(), '{JavaScriptEncode(replacement)}')" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Get the cursor position of the document - ex: PositionSCEE pos = await component_name.GetCursorPosition();
        ///</summary>
        public async Task<PositionSCEE> GetCursorPosition()
        {
            if (Initialized)
            {
                string[] row = { "'' + editor.getPosition().lineNumber" };
                string[] column = { "'' + editor.getPosition().column" };
                return new PositionSCEE
                {
                    row = int.Parse(await editor_view.InvokeScriptAsync("eval", row)),
                    column = int.Parse(await editor_view.InvokeScriptAsync("eval", column))
                };
            }
            return new PositionSCEE();
        }

        ///<summary>
        ///Set the cursor position on the document - ex: component_name.SetCursorPosition(new PositionSCEE {row = 1, column = 1});
        ///</summary>
        public async void SetCursorPosition(PositionSCEE pos)
        {
            if (Initialized)
            {
                string[] set_code = { $"editor.setPosition(new monaco.Position({pos.row},{pos.column})); editor.revealLineInCenter(editor.getPosition().lineNumber);" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Undo an action of the document - ex: component_name.UndoAction();
        ///</summary>
        public async void UndoAction()
        {
            if (Initialized)
            {
                string[] set_code = { "editor.trigger('editor', 'undo');" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Redo an action of the document - ex: component_name.RedoAction();
        ///</summary>
        public async void RedoAction()
        {
            if (Initialized)
            {
                string[] set_code = { "editor.trigger('editor', 'redo');" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Clear the undo/redo history of the document - ex: component_name.ClearUndoRedoHistory();
        ///</summary>
        public async void ClearUndoRedoHistory()
        {
            if (Initialized)
            {
                string[] set_code = { "editor.session.getUndoManager().reset()" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Find and select string in actual document - ex: component_name.FindText("i like trains");
        ///</summary>
        public async void FindText(string find)
        {
            if (Initialized)
            {
                string[] set_code = { $"editor.find('{JavaScriptEncode(find)}'); $('html, body').animate({{ scrollTop: $('.ace_text-input').offset().top }}, 500);" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Find and replace string in actual document - ex: component_name.FindAndReplaceText("i like trains", "i like french fries");
        ///</summary>
        public async void FindAndReplaceText(string find, string replace)
        {
            if (Initialized)
            {
                string[] set_code = { $"editor.replace('{JavaScriptEncode(find)}', '{JavaScriptEncode(replace)}'); $('html, body').animate({{ scrollTop: $('.ace_text-input').offset().top }}, 500);" };
                await editor_view.InvokeScriptAsync("eval", set_code);
            }
        }

        ///<summary>
        ///Send javascript command to the editor - ex: component_name.SendAndExecuteJavaScript("");
        ///</summary>
        public async void SendAndExecuteJavaScript(string command)
        {
            if (Initialized)
            {
                try
                {
                    string[] js_command = { command };
                    await editor_view.InvokeScriptAsync("eval", js_command);
                }
                catch { }
            }
        }

        ///<summary>
        ///Send javascript command to the editor and get result - ex: component_name.SendAndExecuteJavaScriptWithReturn("");
        ///</summary>
        public IAsyncOperation<string> SendAndExecuteJavaScriptWithReturn(string command)
        {
            if (Initialized)
            {
                try
                {
                    string[] js_command = { command };
                    return editor_view.InvokeScriptAsync("eval", js_command);
                }
                catch { return null; }
            }
            else
            {
                return null;
            }
        }




        /*==================
         ------EVENTS-------
        ====================*/




        public event EventHandler EditorTextChanged, EditorLoaded;
        public event EventHandler<EventSCEE> EditorCommands, EditorTextShortcutTabs;

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            editor_view = new WebView(WebViewExecutionMode.SeparateThread);
            editor_view.NavigationFailed += editor_view_NavigationFailed;
            editor_view.ScriptNotify += editor_view_ScriptNotify;
            editor_view.NavigationStarting += Editor_view_NavigationStarting;
            editor_view.SizeChanged += EditorView_SizeChanged;
            editor_view.LostFocus += Editor_view_LostFocus;
            InputPane.GetForCurrentView().Hiding += EditorView_Hiding;

            editor_view.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Visible);

            MasterGrid.Children.Insert(0, editor_view);

            InitializeEditor();
        }

        private async void EditorView_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (Initialized)
            {
                await editor_view.InvokeScriptAsync("eval", new string[] { @"document.activeElement.blur();" });
            }
        }

        private async void Editor_view_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Initialized && isWindowsPhone)
            {
                await editor_view.InvokeScriptAsync("eval", new string[] { @"document.activeElement.blur();" });
            }

        }

        private async void EditorView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Initialized)
            {
                await editor_view.InvokeScriptAsync("eval", new string[] { @"editor.layout();" });
            }
        }

        private void editor_view_ScriptNotify(object sender, NotifyEventArgs e)
        {
            if (e.Value.Contains("command://"))
            {
                EditorCommands?.Invoke(this, new EventSCEE { message = e.Value });
            }

            if (e.Value.Contains("console://"))
            {
                Debug.WriteLine(e.Value.Replace("console://", ""));
            }

            switch (e.Value)
            {
                case "click":
                    EditorCommands?.Invoke(this, new EventSCEE { message = "click" });
                    break;

                case "loaded":
                    Initialized = true;
                    EditorLoaded?.Invoke(this, new EventArgs());
                    break;

                case "change":
                    EditorCommands?.Invoke(this, new EventSCEE { message = "change" });
                    break;

            }

            if (e.Value.Contains("tab_select:///"))
            {
                EditorTextShortcutTabs?.Invoke(this, new EventSCEE { message = e.Value.Replace("tab_select:///", "") });
            }
        }

    }

    public static class AsyncHelpers
    {
        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSync(Func<Task> task)
        {
            SynchronizationContext oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            SynchronizationContext oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            var ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }

}
