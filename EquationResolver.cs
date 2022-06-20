using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static StringCalculator.EquationResolver;

namespace StringCalculator
{


    [Serializable]
    public class EquationResolverException : Exception
    {
        public EquationResolverException() { }
        public EquationResolverException(string message) : base(message) { }
        public EquationResolverException(string message, Exception inner) : base(message, inner) { }
        protected EquationResolverException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public interface IEquationResolver
    {
        T EvaluateExpression<T>(string expr) where T : struct, IComparable, IFormattable, IConvertible;
        decimal EvaluateExpression(string expr);
        bool EvaluateBooleanExpression<T>(string expr, T referenceValue = default(T), BooleanComparisons comp = BooleanComparisons.GreaterThanOrEqual)
            where T : struct, IComparable, IFormattable, IConvertible;
        bool EvaluateBooleanExpression(string expr, decimal referenceValue = 0, BooleanComparisons comp = BooleanComparisons.GreaterThanOrEqual);
    }

    public class EquationResolver : IEquationResolver
    {
        private readonly DataTable tbEvaluator = new DataTable();
        private readonly Regex regExpr = new Regex(@"^[\.\,0-9)( \*\/\+\-\=\>\<]+$");
        public enum BooleanComparisons
        {
            GreaterThan,
            GreaterThanOrEqual,
            Equal,
            LessThan,
            LessThanOrEqual
        }

        public decimal EvaluateExpression(string expr)
        {
            return EvaluateExpression<decimal>(expr);
        }

        public T EvaluateExpression<T>(string expr) where T : struct, IComparable, IFormattable, IConvertible
         {
             if (string.IsNullOrEmpty(expr) || !regExpr.IsMatch(expr))
                 throw new EquationResolverException("invalid or blank expression");

             var result = tbEvaluator.Compute(expr, string.Empty);
            if (result.GetType().Name != typeof(T).Name)
                return (T)Convert.ChangeType(result, typeof(T));
            else
                return (T)result;
         }

        public bool EvaluateBooleanExpression(string expr, decimal referenceValue = 0, BooleanComparisons comp = BooleanComparisons.GreaterThanOrEqual)
        {
            return EvaluateBooleanExpression<decimal>(expr, referenceValue, comp);
        }

        public bool EvaluateBooleanExpression<T>(string expr, T referenceValue = default(T), BooleanComparisons comp = BooleanComparisons.GreaterThanOrEqual)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            if (string.IsNullOrEmpty(expr) || !regExpr.IsMatch(expr))
                throw new EquationResolverException("invalid or blank expression");

            var result = tbEvaluator.Compute(expr, string.Empty);
            if(result is bool)
                return Convert.ToBoolean(result);
            else
            {
                string logicExpr = result.ToString();
                switch (comp)
                {
                    case BooleanComparisons.Equal: logicExpr += $" = {referenceValue}"; break;
                    case BooleanComparisons.GreaterThan: logicExpr += $" > {referenceValue}"; break;
                    case BooleanComparisons.GreaterThanOrEqual: logicExpr += $" >= {referenceValue}"; break;
                    case BooleanComparisons.LessThan: logicExpr += $" < {referenceValue}"; break;
                    case BooleanComparisons.LessThanOrEqual: logicExpr += $" <= {referenceValue}"; break;
                }
                return Convert.ToBoolean(EvaluateExpression<T>(logicExpr));
            }
        }



    }
}
