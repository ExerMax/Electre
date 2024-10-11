using Electre.src;
using Electre.src.Data;
using Index = Electre.src.Data.Index;

namespace Electre
{
    public class MyElectrePrinter : ElectrePrinter
    {
        private bool _print = true;
        public MyElectrePrinter() { }
        internal override void SetPrint(bool print = true)
        {
            _print = print;
        }

        internal override void PrintComparedPairs(List<ComparedPair> comparedPairs, List<Criterion> criterions)
        {
            if (_print is false) return;

            var pairs = comparedPairs.Select(x => new { A = x.PropA, B = x.PropB }).Distinct().ToList();

            Console.WriteLine("\nCompared pairs");
            Console.Write("\t\t");
            foreach (var criterion in criterions)
            {
                Console.Write($"{criterion.Name}\t");
            }
            foreach (var pair in pairs)
            {
                var line = comparedPairs.Where(x => x.PropA == pair.A && x.PropB == pair.B).ToList();

                Console.Write($"\n{pair.A}/{pair.B}\t\t");

                foreach (var val in line)
                {
                    Console.Write($"{val.Value}\t");
                }
            }
            Console.WriteLine("");
        }

        internal override void PrintIndexTable(List<Index> table, string title)
        {
            if (_print is false) return;

            var bprops = table.Select(x => x.PropB).Distinct().OrderBy(x => x).ToList();
            var aprops = table.Select(x => x.PropA).Distinct().OrderBy(x => x).ToList();

            Console.WriteLine($"\n{title}");
            Console.Write("\t\t");
            foreach (var prop in bprops)
            {
                Console.Write($"{prop}\t");
            }
            foreach (var prop in aprops)
            {
                Console.Write($"\n{prop}\t\t");

                foreach (var val in bprops)
                {
                    var valToPrint = table.Where(x => x.PropA == prop && x.PropB == val).FirstOrDefault()?.Value;
                    Console.Write($"{(valToPrint is not null ? Math.Round((double)valToPrint, 3) : "-")}\t");
                }
            }
            Console.WriteLine("");
        }

        internal override void PrintResultSet(List<Result> resultSet)
        {
            if (_print is false) return;

            Console.WriteLine("\nResult set");
            foreach (var res in resultSet.OrderByDescending(x => x.Count))
            {
                Console.WriteLine($"{res.Variant} : {res.Count}");
            }
            Console.WriteLine("");
        }
    }
}
