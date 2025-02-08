using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
namespace System.Data
{

    internal static class EntityUtil {

        internal const int AssemblyQualifiedNameIndex = 3;
        internal const int InvariantNameIndex = 2;

        internal const string Parameter = "Parameter";

        internal const CompareOptions StringCompareOptions = CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase;

        internal static bool? ThreeValuedNot(bool? operand) {
            // three-valued logic 'not' (T = true, F = false, U = unknown)
            //      !T = F
            //      !F = T
            //      !U = U
            return operand.HasValue ? !operand.Value : (bool?)null;
        }
        internal static bool? ThreeValuedAnd(bool? left, bool? right) {
            // three-valued logic 'and' (T = true, F = false, U = unknown)
            //
            //      T & T = T
            //      T & F = F
            //      F & F = F
            //      F & T = F
            //      F & U = F
            //      U & F = F
            //      T & U = U
            //      U & T = U
            //      U & U = U
            bool? result;
            if (left.HasValue && right.HasValue) {
                result = left.Value && right.Value;
            }
            else if (!left.HasValue && !right.HasValue) {
                result = null; // unknown
            }
            else if (left.HasValue) {
                result = left.Value ?
                    (bool?)null :// unknown
                    false;
            }
            else {
                result = right.Value ?
                    (bool?)null :
                    false;
            }
            return result;
        }

        internal static bool? ThreeValuedOr(bool? left, bool? right) {
            // three-valued logic 'or' (T = true, F = false, U = unknown)
            //
            //      T | T = T
            //      T | F = T
            //      F | F = F
            //      F | T = T
            //      F | U = U
            //      U | F = U
            //      T | U = T
            //      U | T = T
            //      U | U = U
            bool? result;
            if (left.HasValue && right.HasValue) {
                result = left.Value || right.Value;
            }
            else if (!left.HasValue && !right.HasValue) {
                result = null; // unknown
            }
            else if (left.HasValue) {
                result = left.Value ?
                    true :
                    (bool?)null; // unknown
            }
            else {
                result = right.Value ?
                    true :
                    (bool?)null; // unknown
            }
            return result;
        }

        /// <summary>
        /// Zips two enumerables together (e.g., given {1, 3, 5} and {2, 4, 6} returns {{1, 2}, {3, 4}, {5, 6}})
        /// </summary>
        internal static IEnumerable<KeyValuePair<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
        {
            if (null == first || null == second) { yield break; }
            using (IEnumerator<T1> firstEnumerator = first.GetEnumerator())
            using (IEnumerator<T2> secondEnumerator = second.GetEnumerator())
            {
                while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext())
                {
                    yield return new KeyValuePair<T1, T2>(firstEnumerator.Current, secondEnumerator.Current);
                }
            }
        }

        /// <summary>
        /// Returns true if the type implements ICollection<>
        /// </summary>
        internal static bool IsAnICollection(Type type)
        {
            return typeof(ICollection<>).IsAssignableFrom(type.GetGenericTypeDefinition()) ||
                    type.GetInterface(typeof(ICollection<>).FullName) != null;

        }

        /// <summary>
        /// Given a type that represents a collection, determine if the type implements ICollection&lt&gt, and if
        /// so return the element type of the collection.  Currently, if the collection implements ICollection&lt&gt
        /// multiple times with different types, then we will return false since this is not supported.
        /// </summary>
        /// <param name="collectionType">the collection type to examine</param>
        /// <param name="elementType">the type of element</param>
        /// <returns>true if the collection implement ICollection&lt&gt; false otherwise</returns>
        internal static bool TryGetICollectionElementType(Type collectionType, out Type elementType)
        {
            elementType = null;
            // We have to check if the type actually is the interface, or if it implements the interface:
            try
            {
                Type collectionInterface =
                     (collectionType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(collectionType.GetGenericTypeDefinition())) ?
                     collectionType :
                     collectionType.GetInterface(typeof(ICollection<>).FullName);

                // We need to make sure the type is fully specified otherwise we won't be able to add element to it.
                if (collectionInterface != null && !collectionInterface.ContainsGenericParameters)
                {
                    elementType = collectionInterface.GetGenericArguments()[0];
                    return true;
                }

            }
            catch (AmbiguousMatchException)
            {
                // Thrown if collection type implements ICollection<> more than once
            }
            return false;
        }

        /// <summary>
        /// Helper method to determine the element type of the collection contained by the given property.
        /// If an unambiguous element type cannot be found, then an InvalidOperationException is thrown.
        /// </summary>
        internal static Type GetCollectionElementType(Type propertyType)
        {
            Type elementType;
            if (!EntityUtil.TryGetICollectionElementType(propertyType, out elementType))
            {
                throw EntityUtil.InvalidOperation("unexpcted type converter");
            }
            return elementType;
        }

        /// <summary>
        /// This is used when we need to determine a concrete collection type given some type that may be
        /// abstract or an interface.
        /// </summary>
        /// <remarks>
        /// The rules are:
        /// If the collection is defined as a concrete type with a publicly accessible parameterless constructor, then create an instance of that type
        /// Else, if HashSet<T> can be assigned to the type, then use HashSet<T>
        /// Else, if List<T> can be assigned to the type, then use List<T>
        /// Else, throw a nice exception.
        /// </remarks>
        /// <param name="requestedType">The type of collection that was requested</param>
        /// <returns>The type to instantiate, or null if we cannot find a supported type to instantiate</returns>
        internal static Type DetermineCollectionType(Type requestedType)
        {
            const BindingFlags constructorBinding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance;

            var elementType = EntityUtil.GetCollectionElementType(requestedType);

            if (requestedType.IsArray)
            {
                throw EntityUtil.InvalidOperation("unable to materialize array");
            }

            if (!requestedType.IsAbstract &&
                requestedType.GetConstructor(constructorBinding, null, System.Type.EmptyTypes, null) != null)
            {
                return requestedType;
            }

            var hashSetOfT = typeof(HashSet<>).MakeGenericType(elementType);
            if (requestedType.IsAssignableFrom(hashSetOfT))
            {
                return hashSetOfT;
            }

            var listOfT = typeof(List<>).MakeGenericType(elementType);
            if (requestedType.IsAssignableFrom(listOfT))
            {
                return listOfT;
            }

            return null;
        }

   
        /// <summary>
        /// Provides a standard helper method for quoting identifiers
        /// </summary>
        /// <param name="identifier">Identifier to be quoted. Does not validate that this identifier is valid.</param>
        /// <returns>Quoted string</returns>
        internal static string QuoteIdentifier(string identifier)
        {
            Debug.Assert(identifier != null, "identifier should not be null");
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        // The class contains functions that take the proper informational variables and then construct
        // the appropriate exception with an error string obtained from the resource file.
        // The exception is then returned to the caller, so that the caller may then throw from its
        // location so that the catcher of the exception will have the appropriate call stack.
        // This class is used so that there will be compile time checking of error messages.

        static internal ArgumentException Argument(string error) {
            return new ArgumentException(error);
        }
        static internal ArgumentException Argument(string error, Exception inner) {
            return new ArgumentException(error, inner);
        }
        static internal ArgumentException Argument(string error, string parameter) {
            return new ArgumentException(error, parameter);
        }
        static internal ArgumentException Argument(string error, string parameter, Exception inner) {
            return new ArgumentException(error, parameter, inner);
        }
        static internal ArgumentNullException ArgumentNull(string parameter) {
            return new ArgumentNullException(parameter);
        }
        static internal ArgumentOutOfRangeException ArgumentOutOfRange(string parameterName) {
            return new ArgumentOutOfRangeException(parameterName);
        }
        static internal ArgumentOutOfRangeException ArgumentOutOfRange(string message, string parameterName) {
            return new ArgumentOutOfRangeException(parameterName, message);
        }
    
        static internal ConstraintException Constraint(string message)
        {
            return new ConstraintException(message);
        }
        static internal IndexOutOfRangeException IndexOutOfRange(string error)
        {
            return new IndexOutOfRangeException(error);
        }
        static internal InvalidOperationException InvalidOperation(string error) {
            return new InvalidOperationException(error);
        }
        static internal InvalidOperationException InvalidOperation(string error, Exception inner) {
            return new InvalidOperationException(error, inner);
        }
        static internal ArgumentException InvalidStringArgument(string parameterName) {
            return Argument(InvalidStringArgument(parameterName).Message, parameterName);
        }
      
        static internal NotSupportedException NotSupported()
        {
            return new NotSupportedException();
        }
        static internal NotSupportedException NotSupported(string error) {
            return new NotSupportedException(error);
        }
        static internal ObjectDisposedException ObjectDisposed(string error) {
            return new ObjectDisposedException(null, error);
        }
        static internal ObjectNotFoundException ObjectNotFound(string error) {
            return new ObjectNotFoundException(error);
        }

        // SSDL Generator
        //static internal StrongTypingException StrongTyping(string error, Exception innerException) {
        //    StrongTypingException e = new StrongTypingException(error, innerException);
        //    TraceExceptionAsReturnValue(e);
        //    return e;
        //}
  
   
        #region Internal Errors

        // Internal error code to use with the InternalError exception.
        //
        // error numbers end up being hard coded in test cases; they can be removed, but should not be changed.
        // reusing error numbers is probably OK, but not recommended.
        //
        // The acceptable range for this enum is
        // 1000 - 1999
        //
        // The Range 10,000-15,000 is reserved for tools
        //
        /// You must never renumber these, because we rely upon them when
        /// we get an exception report once we release the bits.
        internal enum InternalErrorCode {
            WrongNumberOfKeys = 1000,
            UnknownColumnMapKind = 1001,
            NestOverNest = 1002,
            ColumnCountMismatch = 1003,

            /// <summary>
            /// Some assertion failed
            /// </summary>
            AssertionFailed = 1004,

            UnknownVar = 1005,
            WrongVarType = 1006,
            ExtentWithoutEntity = 1007,
            UnnestWithoutInput = 1008,
            UnnestMultipleCollections = 1009,
            CodeGen_NoSuchProperty = 1011,
            JoinOverSingleStreamNest = 1012,
            InvalidInternalTree = 1013,
            NameValuePairNext = 1014,
            InvalidParserState1 = 1015,
            InvalidParserState2 = 1016,
            /// <summary>
            /// Thrown when SQL gen produces parameters for anything other than a 
            /// modification command tree.
            /// </summary>
            SqlGenParametersNotPermitted = 1017,
            EntityKeyMissingKeyValue = 1018,
            /// <summary>
            /// Thrown when an invalid data request is presented to a PropagatorResult in
            /// the update pipeline (confusing simple/complex values, missing key values, etc.).
            /// </summary>
            UpdatePipelineResultRequestInvalid = 1019,
            InvalidStateEntry = 1020,
            /// <summary>
            /// Thrown when the update pipeline encounters an invalid PrimitiveTypeKind
            /// during a cast.
            /// </summary>
            InvalidPrimitiveTypeKind = 1021,
            /// <summary>
            /// Thrown when an unknown node type is encountered in ELinq expression translation.
            /// </summary>
            UnknownLinqNodeType = 1023,
            /// <summary>
            /// Thrown by result assembly upon encountering a collection column that does not use any columns
            /// nor has a descriminated nested collection.
            /// </summary>
            CollectionWithNoColumns = 1024,
            /// <summary>
            /// Thrown when a lambda expression argument has an unexpected node type.
            /// </summary>
            UnexpectedLinqLambdaExpressionFormat = 1025,
            /// <summary>
            /// Thrown when a CommandTree is defined on a stored procedure EntityCommand instance.
            /// </summary>
            CommandTreeOnStoredProcedureEntityCommand = 1026,
            /// <summary>
            /// Thrown when an operation in the BoolExpr library is exceeding anticipated complexity.
            /// </summary>
            BoolExprAssert = 1027,
            // AttemptToGenerateDefinitionForFunctionWithoutDef = 1028,
            /// <summary>
            /// Thrown when type A is promotable to type B, but ranking algorithm fails to rank the promotion.
            /// </summary>
            FailedToGeneratePromotionRank = 1029,
        }

        static internal Exception InternalError(InternalErrorCode internalError) {
            return InvalidOperation($"internal error :{internalError}");
        }

        static internal Exception InternalError(InternalErrorCode internalError, int location, object additionalInfo) {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}, {1}", (int)internalError, location);
            if (null != additionalInfo) {
                sb.AppendFormat(", {0}", additionalInfo);
            }
            return InvalidOperation($"internal error :{sb}");
        }

        static internal Exception InternalError(InternalErrorCode internalError, int location) {
            return InternalError(internalError, location, null);
        }

        #endregion

      
        static internal ArgumentOutOfRangeException InvalidParameterDirection(ParameterDirection value)
        {
#if DEBUG
            switch (value)
            {
                case ParameterDirection.Input:
                case ParameterDirection.Output:
                case ParameterDirection.InputOutput:
                case ParameterDirection.ReturnValue:
                    Debug.Assert(false, "valid ParameterDirection " + value.ToString());
                    break;
            }
#endif
            return InvalidEnumerationValue(typeof(ParameterDirection), (int)value);
        }
        static internal ArgumentOutOfRangeException InvalidDataRowVersion(DataRowVersion value)
        {
#if DEBUG
            switch (value)
            {
                case DataRowVersion.Default:
                case DataRowVersion.Current:
                case DataRowVersion.Original:
                case DataRowVersion.Proposed:
                    Debug.Assert(false, "valid DataRowVersion " + value.ToString());
                    break;
            }
#endif

            return InvalidEnumerationValue(typeof(DataRowVersion), (int)value);
        }
        //
        // UpdateException
        //
       
        static private string ConvertCardinalityToString(int? cardinality) {
            string result;
            if (!cardinality.HasValue) { // null indicates * (unlimited)
                result = "*";
            }
            else {
                result = cardinality.Value.ToString(CultureInfo.CurrentCulture);
            }
            return result;
        }
     
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        //
        // Helper Functions
        //
        internal static void ThrowArgumentNullException(string parameterName)
        {
            throw ArgumentNull(parameterName);
        }
        internal static void ThrowArgumentOutOfRangeException(string parameterName)
        {
            throw ArgumentOutOfRange(parameterName);
        }
        internal static T CheckArgumentOutOfRange<T>(T[] values, int index, string parameterName)
        {
            Debug.Assert(null != values, "null values"); // use a different method if values can be null
            if (unchecked((uint)values.Length <= (uint)index))
            {
                ThrowArgumentOutOfRangeException(parameterName);
            }
            return values[index];
        }

        static internal T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                ThrowArgumentNullException(parameterName);
            }
            return value;
        }

        static internal IEnumerable<T> CheckArgumentContainsNull<T>(ref IEnumerable<T> enumerableArgument, string argumentName) where T : class
        {
            GetCheapestSafeEnumerableAsCollection(ref enumerableArgument);
            foreach (T item in enumerableArgument)
            {
                if(item == null)
                {
                    throw EntityUtil.Argument("Argument contains null value");
                }
            }
            return enumerableArgument;
        }

        static internal IEnumerable<T> CheckArgumentEmpty<T>(ref IEnumerable<T> enumerableArgument, Func<string, string> errorMessage, string argumentName)
        {
            int count;
            GetCheapestSafeCountOfEnumerable(ref enumerableArgument, out count);
            if (count <= 0)
            {
                throw EntityUtil.Argument(errorMessage(argumentName));
            }
            return enumerableArgument;
        }

        private static void GetCheapestSafeCountOfEnumerable<T>(ref IEnumerable<T> enumerable, out int count)
        {
            ICollection<T> collection = GetCheapestSafeEnumerableAsCollection(ref enumerable);
            count = collection.Count;
        }

        private static ICollection<T> GetCheapestSafeEnumerableAsCollection<T>(ref IEnumerable<T> enumerable)
        {
            ICollection<T> collection = enumerable as ICollection<T>;
            if (collection != null)
            {
                // cheap way
                return collection;
            }

            // expensive way, but we don't know if the enumeration is rewindable so...
            enumerable = new List<T>(enumerable);
            return enumerable as ICollection<T>;
        }

        static internal T GenericCheckArgumentNull<T>(T value, string parameterName) where T: class
        {
            return CheckArgumentNull(value, parameterName);
        }

        // EntityConnectionStringBuilder
        static internal ArgumentException KeywordNotSupported(string keyword)
        {
            return Argument($"'{keyword}' keyword not supported");
        }

        static internal ArgumentException ADP_KeywordNotSupported(string keyword)
        {
            return Argument($"'{keyword}' keyword not supported");
        }

        // Invalid Enumeration

        static internal ArgumentOutOfRangeException InvalidEnumerationValue(Type type, int value) {
            return EntityUtil.ArgumentOutOfRange($"{(type.Name, value.ToString(System.Globalization.CultureInfo.InvariantCulture))}", type.Name);
        }

        /// <summary>
        /// Given a provider factory, this returns the provider invariant name for the provider. 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static bool TryGetProviderInvariantName(DbProviderFactory providerFactory, out string invariantName)
        {
            Debug.Assert(providerFactory != null);

            var connectionProviderFactoryType = providerFactory.GetType();
            var connectionProviderFactoryAssemblyName = new AssemblyName(
                connectionProviderFactoryType.Assembly.FullName);

            foreach (DataRow row in DbProviderFactories.GetFactoryClasses().Rows)
            {
                var assemblyQualifiedTypeName = (string)row[AssemblyQualifiedNameIndex];

                AssemblyName rowProviderFactoryAssemblyName = null;

                // parse the provider factory assembly qualified type name
                Type.GetType(
                    assemblyQualifiedTypeName,
                    a =>
                    {
                        rowProviderFactoryAssemblyName = a;

                        return null;
                    },
                    (_, __, ___) => null);

                if (rowProviderFactoryAssemblyName != null)
                {
                    if (string.Equals(
                        connectionProviderFactoryAssemblyName.Name,
                        rowProviderFactoryAssemblyName.Name,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var foundFactory = DbProviderFactories.GetFactory(row);

                            if (foundFactory.GetType().Equals(connectionProviderFactoryType))
                            {
                                invariantName = (string)row[InvariantNameIndex];
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail("GetFactory failed with: " + ex);
                            // Ignore bad providers.
                        }
                    }
                }
            }
            invariantName = null;
            return false;
        }

        static internal bool AssemblyNamesMatch(string infoRowProviderAssemblyName, AssemblyName targetAssemblyName)
        {
            if (string.IsNullOrWhiteSpace(infoRowProviderAssemblyName))
            {
                return false;
            }

            AssemblyName assemblyName = null;
            try
            {
                assemblyName = new AssemblyName(infoRowProviderAssemblyName);
            }
            catch (Exception e)
            {
                // Ignore broken provider entries
                if (!IsCatchableExceptionType(e))
                {
                    throw;
                }
                return false;
            }

            Debug.Assert(assemblyName != null, "assemblyName should not be null at this point");

            // Match the provider assembly details
            if (! string.Equals(targetAssemblyName.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (targetAssemblyName.Version == null || assemblyName.Version == null)
            {
                return false;
            }

            if ((targetAssemblyName.Version.Major != assemblyName.Version.Major) || 
                (targetAssemblyName.Version.Minor != assemblyName.Version.Minor))
            {
                return false;
            }

            var targetPublicKeyToken = targetAssemblyName.GetPublicKeyToken();        
            return (targetPublicKeyToken != null)
                && targetPublicKeyToken.SequenceEqual(assemblyName.GetPublicKeyToken());
        }

        // Invalid string argument
        static internal void CheckStringArgument(string value, string parameterName)
        {
            // Throw ArgumentNullException when string is null
            CheckArgumentNull(value, parameterName);

            // Throw ArgumentException when string is empty
            if (value.Length == 0)
            {
                throw InvalidStringArgument(parameterName);
            }
        }

        // only StackOverflowException & ThreadAbortException are sealed classes
        static private readonly Type StackOverflowType   = typeof(System.StackOverflowException);
        static private readonly Type OutOfMemoryType     = typeof(System.OutOfMemoryException);
        static private readonly Type ThreadAbortType     = typeof(System.Threading.ThreadAbortException);
        static private readonly Type NullReferenceType   = typeof(System.NullReferenceException);
        static private readonly Type AccessViolationType = typeof(System.AccessViolationException);
        static private readonly Type SecurityType        = typeof(System.Security.SecurityException);
        
        
        

        static internal bool IsCatchableExceptionType (Exception e) {
            // a 'catchable' exception is defined by what it is not.
            Debug.Assert(e != null, "Unexpected null exception!");
            Type type = e.GetType();

            return ( (type != StackOverflowType) &&
                     (type != OutOfMemoryType)   &&
                     (type != ThreadAbortType)   &&
                     (type != NullReferenceType) &&
                     (type != AccessViolationType) &&
                     !SecurityType.IsAssignableFrom(type));
        }

    

        static internal bool IsNull(object value) {
            if ((null == value) || (DBNull.Value == value)) {
                return true;
            }
            INullable nullable = (value as INullable);
            return ((null != nullable) && nullable.IsNull);
        }

        /// <summary>
        /// Utility method to raise internal error when a throttling constraint is violated during
        /// Boolean expression analysis. An internal exception is thrown including the given message
        /// if the given condition is false. This allows us to give up on an unexpectedly difficult
        /// computation rather than risk hanging the user's machine.
        /// </summary>
        static internal void BoolExprAssert(bool condition, string message)
        {
            if (!condition)
            {
                throw InternalError(InternalErrorCode.BoolExprAssert, 0, message);
            }
        }

        static internal PropertyInfo GetTopProperty(Type t, string propertyName)
        {
            return GetTopProperty(ref t, propertyName);
        }

        /// <summary>
        /// Returns the PropertyInfo and Type where a given property is defined
        /// This is done by traversing the type hierarchy to find the type match.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        static internal PropertyInfo GetTopProperty(ref Type t, string propertyName)
        {
            PropertyInfo propertyInfo = null;
            while (propertyInfo == null && t != null)
            {
                propertyInfo = t.GetProperty(propertyName, BindingFlags.Instance |
                                                           BindingFlags.Public |
                                                           BindingFlags.NonPublic |
                                                           BindingFlags.DeclaredOnly);
                t = t.BaseType;
            }
            t = propertyInfo.DeclaringType;
            return propertyInfo;
        }

        static internal int SrcCompare(string strA, string strB)
        { 
            return ((strA == strB) ? 0 : 1);
        }
        static internal int DstCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, EntityUtil.StringCompareOptions);
        }

        internal static Dictionary<string,string> COMPILER_VERSION = new Dictionary<string, string>() { { "CompilerVersion", "V3.5" } }; //v3.5 required for compiling model files with partial methods.

      
#if false
        public static T FieldCast<T>(object value) {
            try {
                // will result in an InvalidCastException if !(value is T)
                // this pattern also supports handling System.Data.SqlTypes
                return (T)((DBNull.Value == value) ? null : value);
            }
            catch(NullReferenceException) {
                // (value == null) and (T is struct) and (T is not Nullable<>), convert to InvalidCastException
                return (T)(object)System.DBNull.Value;
            }
        }
#endif    
    
        
        /// <summary>
        /// This method uses the .net Fx target framework moniker (introduced in .net 4.0 Multitargeting feature)
        /// to provide a 'quirks' mode that serves as a compatibility flag for features that can be considered
        /// breaking changes from 4.0 to 4.5 which is a in-place upgrade to 4.0.  For details see DevDiv2 bug#488375.
        /// </summary>
        static bool? useFx40CompatMode;
        static public bool UseFx40CompatMode
        {
            get
            {
                if (!useFx40CompatMode.HasValue)
                {
                    string fxname = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;

                    if (string.IsNullOrWhiteSpace(fxname))
                    {
                        Assembly entryAssembly = Assembly.GetEntryAssembly(); // entry assembly can be unmanaged.

                        if (entryAssembly != null)
                        {
                            TargetFrameworkAttribute fxAttrib = entryAssembly.GetCustomAttribute<TargetFrameworkAttribute>();
                            if (fxAttrib != null)
                            {
                                fxname = fxAttrib.FrameworkName;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(fxname))
                    {
                        try
                        {
                            FrameworkName compiledFxName = new FrameworkName(fxname);
                            Version fxv45 = new Version(4, 5);

                            useFx40CompatMode = compiledFxName.Version < fxv45;
                        }
                        catch (System.ArgumentException)
                        {
                        }
                    }

                    if (!useFx40CompatMode.HasValue)
                    {
                        useFx40CompatMode = true;
                    }
                }

                return useFx40CompatMode.Value;
            }
        }

    }
}
