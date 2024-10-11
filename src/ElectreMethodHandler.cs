using Electre.src.Data;
using Index = Electre.src.Data.Index;

namespace Electre.src
{
    public class ElectreMethodHandler
    {
        private ElectrePrinter _electrePrinter { get; set; }
        private List<Criterion> _criterions { get; set; }
        private List<Value> _values { get; set; }
        private int _criterionsSum {  get; set; }
        private List<ComparedPair> _comparedPairs { get; set; }
        private List<Index> _agreementMatrix { get; set; }
        private List<Index> _disagreementMatrix { get; set; }
        private List<Index> _superTable { get; set; }
        private List<Result> _resultSet { get; set; }

        public ElectreMethodHandler(ElectrePrinter printer, List<Criterion> criterions, List<Value> values)
        {
            _criterions = new List<Criterion>(criterions);
            _values = new List<Value>(values);
            _criterionsSum = _criterions.Sum(c => c.Weight);
            _electrePrinter = printer;
        }

        public List<Result> Handle(bool print = false)
        {
            _electrePrinter.SetPrint(print);

            ComparePairs();

            _electrePrinter.PrintComparedPairs(_comparedPairs, _criterions);

            CalculateAgreementMatrix();

            _electrePrinter.PrintIndexTable(_agreementMatrix, "Agreement maix");

            CalculateDisagreementMatrix();

            _electrePrinter.PrintIndexTable(_disagreementMatrix, "Disagreement maix");

            CalculateSuperiorityTable();

            _electrePrinter.PrintIndexTable(_superTable, "Superiority maix");

            MakeResultSet();

            _electrePrinter.PrintResultSet(_resultSet);

            return _resultSet;
        }

        private void ComparePairs()
        {
            _comparedPairs = _values
                .Select(x => _values
                    .Where(y => y.Option != x.Option)
                    .Where(y => y.Criterion == x.Criterion)
                    .Select(y => new ComparedPair { PropA = x.Option, PropB = y.Option, Criterion = y.Criterion, Value = Compare(x, y) })
                )
                .SelectMany(x => x)
                .ToList();
        }

        private void CalculateAgreementMatrix()
        {
            _agreementMatrix = (from pair in _comparedPairs
                                  group pair by new { pair.PropA, pair.PropB } into index
                                  select new Index
                                  {
                                      PropA = index.Key.PropA,
                                      PropB = index.Key.PropB,
                                      Value = index.Sum(x => x.Value * x.Criterion.Weight) / (double)_criterionsSum
                                  }).ToList();
        }

        private void CalculateDisagreementMatrix()
        {
            var disagreementMatrix = from pair in _comparedPairs
                                     group pair by new { pair.PropA, pair.PropB } into index
                                     select new Index
                                     {
                                         PropA = index.Key.PropA,
                                         PropB = index.Key.PropB,
                                         Value = _values
                                             .Where(x => x.Option == index.Key.PropB)
                                             .Join(
                                                 _values.Where(x => x.Option == index.Key.PropA),
                                                 a => a.Criterion,
                                                 b => b.Criterion,
                                                 (a, b) => new { Val = b.Val - a.Val, Res = (b.Val - a.Val) / (double)a.Criterion.Weight })
                                             .MaxBy(x => x.Val).Res
                                     };

            _disagreementMatrix = (from i in disagreementMatrix
                                 select new Index
                                 {
                                     PropA = i.PropA,
                                     PropB = i.PropB,
                                     Value = i.Value > 0 ? i.Value : 0
                                 }).ToList();
        }

        private void CalculateSuperiorityTable()
        {
            _superTable = _agreementMatrix.Join
                (
                    _disagreementMatrix,
                    a => a.PropA + a.PropB,
                    dis => dis.PropA + dis.PropB,
                    (a, dis) => new Index { PropA = a.PropA, PropB = a.PropB, Value = Superiority(a, dis) 
                })
                .ToList();
        }

        private void MakeResultSet()
        {
            _resultSet = (from s in _superTable
                            group s by s.PropA into r
                            select new Result() { Variant = r.Key, Count = r.Count(x => x.Value > 0) }).ToList();
        }

        private int Compare(Value a, Value b)
        {
            if (a.Val >= b.Val) return 1;
            else return 0;
        }

        private int Superiority(Index a, Index b)
        {
            if (a.Value > b.Value) return 1;
            else return 0;
        }
    }
}
