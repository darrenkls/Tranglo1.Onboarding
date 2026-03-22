using System;
using System.Linq;
using System.Linq.Expressions;

namespace Tranglo1.Onboarding.Domain.Common
{
	/// <summary>
	/// The implementation class is from here : https://enterprisecraftsmanship.com/posts/specification-pattern-c-implementation/
	/// However, the Expression instance created by <see cref="ToExpression"/> does not work in
	/// Entity Framework Core 3.0 when using <seealso cref="AndSpecification{T}"/> or <seealso cref="OrSpecification{T}"/>.
	/// To fix this, an internal ExpressionVisitor is used to fix the parameter of the right operand.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class Specification<T>
	{
		public static readonly Specification<T> All = new IdentitySpecification<T>();

		public bool IsSatisfiedBy(T entity)
		{
			var _Predicate = ToExpression().Compile();
			return _Predicate(entity);
		}

		public abstract Expression<Func<T, bool>> ToExpression();

		public Specification<T> And(Specification<T> specification)
		{
			if (this == All)
			{
				return specification;
			}

			if (specification == All)
			{
				return this;
			}

			return new AndSpecification<T>(this, specification);
		}

		public override string ToString()
		{
			return ToExpression().Body.ToString();
		}

		public Specification<T> Or(Specification<T> specification)
		{
			if (this == All || specification == All)
			{
				return All;
			}

			return new OrSpecification<T>(this, specification);
		}

		public Specification<T> Not()
		{
			return new NotSpecification<T>(this);
		}
	}

	/// <summary>
	/// https://stackoverflow.com/questions/58630723/add-two-expressions-to-create-a-predicate-in-entity-framework-core-3-does-not-wo
	/// </summary>
	internal sealed class ParameterReplaceVisitor : ExpressionVisitor
	{
		public ParameterExpression Target { get; set; }
		public ParameterExpression Replacement { get; set; }

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return node == Target ? Replacement : base.VisitParameter(node);
		}
	}

	internal sealed class IdentitySpecification<T> : Specification<T>
	{
		public override Expression<Func<T, bool>> ToExpression()
		{
			return c => true;
		}
	}

	internal sealed class AndSpecification<T> : Specification<T>
	{
		private readonly Specification<T> left;
		private readonly Specification<T> right;

		public AndSpecification(Specification<T> left, Specification<T> right)
		{
			this.left = left;
			this.right = right;
		}

		public override Expression<Func<T, bool>> ToExpression()
		{
			var _LeftExpression = left.ToExpression();
			var _RightExpression = right.ToExpression();

			var visitor = new ParameterReplaceVisitor()
			{
				Target = _RightExpression.Parameters[0],
				Replacement = _LeftExpression.Parameters[0],
			};

			var _RewrittenRight = visitor.Visit(_RightExpression.Body);

			BinaryExpression andExpression = Expression.AndAlso(_LeftExpression.Body, _RewrittenRight);
			return Expression.Lambda<Func<T, bool>>(andExpression, _LeftExpression.Parameters.Single());
		}
	}

	internal sealed class OrSpecification<T> : Specification<T>
	{
		private readonly Specification<T> left;
		private readonly Specification<T> right;

		public OrSpecification(Specification<T> left, Specification<T> right)
		{
			this.left = left;
			this.right = right;
		}

		public override Expression<Func<T, bool>> ToExpression()
		{
			var _LeftExpression = left.ToExpression();
			var _RightExpression = right.ToExpression();

			var visitor = new ParameterReplaceVisitor()
			{
				Target = _RightExpression.Parameters[0],
				Replacement = _LeftExpression.Parameters[0],
			};

			var _RewrittenRight = visitor.Visit(_RightExpression.Body);

			BinaryExpression orExpression = Expression.OrElse(_LeftExpression.Body, _RewrittenRight);
			return Expression.Lambda<Func<T, bool>>(orExpression, _LeftExpression.Parameters.Single());
		}
	}

	internal sealed class NotSpecification<T> : Specification<T>
	{
		private readonly Specification<T> specification;

		public NotSpecification(Specification<T> specification)
		{
			this.specification = specification;
		}

		public override Expression<Func<T, bool>> ToExpression()
		{
			Expression<Func<T, bool>> _Expression = specification.ToExpression();

			UnaryExpression notExpression = Expression.Not(_Expression.Body);
			return Expression.Lambda<Func<T, bool>>(notExpression, _Expression.Parameters.Single());
		}
	}
}
