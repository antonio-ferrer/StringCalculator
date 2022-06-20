using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StringCalculator
{
    public class EquationFormula<T> where T : struct, IComparable, IFormattable, IConvertible
    {

        private readonly Regex regTerm = new Regex(@"^\w+$");
        private readonly Dictionary<string, T> terms = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        private readonly Regex regFormula = new Regex(@"^[\.\,a-zA-Z0-9)( \*\/\+\-\=\>\<]+$");
        private readonly Regex regParametrizedSubFormula = new Regex(@"(?<f>\w+)\((?<p>\w+(\s?\,\s?\w+)*)\)");
        private const string SUM = "sum";
        private const string COUNT = "count";
        private const string AVG = "avg";
        private const string MIN = "min";
        private const string MAX = "max";
        private const string SUBS = "subs";
        private readonly string[] subFormulas = new[] { SUM, AVG, COUNT, SUBS, MIN, MAX };
        
        private string formula;

        public string Formula
        {
            get => formula;
            set
            {
                if (!regFormula.IsMatch(value))
                {
                    throw new ArgumentException("Invalid formula sintax");
                }
                formula = value;
            }
        }

        public EquationFormula<T> SetTerm(string term, T value)
        {
            if (!regTerm.IsMatch(term))
                throw new ArgumentException("invalid name");
            if (terms.ContainsKey(term ?? ""))
                terms[term] = value;
            else
                terms.Add(term, value);
            return this;
        }

       
        public T? GetTerm(string term)
        {
            if (terms.ContainsKey(term ?? ""))
                return new Nullable<T>(terms[term]);
            return null;
        }

        public T this[string term]
        {
            set
            {
                SetTerm(term, value);
            }
            get
            {
                return GetTerm(term) ?? default(T);
            }
        }

        public string[] GetTerms()
        {
            var r = Regex.Matches(this.formula??string.Empty, @"(\w+)")?.Cast<Match>();
            r = r.Where(e => !Regex.IsMatch(e.Value, @"^\d+$") && !subFormulas.Contains(e.Value.ToLower()) );
            return r?.Cast<Match>()?.Select(m => m.Value)?.Distinct()?.ToArray() ?? new string[0];
        }

        public static implicit operator EquationFormula<T>(string formulaValue)
        {
            return new EquationFormula<T>() { Formula = formulaValue };
        }

        public override string ToString()
        {
            return Formula;
        }

        private string evaluateParametrizedSubFormula(string fx)
        {
            if (!regParametrizedSubFormula.IsMatch(fx))
                return fx;
            var subs = regParametrizedSubFormula.Matches(fx);
            foreach (Match sub in subs)
            {
                if (!sub.Groups["f"].Success)
                    continue;
                Func<string, string, string, string> eval = null;
                switch (sub.Groups["f"].Value.ToLower())
                {
                    case SUM:
                        eval = evaluateParametrizedSum;
                        break;
                    case AVG:
                        eval = evaluateParametrizedAvg;
                        break;
                    case COUNT:
                        eval = evaluateParametrizedCount;
                        break;
                    case MIN:
                        eval = evaluateParametrizedMin;
                        break;
                    case MAX:
                        eval = evaluateParametrizedMax;
                        break;
                    case SUBS:
                        eval = evaluateParametrizedSubs;
                        break;
                    default:
                        throw new ArgumentException("invalid subformula");
                }
                fx = eval(fx, sub.Groups["p"].Value, sub.Value);
            }
            return fx;
        }

        private string evaluateParametrizedMax(string fx, string parameters, string subExpression)
        {
            var p = parameters.Split(',').Select(i => i.Trim()).ToDictionary(k => k, k => this[k]);
            return fx.Replace(subExpression, $"({p.Values.Max()})");
        }

        private string evaluateParametrizedMin(string fx, string parameters, string subExpression)
        {
            var p = parameters.Split(',').Select(i => i.Trim()).ToDictionary(k => k, k => this[k]);
            return fx.Replace(subExpression, $"({p.Values.Min()})");
        }

        private string evaluateParametrizedCount(string fx, string parameters, string subExpression)
        {
            var p = parameters.Split(',').Select(i => i.Trim()).ToDictionary(k => k, k => this[k]);
            return fx.Replace(subExpression, $"({p.Values.Count()})");
        }

        private string evaluateParametrizedAvg(string fx, string parameters, string subExpression)
        {
            var p = parameters.Split(',').Select(i => i.Trim()).ToDictionary(k => k, k => this[k]);
            var values = p.Values.Select(i => Convert.ToSingle(i));
            return fx.Replace(subExpression, $"({values.Average()})");
        }

        private string evaluateParametrizedSum(string fx, string parameters, string subExpression)
        {
            var p = parameters.Split(',').Select(i => i.Trim()).ToDictionary(k => k, k => this[k]);
            var values = p.Values.Select(i => Convert.ToSingle(i));
            return fx.Replace(subExpression, $"({values.Sum()})");
        }

        private string evaluateParametrizedSubs(string fx, string parameters, string subExpression)
        {
            var p = parameters.Split(',').Select(i => i.Trim()).ToDictionary(k => k, k => this[k]);
            var values = p.Values.Select(i => Convert.ToSingle(i));
            var value = "0";
            foreach(var v in values)
            {
                if(v > default(Single))
                {
                    value = v.ToString();
                    break;
                }
            }
            return fx.Replace(subExpression, $"({value})");
        }

        public EquationFormula<T> UpdateFormula(string newFormula)
        {
            this.Formula = newFormula;
            return this;
        }

        public string[] GetSupportedFormulas()
        {
            return subFormulas;
        }

        public string GetExpression()
        {
            if (string.IsNullOrEmpty(formula))
                return null;

            var items = this.GetTerms();
            
            var fx = evaluateParametrizedSubFormula(formula);
            foreach(var i in items)
            {
                if (terms.ContainsKey(i))
                    fx = fx.Replace(i, terms[i].ToString());
            }
            return fx;
        }

    }
}
