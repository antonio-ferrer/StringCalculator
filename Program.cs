using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace StringCalculator
{
    public class Program
    {
        protected Program()
        {
        }

        private static EquationResolver solver = new EquationResolver();
        static void Main(string[] args)
        {

            // Teste de todas as formulas suportadas
            /*
            EquationFormula<decimal> f =  "(AVG(N1a, N1b) * 0.4) + (N2 * 0.6)";
            f["N1a"] = 5;
            f["N1b"] = 4;
            f["N2"] = 7;
            var expr = f.GetExpression();
            var terms = f.GetTerms();
            var sss = solver.EvaluateExpression(f.GetExpression());

            foreach(var i in f.GetSupportedFormulas())
            {
                string nf = i + "(";
                foreach(var j in f.GetTerms())
                {
                    nf += j + ",";
                }
                //nf.Length--;
                nf += "0)";
                Console.WriteLine(nf);
                Console.WriteLine( f.UpdateFormula(nf).GetExpression());
                sss = solver.EvaluateExpression(f.GetExpression());
                Console.WriteLine($"r = {sss}\r\n");

            }
            */
           


            Console.WriteLine("Digite F para o modo Formula, E para o modo Expressão e S para sair...");
            var v = Console.ReadLine().ToUpper();
            switch (v)
            {
                case "F":
                    FormulaMode();
                    break;
                case "E":
                    ExpressionMode();
                    break;
                default:
                    return;
            }
        }


        static void FormulaMode()
        {
            string formula = "";
            while (true)
            {
                Console.Clear();
                if (string.IsNullOrEmpty(formula))
                {
                    Console.WriteLine("Informe a formula a ser usada:");
                    formula = Console.ReadLine();
                }
                try
                {
                    EquationFormula<decimal> eqFormula = formula;
                    var terms = eqFormula.GetTerms();
                    if(terms.Length == 0)
                    {
                        formula = "";
                        continue;
                    }

                    foreach(var term in terms)
                    {
                        Console.WriteLine("Informe o valor de " + term + ":");
                        eqFormula[term] = Convert.ToDecimal(Console.ReadLine());
                    }
                    var expr = eqFormula.GetExpression();
                    Console.WriteLine($"{eqFormula} = {expr} = {solver.EvaluateExpression<decimal>(expr)}");
                    Console.WriteLine("\r\nPressione V para trocar os valores da formula");
                    Console.WriteLine("Pressione F para inserir uma nova formula");
                    Console.WriteLine("Pressione S para sair");
                    var v = Console.ReadLine().ToUpper();
                    switch (v)
                    {
                        case "F": 
                            formula = ""; 
                            break;
                        case "V":
                            continue;
                        case "S":
                            return;
                    }


                }
                catch(Exception ex)
                {
                    Console.Clear();
                    Console.WriteLine("Ocorreu a falha: " + ex);
                    formula = "";
                    Console.WriteLine("Pressione qualquer tecla para continuar...");
                    Console.ReadLine();
                }
            }
        }

        static void ExpressionMode()
        {
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Digite a equação a ser resolvida e/ou Digite S para sair...");
                string expr = Console.ReadLine();
                if (Regex.IsMatch(expr, "[sS]"))
                    return;
                try
                {
                    var r = solver.EvaluateExpression<decimal>(expr);
                    Console.WriteLine(expr + " = " + r.ToString("0.00", new CultureInfo("pt-BR")));
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
                Console.WriteLine("Pressione qualquer tecla para continuar...");
                Console.ReadLine();
            }
        }
    }
}
