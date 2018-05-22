using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace ImageRotator
{
    /// <summary>
    /// A base view model that fires Property Changed events as needed
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The event that is fired when any child property changes its value
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        /// <summary>
        /// Call this to fire a <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="name"></param>
        public void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }


        #region Command Helpers

        /// <summary>
        /// Runs a command if the updating flag is not set.
        /// If the flag is true (indicating the function is already running) then the action is not run.
        /// If the flag is false (indicating no running function) then the action is run.
        /// Once the action is finished if it was run, then the flag is reset to false
        /// </summary>
        /// <param name="updatingFlag">The boolean property flag defining if the command is already running</param>
        /// <param name="action">The action to run if the command is not already running</param>
        /// <returns></returns>
        protected async Task RunCommandAsync(Expression<Func<bool>> updatingFlag, Func<Task> action)
        {
            // Check if the flag property is true (meaning the function is already running)
            if (updatingFlag.GetPropertyValue())
                return;

            // Set the property flag to true to indicate we are running
            updatingFlag.SetPropertyValue(true);

            try
            {
                // Run the passed in action
                await action();
            }
            finally
            {
                // Set the property flag back to false now it's finished
                updatingFlag.SetPropertyValue(false);
            }
        }

        #endregion
    }


    /// <summary>
    /// A helper for expressions
    /// </summary>
    public static class ExpressionHelpers
    {
        /// <summary>
        /// Compiles an expression and gets the functions return value
        /// </summary>
        /// <typeparam name="T">The type of return value</typeparam>
        /// <param name="lamba">The expression to compile</param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(this Expression<Func<T>> lamba)
        {
            return lamba.Compile().Invoke();
        }

        /// <summary>
        /// Sets the underlying properties value to the given value
        /// from an expression that contains the property
        /// </summary>
        /// <typeparam name="T">The type of value to set</typeparam>
        /// <param name="lamba">The expression</param>
        /// <param name="value">The value to set the property to</param>
        public static void SetPropertyValue<T>(this Expression<Func<T>> lamba, T value)
        {
            // Converts a lamba () => some.Property, to some.Property
            var expression = (lamba as LambdaExpression).Body as MemberExpression;

            // Get the property information so we can set it
            var propertyInfo = (PropertyInfo)expression.Member;
            var target = Expression.Lambda(expression.Expression).Compile().DynamicInvoke();

            // Set the property value
            propertyInfo.SetValue(target, value);

        }
    }
}
