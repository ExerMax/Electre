using Electre.src;
using Index = Electre.src.Index;

//INPUT VALUES
List<Criterion> criterions = new List<Criterion>()
{
    new Criterion { Name = "K1", Weight = 7},
    new Criterion { Name = "K2", Weight = 10}
};

int criterionsSum = criterions.Sum(c => c.Weight);

List<Value> values = new List<Value>()
{
    new Value { Val = 1, Option = "A1", Criterion = criterions[0]},
    new Value { Val = 3, Option = "A2", Criterion = criterions[0]},
    new Value { Val = 6, Option = "A3", Criterion = criterions[0]},
    new Value { Val = 2, Option = "A1", Criterion = criterions[1]},
    new Value { Val = 5, Option = "A2", Criterion = criterions[1]},
    new Value { Val = 4, Option = "A3", Criterion = criterions[1]}
};

//PRINT
Console.WriteLine("Input values");
foreach (var value in values)
{
    Console.WriteLine($"{value.Option} - {value.Criterion.Name} : {value.Val}");
}

//Pair comparison
var comparedPairs = values
    .Select(x => values
        .Where(y => y.Option != x.Option)
        .Where(y => y.Criterion == x.Criterion)
        .Select(y => new ComparedPair { PropA = x.Option, PropB = y.Option, Criterion = y.Criterion, Value = Compare(x, y) })
    )
    .SelectMany(x => x)
    .ToList();

//PRINT
Console.WriteLine("\nCompared pairs");
foreach (var pair in comparedPairs)
{
    Console.WriteLine($"{pair.PropA}/{pair.PropB} - {pair.Criterion.Name} : {pair.Value}");
}

//Agreement matrix
var agreementMatrix = from pair in comparedPairs
                      group pair by new { pair.PropA, pair.PropB } into index
                      select new Index
                      {
                          PropA = index.Key.PropA,
                          PropB = index.Key.PropB,
                          Value = index.Sum(x => x.Value * x.Criterion.Weight) / (double)criterionsSum
                      };

//PRINT
Console.WriteLine("\nAgreement matrix");
foreach (var pair in agreementMatrix)
{
    Console.WriteLine($"{pair.PropA}/{pair.PropB} : {pair.Value}");
}

//Disagreement matrix
var disagreementMatrix = from pair in comparedPairs
                      group pair by new { pair.PropA, pair.PropB } into index
                      select new Index
                      {
                          PropA = index.Key.PropA,
                          PropB = index.Key.PropB,
                          Value = values
                              .Where(x => x.Option == index.Key.PropB)
                              .Join(
                                  values.Where(x => x.Option == index.Key.PropA),
                                  a => a.Criterion,
                                  b => b.Criterion,
                                  (a, b) => new { Val = b.Val - a.Val, Res = (b.Val - a.Val) / (double)a.Criterion.Weight})
                              .MaxBy(x => x.Val).Res
                      };

disagreementMatrix = from i in disagreementMatrix
                     select new Index
                    {
                        PropA = i.PropA,
                        PropB = i.PropB,
                        Value = i.Value > 0 ? i.Value : 0
                    };

//PRINT
Console.WriteLine("\nDisagreement matrix");
foreach (var pair in disagreementMatrix)
{
    Console.WriteLine($"{pair.PropA}/{pair.PropB} : {pair.Value}");
}

//Superiority table
var superTable = agreementMatrix.Join(
    disagreementMatrix,
    a => a.PropA + a.PropB,
    dis => dis.PropA + dis.PropB,
    (a, dis) => new Index { PropA = a.PropA, PropB = a.PropB, Value = Superiority(a, dis) });

//PRINT
Console.WriteLine("\nSuperiority table");
foreach (var pair in superTable)
{
    Console.WriteLine($"{pair.PropA}/{pair.PropB} : {pair.Value}");
}

//Result set
var resultSet = from s in superTable
             group s by s.PropA into r
             select new { Variant = r.Key, Count = r.Count(x => x.Value > 0) };

//PRINT
Console.WriteLine("\nResult set");
foreach (var res in resultSet)
{
    Console.WriteLine($"{res.Variant} : {res.Count}");
}

var result = resultSet.MaxBy(x => x.Count);

//PRINT
Console.WriteLine("\nResult");
Console.WriteLine($"{result.Variant} better than {result.Count} options");

//Compare methods
int Compare(Value a, Value b)
{
    if (a.Val >= b.Val)          return 1;
    else                        return 0;
}

int Superiority(Index a, Index b)
{
    if (a.Value > b.Value) return 1;
    else return 0;
}