﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

// author @_wirepair : github.com/wirepair
// date: 04272013 
// copyright: ME AND MINE but i guess you can use it :D.
namespace Decryptor
{
    public static class Decryptor
    {
        #region Decrypting DLL code.
                
        public static IDecryptor InitDecryptDll(string dll)
        {
            if (!File.Exists(dll))
            {
                Console.WriteLine("Unable to find {0}, are you sure the path is correct?", dll);
                return null;
            }
            if (IsManagedDll(dll))
            {
                return LoadManagedDll(dll);
            }
            Console.WriteLine("Attempting to load an unmanaged DLL.");
            return LoadUnmanagedDll(dll);
        }

        private static IDecryptor LoadManagedDll(string dll)
        {
            Assembly assembly = Assembly.LoadFile(dll);
            Console.WriteLine("Assembly for {0} loaded.", dll);         
            try
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (typeof(IDecryptor).IsAssignableFrom(t))
                    {
                        return Activator.CreateInstance(t) as IDecryptor;
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions; 
                    Console.WriteLine("Reflection Type Load Exception");
                    Console.WriteLine(loaderExceptions[0].Message);
                }
            }
            Console.WriteLine("Unable to get Decryptor type!");
            return null;
        }

        private static UnManagedDecryptor LoadUnmanagedDll(string dll)
        {
            UnManagedDecryptor umd = new UnManagedDecryptor();
            umd.LoadUnmanagedDll(dll);
            return umd;
        }

        private static bool IsManagedDll(string dll)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(dll);
                Console.WriteLine("Loading a managed DLL...");
                return true;
            }
            catch (BadImageFormatException)
            { }
            return false;
        }
        
        #endregion
    }
}
