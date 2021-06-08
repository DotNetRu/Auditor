using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace DotNetRu.Auditor.Data.Xml
{
    internal sealed class XmlModelBuilder<T>
    {
        private readonly XmlAttributeOverrides overrides;
        private readonly HashSet<string> ignoredMembers;

        private XmlModelBuilder(XmlAttributeOverrides attributeOverrides, IEnumerable<string> ignoredMembersByDefault)
        {
            overrides = attributeOverrides;
            ignoredMembers = new HashSet<string>(ignoredMembersByDefault);
        }

        public static XmlModelBuilder<T> Map(string typeName)
        {
            return Create<T>()
                .WithRoot(typeName);
        }

        private static XmlModelBuilder<TSub> Create<TSub>(XmlAttributeOverrides? overrides = null)
        {
            overrides ??= new XmlAttributeOverrides();
            var ignoredMembersByDefault = GetSerializableMembers(typeof(T))
                .Select(member => member.Name);

            return new XmlModelBuilder<TSub>(overrides, ignoredMembersByDefault);
        }

        public XmlModelBuilder<T> Property<TValue>(Expression<Func<T, TValue>> propertyExpression, string propertyName)
        {
            var memberAttributes = new XmlAttributes
            {
                XmlElements = { new XmlElementAttribute(propertyName) }
            };

            var originName = GetMemberName(propertyExpression);

            RegisterMember(originName, memberAttributes);
            return this;
        }

        public XmlModelBuilder<T> Collection<TValue>(Expression<Func<T, TValue>> propertyExpression, string collectionName, string itemName)
        {
            var memberAttributes = new XmlAttributes
            {
                XmlArray = new XmlArrayAttribute(collectionName),
                XmlArrayItems = { new XmlArrayItemAttribute(itemName) }
            };

            var memberName = GetMemberName(propertyExpression);

            RegisterMember(memberName, memberAttributes);
            return this;
        }

        public XmlModelBuilder<T> Collection<TSub>(
            Expression<Func<T, List<TSub>>> propertyExpression,
            string collectionName,
            string itemName,
            Action<XmlModelBuilder<TSub>> configure)
        {
            Collection(propertyExpression, collectionName, itemName);

            var subBuilder = Create<TSub>(overrides);
            configure(subBuilder);
            subBuilder.Build();
            return this;
        }

        public XmlAttributeOverrides Build()
        {
            foreach (var memberName in ignoredMembers)
            {
                Ignore(memberName);
            }

            return overrides;
        }

        private XmlModelBuilder<T> WithRoot(string rootName)
        {
            var rootAttribute = new XmlAttributes
            {
                XmlType = new XmlTypeAttribute(rootName)
            };

            overrides.Add(typeof(T), rootAttribute);
            return this;
        }

        private void Ignore(string memberName)
        {
            var memberAttributes = new XmlAttributes
            {
                XmlIgnore = true
            };

            memberAttributes.XmlElements.Add(new XmlElementAttribute(memberName));

            RegisterMember(memberName, memberAttributes);
        }

        private void RegisterMember(string name, XmlAttributes attributes)
        {
            overrides.Add(typeof(T), name, attributes);
            ignoredMembers.Remove(name);
        }

        private static IReadOnlyList<PropertyInfo> GetSerializableMembers(Type type)
            => type.GetProperties();

        private static string GetMemberName<TSource, TValue>(Expression<Func<TSource, TValue>> expression) =>
            expression.Body is MemberExpression member
                ? member.Member.Name
                : throw new ArgumentException("Expression is not a member access", nameof(expression));
    }
}
