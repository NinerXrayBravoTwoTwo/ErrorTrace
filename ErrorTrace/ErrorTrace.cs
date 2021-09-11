using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ErrorTrace
{
    /// <summary>
    ///     Error Trace Class
    /// </summary>
    public static class ErrorTrace
    {
        private static string DebuggingErrorFormatRecurse(Exception err, int depth)
        {
            if (err == null || depth >= 20)
                return err == null
                    ? string.Empty
                    : $"'{CountInnerDepth(err.InnerException)}' additional nested inner exceptions are not shown.";

            // Linq array of strings builder
            var q =
                (from s in new[] { $"'{err.GetType()}' Error: {err.Message}" }
                 select s).Union(GetTrace(err));

            if (err.InnerException != null)
                q =
                    q.Union(new[]
                    {$"Inner: {DebuggingErrorFormatRecurse(err.InnerException, depth + 1)}"});

            var result = string.Join(Environment.NewLine, q.ToArray());

            return result;
        }

        /// <summary>
        ///     Recursively concatenate an error and it's inner exception texts to a string.
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public static string DebuggingErrorFormat(Exception err)
        {
            return DebuggingErrorFormatRecurse(err, 0);
        }

        /// <summary>
        ///     Report a stack trace top of stack (most recent) down
        /// </summary>
        /// <param name="e">Any exception</param>
        /// <returns>Strings of “filename(line number)” trace result.</returns>
        private static IEnumerable<string> GetTrace(Exception e)
        {
            if (e == null) return new string[] { };

            var trace = new StackTrace(e, true);

            var result = GetTrace(trace);

            return result;
        }

        /// <summary>
        ///     Report a stack trace top of stack (most recent) down
        /// </summary>
        /// <param name="trace"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTrace(StackTrace trace)
        {
            if (trace == null) return new string[] { };

            // GetFrames returns null instead of an empty array if there are no frames
            // so we must 'fix' the empty case in order to use it in a Linq query.
            var frames = trace.FrameCount > 0 ? trace.GetFrames() : new StackFrame[] { };

            //var result =
            //    from s in
            //        (
            //            from frame in frames
            //            where frame != null
            //            select GetTraceItem(frame)
            //            )
            //    where !string.IsNullOrEmpty(s)
            //    select s;

            return 
                frames
                .Where(f => f != null)
                .Select(GetTraceItem)
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s); ;
            
        }

        /// <summary>
        ///     Format/Report a single frame trace information.
        /// </summary>
        /// <param name="frame">A trace frame from  'StackTrace.GetFrame'</param>
        /// <returns>String of Filename(line number)</returns>
        private static string GetTraceItem(StackFrame frame)
        {
            var result = string.Empty;

            try
            {
                if (frame != null && !string.IsNullOrEmpty(frame.GetFileName()))
                    result = $"{frame.GetFileName()}:{frame.GetFileLineNumber()}:{frame.GetFileColumnNumber()}";
            }
            catch (NullReferenceException) // Catch the last nested error
            {
            }

            return result;
        }

        /// <summary>
        ///     Count the depth of InnerException
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public static int CountInnerDepth(Exception err)
        {
            return err == null
                ? 0
                : 1 + CountInnerDepth(err.InnerException);
        }
    }
}
