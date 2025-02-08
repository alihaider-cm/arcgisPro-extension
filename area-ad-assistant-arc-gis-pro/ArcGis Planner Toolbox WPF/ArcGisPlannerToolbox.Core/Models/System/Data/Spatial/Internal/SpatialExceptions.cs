//------------------------------------------------------------------------------
// <copyright file="SqlSpatialServices.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner Microsoft
//------------------------------------------------------------------------------

namespace System.Data.Spatial.Internal
{
    using System;
    using System.Data;

    internal static class SpatialExceptions
    {
        internal static ArgumentNullException ArgumentNull(string argumentName)
        {
            // 
            return EntityUtil.ArgumentNull(argumentName);
        }

        internal static Exception ProviderValueNotCompatibleWithSpatialServices()
        {
            // 
            return EntityUtil.Argument("providerValue");
        }

        /// <summary>
        /// Thrown whenever DbGeograpy/DbGeometry.WellKnownValue is set after regular construction (not deserialization instantiation).
        /// </summary>
        /// <returns><see cref="InvalidOperationException"/></returns>
        internal static InvalidOperationException WellKnownValueSerializationPropertyNotDirectlySettable()
        {
            // 
            return EntityUtil.InvalidOperation("");
        }

        #region Geography-specific exceptions

        internal static Exception GeographyValueNotCompatibleWithSpatialServices(string argumentName)
        {
            // 
            return EntityUtil.Argument("", argumentName);
        }

        internal static Exception WellKnownGeographyValueNotValid(string argumentName)
        {
            // 
            return EntityUtil.Argument("", argumentName);
        }

        internal static Exception CouldNotCreateWellKnownGeographyValueNoSrid(string argumentName)
        {
            // 
            return EntityUtil.Argument("", argumentName);
        }

        internal static Exception CouldNotCreateWellKnownGeographyValueNoWkbOrWkt(string argumentName)
        {
            // 
            return EntityUtil.Argument("", argumentName);
        }

        #endregion

        #region Geometry-specific exceptions

        internal static Exception GeometryValueNotCompatibleWithSpatialServices(string argumentName)
        {
            // 
            return EntityUtil.Argument("", argumentName);
        }

        internal static Exception WellKnownGeometryValueNotValid(string argumentName)
        {
            // 
            throw EntityUtil.Argument(nameof(WellKnownGeometryValueNotValid), argumentName);
        }

        internal static Exception CouldNotCreateWellKnownGeometryValueNoSrid(String argumentName)
        {
            // 
            return EntityUtil.Argument(nameof(CouldNotCreateWellKnownGeometryValueNoSrid), argumentName);
        }

        internal static Exception CouldNotCreateWellKnownGeometryValueNoWkbOrWkt(String argumentName)
        {
            // 
            return EntityUtil.Argument(nameof(CouldNotCreateWellKnownGeometryValueNoWkbOrWkt), argumentName);
        }
               
        #endregion

        #region SqlSpatialServices-specific Exceptions

        internal static Exception SqlSpatialServices_ProviderValueNotSqlType(Type requiredType)
        {
            return EntityUtil.Argument(nameof(SqlSpatialServices_ProviderValueNotSqlType), "providerValue");
        }
                
        #endregion
    }
}
