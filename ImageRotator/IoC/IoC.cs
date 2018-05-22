using Ninject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ImageRotator.Core;
using System.Threading.Tasks;

namespace ImageRotator.Core
{
    class IoC
    {
        #region Public Properties

#pragma warning disable CS0618 // Type or member is obsolete
                              /// <summary>
                              /// The kernel for our IoC container
                              /// </summary>
        public static IKernel Kernel { get; private set; } = new StandardKernel();
#pragma warning restore CS0618 // Type or member is obsolete


        #endregion

        #region Construction

        /// <summary>
        /// Sets up the IoC container, binds all information required and is ready for use
        /// NOTE: Must be called as soon as your application starts up to ensure all 
        ///       services can be found
        /// </summary>
        public static void Setup()
        {
            // Bind all required view models
            BindViewModels();

        

        }


        /// <summary>
        /// Binds all singleton view models
        /// </summary>
        private static void BindViewModels()
        {
            // Bind to a single instance of Application view model
          
        }

        #endregion

        /// <summary>
        /// Get's a service from the IoC, of the specified type
        /// </summary>
        /// <typeparam name="T">The type to get</typeparam>
        /// <returns></returns>
        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }
    }
}


