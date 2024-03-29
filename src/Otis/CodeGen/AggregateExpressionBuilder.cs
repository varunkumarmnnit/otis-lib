using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using Otis.Functions;
using Otis.Parsing;

namespace Otis.CodeGen
{
	class AggregateExpressionBuilder
	{
		private List<CodeStatement> m_statements = null;
		private List<AggregateFunctionContext> m_contexts = null;

		public AggregateExpressionBuilder(ClassMappingDescriptor descriptor, ICollection<MemberMappingDescriptor> members, FunctionMap functionMap)
		{
			m_contexts = new List<AggregateFunctionContext>(members.Count);

			foreach (MemberMappingDescriptor member in members)
			{
				m_contexts.Add(CreateMemberContext(descriptor, member, functionMap));
			}
		}

		public CodeStatement[] GetStatements()
		{
			if (m_statements == null)
				Generate();
			
			return m_statements.ToArray();
		}

		private void Generate()
		{
			m_statements = new List<CodeStatement>(50);
			if(m_contexts.Count < 1)
				return;

			foreach (AggregateFunctionContext context in m_contexts)
			{
				m_statements.AddRange(context.Generator.GetInitializationStatements(context));		
			}

			m_statements.AddRange(GetPathTraversalStatements());

			foreach (AggregateFunctionContext context in m_contexts)
			{
				m_statements.Add(context.Generator.GetAssignmentStatement(context));
			}
		}

		private IEnumerable<CodeStatement> GetPathTraversalStatements()
		{
			string exp = "";
			IList<AggregateExpressionPathItem> pathItems
				= ExpressionParser.BuildAggregatePathItem(m_contexts[0].Descriptor, m_contexts[0].Member);

			foreach (AggregateExpressionPathItem pathItem in pathItems)
			{
				if (pathItem.IsCollection)
				{
					exp += string.Format("foreach({0} {1} in {2}.{3})",
						TypeHelper.GetTypeDefinition(pathItem.Type),
						pathItem.Object,
						pathItem.Target,
						pathItem.Expression);
				}
			}

			exp += Environment.NewLine + "\t\t\t\t{" + Environment.NewLine;
			foreach (AggregateFunctionContext context in m_contexts)
			{
				IEnumerable<string> itemExpressions = context.Generator.GetIterationStatements(context, context.PathItems);
				foreach (string itemExpression in itemExpressions)
				{
					exp += "\t\t\t\t\t";
					exp += itemExpression;
					if(!exp.EndsWith(";"))
						exp += ";";
					exp += Environment.NewLine; // todo: smarter way
				}
			}
			exp += "\t\t\t\t}";

			CodeConditionStatement ifStatement = new CodeConditionStatement(
				new CodeSnippetExpression(pathItems[0].Target + "." + pathItems[0].Expression + " != null"),
				new CodeStatement[] { new CodeExpressionStatement(new CodeSnippetExpression(exp)) },
				new CodeStatement[0]);

			
			CodeStatementCollection statements = new CodeStatementCollection();
			statements.Add(ifStatement);

			CodeStatement[] arr = new CodeStatement[statements.Count];
			statements.CopyTo(arr, 0);
			return arr;		
		}

		private Type GetSourceItemType(IList<AggregateExpressionPathItem> items, MemberMappingDescriptor member)
		{
			if (items == null || items.Count < 1)
			{
				string msg = ErrorBuilder.InvalidAggregatePathError(member);
				throw new OtisException(msg);
			}

			return items[items.Count - 1].Type;
		}

		private IAggregateFunctionCodeGenerator GetGeneratorImpl(Type implementationType, MemberMappingDescriptor member)
		{
			Type[] interfaces = implementationType.GetInterfaces();
			foreach (Type itf in interfaces) // first check if it has its own implementation of IAggregateFunctionCodeGenerator
			{
				if (itf == typeof (IAggregateFunctionCodeGenerator))
					return Activator.CreateInstance(implementationType, true) as IAggregateFunctionCodeGenerator;
			}

			foreach (Type itf in interfaces) // now check if it only implement IAggregateFunction
			{
				if (itf.IsGenericType && itf.GetGenericTypeDefinition() == typeof(IAggregateFunction<>))
					return new DefaultAggregateFunctionCodeGenerator();
			}

			string msg = ErrorBuilder.InvalidAggregatePathError(member);
			throw new OtisException(msg); // test
		}

		private AggregateFunctionContext CreateMemberContext(ClassMappingDescriptor descriptor, MemberMappingDescriptor member, FunctionMap functionMap)
		{
			Type implementationType = functionMap.GetTypeForFunction(member.AggregateMappingDescription.FunctionName);
			string functionObjectName = string.Format("_{0}_to_{1}_Fn_",
				member.AggregateMappingDescription.FunctionObject,
				member.Member);
			if (implementationType.IsGenericType)
			{
				if (member.IsArray || member.IsList)
				{
					Type instanceType = member.IsArray ?
						member.AggregateMappingDescription.TargetType.GetElementType() :
						member.AggregateMappingDescription.TargetType.GetGenericArguments()[0];
					implementationType = implementationType.MakeGenericType(instanceType);
				}
				else
				{
					implementationType = implementationType.MakeGenericType(member.AggregateMappingDescription.TargetType);
				}
			}

			IAggregateFunctionCodeGenerator generator = GetGeneratorImpl(implementationType, member);

			return new AggregateFunctionContext(member,
								descriptor,
								implementationType,
								functionObjectName,
								generator);
		}

	}
}
