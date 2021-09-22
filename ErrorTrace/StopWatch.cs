#region Header

// ReSharper disable once CommentTypo
/*
 // Editors Note:  This is an ancient piece of code, from C and perl in late 80s, C++ in early 90's, VB in late 90s, and then C# 2003.
 // This is its 6th life into C# 6.0 Sept 2021.
 // And it's still a pile :)

< title ></ title >
< revision > $Revision: 1 $</ revision >
< author >$Author: jill $</ author >
< date >$Date: 10 / 11 / 04 11:32a $</ date >
< copyright > Copyright Jill England(c) 2004, All Rights Reserved.</copyright>
 
<summary>
 This is a timer class for capturing elapsed time of queries and web page displays.
</ summary >

< usage >
 C#
 StopWatch sw = new StopWatch();  // Causes timer to be created and started.
 ...
 sw.AddComment("Im in method abc and had an error: " + e.Message);   // Add a comment to a stopwatch.
 ...
 sw.Stop("Timer stopped while destroying xyz");  // Stop a stopwatch.
 ...
 Console.Writeline(sw.ToString() + Environment.NewLin);		// Return a string that reports the start/stop times and all the comments in order of occurrence.
 ...
 string html_text = sw.ToHtml(); // Formats a series of <br> separated lines suitable for html display.
 string xmlText = Simple serialization of this timer.
</usage>

<history>
</history>
*/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
// ReSharper disable All

namespace ErrorTrace

{
    /// <summary>
    ///  StopWatch keeps track of it's start / stop times and a list of comments that may be added to it.
    ///  This results is an array of Comments and a time span.
    /// </summary>
    [Serializable]
    public class StopWatch
    {
        #region Properties
        /// <summary>
        /// An array os string comments added to by this.AddComment.
        /// </summary>
        private readonly List<string> _comments;

        /// <summary>
        /// The time that this timer started.
        /// </summary>
        private readonly DateTime _timeStart;

        /// <summary>
        /// The total elapsed time calculated when this timer was stopped.
        /// </summary>
        private TimeSpan _timeElapsed;

        /// <summary>
        /// A simple way to reference all the comments without using the Collection.
        /// </summary>
        public string[] AllComments
        {
            get
            {
                var strAry = new string[_comments.Count];

                for (var i = 0; i < _comments.Count - 1; i++)
                    strAry[i] = (string)_comments[i];

                return strAry;
            }
        }
        /// <summary>
        /// Total number of comments so far.
        /// </summary>
        public int CountComments => _comments.Count;

        /// <summary>
        /// The very last comment saved to this sw.
        /// </summary>
        public string LastComment => (string)_comments[^1];

        /// <summary>
        /// The TimeSpan since this object was instantiated 
        /// OR the timespan between instantiation and the first call to the Stop Method.
        /// </summary>
        public TimeSpan Value => _timeElapsed.Ticks == 0 ? new TimeSpan(DateTime.Now.Ticks - _timeStart.Ticks) : _timeElapsed;

        /// <summary>
        /// This regular expression finds empty line. Used internally to work with empty comments.
        /// </summary>
        private static readonly Regex IsEmpty = new Regex(@"^\s*$");
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public StopWatch()
        {
            _timeStart = DateTime.Now;
            _timeElapsed = new TimeSpan(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comment"></param>
        public StopWatch(string comment) : this()
        {
            _comments ??= new List<string>();

            this.AddComment(comment);
        }

        /// <summary>
        /// Stops the timer and returns a TimeSpan object. 
        /// You can only stop a timer once further calls to Stop will always return the same TimeSpan.  See the .Value method
        /// </summary>
        /// <param name="comment">Optional comment</param>
        /// <returns></returns>
        public TimeSpan Stop(string comment)
        {
            if (_timeElapsed.Ticks != 0) return _timeElapsed;

            AddComment(comment);
            _timeElapsed = new TimeSpan(DateTime.Now.Ticks - _timeStart.Ticks);

            return _timeElapsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TimeSpan Stop()
        {
            return this.Stop(string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comment"></param>
        public void AddComment(string comment)
        {
            if (IsEmpty.IsMatch(comment) || _timeElapsed.Milliseconds <= 0)
                _comments.Add(comment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(Environment.NewLine);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string ToString(string separator)
        {
            return string.Join(separator, _comments) + $"{separator}{Value.TotalSeconds}";
        }
    }
}
