﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNetRu.Auditor.Data;
using DotNetRu.Auditor.Data.Xml;
using Xunit;

namespace DotNetRu.Auditor.UnitTests.Data.Xml
{
    public sealed class XmlDataSerializerFactoryTest
    {
        [Theory]
        [MemberData(nameof(ModelTypesAsGenericArgument))]
        [SuppressMessage("ReSharper", "xUnit1026")]
        public void ShouldKnowAboutTheWholeModel<T>(T _)
            where T : IDocument
        {
            // Act
            var builder = XmlDocumentSerializerFactory.CreateModelBuilder<T>();

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.Build());
        }

        [Fact]
        public void ShouldHaveModelTypes()
        {
            Assert.NotEmpty(ModelTypes);
        }

        public static TheoryData<object> ModelTypesAsGenericArgument => ModelTypes
            .Select(Activator.CreateInstance)
            .WhereNotNull()
            .ToTheoryData();

        public static IReadOnlyList<Type> ModelTypes => typeof(XmlDocumentSerializerFactory)
            .Assembly
            .ExportedTypes
            .Where(type => type.IsPublic)
            .Where(type => type.GetInterfaces().Contains(typeof(IDocument)))
            .ToList();
    }
}
